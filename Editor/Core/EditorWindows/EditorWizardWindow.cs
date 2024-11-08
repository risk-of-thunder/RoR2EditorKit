using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// The EditorWizardWindow is an <see cref="ExtendedEditorWindow"/> that's used to create complex wizard setups.
    /// <br>These wizards are powered by the <see cref="Unity.EditorCoroutines"/> system, allowing it to run code semi-asynchronously by waiting on execution.</br>
    /// <para>Unlike <see cref="ObjectEditingEditorWindow{TObject}"/>s, EditorWizardWindows only work with Visual Elements.</para>
    /// </summary>
    public abstract class EditorWizardWindow : ExtendedEditorWindow
    {
        /// <summary>
        /// The Header container for the Wizard, contains elements such as the Wizard's title.
        /// </summary>
        protected VisualElement headerContainer { get; private set; }

        /// <summary>
        /// The "Central" or in this case, the element that contains your wizard's control elements, such as fields, labels, etc
        /// </summary>
        protected VisualElement wizardElementContainer { get; private set; }

        /// <summary>
        /// The Footer container for the wizard, which contains a Progress bar and buttons to run or close the wizard.
        /// </summary>
        protected VisualElement footerContainer { get; private set; }

        /// <summary>
        /// Overriding this sets the tooltip of the Wizard's title, useful to display an explanation of what it does.
        /// </summary>
        protected virtual string wizardTitleTooltip { get; }

        /// <summary>
        /// Overriding this allows you to enforce a Token Prefix for this wizard to run. the wizard will not run if there's no token present in <see cref="R2EKSettings.tokenPrefix"/> and this is set to true
        /// </summary>
        protected virtual bool requiresTokenPrefix => false;

        private Button _runButton;
        private Button _closeButton;
        private ProgressBar _progressBar;
        private EditorCoroutine _coroutine;

        /// <summary>
        /// Used to validate the path of a potential UXML asset, overwrite this if youre making a window that isnt in the same assembly as R2EK.
        /// </summary>
        /// <param name="path">A potential UXML asset path</param>
        /// <returns>True if the path is for this editor window, false otherwise</returns>
        protected virtual bool ValidateUXMLPath(string path) => path.Contains(R2EKConstants.PACKAGE_NAME);

        /// <summary>
        /// Method that's always called when the wizard finishes execution. This should clean up the wizard and get rid of temporary files or memory.
        /// </summary>
        protected virtual void Cleanup() { }

        /// <summary>
        /// Implement your Wizard's actions in this Coroutine.
        /// 
        /// <br>See also <see cref="WizardCoroutineHelper"/> to run complex scenarios that may require multiple coroutines in succession.</br>
        /// </summary>
        protected abstract IEnumerator RunWizardCoroutine();

        protected sealed override void CreateGUI()
        {
            rootVisualElement.Clear();
            VisualElementTemplateDictionary.instance.GetTemplateInstance(typeof(EditorWizardWindow).Name, rootVisualElement);

            SetupDefaultElements();

            wizardElementContainer.Add(VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, null, ValidateUXMLPath));

            SetupControls();

            if (requiresTokenPrefix)
            {
                Label label = new Label();
                label.name = "TokenPrefixNotice";
                label.text = "Note: A Token prefix is required for this wizard to run.";
                label.tooltip = "You can set the token prefix in the RoR2EditorKit settings window under your ProjectSettings.";

                var fontStyle = label.style.unityFontStyleAndWeight;
                fontStyle.value = FontStyle.Bold;
                label.style.unityFontStyleAndWeight = fontStyle;

                wizardElementContainer.Add(label);
                label.BringToFront();
            }
            serializedObject = new SerializedObject(_sourceSerializedObject ?? this);
        }

        protected override void OnSerializedObjectChanged()
        {
            rootVisualElement.Unbind();
            rootVisualElement.Bind(serializedObject);
        }

        /// <summary>
        /// This method updates the EditorWizard's progress bar, giving valuable feedback to the end user.
        /// </summary>
        /// <param name="zeroTo1Progress">The progress of how much the wizard has done its work, goes from 0 to 1, where 0 is 0% and 1 is 100%</param>
        /// <param name="title">The text to display on the progress bar</param>
        public void UpdateProgress(float zeroTo1Progress, string title = null)
        {
            if (_coroutine == null)
            {
                Debug.Log($"Cannot update progress when wizard is not running.");
                return;
            }

            _progressBar.value = Mathf.Clamp01(zeroTo1Progress);
            _progressBar.title = title ?? _progressBar.title;
        }

        /// <summary>
        /// When the end user clicks the Run button, and before <see cref="RunWizardCoroutine"/> gets called, the EditorWizard calls this method to check if the Wizard is in a valid state to run.
        /// 
        /// <br>Overriding this method allows you to make sure the inputted data is in correct format to avoid exceptions during the wizard's coroutine.</br>
        /// </summary>
        /// <returns></returns>
        protected virtual bool ValidateData() => true;

        /// <summary>
        /// Method called when your Wizard's UXML Element is instantiated on the <see cref="wizardElementContainer"/>
        /// </summary>
        protected virtual void SetupControls() { }

        private void OnGUI() { }

        private void SetupDefaultElements()
        {
            headerContainer = rootVisualElement.Q<VisualElement>("Header");
            var label = headerContainer.Q<Label>();
            label.text = ObjectNames.NicifyVariableName(GetType().Name);
            label.tooltip = wizardTitleTooltip;

            wizardElementContainer = rootVisualElement.Q<VisualElement>("WizardElementContainer");

            footerContainer = rootVisualElement.Q<VisualElement>("Footer");

            _progressBar = footerContainer.Q<ProgressBar>();
            _progressBar.SetDisplay(false);

            _closeButton = footerContainer.Q<Button>("CloseWizardButton");
            _closeButton.clicked += CloseInternal;

            _runButton = footerContainer.Q<Button>("RunWizard");
            _runButton.clicked += StartCoroutine;
        }

        private void StartCoroutine()
        {
            if (requiresTokenPrefix)
            {
                if (!R2EKSettings.instance.tokenExists)
                {
                    if (EditorUtility.DisplayDialog("No Token Prefix", "This wizard requires a Token Prefix to run properly, Token prefixes are used in LanguageToken generation to ensure unique tokens in the modding ecosystem. You can click the left button below to open the R2EKSettings window to set a Token prefix.", "Open Settings", "Close"))
                    {
                        SettingsService.OpenProjectSettings("Project/RoR2EditorKit");
                    }
                    return;
                }
            }

            if (!ValidateData())
            {
                return;
            }

            if (_coroutine != null)
            {
                return;
            }

            AssetDatabase.Refresh();

            Debug.Log("Starting " + GetType().Name);
            _runButton.SetEnabled(false);
            _closeButton.SetEnabled(false);
            _progressBar.SetDisplay(true);
            _progressBar.title = "Running Wizard";

            _coroutine = this.StartCoroutine(InternalWizardCoroutine());
        }

        private IEnumerator InternalWizardCoroutine()
        {
            var wizardCoroutine = RunWizardCoroutine();
            try
            {
                while (wizardCoroutine.MoveNext())
                {
                    yield return wizardCoroutine.Current;
                }
            }
            finally
            {
                Debug.Log(GetType().Name + " Completed, cleaning up...");
                CleanupInternal();
            }
            yield break;
        }

        private void CleanupInternal()
        {
            Cleanup();
            _coroutine = null;
            _runButton.SetEnabled(true);
            _closeButton.SetEnabled(true);
            _progressBar.SetDisplay(false);
            _progressBar.title = "";
        }

        private void CloseInternal()
        {
            if (_coroutine != null)
            {
                this.StopCoroutine(_coroutine);
                CleanupInternal();
            }
            Close();
        }
    }
}