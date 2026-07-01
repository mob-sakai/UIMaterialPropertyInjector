using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Coffee.UIMaterialPropertyInjectorInternal;
using UnityEditor;
using UnityEngine;

namespace Coffee.UIExtensions
{
    [CustomPropertyDrawer(typeof(InjectionProperty))]
    internal class InjectionPropertyDrawer : PropertyDrawer
    {
        private static readonly Func<bool> s_InAnimationRecording = typeof(AnimationMode)
            .GetMethod("InAnimationRecording", BindingFlags.Static | BindingFlags.NonPublic)
            .CreateDelegate(typeof(Func<bool>)) as Func<bool>;

        private static SerializedProperty GetProperty(SerializedProperty property, PropertyType type)
        {
            switch (type)
            {
                case PropertyType.Color:
                    return property.FindPropertyRelative("m_Color");
                case PropertyType.Float:
                case PropertyType.Range:
                    return property.FindPropertyRelative("m_Float");
                case PropertyType.Vector:
                    return property.FindPropertyRelative("m_Vector");
                case PropertyType.Texture:
                    return property.FindPropertyRelative("m_Texture");
                case PropertyType.Int:
                    return property.FindPropertyRelative("m_Int");
                case PropertyType.Matrix:
                    return property.FindPropertyRelative("m_Matrix");
                case PropertyType.MatrixArray:
                    return property.FindPropertyRelative("m_MatrixArray");
                case PropertyType.FloatArray:
                    return property.FindPropertyRelative("m_FloatArray");
                case PropertyType.VectorArray:
                    return property.FindPropertyRelative("m_VectorArray");
                default:
                    return null;
            }
        }

