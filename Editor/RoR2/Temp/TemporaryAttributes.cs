using System;
using UnityEngine;

namespace RoR2.Editor
{
    [Obsolete("This purely exists until i update r2api.addressables to include this attribute there.")]
    public class NoCatalogLoadAttribute : Attribute
    {

    }

    public class AddressableComponentRequirement : Attribute
    {
        public Type requiredComponentType { get; set; }
        public bool searchInChildren { get; set; }

        public AddressableComponentRequirement(Type requiredComponent)
        {
            this.requiredComponentType = requiredComponent;
        }
    }
}