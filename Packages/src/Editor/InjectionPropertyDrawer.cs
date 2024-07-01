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
                default:
                    return null;
            }
        }

        public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
        {
            var name = property.FindPropertyRelative("m_PropertyName").stringValue;
            var injector = property.FindPropertyRelative("m_Injector").objectReferenceValue as Injector;
            if (injector)
            {
                injector.hideFlags = HideFlags.None;
                var prop = new SerializedObject(injector).FindProperty("m_Value");
                DrawerRepository.instance.Get(injector.host.material)
                    .OnGUI(r, label, name, injector.type, prop);
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
                    .OnGUI(r, label, name, type, prop);
            }
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

                var key = material.shader.GetInstanceID();
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

            public void OnGUI(Rect r, GUIContent label, string name, PropertyType type, SerializedProperty prop)
            {
                var material = _editor ? _editor.target as Material : null;
                if (Event.current.type == EventType.Layout || !material || !material.shader) return;

                if (!IsValid(material, name, type))
                {
                    var warn = EditorGUIUtility.TrTextContentWithIcon(
                        "", $"{name} ({type}) is not found in the material.", "console.warnicon.sml");
                    EditorGUI.LabelField(new Rect(r.x, r.y, 18, 18), warn);
                    r.xMin += 18;
                }

                var bg = GUI.backgroundColor;
                if (prop.isAnimated)
                {
                    GUI.backgroundColor = s_InAnimationRecording()
                        ? AnimationMode.recordedPropertyColor
                        : AnimationMode.animatedPropertyColor;
                }

                ReadFrom(name, type, prop, material);
                var mp = MaterialEditor.GetMaterialProperty(_editor.targets, name);
                if (type == PropertyType.Texture || mp.name != name)
                {
                    if (prop.propertyType == SerializedPropertyType.Vector4)
                    {
                        EditorGUI.BeginChangeCheck();
                        var newValue = EditorGUI.Vector4Field(r, label, prop.vector4Value);
                        if (EditorGUI.EndChangeCheck())
                        {
                            prop.vector4Value = newValue;
                        }
                    }
                    else
                    {
                        EditorGUI.PropertyField(r, prop, label);
                    }
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    _editor.ShaderProperty(r, mp, label);
                    if (EditorGUI.EndChangeCheck())
                    {
                        WriteTo(type, prop, mp);
                        prop.serializedObject.ApplyModifiedProperties();
                    }
                }

                GUI.backgroundColor = bg;
            }

            private static bool IsValid(Material material, string propertyName, PropertyType type)
            {
                if (!material || !material.shader) return false;

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
                            return IsValid(material, origin, PropertyType.Texture);
                        }
                    }
                }

                return false;
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

            // private static void ReadFrom(PropertyType type, SerializedProperty prop, MaterialProperty mp)
            // {
            //     switch (type)
            //     {
            //         case PropertyType.Color:
            //             mp.colorValue = prop.colorValue;
            //             break;
            //         case PropertyType.Vector:
            //             mp.vectorValue = prop.vector4Value;
            //             break;
            //         case PropertyType.Float:
            //         case PropertyType.Range:
            //             mp.floatValue = prop.floatValue;
            //             break;
            //         case PropertyType.Int:
            //             mp.intValue = prop.intValue;
            //             break;
            //     }
            // }
        }
    }
}
