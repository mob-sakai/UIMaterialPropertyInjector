using System;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Coffee.UIExtensions
{
    internal class InjectionPropertyListDrawer : ReorderableList
    {
        public Action<SerializedProperty, string> postAddCallback;
        public Action resetCallback;

        public InjectionPropertyListDrawer(SerializedProperty prop) : base(prop.serializedObject, prop)
        {
            drawHeaderCallback = DrawHeaderCallback;
            drawElementCallback = DrawElementCallback;
            onAddDropdownCallback = OnAddDropdownCallback;
            elementHeightCallback = i => EditorGUI.GetPropertyHeight(prop.GetArrayElementAtIndex(i)) + 2;
        }

        private void DrawElementCallback(Rect r, int i, bool _, bool __)
        {
            r = new Rect(r.x, r.y + 1, r.width, r.height - 2);
            EditorGUI.PropertyField(r, serializedProperty.GetArrayElementAtIndex(i));
        }

        private void DrawHeaderCallback(Rect r)
        {
            EditorGUI.LabelField(new Rect(r.x, r.y + 1, 100, r.height - 2), "Properties");
            var buttonRect = new Rect(r.x + r.width - 100, r.y + 1, 100, r.height - 2);
            if (resetCallback != null && GUI.Button(buttonRect, "Reset Values"))
            {
                EditorApplication.delayCall += resetCallback.Invoke;
            }
        }

        private void OnAddDropdownCallback(Rect r, ReorderableList _)
        {
            var propArray = serializedProperty;
            var target = propArray.serializedObject.targetObject;
            var current = target as UIMaterialPropertyInjector;
            if (!current && target is UIMaterialPropertyTweener tweener)
            {
                current = tweener.target;
            }

            if (!current) return;

            var shader = current.defaultMaterialForRendering.shader;
            var menu = new GenericMenu();
            var propCount = shader.GetPropertyCount();
            for (var i = 0; i < propCount; i++)
            {
                var propertyName = shader.GetPropertyName(i);
                var type = (PropertyType)shader.GetPropertyType(i);
                AddToMenu(menu, propArray, propertyName, type);

                if (type == PropertyType.Texture)
                {
                    AddToMenu(menu, propArray, $"{propertyName}_ST");
                    AddToMenu(menu, propArray, $"{propertyName}_HDR");
                    AddToMenu(menu, propArray, $"{propertyName}_TexelSize");
                }
            }

            menu.DropDown(r);
        }

        private void AddToMenu(GenericMenu menu, SerializedProperty propArray, string propertyName,
            PropertyType type = PropertyType.Vector)
        {
            if (Enumerable.Range(0, propArray.arraySize)
                .Select(propArray.GetArrayElementAtIndex)
                .Any(x => x.FindPropertyRelative("m_PropertyName")?.stringValue == propertyName))
            {
                return;
            }

            menu.AddItem(new GUIContent($"{propertyName} ({type})"), false, () =>
            {
                var prop = propArray.GetArrayElementAtIndex(propArray.arraySize++);
                postAddCallback?.Invoke(prop, propertyName);
                propArray.serializedObject.ApplyModifiedProperties();
            });
        }
    }
}
