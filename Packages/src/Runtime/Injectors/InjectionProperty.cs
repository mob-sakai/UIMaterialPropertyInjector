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
        private UnityEngine.Vector4 m_Vector;

        [SerializeField]
        private UnityEngine.Texture m_Texture;

        [SerializeField]
        private UnityEngine.Matrix4x4 m_Matrix;

        [SerializeField]
        private UnityEngine.Matrix4x4[] m_MatrixArray;

        [SerializeField]
        private float[] m_FloatArray;

        [SerializeField]
        private UnityEngine.Vector4[] m_VectorArray;

        [SerializeField]
        private Injector m_Injector;

        [SerializeField]
        private bool m_IsCustom;
#if UNITY_EDITOR
        [SerializeField]
        private bool m_ShouldInit;
#endif

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
            get => injector is Injectors.Int i ? i.value : m_Int;
            set
            {
                if (injector is Injectors.Int i)
                {
                    i.value = value;
                }

                if (m_Int == value) return;
                m_Int = value;

                if (host != null)
                {
                    host.SetDirty();
                }
            }
        }

        public float floatValue
        {
            get => injector is Injectors.Float i ? i.value : m_Float;
            set
            {
                if (injector is Injectors.Float i)
                {
                    i.value = value;
                }

                if (Mathf.Approximately(m_Float, value)) return;
                m_Float = value;

                if (host != null)
                {
                    host.SetDirty();
                }
            }
        }

        public UnityEngine.Color colorValue
        {
            get => injector is Injectors.Color i ? i.value : m_Color;
            set
            {
                if (injector is Injectors.Color i)
                {
                    i.value = value;
                }

                if (m_Color == value) return;
                m_Color = value;

                if (host != null)
                {
                    host.SetDirty();
                }
            }
        }

        public Vector4 vectorValue
        {
            get => injector is Injectors.Vector i ? i.value : m_Vector;
            set
            {
                if (injector is Injectors.Vector i)
                {
                    i.value = value;
                }

                if (m_Vector == value) return;
                m_Vector = value;

                if (host != null)
                {
                    host.SetDirty();
                }
            }
        }

        public UnityEngine.Texture textureValue
        {
            get => injector is Injectors.Texture i ? i.value : m_Texture;
            set
            {
                if (injector is Injectors.Texture i)
                {
                    i.value = value;
                }

                if (m_Texture == value) return;
                m_Texture = value;

                if (host != null)
                {
                    host.SetDirty();
                }
            }
        }

        public UnityEngine.Matrix4x4 matrixValue
        {
            get => m_Matrix;
            set
            {
                if (m_Matrix == value) return;
                m_Matrix = value;

                if (host != null)
                {
                    host.SetDirty();
                }
            }
        }

        public UnityEngine.Matrix4x4[] matrixArrayValue
        {
            get => m_MatrixArray;
            set
            {
                if (m_MatrixArray == value) return;
                m_MatrixArray = value;

                if (host != null)
                {
                    host.SetDirty();
                }
            }
        }

        public float[] floatArrayValue
        {
            get => m_FloatArray;
            set
            {
                if (m_FloatArray == value) return;
                m_FloatArray = value;

                if (host != null)
                {
                    host.SetDirty();
                }
            }
        }

        public UnityEngine.Vector4[] vectorArrayValue
        {
            get => m_VectorArray;
            set
            {
                if (m_VectorArray == value) return;
                m_VectorArray = value;

                if (host != null)
                {
                    host.SetDirty();
                }
            }
        }

        internal Injector injector
        {
            get => m_Injector;
            set
            {
                if (m_Injector == value) return;
                m_Injector = value;
                if (m_Injector is Injectors.Color ci)
                {
                    ci.value = m_Color;
                }
                else if (m_Injector is Injectors.Float fi)
                {
                    fi.value = m_Float;
                }
                else if (m_Injector is Injectors.Vector vi)
                {
                    vi.value = m_Vector;
                }
                else if (m_Injector is Injectors.Texture ti)
                {
                    ti.value = m_Texture;
                }
                else if (m_Injector is Injectors.Int ii)
                {
                    ii.value = m_Int;
                }
            }
        }

        public UIMaterialPropertyInjector host { get; set; }

        public bool shouldInit
        {
#if UNITY_EDITOR
            get => m_ShouldInit || m_Type == PropertyType.Undefined;
#else
            get => m_Type == PropertyType.Undefined;
#endif
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            id = Shader.PropertyToID(m_PropertyName);
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            id = Shader.PropertyToID(m_PropertyName);
        }

        public void Inject(List<Material> materials)
        {
            Profiler.BeginSample("(MPI)[InjectionProperty] Inject");
            for (var j = 0; j < materials.Count; j++)
            {
                Inject(materials[j]);
            }

            Profiler.EndSample();
        }

        public void Inject(Material material)
        {
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
                case PropertyType.Matrix:
                    material.SetMatrix(id, matrixValue);
                    break;
                case PropertyType.MatrixArray:
                    if (0 < matrixArrayValue?.Length)
                    {
                        material.SetMatrixArray(id, matrixArrayValue);
                    }

                    break;
                case PropertyType.FloatArray:
                    if (0 < floatArrayValue?.Length)
                    {
                        material.SetFloatArray(id, floatArrayValue);
                    }

                    break;
                case PropertyType.VectorArray:
                    if (0 < vectorArrayValue?.Length)
                    {
                        material.SetVectorArray(id, vectorArrayValue);
                    }

                    break;
            }
        }

        public void Inject(MaterialPropertyBlock material)
        {
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
                case PropertyType.Matrix:
                    material.SetMatrix(id, matrixValue);
                    break;
                case PropertyType.MatrixArray:
                    if (0 < matrixArrayValue?.Length)
                    {
                        material.SetMatrixArray(id, matrixArrayValue);
                    }

                    break;
                case PropertyType.FloatArray:
                    if (0 < floatArrayValue?.Length)
                    {
                        material.SetFloatArray(id, floatArrayValue);
                    }

                    break;
                case PropertyType.VectorArray:
                    if (0 < vectorArrayValue?.Length)
                    {
                        material.SetVectorArray(id, vectorArrayValue);
                    }

                    break;
            }
        }

        public void ResetToDefault(Material material)
        {
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
                case PropertyType.Matrix:
                    matrixValue = material.GetMatrix(id);
                    break;
                case PropertyType.MatrixArray:
                    matrixArrayValue = material.GetMatrixArray(id);
                    break;
                case PropertyType.FloatArray:
                    floatArrayValue = material.GetFloatArray(id);
                    break;
                case PropertyType.VectorArray:
                    vectorArrayValue = material.GetVectorArray(id);
                    break;
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
                    return Injector.AddInjector<Injectors.Color>(m_PropertyName, host);
                case PropertyType.Float:
                case PropertyType.Range:
                    return Injector.AddInjector<Injectors.Float>(m_PropertyName, host);
                case PropertyType.Vector:
                    return Injector.AddInjector<Injectors.Vector>(m_PropertyName, host);
                case PropertyType.Texture:
                    return Injector.AddInjector<Injectors.Texture>(m_PropertyName, host);
                case PropertyType.Int:
                    return Injector.AddInjector<Injectors.Int>(m_PropertyName, host);
            }

            return null;
        }

        public void Init(Material mat)
        {
            Profiler.BeginSample("(MPI)[InjectionProperty] Init");
            m_Color = default;
            m_Float = 0;
            m_Vector = default;
            m_Texture = null;
            m_Int = 0;
            m_Matrix = default;
            m_MatrixArray = null;
            m_FloatArray = null;
            m_VectorArray = null;
            m_Injector = null;
#if UNITY_EDITOR
            m_ShouldInit = false;
#endif
            id = Shader.PropertyToID(m_PropertyName);
            if (!mat || !mat.shader)
            {
                Profiler.EndSample();
                return;
            }

            var index = mat.shader.FindPropertyIndex(m_PropertyName);
            if (!m_IsCustom)
            {
                m_Type = 0 <= index
                    ? (PropertyType)mat.shader.GetPropertyType(index)
                    : PropertyType.Vector;
            }

            ResetToDefault(mat);
            Profiler.EndSample();
        }
    }
}
