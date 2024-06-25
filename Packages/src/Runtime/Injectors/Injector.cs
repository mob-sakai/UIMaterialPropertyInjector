using Coffee.UIMaterialPropertyInjectorInternal;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Profiling;

namespace Coffee.UIExtensions
{
    [ExecuteAlways]
    public abstract class Injector : MonoBehaviour, ISerializationCallbackReceiver
    {
        [SerializeField] protected string m_PropertyName;
        [SerializeField] private UIMaterialPropertyInjector m_Host;

        public int id { get; private set; }

        internal UIMaterialPropertyInjector host => m_Host;
        public string propertyName => m_PropertyName;
        public abstract PropertyType type { get; }

        private void OnDidApplyAnimationProperties()
        {
            SetDirty();
        }

        protected abstract void OnValidate();

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            id = Shader.PropertyToID(m_PropertyName);
        }

        protected void SetDirty()
        {
            if (host)
            {
                host.SetDirty();
            }
        }

        public void SetHost(UIMaterialPropertyInjector newHost)
        {
            if (m_Host == newHost || !newHost) return;
            m_Host = newHost;
            transform.SetParent(newHost.transform, false);

#if ENABLE_ANIMATION
            Profiler.BeginSample("(MPI)[Injector] Animator.BindSceneTransform");
            var tr = newHost.transform;
            while (tr)
            {
                if (tr.TryGetComponent<Animator>(out var animator))
                {
                    animator.BindSceneTransform(transform);
                }

                tr = tr.parent;
            }

            Profiler.EndSample();
#endif
        }

        public static T AddInjector<T>(string propertyName, UIMaterialPropertyInjector host) where T : Injector
        {
            Profiler.BeginSample("(MPI)[Injector] AddInjector");
            var go = new GameObject($"Material.{propertyName}")
            {
                hideFlags = HideFlags.HideAndDontSave
            };
            var injector = go.AddComponent<T>();
            injector.m_PropertyName = propertyName;
            injector.id = Shader.PropertyToID(propertyName);
            injector.SetHost(host);
            Profiler.EndSample();

            return injector;
        }

        public void Destroy()
        {
            Misc.Destroy(gameObject);
        }
    }
}
