using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Coffee.UIMaterialPropertyInjectorInternal;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;
using UnityEngine.UI;
#if TMP_ENABLE
using TMPro;
#endif

[assembly: InternalsVisibleTo("Coffee.UIMaterialPropertyInjector.Editor")]
[assembly: InternalsVisibleTo("Coffee.UIMaterialPropertyInjector.Demo")]

namespace Coffee.UIExtensions
{
    [ExecuteAlways]
    [RequireComponent(typeof(Graphic))]
    public class UIMaterialPropertyInjector : MonoBehaviour, IMaterialModifier, ISerializationCallbackReceiver
    {
        private static readonly List<Material> s_Materials = new List<Material>();

        [Tooltip("Reset all properties with the material properties when enabled.")]
        [HideInInspector]
        [SerializeField]
        private bool m_ResetValuesOnEnable;

        [Tooltip("Makes it animatable in the Animation view.")]
        [HideInInspector]
        [SerializeField]
        private bool m_Animatable = true;

        [Tooltip("Sharing group ID. If set, it shares the same material with the same group ID.\n" +
                 "NOTE: The material instances cannot be shared if the mask depth is different.")]
        [SerializeField]
        [HideInInspector]
        private uint m_SharingGroupId;

        [Tooltip("Properties to inject to the material.")]
        [HideInInspector]
        [SerializeField]
        private List<InjectionProperty> m_Properties = new List<InjectionProperty>();

        private List<UIMaterialPropertyInjector> _children;
        private bool _defaultMaterialMode;
        private bool _dirty;
        private Graphic _graphic;
        private Action _injectIfNeeded;
        private Material _material;
        private UIMaterialPropertyInjector _parent;
        private bool _shouldRebuild;

        private List<UIMaterialPropertyInjector> children => _children != null
            ? _children
            : _children = ListPool<UIMaterialPropertyInjector>.Rent();

        private bool canInject => _parent ? _parent.canInject : isActiveAndEnabled && 0 < m_Properties.Count;

        public List<InjectionProperty> properties
        {
            get
            {
                RebuildPropertiesIfNeeded();
                return _parent ? _parent.m_Properties : m_Properties;
            }
        }

        /// <summary>
        /// Makes it animatable in the Animation view.
        /// </summary>
        public virtual bool animatable
        {
            get => m_Animatable;
            set
            {
                if (m_Animatable == value) return;
                m_Animatable = value;
                ShouldRebuild();
                SetDirty();
            }
        }

        /// <summary>
        /// Sharing group ID. If set, it shares the same material with the same group ID.
        /// <para />
        /// NOTE: The material instances cannot be shared if the mask depth is different.
        /// </summary>
        public uint sharingGroupId
        {
            get => _parent ? _parent.sharingGroupId : m_SharingGroupId;
            set
            {
                if (m_SharingGroupId == value) return;
                m_SharingGroupId = value;
                SetMaterialDirty();
                SetDirty();
            }
        }

        public Graphic graphic => _graphic ? _graphic : _graphic = GetComponent<Graphic>();

        public Material material => 0 < graphic.canvasRenderer.materialCount
            ? graphic.canvasRenderer.GetMaterial()
            : graphic.materialForRendering;

        public Material defaultMaterialForRendering
        {
            get
            {
                Profiler.BeginSample("(MPI)[MPInjector] defaultMaterialForRendering");
                _defaultMaterialMode = true;
                var mat = graphic.materialForRendering;
                SetMaterialDirty();
                Profiler.EndSample();

                return mat;
            }
        }

        /// <summary>
        /// Reset all properties with the material properties when enabled.
        /// </summary>
        public bool resetOnEnable
        {
            get => m_ResetValuesOnEnable;
            set => m_ResetValuesOnEnable = value;
        }

        public int intValue
        {
            get => 0 < properties.Count ? properties[0].intValue : 0;
            set
            {
                if (properties.Count == 0) return;
                properties[0].intValue = value;
            }
        }

        public float floatValue
        {
            get => 0 < properties.Count ? properties[0].floatValue : 0;
            set
            {
                if (properties.Count == 0) return;
                properties[0].floatValue = value;
            }
        }

        public UnityEngine.Color colorValue
        {
            get => 0 < properties.Count ? properties[0].colorValue : UnityEngine.Color.white;
            set
            {
                if (properties.Count == 0) return;
                properties[0].colorValue = value;
            }
        }

        public Vector4 vectorValue
        {
            get => 0 < properties.Count ? properties[0].vectorValue : Vector4.zero;
            set
            {
                if (properties.Count == 0) return;
                properties[0].vectorValue = value;
            }
        }

        public UnityEngine.Texture textureValue
        {
            get => 0 < properties.Count ? properties[0].textureValue : null;
            set
            {
                if (properties.Count == 0) return;
                properties[0].textureValue = value;
            }
        }

