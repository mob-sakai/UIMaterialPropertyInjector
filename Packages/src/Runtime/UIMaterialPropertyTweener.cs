using System;
using UnityEngine;
using Coffee.UIMaterialPropertyInjectorInternal;
using UnityEngine.Serialization;

namespace Coffee.UIExtensions
{
    [Icon("Packages/com.coffee.ui-material-property-injector/Icons/UIMaterialPropertyInjectorIcon.png")]
    public class UIMaterialPropertyTweener : MonoBehaviour, ISerializationCallbackReceiver
    {
        public enum UpdateMode
        {
            Normal,
            Unscaled,
            Manual
        }

        public enum WrapMode
        {
            Clamp,
            Loop,
            PingPongOnce,
            PingPong
        }

        public enum Direction
        {
            Forward,
            Reverse
        }

        public enum PlayOnEnable
        {
            None,
            PlayForward,
            PlayReverse,
            Play
        }

        [Tooltip("The target UIMaterialPropertyInjector to tween.")]
        [SerializeField]
        private UIMaterialPropertyInjector m_Target;

        [Tooltip("The direction of the tween.")]
        [SerializeField]
        private Direction m_Direction = Direction.Forward;

        [Tooltip("The curve to tween the properties.")]
        [SerializeField]
        private AnimationCurve m_Curve = AnimationCurve.Linear(0, 0, 1, 1);

        [SerializeField]
        private bool m_SeparateReverseCurve;

        [Tooltip("The curve to tween the properties.")]
        [SerializeField]
        private AnimationCurve m_ReverseCurve = AnimationCurve.Linear(0, 0, 1, 1);

        [Tooltip("The delay in seconds before the tween starts.")]
        [SerializeField]
        [Range(0f, 10)]
        private float m_Delay;

        [Tooltip("The duration in seconds of the tween.")]
        [SerializeField]
        [Range(0.05f, 10)]
        private float m_Duration = 1;

        [Tooltip("The interval in seconds between each loop.")]
        [SerializeField]
        [Range(0f, 10)]
        private float m_Interval;

        [FormerlySerializedAs("m_ResetTimeOnEnable")]
        [Tooltip("Play the tween when the component is enabled.")]
        [SerializeField]
        private PlayOnEnable m_PlayOnEnable = PlayOnEnable.PlayForward;

        [Tooltip("Reset the tweening time when the component is enabled.")]
        [SerializeField]
        private bool m_ResetTimeOnEnable = true;

        [Tooltip("The wrap mode of the tween.\n" +
                 "  Clamp: Clamp the tween value (not loop).\n" +
                 "  Loop: Loop the tween value.\n" +
                 "  PingPongOnce: PingPong the tween value (not loop).\n" +
                 "  PingPong: PingPong the tween value.")]
        [SerializeField]
        private WrapMode m_WrapMode = WrapMode.Loop;

        [Tooltip("Specifies how to get delta time.\n" +
                 "  Normal: Use `Time.deltaTime`.\n" +
                 "  Unscaled: Use `Time.unscaledDeltaTime`.\n" +
                 "  Manual: Not updated automatically and update manually with `UpdateTime` or `SetTime` method.")]
        [SerializeField]
        private UpdateMode m_UpdateMode = UpdateMode.Normal;

        [SerializeField]
        private InjectionPropertyPair[] m_PropertyPairs = new InjectionPropertyPair[0];

        private bool _isPaused;
        private float _rate = -1;
        private float _time;

        public Material defaultMaterialForRendering => m_Target ? m_Target.defaultMaterialForRendering : null;

        /// <summary>
        /// The target UIMaterialPropertyInjector to tween.
        /// </summary>
        public UIMaterialPropertyInjector target => m_Target;

        public InjectionPropertyPair[] propertyPairs => m_PropertyPairs;

        /// <summary>
        /// The direction of the tween.
        /// </summary>
        public Direction direction
        {
            get => m_Direction;
            set => m_Direction = value;
        }

        /// <summary>
        /// The rate of the tween.
        /// </summary>
        public float rate
        {
            get => _rate;
            set
            {
                value = Mathf.Clamp01(value);
                if (Mathf.Approximately(_rate, value)) return;

                _rate = value;

                if (target == null) return;

                var currentCurve = curve;
                if (separateReverseCurve)
                {
                    switch (wrapMode)
                    {
                        case WrapMode.Clamp:
                        case WrapMode.Loop:
                            if (direction == Direction.Reverse)
                            {
                                currentCurve = reverseCurve;
                            }

                            break;
                        case WrapMode.PingPongOnce:
                        case WrapMode.PingPong:
                            if (delay + duration + interval <= _time)
                            {
                                currentCurve = reverseCurve;
                            }

                            break;
                    }
                }

                var evaluatedRate = currentCurve.Evaluate(_rate);
                foreach (var p in m_PropertyPairs)
                {
                    p.SetValue(m_Target, evaluatedRate);
                }
            }
        }

