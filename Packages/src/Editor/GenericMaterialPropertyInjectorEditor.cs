using UnityEditor;
using UnityEngine;

namespace Coffee.UIExtensions
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(GenericMaterialPropertyInjector))]
    internal class GenericMaterialPropertyInjectorEditor : UIMaterialPropertyInjectorEditor
    {
        private SerializedProperty _accessor;
        private Editor _settingsEditor;

        protected override void OnEnable()
        {
            base.OnEnable();
            _accessor = serializedObject.FindProperty("m_Accessor");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(_accessor);
            serializedObject.ApplyModifiedProperties();

            base.OnInspectorGUI();

            if (target is GenericMaterialPropertyInjector injector)
            {
                var labelWidth = EditorGUIUtility.labelWidth;
                EditorGUIUtility.labelWidth = 70;
                EditorGUILayout.LabelField("Debug", EditorStyles.boldLabel);
                EditorGUILayout.ObjectField("[Injected]", injector.material, typeof(Material), false);

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.ObjectField("[Base]", injector.baseMaterial, typeof(Material), false);
                if (GUILayout.Button("Inject", GUILayout.Width(50)))
                {
                    injector.Inject(injector.baseMaterial);
                }

                EditorGUILayout.EndHorizontal();
                EditorGUIUtility.labelWidth = labelWidth;
            }
        }
    }
}