        protected virtual void OnEnable()
        {
            Profiler.BeginSample("(MPI)[MPInjector] OnEnable");
            UpdateProperties();
            SetDirty();
            SetMaterialDirty();
            ShouldRebuild();

            if (resetOnEnable)
            {
                Profiler.BeginSample("(MPI)[MPInjector] OnEnable > ResetPropertiesToDefault");
                ResetPropertiesToDefault();
                Profiler.EndSample();
            }

            Profiler.BeginSample("(MPI)[MPInjector] OnEnable > Append Callback");
            UIExtraCallbacks.onAfterCanvasRebuild += _injectIfNeeded ?? (_injectIfNeeded = InjectIfNeeded);
            Profiler.EndSample();

            Profiler.EndSample();
        }

        protected virtual void OnDisable()
        {
            Profiler.BeginSample("(MPI)[MPInjector] OnDisable");
            UIExtraCallbacks.onAfterCanvasRebuild -= _injectIfNeeded;

            SetDirty();
            SetMaterialDirty();
            ShouldRebuild();
            MaterialRepository.Release(ref _material);
            Profiler.EndSample();
        }

        protected virtual void OnDestroy()
        {
            Profiler.BeginSample("(MPI)[MPInjector] OnDestroy");
            var childCount = transform.childCount;
            for (var i = childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).TryGetComponent<Injector>(out var injector))
                {
                    injector.Destroy();
                }
            }

            ListPool<UIMaterialPropertyInjector>.Return(ref _children);
            _injectIfNeeded = null;
            _graphic = null;
            _material = null;
            _parent = null;
            Profiler.EndSample();
        }

#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            UpdateProperties();
            SetDirty();
            SetMaterialDirty();
            ShouldRebuild();

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                EditorApplication.QueuePlayerLoopUpdate();
            }
#endif
        }
