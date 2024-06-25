using UnityEngine;

namespace Coffee.UIExtensions
{
    [AddComponentMenu("")]
    public class Float : Injector
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
            if (host)
            {
                host.SetFloat(propertyName, value);
            }
        }
    }
}
