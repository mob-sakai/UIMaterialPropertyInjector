using System;
using UnityEngine;

namespace Coffee.UIExtensions
{
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

        [Tooltip("The target UIMaterialPropertyInjector to tween.")]
        [SerializeField]
        private UIMaterialPropertyInjector m_Target;

        [Tooltip("The curve to tween the properties.")]
        [SerializeField]
        private AnimationCurve m_Curve = AnimationCurve.Linear(0, 0, 1, 1);

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

        [Tooltip("Whether to restart the tween when enabled.")]
        [SerializeField]
        private bool m_RestartOnEnable = true;

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

        private float _rate;
        private float _time;

        public Material defaultMaterialForRendering => m_Target ? m_Target.defaultMaterialForRendering : null;

        /// <summary>
        /// The target UIMaterialPropertyInjector to tween.
        /// </summary>
        public UIMaterialPropertyInjector target => m_Target;

        public InjectionPropertyPair[] propertyPairs => m_PropertyPairs;

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
                var evaluatedRate = m_Curve.Evaluate(_rate);
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
                if (_time < delay) return _time;
                var t = _time - delay;
                switch (wrapMode)
                {
                    case WrapMode.Clamp:
                    case WrapMode.PingPongOnce:
                        return Mathf.Clamp(t, 0, totalTime - delay) + delay;
                    case WrapMode.Loop:
                    case WrapMode.PingPong:
                        return Mathf.Repeat(t, totalTime - delay) + delay;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

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

        public bool restartOnEnable
        {
            get => m_RestartOnEnable;
            set => m_RestartOnEnable = value;
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

        private void Reset()
        {
            m_Target = GetComponent<UIMaterialPropertyInjector>();
        }

        private void Update()
        {
            switch (m_UpdateMode)
            {
                case UpdateMode.Normal:
                    UpdateTime(Time.deltaTime);
                    break;
                case UpdateMode.Unscaled:
                    UpdateTime(Time.unscaledDeltaTime);
                    break;
            }
        }

        private void OnEnable()
        {
            if (m_RestartOnEnable)
            {
                Restart();
            }
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
                if (p.from.propertyType == PropertyType.Undefined)
                {
                    p.from.Init(material);
                }

                if (p.to.propertyType == PropertyType.Undefined)
                {
                    p.to.Init(material);
                }
            }
        }

        public void Restart()
        {
            SetTime(0);
        }

        public void SetTime(float sec)
        {
            _time = 0;
            UpdateTime(sec);
        }

        public void UpdateTime(float deltaSec)
        {
            rate = UpdateTime_Internal(Mathf.Max(0, deltaSec)) / duration;
        }

        private float UpdateTime_Internal(float delta)
        {
            _time += delta;
            if (_time < delay) return 0;

            var t = _time - delay;
            switch (wrapMode)
            {
                case WrapMode.Loop:
                    t = Mathf.Repeat(t, duration + interval);
                    break;
                case WrapMode.PingPongOnce:
                    t = Mathf.Clamp(t, 0, duration * 2 + interval);
                    t = Mathf.PingPong(t, duration + interval * 0.5f);
                    break;
                case WrapMode.PingPong:
                    t = Mathf.Repeat(t, (duration + interval) * 2);
                    t = t < duration * 2 + interval
                        ? Mathf.PingPong(t, duration + interval * 0.5f)
                        : 0;
                    break;
            }

            return Mathf.Clamp(t, 0, duration);
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