#endif

        Material IMaterialModifier.GetModifiedMaterial(Material baseMaterial)
        {
            if (_defaultMaterialMode)
            {
                _defaultMaterialMode = false;
                return baseMaterial;
            }

            if (!isActiveAndEnabled || baseMaterial == null || !canInject)
            {
                MaterialRepository.Release(ref _material);
                return baseMaterial;
            }

            Profiler.BeginSample("(MPI)[MPInjector] GetModifiedMaterial");
            Profiler.BeginSample("(MPI)[MPInjector] GetModifiedMaterial > Calc Hash");
            var pHash = 0L;
            for (var i = 0; i < properties.Count; i++)
            {
                pHash += (uint)properties[i].id;
            }

            Profiler.EndSample();

            Profiler.BeginSample("(MPI)[MPInjector] GetModifiedMaterial > Get");
            var groupId = sharingGroupId != 0 ? sharingGroupId : (uint)pHash.GetHashCode();
            var localId = sharingGroupId != 0 ? 0 : (uint)GetInstanceID();
            var hash = new Hash128((uint)baseMaterial.GetInstanceID(), groupId, localId, 0);

            // If the material has been changed, mark as dirty.
            _dirty |= !MaterialRepository.Valid(hash, _material);

            MaterialRepository.Get(hash, ref _material, m => new Material(m)
            {
                hideFlags = HideFlags.DontSave | HideFlags.NotEditable
            }, baseMaterial);
            Profiler.EndSample();

            Profiler.EndSample();
            return _material;
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            // Rebuild properties.
            m_Properties.Rebuild(this, false, true);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            // Rebuild properties.
            m_Properties.Rebuild(this, false, false);
        }

        private void OnDidApplyAnimationProperties()
        {
            UpdateProperties();
        }

        internal void RebuildPropertiesIfNeeded()
        {
            // If parent exists, rebuild properties in parent.
            if (_parent)
            {
                _parent.RebuildPropertiesIfNeeded();
                return;
            }

            // If not dirty, skip.
            if (!_shouldRebuild) return;
            _shouldRebuild = false;

            Profiler.BeginSample("(MPI)[MPInjector] RebuildPropertiesIfNeeded");
            SetDirty();
            var materialDirty = false;
            children.Clear();

            // Rebuild injectors in child transforms.
            var childCount = transform.childCount;
            for (var i = childCount - 1; i >= 0; i--)
            {
#if TMP_ENABLE
                // If TMP_SubMeshUI components are found in children, add UIMaterialPropertyInjector to them.
                // It links to the parent UIMaterialPropertyInjector to inject properties.
                if (graphic is TextMeshProUGUI && transform.GetChild(i).TryGetComponent<TMP_SubMeshUI>(out var sub))
                {
                    Profiler.BeginSample("(MPI)[MPInjector] RebuildPropertiesIfNeeded > Add for TMP_SubMeshUI");
                    var mpi = sub.GetOrAddComponent<UIMaterialPropertyInjector>();
                    mpi.hideFlags = HideFlags.DontSave;
                    mpi._parent = this;
                    mpi.SetDirty();
                    materialDirty = true;
                    children.Add(mpi);
                    Profiler.EndSample();
                }
#endif

                // Find injector in children.
                if (!transform.GetChild(i).TryGetComponent<Injector>(out var injector)) continue;
                if (animatable && m_Properties.TryGet(injector.id, out var ip))
                {
                    // Found: Rebind the injector
                    ip.injector = injector;
                    ip.injector.SetHost(this);
                    SetDirty();
                }
                else
                {
                    // Not found: Destroy the injector because it's not needed.
                    injector.Destroy();
                }
            }

            // Rebuild injectors.
            m_Properties.Rebuild(this, true, true);

            if (materialDirty)
            {
                SetMaterialDirty();
            }

            Profiler.EndSample();
        }

        /// <summary>
        /// Get or add property.
        /// </summary>
        private InjectionProperty GetOrAddProperty(string propertyName, PropertyType type)
        {
            // Find property by name.
            if (m_Properties.TryGet(propertyName, out var result)) return result;

            // Not found: Add new property.
            Profiler.BeginSample("(MPI)[Injector] GetOrAddProperty");
            var p = new InjectionProperty(this, propertyName, type);
            if (animatable)
            {
                // Animatable: Create new Injector if needed and rebind it.
                Profiler.BeginSample("(MPI)[Injector] GetOrAddProperty > AddInjector");
                p.injector = p.AddInjector(this);
                Profiler.EndSample();
            }

            // Reset to default first.
            p.ResetToDefault(material);
            m_Properties.Add(p);
            ShouldRebuild();
            Profiler.EndSample();
            return p;
        }

        private void InjectIfNeeded()
        {
            // Skip if not dirty.
            if (!graphic || !_dirty || !canInject) return;
            _dirty = false;
            Profiler.BeginSample("(MPI)[Injector] InjectIfNeeded");

            // Inject properties to materials.
            graphic.GetMaterialsForRendering(s_Materials);
            for (var i = 0; i < properties.Count; i++)
            {
                properties[i].Inject(s_Materials);
            }

            s_Materials.Clear();
            Profiler.EndSample();
        }

        /// <summary>
        /// Set material dirty for the graphic and children.
        /// </summary>
        private void SetMaterialDirty()
        {
            Profiler.BeginSample("(MPI)[MPInjector] SetMaterialDirty");

            // Set material dirty for the graphic.
            if (graphic && graphic.isActiveAndEnabled)
            {
                graphic.SetMaterialDirty();
            }

            // Set material dirty for children.
            if (_children == null)
            {
                Profiler.EndSample();
                return;
            }

            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child)
                {
                    child.SetMaterialDirty();
                }
            }

            Profiler.EndSample();
        }

        /// <summary>
        /// Mark the injectors as dirty.
        /// </summary>
        public void SetDirty()
        {
            _dirty = true;
            if (_children == null) return;

            Profiler.BeginSample("(MPI)[MPInjector] SetDirty");
            for (var i = 0; i < children.Count; i++)
            {
                var child = children[i];
                if (child)
                {
                    child.SetDirty();
                }
            }

            Profiler.EndSample();
        }

        /// <summary>
        /// Inject properties should be rebuilt because the properties added or removed.
        /// </summary>
        protected void ShouldRebuild()
        {
            _shouldRebuild = true;
        }


        /// <summary>
        /// Remove all properties.
        /// </summary>
        public void RemoveAllProperties()
        {
            m_Properties.Clear();
            ShouldRebuild();
        }

        /// <summary>
        /// Remove property by name.
        /// </summary>
        public void RemoveProperty(string propertyName)
        {
            if (!m_Properties.TryGet(propertyName, out var ip)) return;

            m_Properties.Remove(ip);
            ShouldRebuild();
        }

        /// <summary>
        /// Set color property.
        /// </summary>
        public void SetColor(string propertyName, UnityEngine.Color value)
        {
            GetOrAddProperty(propertyName, PropertyType.Color).colorValue = value;
        }

        /// <summary>
        /// Set float property.
        /// </summary>
        public void SetFloat(string propertyName, float value)
        {
            GetOrAddProperty(propertyName, PropertyType.Float).floatValue = value;
        }

        /// <summary>
        /// Set int property.
        /// </summary>
        public void SetInt(string propertyName, int value)
        {
            GetOrAddProperty(propertyName, PropertyType.Int).intValue = value;
        }

        /// <summary>
        /// Set vector property.
        /// </summary>
        public void SetVector(string propertyName, Vector4 value)
        {
            GetOrAddProperty(propertyName, PropertyType.Vector).vectorValue = value;
        }

        /// <summary>
        /// Set texture property.
        /// </summary>
        public void SetTexture(string propertyName, UnityEngine.Texture value)
        {
            GetOrAddProperty(propertyName, PropertyType.Texture).textureValue = value;
        }

        /// <summary>
        /// Reset all properties to default.
        /// </summary>
        public void ResetPropertiesToDefault()
        {
            Profiler.BeginSample("(MPI)[MPInjector] ResetPropertiesToDefault");
            var mat = defaultMaterialForRendering;
            RebuildPropertiesIfNeeded();
            m_Properties.ResetToDefault(mat);
            SetMaterialDirty();
            Profiler.EndSample();
        }

        /// <summary>
        /// Update properties.
        /// </summary>
        protected virtual void UpdateProperties()
        {
        }
    }
}
