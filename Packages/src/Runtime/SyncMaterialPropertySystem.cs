#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Coffee.UIMaterialPropertyInjectorInternal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Coffee.UIExtensions
{
    internal class SyncMaterialPropertySystem : ScriptableSingleton<SyncMaterialPropertySystem>
    {
        public bool m_Foldout = true;
        public bool m_EnableAutomaticSync = false;
        public bool m_BaseToInjected = true;
        public bool m_InjectedToBase = true;
        public bool m_IgnoreInjectedProperties = true;

        private readonly HashSet<GenericMaterialPropertyInjector> _injectors =
            new HashSet<GenericMaterialPropertyInjector>();

        private readonly Dictionary<int, int> _dirtyCountMap = new Dictionary<int, int>();

        public static void Register(GenericMaterialPropertyInjector injector)
        {
            instance._injectors.Add(injector);
        }

        public static void Unregister(GenericMaterialPropertyInjector injector)
        {
            instance._injectors.Remove(injector);
        }

        [InitializeOnLoadMethod]
        private static void Init()
        {
            EditorSceneManager.sceneSaving += (_, __) =>
            {
                foreach (var injector in instance._injectors)
                {
                    if (!injector) continue;

                    injector.RestoreMaterial();
                }
            };

            UIExtraCallbacks.onBeforeCanvasRebuild += () =>
            {
                if (!instance.m_EnableAutomaticSync) return;

                var ignore = instance.m_IgnoreInjectedProperties;
                if (instance.m_InjectedToBase)
                {
                    foreach (var injector in instance._injectors)
                    {
                        CopyInjectedToBase(injector, ignore);
                    }
                }

                if (instance.m_BaseToInjected)
                {
                    foreach (var injector in instance._injectors)
                    {
                        CopyBaseToInjected(injector, ignore);
                    }
                }
            };
        }

        private static void CopyBaseToInjected(GenericMaterialPropertyInjector injector, bool ignore = true)
        {
            if (!injector) return;

            CopyProperty(injector.baseMaterial, injector.material,
                ignore ? x => injector.properties.All(y => y.propertyName != x) : (Predicate<string>)null);
        }

        private static void CopyInjectedToBase(GenericMaterialPropertyInjector injector, bool ignore = true)
        {
            if (!injector) return;

            CopyProperty(injector.material, injector.baseMaterial,
                ignore ? x => injector.properties.All(y => y.propertyName != x) : (Predicate<string>)null);
        }

        private static void CopyProperty(Material from, Material to, Predicate<string> valid)
        {
            if (!to || !HasChanged(from)) return;

            var shader = from.shader;
            var count = shader.GetPropertyCount();
            for (var i = 0; i < count; i++)
            {
                if ((shader.GetPropertyFlags(i) & ShaderPropertyFlags.PerRendererData) != 0) continue;

                var propertyName = shader.GetPropertyName(i);
                if (valid != null && !valid.Invoke(propertyName)) continue;

                CopyProperty(from, to, propertyName, shader.GetPropertyType(i));
            }

            FrameCache.Set(to, "hasChanged", true);
            instance._dirtyCountMap[from.GetHashCode()] = EditorUtility.GetDirtyCount(from);
            instance._dirtyCountMap[to.GetHashCode()] = EditorUtility.GetDirtyCount(to);
        }

        private static void CopyProperty(Material from, Material to, string propertyName, ShaderPropertyType type)
        {
            if (!from || !to) return;
            switch (type)
            {
                case ShaderPropertyType.Color:
                    to.SetColor(propertyName, from.GetColor(propertyName));
                    break;
                case ShaderPropertyType.Vector:
                    to.SetVector(propertyName, from.GetVector(propertyName));
                    break;
                case ShaderPropertyType.Float:
                case ShaderPropertyType.Range:
                    to.SetFloat(propertyName, from.GetFloat(propertyName));
                    break;
                case ShaderPropertyType.Texture:
                    to.SetTexture(propertyName, from.GetTexture(propertyName));
                    break;
            }
        }

        private static bool HasChanged(Material material)
        {
            if (!material) return false;
            if (FrameCache.TryGet(material, "hasChanged", out bool hasChanged)) return hasChanged;

            var id = material.GetHashCode();
            var dirtyCount = EditorUtility.GetDirtyCount(material);
            if (instance._dirtyCountMap.TryGetValue(id, out var prevDirtyCount))
            {
                hasChanged = dirtyCount != prevDirtyCount;
            }

            instance._dirtyCountMap[id] = dirtyCount;
            FrameCache.Set(material, "hasChanged", hasChanged);
            return hasChanged;
        }

        private static bool HasChangedWithoutCache(Material material)
        {
            if (!material) return false;

            var id = material.GetHashCode();
            var dirtyCount = EditorUtility.GetDirtyCount(material);
            if (instance._dirtyCountMap.TryGetValue(id, out var prevDirtyCount))
            {
                return dirtyCount != prevDirtyCount;
            }

            instance._dirtyCountMap[id] = dirtyCount;
            return false;
        }

        public static void UpdateMaterialDirtyCount(Material material)
        {
            if (!material) return;

            var id = material.GetHashCode();
            var dirtyCount = EditorUtility.GetDirtyCount(material);
            instance._dirtyCountMap[id] = dirtyCount;
            FrameCache.Set(material, "hasChanged", false);
        }

        public void OnInspectorGUI(GenericMaterialPropertyInjector injector)
        {
            if (!injector) return;

            m_Foldout = EditorGUILayout.Foldout(m_Foldout, "[Debug] Sync Material Properties",
                EditorStyles.foldoutHeader);
            if (!m_Foldout) return;

            EditorGUI.BeginChangeCheck();
            Toggle("Enable Automatic Sync", ref m_EnableAutomaticSync);
            if (EditorGUI.EndChangeCheck() && m_EnableAutomaticSync)
            {
                _dirtyCountMap.Clear();
            }

            EditorGUI.BeginDisabledGroup(!m_EnableAutomaticSync);
            EditorGUI.indentLevel++;
            {
                Toggle("Injected To Base", ref m_InjectedToBase);
                Toggle("Base To Injected", ref m_BaseToInjected);
                Toggle("Ignore Injected Properties", ref m_IgnoreInjectedProperties);
            }
            EditorGUI.indentLevel--;
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.LabelField("Manual Sync");
            EditorGUI.indentLevel++;
            {
                var baseMat = injector.baseMaterial;
                var injectedMat = injector.material;
                DrawManualSync("[Base]", baseMat, injectedMat, () => CopyInjectedToBase(injector));
                DrawManualSync("[Injected]", injectedMat, baseMat, () => CopyBaseToInjected(injector));
            }
            EditorGUI.indentLevel--;
        }

        private static void Toggle(string label, ref bool value)
        {
            value = EditorGUILayout.Toggle(label, value);
        }

        private static void DrawManualSync(string label, Material material, Material from, Action action)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(label, material, typeof(Material), false);
            EditorGUI.BeginDisabledGroup(!HasChangedWithoutCache(from));
            if (GUILayout.Button("Sync", GUILayout.MaxWidth(60)))
            {
                action.Invoke();
            }

            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
    }
}
#endif
