using UnityEngine;

namespace Coffee.UIExtensions.Injectors
{
    [AddComponentMenu("")]
    internal class Texture : Injector
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
            if (host != null)
            {
                host.SetTexture(propertyName, value);
            }
        }
    }
}
