using UnityEngine;

namespace Coffee.UIExtensions
{
    [AddComponentMenu("")]
    public class Int : Injector
    {
        [SerializeField] private int m_Value;

        public int value
        {
            get => m_Value;
            set
            {
                if (m_Value == value) return;

                m_Value = value;
                SetDirty();
            }
        }

        public override PropertyType type => PropertyType.Int;

        protected override void OnValidate()
        {
            SetDirty();
            if (host)
            {
                host.SetInt(propertyName, value);
            }
        }
    }
}
