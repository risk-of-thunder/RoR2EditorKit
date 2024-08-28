using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public abstract class VisualElementComponentInspector<T> : ComponentInspector<T> where T : Component
    {
        private VisualElement _templateInstance;
        public virtual bool canReuseInstance { get; }
        private bool _hasAlreadyInitialized = false;

        protected sealed override VisualElement CreateInspectorUI()
        {
            if (canReuseInstance && _templateInstance == null)
            {
                _templateInstance = VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, null, ValidatePath);
                if (!_hasAlreadyInitialized)
                {
                    _hasAlreadyInitialized = true;
                    InitializeVisualElement(_templateInstance);
                }
                return _templateInstance;
            }

            var visualElement = VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, null, ValidatePath);
            InitializeVisualElement(visualElement);
            return visualElement;
        }

        protected abstract void InitializeVisualElement(VisualElement templateInstanceRoot);

        protected virtual bool ValidatePath(string path) => path.Contains(R2EKConstants.PACKAGE_NAME);
    }
}