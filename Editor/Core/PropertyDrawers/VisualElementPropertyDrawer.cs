using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public abstract class VisualElementPropertyDrawer<T> : ExtendedPropertyDrawer<T>
    {
        protected VisualElement rootVisualElement
        {
            get
            {
                if(_rootVisualElement == null)
                {
                    _rootVisualElement = new VisualElement();
                    _rootVisualElement.name = $"{typeof(T).Name}_RootElement";
                }
                return _rootVisualElement;
            }
        }
        private VisualElement _rootVisualElement;

        public sealed override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }

        public sealed override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, rootVisualElement, ValidatePath);
            FinishUI();
            return rootVisualElement;
        }

        protected virtual bool ValidatePath(string path) => path.Contains(R2EKConstants.PACKAGE_NAME);

        protected abstract void FinishUI();
    }
}