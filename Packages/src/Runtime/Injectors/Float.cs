using UnityEngine;

namespace Coffee.UIExtensions.Injectors
{
    [AddComponentMenu("")]
    internal class Float : Injector
    {
        [SerializeField] private float m_Value;

        public float value
        {
            get => m_Value;
            set
            {
                if (Mathf.Approximately(m_Value, value)) return;

                m_Value = value;
                SetDirty();
            }
        }

        public override PropertyType type => PropertyType.Float;

        protected override void OnValidate()
        {
            SetDirty();
            if (host != null)
            {
                host.SetFloat(propertyName, value);
            }
        }
    }
}
