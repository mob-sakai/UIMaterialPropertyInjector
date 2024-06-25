using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Coffee.UIExtensions
{
    [CustomPropertyDrawer(typeof(UIMaterialPropertyTweener.InjectionPropertyPair))]
    internal class InjectionPropertyPairDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return (EditorGUIUtility.singleLineHeight + 2) * 2;
        }

        public override void OnGUI(Rect r, SerializedProperty property, GUIContent label)
        {
            GUI.Box(new Rect(r.x - 2, r.y, r.width + 4, r.height), GUIContent.none);
            var from = property.FindPropertyRelative("m_From");
            label = new GUIContent(from.FindPropertyRelative("m_PropertyName").stringValue);
            var rect = new Rect(r.x, r.y + 2, r.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(rect, from, label);

            rect.y += rect.height + 2;
            rect.xMin += 24;
            var to = property.FindPropertyRelative("m_To");
            EditorGUI.PropertyField(rect, to);
        }
    }

    [CanEditMultipleObjects]
    [CustomEditor(typeof(UIMaterialPropertyTweener))]
    internal class UIMaterialPropertyTweenerEditor : Editor
    {
        private SerializedProperty _curve;
        private SerializedProperty _delay;
        private SerializedProperty _duration;
        private SerializedProperty _interval;
        private InjectionPropertyListDrawer _list;
        private SerializedProperty _restartOnEnable;
        private SerializedProperty _target;
        private SerializedProperty _updateMode;
        private SerializedProperty _wrapMode;

        private void OnEnable()
        {
            _curve = serializedObject.FindProperty("m_Curve");
            _restartOnEnable = serializedObject.FindProperty("m_RestartOnEnable");
            _delay = serializedObject.FindProperty("m_Delay");
            _duration = serializedObject.FindProperty("m_Duration");
            _interval = serializedObject.FindProperty("m_Interval");
            _wrapMode = serializedObject.FindProperty("m_WrapMode");
            _updateMode = serializedObject.FindProperty("m_UpdateMode");
            _target = serializedObject.FindProperty("m_Target");
            _list = new InjectionPropertyListDrawer(serializedObject.FindProperty("m_PropertyPairs"))
            {
                postAddCallback = PostAddElement,
                resetCallback = ResetCallback,
                draggable = false
            };
        }

        public override void OnInspectorGUI()
        {
            Profiler.BeginSample("(MPI)[MPTweenerEditor] OnInspectorGUI");
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(_target);
            EditorGUI.BeginDisabledGroup(!_target.objectReferenceValue);
            EditorGUILayout.PropertyField(_curve);
            EditorGUILayout.PropertyField(_delay);
            EditorGUILayout.PropertyField(_duration);
            EditorGUILayout.PropertyField(_interval);
            EditorGUILayout.PropertyField(_restartOnEnable);
            EditorGUILayout.PropertyField(_wrapMode);
            EditorGUILayout.PropertyField(_updateMode);
            _list.DoLayoutList();
            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();

            DrawPlayer(target as UIMaterialPropertyTweener);
            Profiler.EndSample();
        }

        private void DrawPlayer(UIMaterialPropertyTweener tweener)
        {
            if (!tweener) return;


            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            var icon = EditorGUIUtility.IconContent("icons/playbutton.png");
            if (GUILayout.Button(icon, "IconButton", GUILayout.Width(20)))
            {
                tweener.SetTime(0);
            }

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginChangeCheck();
            var totalTime = tweener.totalTime;
            var time = tweener.time;
            time = GUILayout.HorizontalSlider(time, 0, totalTime, GUILayout.ExpandWidth(true));
            if (EditorGUI.EndChangeCheck())
            {
                tweener.SetTime(time);
            }

            GUILayout.Label($"{time:N2}/{totalTime:N2}", GUILayout.ExpandWidth(false));
            EditorGUILayout.EndHorizontal();

            if (Application.isPlaying && tweener.isActiveAndEnabled)
            {
                Repaint();
            }
        }

        private static void PostAddElement(SerializedProperty prop, string propertyName)
        {
            prop.FindPropertyRelative("m_From.m_Type").intValue = -1;
            prop.FindPropertyRelative("m_From.m_PropertyName").stringValue = propertyName;
            prop.FindPropertyRelative("m_To.m_Type").intValue = -1;
            prop.FindPropertyRelative("m_To.m_PropertyName").stringValue = propertyName;
        }

        private void ResetCallback()
        {
            var current = serializedObject.targetObject as UIMaterialPropertyTweener;
            if (!current) return;

            Undo.RecordObject(current, "Reset Values");
            current.ResetPropertiesToDefault();
        }
    }
}
