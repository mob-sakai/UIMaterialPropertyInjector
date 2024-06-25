using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace Coffee.UIExtensions
{
    [Serializable]
    public class InjectionProperty : ISerializationCallbackReceiver
    {
        [SerializeField]
        private string m_PropertyName;

        [SerializeField]
        private PropertyType m_Type;

        [SerializeField]
        private int m_Int;

        [SerializeField]
        private float m_Float;

        [SerializeField]
        private UnityEngine.Color m_Color;

        [SerializeField]
        private Vector4 m_Vector;

        [SerializeField]
        private UnityEngine.Texture m_Texture;

        [SerializeField]
        private Injector m_Injector;

        public InjectionProperty()
        {
        }

        public InjectionProperty(UIMaterialPropertyInjector host, string propertyName, PropertyType type)
        {
            m_PropertyName = propertyName;
            m_Type = type;
            id = Shader.PropertyToID(propertyName);
            this.host = host;
        }

        public int id { get; private set; }
        public string propertyName => m_PropertyName;
        public PropertyType propertyType => m_Type;

        public int intValue
        {
            get => injector is Int i ? i.value : m_Int;
            set
            {
                if (injector is Int i)
                {
                    i.value = value;
                }

                if (m_Int == value) return;
                m_Int = value;

                if (host)
                {
                    host.SetDirty();
                }
            }
        }

        public float floatValue
        {
            get => injector is Float i ? i.value : m_Float;
            set
            {
                if (injector is Float i)
                {
                    i.value = value;
                }

                if (Mathf.Approximately(m_Float, value)) return;
                m_Float = value;

                if (host)
                {
                    host.SetDirty();
                }
            }
        }

        public UnityEngine.Color colorValue
        {
            get => injector is Color i ? i.value : m_Color;
            set
            {
                if (injector is Color i)
                {
                    i.value = value;
                }

                if (m_Color == value) return;
                m_Color = value;

                if (host)
                {
                    host.SetDirty();
                }
            }
        }

        public Vector4 vectorValue
        {
            get => injector is Vector i ? i.value : m_Vector;
            set
            {
                if (injector is Vector i)
                {
                    i.value = value;
                }

                if (m_Vector == value) return;
                m_Vector = value;

                if (host)
                {
                    host.SetDirty();
                }
            }
        }

        public UnityEngine.Texture textureValue
        {
            get => injector is Texture i ? i.value : m_Texture;
            set
            {
                if (injector is Texture i)
                {
                    i.value = value;
                }

                if (m_Texture == value) return;
                m_Texture = value;

                if (host)
                {
                    host.SetDirty();
                }
            }
        }

        public Injector injector
        {
            get => m_Injector;
            set
            {
                if (m_Injector == value) return;
                m_Injector = value;
                if (m_Injector is Color ci)
                {
                    ci.value = m_Color;
                }
                else if (m_Injector is Float fi)
                {
                    fi.value = m_Float;
                }
                else if (m_Injector is Vector vi)
                {
                    vi.value = m_Vector;
                }
                else if (m_Injector is Texture ti)
                {
                    ti.value = m_Texture;
                }
                else if (m_Injector is Int ii)
                {
                    ii.value = m_Int;
                }
            }
        }

        public UIMaterialPropertyInjector host { get; set; }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            id = Shader.PropertyToID(m_PropertyName);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            id = Shader.PropertyToID(m_PropertyName);
        }

        public bool IsValid(Material material)
        {
            if (!material) return false;

#if UNITY_2021_1_OR_NEWER
            switch (propertyType)
            {
                case PropertyType.Color:
                    return material.HasColor(id);
                case PropertyType.Float:
                case PropertyType.Range:
                    return material.HasFloat(id);
                case PropertyType.Vector:
                    return material.HasVector(id);
                case PropertyType.Texture:
                    return material.HasTexture(id);
                case PropertyType.Int:
                    return material.HasInt(id);
            }

            return false;
#else
            return material.HasProperty(id);
#endif
        }

        public void Inject(List<Material> materials)
        {
            for (var j = 0; j < materials.Count; j++)
            {
                Inject(materials[j]);
            }
        }

        public void Inject(Material material)
        {
            if (!IsValid(material)) return;

            switch (propertyType)
            {
                case PropertyType.Color:
                    material.SetColor(id, colorValue);
                    break;
                case PropertyType.Float:
                case PropertyType.Range:
                    material.SetFloat(id, floatValue);
                    break;
                case PropertyType.Vector:
                    material.SetVector(id, vectorValue);
                    break;
                case PropertyType.Texture:
                    material.SetTexture(id, textureValue);
                    break;
                case PropertyType.Int:
                    material.SetInt(id, intValue);
                    break;
            }
        }

        public void ResetToDefault(Material material)
        {
            if (!IsValid(material)) return;

            switch (propertyType)
            {
                case PropertyType.Color:
                    colorValue = material.GetColor(id);
                    break;
                case PropertyType.Float:
                case PropertyType.Range:
                    floatValue = material.GetFloat(id);
                    break;
                case PropertyType.Vector:
                    vectorValue = material.GetVector(id);
                    break;
                case PropertyType.Texture:
                    textureValue = material.GetTexture(id);
                    break;
                case PropertyType.Int:
                    intValue = material.GetInt(id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        internal Injector AddInjector(UIMaterialPropertyInjector host)
        {
#if UNITY_EDITOR
            // Prefab asset: New GameObject can not be created. skip.
            if (PrefabUtility.IsPartOfPrefabAsset(host.gameObject)) return null;
#endif
            switch (propertyType)
            {
                case PropertyType.Color:
                    return Injector.AddInjector<Color>(m_PropertyName, host);
                case PropertyType.Float:
                case PropertyType.Range:
                    return Injector.AddInjector<Float>(m_PropertyName, host);
                case PropertyType.Vector:
                    return Injector.AddInjector<Vector>(m_PropertyName, host);
                case PropertyType.Texture:
                    return Injector.AddInjector<Texture>(m_PropertyName, host);
                case PropertyType.Int:
                    return Injector.AddInjector<Int>(m_PropertyName, host);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void Init(Material mat)
        {
            Profiler.BeginSample("(MPI)[InjectionProperty] Init");
            m_Color = default;
            m_Float = default;
            m_Vector = default;
            m_Texture = default;
            m_Int = default;
            m_Injector = default;
            id = Shader.PropertyToID(m_PropertyName);
            if (!mat || !mat.shader) return;

            var index = mat.shader.FindPropertyIndex(m_PropertyName);
            m_Type = 0 <= index
                ? (PropertyType)mat.shader.GetPropertyType(index)
                : PropertyType.Vector;
            ResetToDefault(mat);
            Profiler.EndSample();
        }
    }
}
