using System;
using UnityEditor;
using UnityEngine;

namespace Coffee.UIExtensions
{
    internal class TweenPlayer
    {
        public enum WrapMode
        {
            Once,
            Loop,
            PingPongOnce,
            PingPongLoop
        }

        public enum Event
        {
            Fetch,
            Delta,
            SetTime,
            Reset,
            Play,
            Pause
        }

        private double _lastTime;
        private Action<Event> _callback;
        private GUIStyle _backgroundStyle;
        private static readonly Color s_DelayColor = new Color(0.5f, 0.5f, 1.0f);
        private static readonly Color s_TweeningColor = new Color(0.5f, 1.0f, 0.5f);
        private static readonly Color s_IntervalColor = new Color(1.0f, 0.3f, 0.3f);


        public bool isPlaying { get; set; }
        public float totalTime { get; set; }
        public float time { get; set; }
        public float delay { get; set; }
        public float duration { get; set; }
        public float interval { get; set; }
        public WrapMode wrapMode { get; set; }
        public float delta { get; set; }

        public void OnEnable(Action<Event> callback)
        {
            _callback = callback;
            EditorApplication.update += Update;
        }

        public void OnDisable()
        {
            EditorApplication.update -= Update;
        }

        private void Update()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isPlaying || !isPlaying) return;

            delta = (float)(EditorApplication.timeSinceStartup - _lastTime);
            _lastTime = EditorApplication.timeSinceStartup;
            _callback?.Invoke(Event.Delta);
        }

        public void Draw()
        {
            if (_backgroundStyle == null)
            {
                _backgroundStyle = new GUIStyle("helpbox") { padding = new RectOffset(6, 6, 4, 2) };
            }

            if (UnityEngine.Event.current.type != EventType.Layout)
            {
                _callback?.Invoke(Event.Fetch);
            }

            EditorGUILayout.Space(4);
            GUILayout.Label(GUIContent.none, "sv_iconselector_sep", GUILayout.ExpandWidth(true));

            EditorGUILayout.BeginHorizontal(_backgroundStyle);
            var r = EditorGUILayout.GetControlRect(false);
            var rResetTimeButton = new Rect(r.x, r.y, 20, r.height);
            var resetContent = EditorGUIUtility.TrIconContent("animation.firstkey", "Reset Time");
            if (GUI.Button(rResetTimeButton, resetContent, "IconButton"))
            {
                _callback?.Invoke(Event.Reset);
            }

            var rPlayButton = new Rect(r.x + 20, r.y, 20, r.height);
            var playContent = EditorGUIUtility.TrIconContent("playbutton", "Play");
            if (GUI.Button(rPlayButton, playContent, "IconButton"))
            {
                isPlaying = true;
                _lastTime = EditorApplication.timeSinceStartup;
                _callback?.Invoke(Event.Play);
            }

            var rPauseButton = new Rect(r.x + 40, r.y, 20, r.height);
            var pauseContent = EditorGUIUtility.TrIconContent("pausebutton", "Pause");
            if (GUI.Button(rPauseButton, pauseContent, "IconButton"))
            {
                isPlaying = false;
                _callback?.Invoke(Event.Pause);
            }

            var label = EditorGUIUtility.TrTempContent($"{time:N2}/{totalTime:N2}");
            var rLabel = new Rect(r.x + r.width - 80, r.y, 80, r.height);
            GUI.Label(rLabel, label, "RightLabel");
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            var rSlider = new Rect(r.x + 60, r.y, r.width - 140, r.height);
            var r0 = new Rect(rSlider.x, rSlider.y + 4, rSlider.width, rSlider.height - 8);
            r0.x += DrawBackground(r0, rSlider.width * delay / totalTime, s_DelayColor);
            r0.x += DrawBackground(r0, rSlider.width * duration / totalTime, s_TweeningColor);

            if (WrapMode.Loop <= wrapMode)
            {
                r0.x += DrawBackground(r0, rSlider.width * interval / totalTime, s_IntervalColor);
            }

            if (WrapMode.PingPongOnce <= wrapMode)
            {
                r0.x += DrawBackground(r0, rSlider.width * duration / totalTime, s_TweeningColor);
            }

            if (WrapMode.PingPongLoop <= wrapMode)
            {
                r0.x += DrawBackground(r0, rSlider.width * interval / totalTime, s_IntervalColor);
            }

            GUI.color = Color.white;
            time = GUI.HorizontalSlider(rSlider, time, 0, totalTime);
            if (EditorGUI.EndChangeCheck())
            {
                _callback?.Invoke(Event.SetTime);
            }
        }

        private static float DrawBackground(Rect r, float width, Color color)
        {
            r.width = width;
            GUI.color = color;
            GUI.Label(r, GUIContent.none, "TE DefaultTime");
            return width;
        }
    }
}
