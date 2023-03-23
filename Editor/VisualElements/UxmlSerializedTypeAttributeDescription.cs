using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using HG;

namespace RoR2EditorKit.VisualElements
{
    public class UxmlSerializedTypeAttributeDescription : TypedUxmlAttributeDescription<SerializableSystemType>
    {
        public override string defaultValueAsString => base.defaultValueAsString;
        public Type baseType { get; set; }
        public UxmlSerializedTypeAttributeDescription()
        {
            type = "string";
            typeNamespace = "http://www.w3.org/2001/XMLSchema";
            defaultValue = default(SerializableSystemType);
        }
        public override SerializableSystemType GetValueFromBag(IUxmlAttributes bag, CreationContext cc)
        {
            return TryGetValueFromBag(bag, cc, out var typeref) ? typeref : default(SerializableSystemType);
        }

        public bool TryGetValueFromBag(IUxmlAttributes bag, CreationContext cc, out SerializableSystemType typeRef)
        {
            var value = GetValueFromBag(bag, cc, ConvertStringToSystemType, default(SerializableSystemType));
            if(value != default)
            {
                typeRef = value;
                return true;
            }
            typeRef = default;
            return false;
        }

        private SerializableSystemType ConvertStringToSystemType(string serializedValue, SerializableSystemType defaultValue)
        {
            Type type = Type.GetType(serializedValue);
            return type == null ? defaultValue : (SerializableSystemType)type;
        }
    }
}
