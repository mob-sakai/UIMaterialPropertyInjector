using System;
using System.Reflection;
using UnityEngine;

namespace Coffee.UIExtensions
{
    /// <summary>
    /// Reflection-based accessor that gets/sets a <see cref="Material"/> from another component.
    /// </summary>
    [Serializable]
    public class MaterialAccessor
    {
        private const BindingFlags k_Flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                             BindingFlags.FlattenHierarchy;

        [SerializeField] private string m_Target = "";
        [SerializeField] private string m_Getter = "";
        [SerializeField] private string m_Setter = "";

        private Component _target;
        private Func<Material> _getter;
        private Action<Material> _setter;

        /// <summary>
        /// Gets the current material from the configured target component.
        /// </summary>
        /// <returns>
        /// The material returned by the target getter, or null when the accessor is not valid.
        /// </returns>
        public Material Get()
        {
            if (!IsValid()) return null;
            return _getter.Invoke();
        }

        /// <summary>
        /// Sets a material to the configured target component.
        /// </summary>
        /// <param name="material">
        /// Material instance to apply.
        /// </param>
        public void Set(Material material)
        {
            if (!IsValid()) return;
            _setter.Invoke(material);
        }

        /// <summary>
        /// Invalidates cached delegates so they are rebuilt on next initialize.
        /// </summary>
        public void SetDirty()
        {
            _getter = null;
            _setter = null;
        }

        /// <summary>
        /// Initializes accessor bindings if needed.
        /// </summary>
        /// <param name="parent">
        /// GameObject that contains the target component.
        /// </param>
        /// <returns>
        /// True when accessor bindings are valid after this call.
        /// </returns>
        public bool InitializeIfNeeded(GameObject parent)
        {
            if (IsValid()) return true;

            _getter = null;
            _setter = null;
            if (string.IsNullOrEmpty(m_Target) || string.IsNullOrEmpty(m_Getter) || string.IsNullOrEmpty(m_Setter))
            {
                return false;
            }

            var type = Type.GetType(m_Target);
            if (type == null || !type.IsSubclassOf(typeof(Component)))
            {
                Debug.LogError($"Type '{m_Target}' is not a subclass of Component.");
                return false;
            }

            if (!parent.TryGetComponent(type, out _target))
            {
                Debug.LogError($"Target '{m_Target}' is not found in '{parent.name}'");
                return false;
            }

            try
            {
                // Bind method by name once and cache delegate to avoid per-frame reflection.
                var method = _target.GetType().GetMethod(m_Getter, k_Flags);
                _getter = Delegate.CreateDelegate(typeof(Func<Material>), _target, method) as Func<Material>;
            }
            catch (Exception)
            {
                Debug.LogException(new Exception($"Getter<Material> '{m_Getter}' is not found in '{_target}'"));
                return false;
            }

            try
            {
                // Setter delegate is validated symmetrically with getter for consistency.
                var method = _target.GetType().GetMethod(m_Setter, k_Flags);
                _setter = Delegate.CreateDelegate(typeof(Action<Material>), _target, method) as Action<Material>;
            }
            catch (Exception)
            {
                Debug.LogException(new Exception($"Setter<Material> '{m_Setter}' is not found in '{_target}'"));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Returns whether the accessor currently points to a valid target and delegates.
        /// </summary>
        public bool IsValid()
        {
            return _target && (Component)_getter?.Target == _target && (Component)_setter?.Target == _target;
        }
    }
}
