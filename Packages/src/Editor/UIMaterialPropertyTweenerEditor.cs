using System;
using System.Linq;
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
        private SerializedProperty _direction;
        private SerializedProperty _curve;
        private SerializedProperty _separateReverseCurve;
        private SerializedProperty _reverseCurve;
        private SerializedProperty _delay;
        private SerializedProperty _duration;
        private SerializedProperty _interval;
        private InjectionPropertyListDrawer _list;
        private SerializedProperty _playOnEnable;
        private SerializedProperty _resetTimeOnEnable;
        private SerializedProperty _target;
        private SerializedProperty _updateMode;
        private SerializedProperty _wrapMode;
        private readonly TweenPlayer _tweenPlayer = new TweenPlayer();

        private void OnEnable()
        {
            _direction = serializedObject.FindProperty("m_Direction");
            _curve = serializedObject.FindProperty("m_Curve");
            _separateReverseCurve = serializedObject.FindProperty("m_SeparateReverseCurve");
            _reverseCurve = serializedObject.FindProperty("m_ReverseCurve");
            _playOnEnable = serializedObject.FindProperty("m_PlayOnEnable");
            _resetTimeOnEnable = serializedObject.FindProperty("m_ResetTimeOnEnable");
            _delay = serializedObject.FindProperty("m_Delay");
            _duration = serializedObject.FindProperty("m_Duration");
            _interval = serializedObject.FindProperty("m_Interval");
            _wrapMode = serializedObject.FindProperty("m_WrapMode");
            _updateMode = serializedObject.FindProperty("m_UpdateMode");
            _target = serializedObject.FindProperty("m_Target");
            _list = new InjectionPropertyListDrawer(serializedObject.FindProperty("m_PropertyPairs"), false)
            {
                postAddCallback = PostAddElement,
                resetCallback = ResetCallback,
                draggable = false,
                elementHeight = (EditorGUIUtility.singleLineHeight + 2) * 2 + 2
            };

            _tweenPlayer.OnEnable(OnTweenEvent);
        }

        private void OnDisable()
        {
            _tweenPlayer.OnDisable();
        }

        public override void OnInspectorGUI()
        {
            Profiler.BeginSample("(MPI)[MPTweenerEditor] OnInspectorGUI");
            serializedObject.UpdateIfRequiredOrScript();
            EditorGUILayout.PropertyField(_target);
            EditorGUI.BeginDisabledGroup(!_target.objectReferenceValue);

            EditorGUILayout.PropertyField(_direction);
            EditorGUILayout.PropertyField(_curve);

            var pos = EditorGUILayout.GetControlRect();
            var rect = new Rect(pos.x, pos.y, EditorGUIUtility.labelWidth + 20, pos.height);
            EditorGUI.PropertyField(rect, _separateReverseCurve);
            if (_separateReverseCurve.boolValue)
            {
                rect.x += rect.width;
                rect.width = pos.width - rect.width;
                EditorGUI.PropertyField(rect, _reverseCurve, GUIContent.none);
            }

            EditorGUILayout.PropertyField(_delay);
            EditorGUILayout.PropertyField(_duration);
            EditorGUILayout.PropertyField(_interval);
            EditorGUILayout.PropertyField(_playOnEnable);
            EditorGUILayout.PropertyField(_resetTimeOnEnable);
            EditorGUILayout.PropertyField(_wrapMode);
            EditorGUILayout.PropertyField(_updateMode);
            _list.DoLayoutList();
            EditorGUI.EndDisabledGroup();
            serializedObject.ApplyModifiedProperties();

            _tweenPlayer.Draw();
            Profiler.EndSample();
        }

        private static void PostAddElement(SerializedProperty prop, ShaderProperty s)
        {
            var name = s.isCustom ? s.type.ToString() : s.name;
            prop.FindPropertyRelative("m_From.m_PropertyName").stringValue =
                prop.FindPropertyRelative("m_To.m_PropertyName").stringValue = name;
            prop.FindPropertyRelative("m_From.m_IsCustom").boolValue =
                prop.FindPropertyRelative("m_To.m_IsCustom").boolValue = s.isCustom;
            prop.FindPropertyRelative("m_From.m_Type").intValue =
                prop.FindPropertyRelative("m_To.m_Type").intValue = s.isCustom ? (int)s.type : -1;
            prop.FindPropertyRelative("m_From.m_ShouldInit").boolValue =
                prop.FindPropertyRelative("m_To.m_ShouldInit").boolValue = true;
        }

        private void ResetCallback()
        {
            var current = serializedObject.targetObject as UIMaterialPropertyTweener;
            if (!current) return;

            Undo.RecordObject(current, "Reset Values");
            current.ResetPropertiesToDefault();
        }

        private void OnTweenEvent(TweenPlayer.Event ev)
        {
            switch (ev)
            {
                case TweenPlayer.Event.Fetch:

                    if (target is UIMaterialPropertyTweener current)
                    {
                        _tweenPlayer.totalTime = current.totalTime;
                        _tweenPlayer.time = current.time;
                        _tweenPlayer.delay = current.delay;
                        _tweenPlayer.duration = current.duration;
                        _tweenPlayer.interval = current.interval;
                        _tweenPlayer.wrapMode = (TweenPlayer.WrapMode)current.wrapMode;

                        if (current.isTweening && EditorApplication.isPlaying)
                        {
                            // Repaint();
                        }
                    }

                    break;
                case TweenPlayer.Event.Delta:
                    ForEach(t =>
                    {
                        t.UpdateTime(t.direction == UIMaterialPropertyTweener.Direction.Forward
                            ? _tweenPlayer.delta
                            : -_tweenPlayer.delta);
                    });
                    Repaint();
                    break;
                case TweenPlayer.Event.SetTime:
                    ForEach(t => t.SetTime(_tweenPlayer.time));
                    break;
                case TweenPlayer.Event.Reset:
                    ForEach(t => t.ResetTime(t.direction));
                    break;
                case TweenPlayer.Event.Play:
                    ForEach(t => t.Play(false));
                    break;
                case TweenPlayer.Event.Pause:
                    ForEach(t => t.SetPause(true));
                    break;
            }
        }

        private void ForEach(Action<UIMaterialPropertyTweener> action)
        {
            foreach (var tweener in targets.OfType<UIMaterialPropertyTweener>())
            {
                action(tweener);
            }
        }
    }
}