        /// <summary>
        /// The duration in seconds of the tween.
        /// </summary>
        public float duration
        {
            get => m_Duration;
            set => m_Duration = Mathf.Max(0.001f, value);
        }

        /// <summary>
        /// The delay in seconds before the tween starts.
        /// </summary>
        public float delay
        {
            get => m_Delay;
            set => m_Delay = Mathf.Max(0, value);
        }

        /// <summary>
        /// The interval in seconds between each loop.
        /// </summary>
        public float interval
        {
            get => m_Interval;
            set => m_Interval = Mathf.Max(0, value);
        }

        /// <summary>
        /// The current time of the tween.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public float time
        {
            get
            {
                if (wrapMode == WrapMode.Clamp || wrapMode == WrapMode.PingPongOnce)
                {
                    return Mathf.Clamp(_time, 0, totalTime);
                }

                return Mathf.Repeat(_time, totalTime);
            }
        }

        /// <summary>
        /// The total time of the tween. (read only)
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public float totalTime
        {
            get
            {
                switch (wrapMode)
                {
                    case WrapMode.Clamp: return delay + duration;
                    case WrapMode.Loop: return delay + duration + interval;
                    case WrapMode.PingPongOnce: return delay + duration * 2 + interval;
                    case WrapMode.PingPong: return delay + duration * 2 + interval * 2;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Play the tween when the component is enabled.
        /// </summary>
        public PlayOnEnable playOnEnable
        {
            get => m_PlayOnEnable;
            set => m_PlayOnEnable = value;
        }

        /// <summary>
        /// Reset the tweening time when the component is enabled..
        /// </summary>
        public bool resetTimeOnEnable
        {
            get => m_ResetTimeOnEnable;
            set => m_ResetTimeOnEnable = value;
        }

        public bool restartOnEnable
        {
            get => m_PlayOnEnable == PlayOnEnable.PlayForward;
            set => m_PlayOnEnable = value ? PlayOnEnable.PlayForward : PlayOnEnable.None;
        }

        public WrapMode wrapMode
        {
            get => m_WrapMode;
            set => m_WrapMode = value;
        }

        public UpdateMode updateMode
        {
            get => m_UpdateMode;
            set => m_UpdateMode = value;
        }

        public AnimationCurve curve
        {
            get => m_Curve;
            set => m_Curve = value;
        }

        public bool separateReverseCurve
        {
            get => m_SeparateReverseCurve;
            set => m_SeparateReverseCurve = value;
        }

        public AnimationCurve reverseCurve
        {
            get => m_ReverseCurve;
            set => m_ReverseCurve = value;
        }

        /// <summary>
        /// Is the tween playing?
        /// </summary>
        public bool isTweening
        {
            get
            {
                if (_isPaused) return false;
                if (wrapMode == WrapMode.Loop || wrapMode == WrapMode.PingPong) return true;

                return direction == Direction.Forward
                    ? _time < totalTime
                    : 0 < _time;
            }
        }

        /// <summary>
        /// Is the tween paused?
        /// </summary>
        public bool isPaused => _isPaused;

        /// <summary>
        /// Is the tween delaying?
        /// </summary>
        public bool isDelaying => _time < delay;

        private void Reset()
        {
            m_Target = GetComponent<UIMaterialPropertyInjector>();
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            if (!isTweening) return;

            var deltaTime = m_UpdateMode == UpdateMode.Unscaled
                ? Time.unscaledDeltaTime
                : Time.deltaTime;
            UpdateTime(direction == Direction.Forward ? deltaTime : -deltaTime);
        }

        private void OnEnable()
        {
            _isPaused = true;

#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif

            switch (playOnEnable)
            {
                case PlayOnEnable.Play:
                    Play(resetTimeOnEnable);
                    break;
                case PlayOnEnable.PlayForward:
                    PlayForward(resetTimeOnEnable);
                    break;
                case PlayOnEnable.PlayReverse:
                    PlayReverse(resetTimeOnEnable);
                    break;
            }
        }

        private void OnDisable()
        {
            _isPaused = true;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            if (Application.isBatchMode || !m_Target) return;

            Rebuild();
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
        }

        private void Rebuild()
        {
            // Initialized yet.
            if (Array.FindIndex(m_PropertyPairs, x => x.shouldInit) < 0) return;

            var material = defaultMaterialForRendering;
            for (var i = 0; i < m_PropertyPairs.Length; i++)
            {
                var p = m_PropertyPairs[i];
                if (p.from.shouldInit)
                {
                    p.from.Init(material);
                }

                if (p.to.shouldInit)
                {
                    p.to.Init(material);
                }
            }
        }

        [Obsolete(
            "UIMaterialPropertyTweener.Restart has been deprecated. Use UIMaterialPropertyTweener.ResetTime instead (UnityUpgradable) -> ResetTime")]
        public void Restart()
        {
            ResetTime();
        }

        public void Play(bool resetTime)
        {
            if (resetTime)
            {
                ResetTime(direction);
            }

            Play();
        }

        public void Play()
        {
            _isPaused = false;
        }

        public void PlayForward(bool resetTime)
        {
            if (resetTime)
            {
                ResetTime(Direction.Forward);
            }

            PlayForward();
        }

        public void PlayForward()
        {
            direction = Direction.Forward;
            _isPaused = false;
        }

        public void PlayReverse(bool resetTime)
        {
            if (resetTime)
            {
                ResetTime(Direction.Reverse);
            }

            PlayReverse();
        }

        public void PlayReverse()
        {
            direction = Direction.Reverse;
            _isPaused = false;
        }

        public void Stop()
        {
            _isPaused = true;
            ResetTime();
        }

        public void SetPause(bool pause)
        {
            _isPaused = pause;
        }

        public void ResetTime()
        {
            SetTime(0);
        }

        public void ResetTime(Direction dir)
        {
            if (dir == Direction.Forward)
            {
                SetTime(0);
            }
            else
            {
                SetTime(totalTime - 0.0001f);
            }
        }

        public void SetTime(float sec)
        {
            _time = 0;
            UpdateTime(sec);
        }

        public void UpdateTime(float deltaSec)
        {
            var prevTweening = isTweening;
            var isLoop = wrapMode == WrapMode.Loop || wrapMode == WrapMode.PingPong;
            _time += deltaSec;
            if (isLoop)
            {
                if (_time < 0)
                {
                    _time = Mathf.Repeat(_time, totalTime);
                }
                else if (delay < _time)
                {
                    _time = Mathf.Repeat(_time - delay, totalTime - delay) + delay;
                }
                else if (deltaSec < 0 && delay <= _time - deltaSec)
                {
                    _time = Mathf.Repeat(_time - delay, totalTime - delay) + delay;
                }
            }
            else
            {
                _time = Mathf.Clamp(_time, 0, totalTime);
            }

            var t = _time - delay;
            if (t <= 0 && 0 <= _time)
            {
                rate = 0;
                return;
            }

            switch (wrapMode)
            {
                case WrapMode.Clamp:
                    t = Mathf.Clamp(t, 0, duration);
                    _time = t + delay;
                    break;
                case WrapMode.Loop:
                    t = Mathf.Repeat(t, duration + interval);
                    _time = t + delay;
                    break;
                case WrapMode.PingPongOnce:
                    t = Mathf.Clamp(t, 0, duration * 2 + interval);
                    _time = t + delay;
                    t = Mathf.PingPong(t, duration + interval * 0.5f);
                    break;
                case WrapMode.PingPong:
                    t = Mathf.Repeat(t, (duration + interval) * 2);
                    _time = t + delay;
                    t = t < duration * 2 + interval
                        ? Mathf.PingPong(t, duration + interval * 0.5f)
                        : 0;
                    break;
            }

            rate = Mathf.Clamp(t, 0, duration) / duration;
        }

        public void ResetPropertiesToDefault()
        {
            if (!m_Target) return;
            var material = m_Target.defaultMaterialForRendering;
            foreach (var p in m_PropertyPairs)
            {
                p.from.ResetToDefault(material);
                p.to.ResetToDefault(material);
            }
        }

        [Serializable]
        public class InjectionPropertyPair
        {
            [SerializeField] private InjectionProperty m_From;
            [SerializeField] private InjectionProperty m_To;

            public InjectionProperty from => m_From;
            public InjectionProperty to => m_To;

            public bool shouldInit => from.propertyType == PropertyType.Undefined
                                      || to.propertyType == PropertyType.Undefined;

            public void SetValue(UIMaterialPropertyInjector host, float rate)
            {
                var name = m_From.propertyName;
                switch (m_From.propertyType)
                {
                    case PropertyType.Color:
                        host.SetColor(name, UnityEngine.Color.Lerp(m_From.colorValue, m_To.colorValue, rate));
                        break;
                    case PropertyType.Vector:
                        host.SetVector(name, Vector4.Lerp(m_From.vectorValue, m_To.vectorValue, rate));
                        break;
                    case PropertyType.Float:
                    case PropertyType.Range:
                        host.SetFloat(name, Mathf.Lerp(m_From.floatValue, m_To.floatValue, rate));
                        break;
                    case PropertyType.Texture:
                        host.SetTexture(name, rate < 0.5 ? m_From.textureValue : m_To.textureValue);
                        break;
                    case PropertyType.Int:
                        host.SetInt(name, Mathf.RoundToInt(Mathf.Lerp(m_From.intValue, m_To.intValue, rate)));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
