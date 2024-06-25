using UnityEngine;

namespace Coffee.UIExtensions
{
    [AddComponentMenu("")]
    public class Texture : Injector
    {
        [SerializeField] private UnityEngine.Texture m_Value;

        public UnityEngine.Texture value
        {
            get => m_Value;
            set
            {
                if (m_Value == value) return;

                m_Value = value;
                SetDirty();
            }
        }

        public override PropertyType type => PropertyType.Texture;

        protected override void OnValidate()
        {
            SetDirty();
            if (host)
            {
                host.SetTexture(propertyName, value);
            }
        }
    }
}