        public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
        {
            var name = property.FindPropertyRelative("m_PropertyName");
            var isCustom = property.FindPropertyRelative("m_IsCustom").boolValue && property.name != "m_To";
            var injector = property.FindPropertyRelative("m_Injector").objectReferenceValue as Injector;
            if (injector)
            {
                injector.hideFlags = HideFlags.None;
                var prop = new SerializedObject(injector).FindProperty("m_Value");
                DrawerRepository.instance.Get(injector.host.material)
                    .OnGUI(r, label, name, injector.type, prop, isCustom);
                injector.hideFlags = HideFlags.HideAndDontSave;
            }
            else
            {
                var so = property.serializedObject;
                var host = so.targetObject as UIMaterialPropertyInjector;
                host = host ? host : so.FindProperty("m_Target").objectReferenceValue as UIMaterialPropertyInjector;
                var type = (PropertyType)property.FindPropertyRelative("m_Type").intValue;
                var prop = GetProperty(property, type);
                DrawerRepository.instance.Get(host ? host.material : null)
                    .OnGUI(r, label, name, type, prop, isCustom);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var type = (PropertyType)property.FindPropertyRelative("m_Type").intValue;
            var prop = GetProperty(property, type);
            return prop != null ? EditorGUI.GetPropertyHeight(prop) : base.GetPropertyHeight(property, label);
        }

        private class DrawerRepository : ScriptableSingleton<DrawerRepository>
        {
            private readonly Dictionary<int, Drawer> _cache = new Dictionary<int, Drawer>();
            private readonly Drawer _invalidDrawer = new Drawer(null);

            private void OnDisable()
            {
                _invalidDrawer.Dispose();
                foreach (var drawer in _cache.Values)
                {
                    drawer.Dispose();
                }

                _cache.Clear();
            }

            public Drawer Get(Material material)
            {
                if (!material || !material.shader) return _invalidDrawer;

                var key = material.shader.GetHashCode();
                if (_cache.TryGetValue(key, out var drawer)) return drawer;
                return _cache[key] = new Drawer(new Material(material.shader) { hideFlags = HideFlags.DontSave });
            }
        }

        private class Drawer : IDisposable
        {
            private static readonly string[] s_HiddenPatterns = { "_ST$", "_HDR$", "_TexelSize$" };
            private MaterialEditor _editor;

            public Drawer(Material material)
            {
                _editor = material ? Editor.CreateEditor(material) as MaterialEditor : null;
            }

            public void Dispose()
            {
                if (_editor)
                {
                    Misc.DestroyImmediate(_editor.target);
                    Misc.DestroyImmediate(_editor);
                }

                _editor = null;
            }

            public void OnGUI(Rect r, GUIContent label, SerializedProperty nameProp, PropertyType type,
                SerializedProperty valueProp, bool isCustom)
            {
                var material = _editor ? _editor.target as Material : null;
                if (Event.current.type == EventType.Layout || !material || !material.shader) return;

                var name = nameProp.stringValue;
                if (!IsValid(material, name, type, isCustom))
                {
                    var warn = EditorGUIUtility.TrTextContentWithIcon(
                        "", $"{name} ({type}) is not found in the material.", "console.warnicon.sml");
                    EditorGUI.LabelField(new Rect(r.x, r.y, 18, 18), warn);
                    r.xMin += 18;
                }

                var bg = GUI.backgroundColor;
                if (valueProp.isAnimated)
                {
                    GUI.backgroundColor = s_InAnimationRecording()
                        ? AnimationMode.recordedPropertyColor
                        : AnimationMode.animatedPropertyColor;
                }

                var wideMode = EditorGUIUtility.wideMode;
                EditorGUIUtility.wideMode = true;
                ReadFrom(name, type, valueProp, material);
                var mp = MaterialEditor.GetMaterialProperty(_editor.targets, name);
                if (type == PropertyType.Texture || mp.name != name)
                {
                    if (valueProp.propertyType == SerializedPropertyType.Vector4)
                    {
                        EditorGUI.BeginChangeCheck();
                        if (isCustom)
                        {
                            DrawCustomLabel(ref r, nameProp, label);
                        }

                        var newValue = EditorGUI.Vector4Field(r, label, valueProp.vector4Value);
                        if (EditorGUI.EndChangeCheck())
                        {
                            valueProp.vector4Value = newValue;
                            valueProp.serializedObject.ApplyModifiedProperties();
                        }
                    }
                    else
                    {
                        EditorGUI.BeginChangeCheck();
                        if (isCustom)
                        {
                            DrawCustomLabel(ref r, nameProp, label);
                        }

                        EditorGUI.PropertyField(r, valueProp, label, true);
                        if (EditorGUI.EndChangeCheck())
                        {
                            valueProp.serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    if (isCustom)
                    {
                        DrawCustomLabel(ref r, nameProp, label);
                    }

                    var attributes = material.shader.GetPropertyAttributes(mp.name);
                    var attr = MaterialPropertyAttribute.Find(mp, attributes);
                    if (attr != null)
                    {
                        attr.OnGUI(r, label.text, mp);
                    }
                    else
                    {
                        DrawDefaultGUI(r, label.text, mp);
                    }

                    if (EditorGUI.EndChangeCheck())
                    {
                        WriteTo(type, valueProp, mp);
                        valueProp.serializedObject.ApplyModifiedProperties();
                    }
                }

                GUI.backgroundColor = bg;
                EditorGUIUtility.wideMode = wideMode;
            }

            private static bool IsValid(Material material, string propertyName, PropertyType type, bool isCustom)
            {
                if (!material || !material.shader) return false;
                if (isCustom) return true;

                var shader = material.shader;
                var index = shader.FindPropertyIndex(propertyName);
                if (0 <= index)
                {
                    var propertyType = (PropertyType)shader.GetPropertyType(index);
                    return propertyType == type
                           || (type == PropertyType.Range && propertyType == PropertyType.Float)
                           || (type == PropertyType.Float && propertyType == PropertyType.Range);
                }

                if (type == PropertyType.Vector)
                {
                    for (var i = 0; i < s_HiddenPatterns.Length; i++)
                    {
                        var pattern = s_HiddenPatterns[i];
                        var origin = Regex.Replace(propertyName, pattern, "");
                        if (propertyName != origin)
                        {
                            return IsValid(material, origin, PropertyType.Texture, false);
                        }
                    }
                }

                return false;
            }

            private static void DrawCustomLabel(ref Rect r, SerializedProperty nameProp, GUIContent label)
            {
                var labelWidth = EditorGUIUtility.labelWidth;
                var rLabel = r;
                rLabel.width = labelWidth - 18;
                rLabel.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.PropertyField(rLabel, nameProp, GUIContent.none);

                r.xMin += labelWidth;
                label.text = "";
            }

            private void DrawDefaultGUI(Rect r, string name, MaterialProperty mp)
            {
                switch (mp.type)
                {
                    case MaterialProperty.PropType.Color:
                        _editor.ColorProperty(r, mp, name);
                        break;
                    case MaterialProperty.PropType.Vector:
                        _editor.VectorProperty(r, mp, name);
                        break;
                    case MaterialProperty.PropType.Float:
                        _editor.FloatProperty(r, mp, name);
                        break;
                    case MaterialProperty.PropType.Range:
                        _editor.RangeProperty(r, mp, name);
                        break;
#if UNITY_2021_1_OR_NEWER
                    case MaterialProperty.PropType.Int:
                        _editor.IntegerProperty(r, mp, name);
                        break;
#endif
                    default:
                        _editor.ShaderProperty(r, mp, name);
                        break;
                }
            }

            private static void ReadFrom(string name, PropertyType type, SerializedProperty prop, Material material)
            {
                switch (type)
                {
                    case PropertyType.Color:
                        material.SetColor(name, prop.colorValue);
                        break;
                    case PropertyType.Vector:
                        material.SetVector(name, prop.vector4Value);
                        break;
                    case PropertyType.Float:
                    case PropertyType.Range:
                        material.SetFloat(name, prop.floatValue);
                        break;
                    case PropertyType.Int:
                        material.SetInt(name, prop.intValue);
                        break;
                }
            }

            private static void WriteTo(PropertyType type, SerializedProperty prop, MaterialProperty mp)
            {
                switch (type)
                {
                    case PropertyType.Color:
                        prop.colorValue = mp.colorValue;
                        break;
                    case PropertyType.Vector:
                        prop.vector4Value = mp.vectorValue;
                        break;
                    case PropertyType.Float:
                    case PropertyType.Range:
                        prop.floatValue = mp.floatValue;
                        break;
                    case PropertyType.Int:
#if UNITY_2021_1_OR_NEWER
                        prop.intValue = mp.intValue;
#else
                        prop.intValue = Mathf.RoundToInt(mp.floatValue);
#endif
                        break;
                }
            }
        }
    }
}
