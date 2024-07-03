using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Coffee.UIExtensions
{
    public class ShaderProperty
    {
        public string name { get; private set; }
        public string description { get; private set; }
        public PropertyType type { get; private set; }
        public Vector2 range { get; private set; }
        public ShaderPropertyFlags flags { get; private set; }
        public string[] attributes { get; private set; }

        public ShaderProperty(string name, PropertyType type)
        {
            this.name = name;
            this.type = type;
            description = string.Empty;
            attributes = Array.Empty<string>();
        }

        public ShaderProperty(Shader shader, int index)
        {
            name = shader.GetPropertyName(index);
            type = (PropertyType)shader.GetPropertyType(index);
            attributes = shader.GetPropertyAttributes(index);
            flags = shader.GetPropertyFlags(index);
            description = shader.GetPropertyDescription(index);
            range = type == PropertyType.Range
                ? shader.GetPropertyRangeLimits(index)
                : Vector2.zero;
        }
    }

    public static class ShaderExtensions
    {
        public static IEnumerable<ShaderProperty> GetAllProperties(this Shader self)
        {
            if (!self) yield break;

            var count = self.GetPropertyCount();
            for (var i = 0; i < count; i++)
            {
                var p = new ShaderProperty(self, i);
                yield return p;

                if (p.type == PropertyType.Texture)
                {
                    yield return new ShaderProperty($"{p.name}_ST", PropertyType.Vector);
                    yield return new ShaderProperty($"{p.name}_HDR", PropertyType.Vector);
                    yield return new ShaderProperty($"{p.name}_TexelSize", PropertyType.Vector);
                }
            }
        }
    }
}
