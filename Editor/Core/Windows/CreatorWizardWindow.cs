using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Core.EditorWindows
{
    using static ThunderKit.Core.UIElements.TemplateHelpers;

    /// <summary>
    /// A variation of the ExtendedEditorWindow, a CreatorWizardWindow can be used for create complex assets and jobs that are executed asynchronously.
    /// </summary>
    public abstract class CreatorWizardWindow : ExtendedEditorWindow
    {
        /// <summary>
        /// The Header of the Window, contains the name of the wizard by default.
        /// </summary>
        protected VisualElement headerContainer;
        /// <summary>
        /// The middle container of the window, contains the wizard's specific fields.
        /// </summary>
        protected VisualElement wizardElementContainer;
        /// <summary>
        /// The footer of the Window, contains the buttons for executing the wizard.
        /// </summary>
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

        private async void RunWizardInternal()
        {
            if(warning != null)
            {
                footerContainer.Remove(warning);
            }
            if(await RunWizard())
            {
                Close();
                return;
            }

            warning = new IMGUIContainer(() => EditorGUILayout.HelpBox("Failed to run wizard, check console for errors.", MessageType.Error));
            footerContainer.Add(warning);
        }

        /// <summary>
        /// Implement your wizard's job and what it does here
        /// </summary>
        /// <returns>True if the wizard managed to run without issues, false if an issue has been encountered.</returns>
        protected abstract Task<bool> RunWizard();
    }
}