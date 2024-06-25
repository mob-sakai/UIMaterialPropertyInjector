using System.Collections.Generic;
using System.Linq;
using Coffee.Development;
using UnityEngine;

namespace Coffee.UIExtensions
{
    public class UIMaterialPropertyInjectorAllocTest : AllocTest
    {
        [SerializeField]
        protected List<Animator> m_Animators;

        [SerializeField]
        protected List<UIMaterialPropertyTweener> m_tweeners;

        private void Reset()
        {
            m_Targets = FindObjectsOfType<UIMaterialPropertyInjector>()
                .OfType<MonoBehaviour>()
                .ToList();

            m_Animators = FindObjectsOfType<Animator>()
                .ToList();

            m_tweeners =
                FindObjectsOfType<UIMaterialPropertyTweener>()
                    .ToList();
        }

        protected override void OnExecute(MonoBehaviour target)
        {
            base.OnExecute(target);

            var injector = target as UIMaterialPropertyInjector;
            if (!injector) return;

            injector.SetDirty();
        }

        public void EnableAnimators(bool enabled)
        {
            foreach (var animator in m_Animators)
            {
                animator.enabled = enabled;
            }
        }

        public void EnableTweeners(bool enabled)
        {
            foreach (var tweener in m_tweeners)
            {
                tweener.enabled = enabled;
            }
        }
    }
}
