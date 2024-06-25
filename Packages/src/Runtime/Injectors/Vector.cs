using UnityEngine;

namespace Coffee.UIExtensions
{
    [AddComponentMenu("")]
    public class Vector : Injector
    {
        [SerializeField] private Vector4 m_Value;

        public Vector4 value
        {
            get => m_Value;
            set
            {
                if (m_Value == value) return;

                m_Value = value;
                SetDirty();
            }
        }

        public override PropertyType type => PropertyType.Vector;

        protected override void OnValidate()
        {
            SetDirty();
            if (host)
            {
                host.SetVector(propertyName, value);
            }
        }
    }
}
