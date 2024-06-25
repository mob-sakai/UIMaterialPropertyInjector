using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Coffee.UIExtensions
{
    internal static class InjectionPropertyListExtensions
    {
        public static bool TryGet(this List<InjectionProperty> self, int id, out InjectionProperty result)
        {
            for (var i = 0; i < self.Count; i++)
            {
                var p = self[i];
                if (p.id == id)
                {
                    result = p;
                    return true;
                }
            }

            result = default;
            return false;
        }

        public static bool TryGet(this List<InjectionProperty> self, string name, out InjectionProperty result)
        {
            for (var i = 0; i < self.Count; i++)
            {
                var p = self[i];
                if (p.propertyName == name)
                {
                    result = p;
                    return true;
                }
            }

            result = default;
            return false;
        }

        public static void Rebuild(this List<InjectionProperty> self, UIMaterialPropertyInjector host,
            bool allowAddInjector, bool allowInit)
        {
            Profiler.BeginSample("(MPI)[InjectorList] Rebuild");
            for (var i = 0; i < self.Count; i++)
            {
                var ip = self[i];
                ip.host = host;

                if (allowInit && ip.propertyType == PropertyType.Undefined)
                {
                    ip.Init(host.material);
                    if (ip.propertyType == PropertyType.Undefined) continue;
                }

                if (host.animatable)
                {
                    // Animatable: Create new Injector if needed and rebind it.
                    if (!ip.injector && allowAddInjector)
                    {
                        ip.injector = ip.AddInjector(host);
                    }
                }
                else
                {
                    // Not animatable: Injector is not needed.
                    ip.injector = null;
                }
            }

            Profiler.EndSample();
        }

        public static void ResetToDefault(this List<InjectionProperty> self, Material material)
        {
            for (var i = 0; i < self.Count; i++)
            {
                self[i].ResetToDefault(material);
            }
        }
    }
}
