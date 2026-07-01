using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Coffee.UIExtensions
{
    public class ShaderPropertyDropdown : AdvancedDropdown
    {
        private class ShaderPropertyItem : AdvancedDropdownItem
        {
            public ShaderProperty property;

            public ShaderPropertyItem(ShaderProperty property) : base(
                property.type == PropertyType.Undefined
                    ? property.name
                    : $"{property.name} ({property.type})")
            {
                this.property = property;
            }
        }

        private static readonly Regex s_RegexIgnored = new Regex(@"^(Material)?(Toggle|KeywordEnum)(Drawer)?\s*\(");

        private IEnumerable<ShaderProperty> _properties;
        private Action<ShaderProperty> _onSelected;

        public ShaderPropertyDropdown() : base(new AdvancedDropdownState())
        {
            minimumSize = new Vector2(270f, 308f);
        }

        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem("Select Shader Property");
            if (_properties == null) return root;

            foreach (var property in _properties)
            {
                if (string.IsNullOrEmpty(property.name) && property.type == PropertyType.Undefined)
                {
                    root.AddSeparator();
                }
                else
                {
                    root.AddChild(new ShaderPropertyItem(property));
                }
            }

            return root;
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            if (item is ShaderPropertyItem shaderItem)
            {
                _onSelected?.Invoke(shaderItem.property);
            }
        }

        public void SetProperties(Shader shader, IEnumerable<ShaderProperty> properties)
        {
            _properties = properties
                .Where(p => !shader.GetPropertyAttributes(p.name)
                    .Any(attr => s_RegexIgnored.IsMatch(attr))); // Ignore attributes for keyword
        }

        public void SetCallback(Action<ShaderProperty> onSelected)
        {
            _onSelected = onSelected;
        }
    }
}
