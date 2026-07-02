using UnityEngine;
using UnityEngine.Profiling;
using Coffee.UIMaterialPropertyInjectorInternal;

namespace Coffee.UIExtensions
{
    [ExecuteAlways]
    [Icon("Packages/com.coffee.ui-material-property-injector/Icons/UIMaterialPropertyInjectorIcon.png")]
    public class GenericMaterialPropertyInjector : UIMaterialPropertyInjector
    {
        [SerializeField]
        private MaterialAccessor m_Accessor = new MaterialAccessor();

        [SerializeField]
        private Material m_BaseMaterial;

        public override Material material => _material;
        public Material baseMaterial => m_BaseMaterial;

        public override Material defaultMaterialForRendering => m_BaseMaterial
            ? m_BaseMaterial
            : m_Accessor.InitializeIfNeeded(gameObject)
                ? m_BaseMaterial = m_Accessor.Get()
                : null;

        protected override bool canInject => m_Accessor.InitializeIfNeeded(gameObject) && base.canInject;

        protected override void OnEnable()
        {
            base.OnEnable();
            RestoreMaterial();
            InjectIfNeeded();
        }

        protected override void OnDisable()
        {
            RestoreMaterial();
            base.OnDisable();
        }

        internal void RestoreMaterial()
        {
            if (m_BaseMaterial && m_Accessor.InitializeIfNeeded(gameObject))
            {
                m_Accessor.Set(m_BaseMaterial);
            }

            m_BaseMaterial = null;
        }

        protected override void InjectIfNeeded()
        {
            if (!canInject) return;

            // Base material has been changed.
            Profiler.BeginSample("(MPI)[Injector] InjectIfNeeded > Check the base material has been changed");
            var currentMaterial = m_Accessor.Get();
            if (_material != currentMaterial)
            {
                m_BaseMaterial = currentMaterial;
                _material = GetModifiedMaterial(m_BaseMaterial);
                _dirty = true;
            }

            Profiler.EndSample();

            // Skip if not dirty.
            if (!_dirty || !_material || _material == m_BaseMaterial) return;
            _dirty = false;

            // Inject properties to materials.
            Profiler.BeginSample("(MPI)[Injector] InjectIfNeeded > Inject properties to materials");
            s_Materials.Clear();
            s_Materials.Add(_material);
            for (var i = 0; i < properties.Count; i++)
            {
                properties[i].Inject(s_Materials);
            }

            s_Materials.Clear();
            m_Accessor.Set(_material);
            Profiler.EndSample();
        }
    }
}
