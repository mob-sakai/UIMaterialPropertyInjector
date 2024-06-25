using System.Linq;
using Coffee.UIMaterialPropertyInjectorInternal;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Coffee.UIExtensions
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIMaterialPropertyInjector))]
    internal class UIMaterialPropertyInjectorEditor : Editor
    {
        private SerializedProperty _animatable;
        private InjectionPropertyListDrawer _list;
        private SerializedProperty _resetValuesOnEnable;
        private SerializedProperty _sharingGroupId;

        private void OnEnable()
        {
            _resetValuesOnEnable = serializedObject.FindProperty("m_ResetValuesOnEnable");
            _animatable = serializedObject.FindProperty("m_Animatable");
            _sharingGroupId = serializedObject.FindProperty("m_SharingGroupId");
            _list = new InjectionPropertyListDrawer(serializedObject.FindProperty("m_Properties"))
            {
                postAddCallback = PostAddElement,
                resetCallback = ResetCallback,
                draggable = true
            };
        }

        public override void OnInspectorGUI()
        {
            Profiler.BeginSample("(MPI)[MPIEditor] OnInspectorGUI");
            var host = target as UIMaterialPropertyInjector;
            if (host)
            {
                host.RebuildPropertiesIfNeeded();
            }

            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(_resetValuesOnEnable);
            EditorGUILayout.PropertyField(_animatable);
            EditorGUILayout.PropertyField(_sharingGroupId);
            _list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            Profiler.EndSample();
        }

        private static void PostAddElement(SerializedProperty prop, string propertyName)
        {
            prop.FindPropertyRelative("m_Type").intValue = -1;
            prop.FindPropertyRelative("m_PropertyName").stringValue = propertyName;
        }

        private void ResetCallback()
        {
            var current = serializedObject.targetObject as UIMaterialPropertyInjector;
            if (!current) return;

            var objects = current.GetComponentsInChildren<Injector>(1)
                .OfType<Object>()
                .Append(current)
                .ToArray();
            Undo.RecordObjects(objects, "Reset Values");
            current.ResetPropertiesToDefault();
        }
    }
}
