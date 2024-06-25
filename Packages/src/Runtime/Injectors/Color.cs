using UnityEngine;

namespace Coffee.UIExtensions
{
    [AddComponentMenu("")]
    public class Color : Injector
    {
        [SerializeField] private UnityEngine.Color m_Value;

        public UnityEngine.Color value
        {
            get => m_Value;
            set
            {
                if (m_Value == value) return;

                m_Value = value;
                SetDirty();
            }
        }

        public override PropertyType type => PropertyType.Color;

        protected override void OnValidate()
        {
            SetDirty();
            if (host)
            {
                host.SetColor(propertyName, value);
            }
        }
    }
}
