using RoR2EditorKit.Utilities;
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
        protected VisualElement HeaderContainer { get; private set; }
        /// <summary>
        /// The middle container of the window, contains the wizard's specific fields.
        /// </summary>
        protected VisualElement WizardElementContainer { get; private set; }
        /// <summary>
        /// The footer of the Window, contains the buttons for executing the wizard.
        /// </summary>
        protected VisualElement FooterContainer { get; private set; }

        /// <summary>
        /// If supplied, the Wizard's header title will display this string as a tooltip.
        /// </summary>
        protected virtual string WizardTitleTooltip { get; }

        /// <summary>
        /// If set to true, a notice saying that a token prefix is required will be appended at the bottom of the WizardElementContainer
        /// </summary>
        protected virtual bool RequiresTokenPrefix => false;

        private IMGUIContainer warning;

        protected override void CreateGUI()
        {
            //Copies the base CreatorWizardWindowTemplate.
            rootVisualElement.Clear();
            GetTemplateInstance(typeof(CreatorWizardWindow).Name, rootVisualElement, ValidateUXMLPath);

            SetupDefaultElements();

            //Copies the inheriting class's template to wizardElement.
            GetTemplateInstance(GetType().Name, WizardElementContainer, ValidateUXMLPath);

            if (RequiresTokenPrefix)
            {
                Label label = new Label();
                label.name = "tokenPrefixNotice";
                label.text = "Note: A Token prefix is required for this wizard to run.";
                label.tooltip = "You can set the token prefix on the ror2editorkit settings window.";

                var fontStyle = label.style.unityFontStyleAndWeight;
                fontStyle.value = FontStyle.Bold;
                label.style.unityFontStyleAndWeight = fontStyle;

                WizardElementContainer.Add(label);
                label.BringToFront();
            }
        }

        private void SetupDefaultElements()
        {
            HeaderContainer = rootVisualElement.Q<VisualElement>("header");
            var title = HeaderContainer.Q<Label>("wizardTitle");
            title.text = ObjectNames.NicifyVariableName(GetType().Name);
            title.tooltip = WizardTitleTooltip;

            WizardElementContainer = rootVisualElement.Q<VisualElement>("wizardElement");

            FooterContainer = rootVisualElement.Q<VisualElement>("footer");
            var buttons = FooterContainer.Q<VisualElement>("buttons");
            FooterContainer.Q<Button>("closeWizardButton").clicked += () => Close();
            FooterContainer.Q<Button>("runWizard").clicked += () => RunWizardInternal();
        }

        private async void RunWizardInternal()
        {
            bool shouldRun = true;
            if (warning != null)
            {
                FooterContainer.Remove(warning);
            }

            if (RequiresTokenPrefix)
            {
                if (Settings.TokenPrefix.IsNullOrEmptyOrWhitespace())
                {
                    Debug.LogError("Cannot run wizard as there is no Token Prefix specified.");
                    shouldRun = false;
                }
            }

            if (shouldRun && await RunWizard())
            {
                Cleanup();
                Close();
                return;
            }

            warning = new IMGUIContainer(() => EditorGUILayout.HelpBox("Failed to run wizard, check console for errors.", MessageType.Error));
            FooterContainer.Add(warning);
        }
        /// <summary>
        /// Implement your wizard's job and what it does here
        /// </summary>
        /// <returns>True if the wizard managed to run without issues, false if an issue has been encountered.</returns>
        protected abstract Task<bool> RunWizard();

        /// <summary>
        /// Implement your wizard's cleanup procecss here.
        /// </summary>
        protected virtual void Cleanup() { }
    }
}