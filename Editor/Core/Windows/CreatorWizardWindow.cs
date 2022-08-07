using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Core.EditorWindows
{
    using static ThunderKit.Core.UIElements.TemplateHelpers;
    public abstract class CreatorWizardWindow : ExtendedEditorWindow
    {
        protected VisualElement headerContainer;
        protected VisualElement wizardElementContainer;
        protected VisualElement footerContainer;

        private IMGUIContainer warning;

        protected override void CreateGUI()
        {
            //Copies the base CreatorWizardWindowTemplate.
            rootVisualElement.Clear();
            GetTemplateInstance(typeof(CreatorWizardWindow).Name, rootVisualElement, ValidateUXMLPath);

            SetupDefaultElements();

            //Copies the inheriting class's template to wizardElement.
            GetTemplateInstance(GetType().Name, wizardElementContainer, ValidateUXMLPath);
        }

        private void SetupDefaultElements()
        {
            headerContainer = rootVisualElement.Q<VisualElement>("header");
            var title = headerContainer.Q<Label>("wizardTitle");
            title.text = ObjectNames.NicifyVariableName(GetType().Name);

            wizardElementContainer = rootVisualElement.Q<VisualElement>("wizardElement");

            footerContainer = rootVisualElement.Q<VisualElement>("footer");
            var buttons = footerContainer.Q<VisualElement>("buttons");
            footerContainer.Q<Button>("closeWizardButton").clicked += () => Close();
            footerContainer.Q<Button>("runWizard").clicked += () => RunWizardInternal();
        }

        private void RunWizardInternal()
        {
            if(warning != null)
            {
                footerContainer.Remove(warning);
            }
            if(RunWizard())
            {
                Close();
                return;
            }

            warning = new IMGUIContainer(() => EditorGUILayout.HelpBox("Failed to run wizard, check console for errors.", MessageType.Error));
            footerContainer.Add(warning);
        }

        protected abstract bool RunWizard();
    }
}