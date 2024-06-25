using System;
using Coffee.UIMaterialPropertyInjectorInternal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Coffee.UIExtensions
{
    public class UIMaterialPropertyInjector_Demo : MonoBehaviour
    {
        [SerializeField]
        private UIMaterialPropertyInjector m_Injector;

        [SerializeField]
        private UIMaterialPropertyTweener m_Tweener;

        [SerializeField]
        private Slider m_TimeSlider;

        [SerializeField]
        private LayoutElement[] m_LayoutElements;

        [SerializeField]
        private Text m_MaterialCount;

        private int _materialCount;

        private void Awake()
        {
            if (m_TimeSlider)
            {
                var down = new EventTrigger.Entry() { eventID = EventTriggerType.PointerDown };
                down.callback.AddListener(_ => { enabled = false; });
                var up = new EventTrigger.Entry() { eventID = EventTriggerType.PointerUp };
                up.callback.AddListener(_ => { enabled = true; });
                var trigger = m_TimeSlider.gameObject.AddComponent<EventTrigger>();
                trigger.triggers.Add(down);
                trigger.triggers.Add(up);
                m_TimeSlider.onValueChanged.AddListener(OnTimeSliderValueChanged);
            }
        }

        private void OnEnable()
        {
            if (m_Tweener)
            {
                m_Tweener.updateMode = UIMaterialPropertyTweener.UpdateMode.Normal;
            }
        }

        private void OnDisable()
        {
            if (m_Tweener)
            {
                m_Tweener.updateMode = UIMaterialPropertyTweener.UpdateMode.Manual;
            }
        }

        public void OnTimeSliderValueChanged(float value)
        {
            var t = m_Tweener.totalTime * value;
            if (!Mathf.Approximately(m_Tweener.time, t))
            {
                m_Tweener.SetTime(t);
            }
        }

        private void Update()
        {
            if (m_TimeSlider)
            {
                m_TimeSlider.SetValueWithoutNotify(m_Tweener.time / m_Tweener.totalTime);
            }

            if (0 < m_LayoutElements.Length)
            {
                m_LayoutElements[0].flexibleWidth = m_Tweener.delay;
                m_LayoutElements[1].flexibleWidth = m_Tweener.duration;
                m_LayoutElements[2].flexibleWidth = UIMaterialPropertyTweener.WrapMode.Clamp <= m_Tweener.wrapMode
                    ? m_Tweener.interval
                    : 0;
                m_LayoutElements[3].flexibleWidth =
                    UIMaterialPropertyTweener.WrapMode.PingPongOnce <= m_Tweener.wrapMode
                        ? m_Tweener.duration
                        : 0;
                m_LayoutElements[4].flexibleWidth = UIMaterialPropertyTweener.WrapMode.PingPong <= m_Tweener.wrapMode
                    ? m_Tweener.interval
                    : 0;
            }

            if (m_MaterialCount)
            {
                var count = MaterialRepository.count;
                if (count != _materialCount)
                {
                    _materialCount = count;
                    m_MaterialCount.text = $"Generated Materials: {count.ToString()}";
                }
            }
        }

        public void SetTweenerWrapMode(int mode)
        {
            m_Tweener.wrapMode = (UIMaterialPropertyTweener.WrapMode)mode;
            m_Tweener.Restart();
        }

        public void SetTweenerFrom(float value)
        {
            m_Tweener.propertyPairs[0].from.floatValue = value;
        }

        public void SetTweenerTo(float value)
        {
            m_Tweener.propertyPairs[0].to.floatValue = value;
        }

        public void UseShareGroup(bool flag)
        {
            m_Injector.sharingGroupId = flag ? 1u : 0;
        }
    }
}
