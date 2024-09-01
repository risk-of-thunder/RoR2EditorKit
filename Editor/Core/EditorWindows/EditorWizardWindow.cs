using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEngine;

namespace RoR2.Editor
{
    public abstract class EditorWizardWindow : ExtendedEditorWindow
    {
        protected VisualElement headerContainer { get; private set; }
        protected VisualElement wizardElementContainer { get; private set; }
        protected VisualElement footerContainer { get; private set; }
        protected virtual string wizardTitleTooltip { get; }
        protected virtual bool requiresTokenPrefix => false;

        private Button _runButton;
        private Button _closeButton;
        private ProgressBar _progressBar;
        private EditorCoroutine _coroutine;

        protected virtual bool ValidateUXMLPath(string path) => path.Contains(R2EKConstants.PACKAGE_NAME);

        protected virtual void Cleanup() { }

        protected abstract IEnumerator RunWizardCoroutine();

        protected sealed override void CreateGUI()
        {
            rootVisualElement.Clear();
            VisualElementTemplateDictionary.instance.GetTemplateInstance(typeof(EditorWizardWindow).Name, rootVisualElement);

            SetupDefaultElements();

            wizardElementContainer.Add(VisualElementTemplateDictionary.instance.GetTemplateInstance(GetType().Name, null, ValidateUXMLPath));

            if(requiresTokenPrefix)
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
        }

        protected override void OnSerializedObjectChanged()
        {
            rootVisualElement.Unbind();
            rootVisualElement.Bind(serializedObject);
        }

        protected void UpdateProgress(float zeroTo1Progress, string title = null)
        {
            if(_coroutine == null)
            {
                Debug.Log($"Cannot update progress when wizard is not running.");
            }

            _progressBar.value = Mathf.Clamp01(zeroTo1Progress);
            _progressBar.title = title ?? _progressBar.title;
        }

        protected virtual bool ValidateData() => true;

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
            if(requiresTokenPrefix)
            {
                if(!R2EKSettings.instance.tokenExists)
                {
                    if(EditorUtility.DisplayDialog("No Token Prefix", "This wizard requires a Token Prefix to run properly, Token prefixes are used in LanguageToken generation to ensure unique tokens in the modding ecosystem. You can click the left button below to open the R2EKSettings window to set a Token prefix.", "Open Settings", "Close"))
                    {
                        SettingsService.OpenProjectSettings("Project/RoR2EditorKit");
                    }
                    return;
                }
            }

            if(!ValidateData())
            {
                return;
            }

            if(_coroutine != null)
            {
                return;
            }
            _runButton.SetEnabled(false);
            _closeButton.SetEnabled(false);
            _progressBar.SetDisplay(true);
            _progressBar.title = "Running Wizard";

            _coroutine = this.StartCoroutine(InternalWizardCoroutine());
        }

        private IEnumerator InternalWizardCoroutine()
        {
            var wizardCoroutine = RunWizardCoroutine();
            while(wizardCoroutine.MoveNext())
            {
                yield return wizardCoroutine.Current;
            }

            CleanupInternal();
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
            if(_coroutine != null)
            {
                this.StopCoroutine(_coroutine);
                CleanupInternal();
            }
            Close();
        }
    }
}