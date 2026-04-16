using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Text;
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
    public abstract class EditorWizardWindow : EditorWindow
    {
        [Obsolete("Do not use this method anymore, just utilize GetWindow instead.")]
        public static TEditorWindow Open<TEditorWindow>(UnityEngine.Object serializedObjectOverride = null) where TEditorWindow : EditorWizardWindow
        {
            TEditorWindow window = GetWindow<TEditorWindow>();
            return window;
        }
        /// <summary>
        /// The Header container for the Wizard, contains elements such as the Wizard's title.
        /// </summary>
        protected VisualElement headerContainer { get; private set; }

        [Obsolete("Utilize \"contentContainer\" instead"), EditorBrowsable(EditorBrowsableState.Never)]
        protected VisualElement wizardElementContainer => contentContainer;
        protected VisualElement contentContainer { get; private set; }

        /// <summary>
        /// The Footer container for the wizard, which contains a Progress bar and buttons to run or close the wizard.
        /// </summary>
        protected VisualElement footerContainer { get; private set; }

        protected SerializedObject serializedObject
        {
            get
            {
                if(_serializedObject == null)
                {
                    _serializedObject = new SerializedObject(this);
                    contentContainer.Unbind();
                    contentContainer.Bind(_serializedObject);
                }
                return _serializedObject;
            }
        }
        private SerializedObject _serializedObject;


        [Obsolete("Override \"GetHelpTooltip()\" instead."), EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual string wizardTitleTooltip => GetHelpTooltip();

        /// <summary>
        /// Overriding this allows you to enforce a Token Prefix for this wizard to run. the wizard will not run if there's no token present in <see cref="R2EKSettings.tokenPrefix"/> and this is set to true
        /// </summary>
        protected virtual bool requiresTokenPrefix => false;

        protected bool verboseLogging { get; set; } = false;

        protected string logFilePath { get; set; } = "";

        protected string nicifiedWizardName => ObjectNames.NicifyVariableName(wizardName);
        protected string wizardName
        {
            get
            {
                if(string.IsNullOrWhiteSpace(_wizardName))
                {
                    _wizardName = GetType().Name;
                }
                return _wizardName;
            }
        }
        private string _wizardName;

        private Button _runButton;
        private VisualElement _buttonContainer;
        private ProgressBar _progressBar;
        private VisualElement _loggingContainer;
        private Toggle _verboseLogging;
        private Button _logFilePathButton;
        private TextField _logFilePath;

        protected bool IsCoroutineRunning => _executingCoroutine != null;
        private EditorCoroutine _executingCoroutine;

        /// <summary>
        /// Used to validate the path of a potential UXML asset, overwrite this if youre making a window that isnt in the same assembly as R2EK.
        /// </summary>
        /// <param name="path">A potential UXML asset path</param>
        /// <returns>True if the path is for this editor window, false otherwise</returns>
        protected virtual bool ValidateUXMLPath(string path) => path.Contains(R2EKConstants.PACKAGE_NAME);


        [Obsolete("Overwrite Cleanup(string) instead"), EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual void Cleanup() => Cleanup("Run");
        protected virtual void Cleanup(string coroutineName) { }

        /// <summary>
        /// Implement your Wizard's actions in this Coroutine.
        /// 
        /// <br>See also <see cref="WizardCoroutineHelper"/> to run complex scenarios that may require multiple coroutines in succession.</br>
        /// </summary>
        protected abstract IEnumerator RunWizardCoroutine();

        protected void CreateGUI()
        {
            rootVisualElement.Clear();
            SetupDefaultElements();

            var visualTreeAsset = VisualElementTemplateDictionary.instance.LoadTemplate(GetType().Name, ValidateUXMLPath);
            if(!visualTreeAsset)
            {
                var imguiContainer = new IMGUIContainer(OnIMGUI);
                imguiContainer.name = $"{wizardName}_IMGUIContainer";
                contentContainer.Add(imguiContainer);
                OnIMGUIContainerAdded();
                return;
            }

            visualTreeAsset.CloneTree(contentContainer);
            SetupControls();
            contentContainer.Bind(serializedObject);
        }

        protected virtual void OnIMGUIContainerAdded() { }
        protected virtual void OnIMGUI() => EditorGUILayout.LabelField("No IMGUI Implemented...");
        protected virtual string GetHelpTooltip() => "";
        protected void BeginCoroutine(IEnumerator coroutine, string coroutineName)
        {
            if(_executingCoroutine != null)
            {
                RoR2EKLog.Warning($"Cannot start a wizard coroutine when a coroutine is already running!");
                return;
            }

            _buttonContainer.SetEnabled(false);
            contentContainer.SetEnabled(false);
            _loggingContainer.SetEnabled(false);
            _progressBar.SetDisplay(true);

            _executingCoroutine = EditorCoroutineUtility.StartCoroutine(InternalWizardCoroutine(coroutine, coroutineName), this);
        }

        /// <summary>
        /// This method updates the EditorWizard's progress bar, giving valuable feedback to the end user.
        /// </summary>
        /// <param name="zeroTo1Progress">The progress of how much the wizard has done its work, goes from 0 to 1, where 0 is 0% and 1 is 100%</param>
        /// <param name="title">The text to display on the progress bar</param>
        public void UpdateProgress(float zeroTo1Progress, string title = null)
        {
            if (_executingCoroutine == null)
            {
                RoR2EKLog.Warning($"Cannot update progress when wizard is not running.");
                return;
            }

            _progressBar.value = Mathf.Clamp01(zeroTo1Progress);
            _progressBar.title = title ?? _progressBar.title;
        }

        [Obsolete("Overwrite ValidateData(string) instead."), EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual bool ValidateData() => ValidateData("Run");
        protected virtual bool ValidateData(string coroutineName)
        {
            if (requiresTokenPrefix && coroutineName == "Run")
            {
                if (!R2EKSettings.instance.tokenExists)
                {
                    if (EditorUtility.DisplayDialog("No Token Prefix", "This wizard requires a Token Prefix to run properly, Token prefixes are used in LanguageToken generation to ensure unique tokens in the modding ecosystem. You can click the left button below to open the R2EKSettings window to set a Token prefix.", "Open Settings", "Close"))
                    {
                        SettingsService.OpenProjectSettings("Project/RoR2EditorKit");
                    }
                    return false;
                }
            }
            return true;
        }

        protected Button AddFooterButton(string buttonText, string buttonTooltip, Action onClick)
        {
            var button = new Button(onClick);
            button.name = buttonText;
            button.text = buttonText;
            button.tooltip = buttonTooltip;
            button.style.flexGrow = 1;

            _buttonContainer.Add(button);

            return button;
        }

        /// <summary>
        /// Method called when your Wizard's UXML Element is instantiated on the <see cref="wizardElementContainer"/>
        /// </summary>
        protected virtual void SetupControls() { }

        private void OnGUI() { }

        private void SetupDefaultElements()
        {
            VisualTreeAsset templateAsset = VisualElementTemplateDictionary.instance.LoadTemplate(nameof(EditorWizardWindow));
            templateAsset.CloneTree(rootVisualElement);

            headerContainer = rootVisualElement.Q<VisualElement>("HeaderContainer");
            contentContainer = rootVisualElement.Q<VisualElement>("ContentContainer");
            footerContainer = rootVisualElement.Q<VisualElement>("FooterContainer");
            _buttonContainer = rootVisualElement.Q<VisualElement>("ButtonContainer");
            _loggingContainer = rootVisualElement.Q<VisualElement>("LoggingContainer");

            string tooltip = GetHelpTooltip() ?? wizardTitleTooltip;
            if(!string.IsNullOrEmpty(tooltip))
            {
                VisualElement headerIcon = headerContainer.Q<VisualElement>("TooltipContainer");
                headerIcon.style.backgroundImage = (StyleBackground)EditorGUIUtility.IconContent("console.infoicon").image;
                headerIcon.tooltip = tooltip;
            }

            var label = headerContainer.Q<Label>();
            label.text = nicifiedWizardName;
            label.AddSimpleContextMenu(new ContextMenuData
            {
                menuName = "Open Script",
                userData = this,
                menuAction = (action) =>
                {
                    var monoscript = MonoScript.FromScriptableObject((ScriptableObject)action.userData);
                    AssetDatabase.OpenAsset(monoscript);
                }
            });

            _progressBar = footerContainer.Q<ProgressBar>();
            _progressBar.SetDisplay(false);

            _runButton = footerContainer.Q<Button>("Run");
            _runButton.clicked += RunButtonClicked;

            _verboseLogging = footerContainer.Q<Toggle>("VerboseLogging");
            _verboseLogging.RegisterValueChangedCallback(OnVerboseLoggingChanged);

            _logFilePath = footerContainer.Q<TextField>("LogOutput");
            _logFilePath.RegisterValueChangedCallback(OnLogOutputPathChange);

            _logFilePathButton = footerContainer.Q<Button>("LogOutputButton");
            _logFilePathButton.clicked += SelectLogOutputLocation;

            //TODO: Redo this
            label = new Label();
            label.name = "TokenPrefixNotice";
            label.text = "Note: A Token prefix is required for this wizard to run.";
            label.tooltip = "You can set the token prefix in the RoR2EditorKit settings window under your ProjectSettings.";

            var fontStyle = label.style.unityFontStyleAndWeight;
            fontStyle.value = FontStyle.Bold;
            label.style.unityFontStyleAndWeight = fontStyle;

            wizardElementContainer.Add(label);
            label.BringToFront();

            /*headerContainer = rootVisualElement.Q<VisualElement>("Header");
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
            _runButton.clicked += StartCoroutine;*/
        }

        private void OnVerboseLoggingChanged(ChangeEvent<bool> evt)
        {
            verboseLogging = evt.newValue;
        }

        private void OnLogOutputPathChange(ChangeEvent<string> evt)
        {
            logFilePath = evt.newValue;
        }

        protected struct LogOutputArgs
        {
            public string title;
            public string location;
            public string defaultFileName;
            public string logExtension;
        }
        protected virtual LogOutputArgs GetLogOutputArgs()
        {
            return new LogOutputArgs
            {
                title = "Save Log Output To",
                location = "Assets",
                defaultFileName = $"{wizardName}_Log",
                logExtension = "log"
            };
        }
        private void SelectLogOutputLocation()
        {
            LogOutputArgs logOutputArgs = GetLogOutputArgs();

            var path = EditorUtility.SaveFilePanel(logOutputArgs.title, logOutputArgs.location, logOutputArgs.defaultFileName, logOutputArgs.logExtension);
            _logFilePath.value = path;
        }

        private IEnumerator InternalWizardCoroutine(IEnumerator coroutine, string coroutineName = null)
        {
            while(true)
            {
                yield return null;
                try
                {
                    bool workLeft = coroutine.MoveNext();
                    if (!workLeft)
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    RoR2EKLog.Error($"Exception during {nicifiedWizardName}'s {coroutineName} coroutine: {ex}");
                    break;
                }
            }
            RoR2EKLog.Debug($"{nicifiedWizardName} Completed, cleaning up...");
            CleanupInternal(coroutineName);
        }

        private void RunButtonClicked()
        {
            if(!ValidateData("Run") || !ValidateData())
            {
                return;
            }

            if (_executingCoroutine != null)
                return;

            AssetDatabase.Refresh();

            BeginCoroutine(RunWizardCoroutine(), "Run");
        }

        private void CleanupInternal(string coroutineName)
        {
            EditorCoroutineUtility.StopCoroutine(_executingCoroutine);
            _executingCoroutine = null;

            DumpLog();
            Cleanup(coroutineName);

            _buttonContainer.SetEnabled(true);
            contentContainer.SetEnabled(true);
            _loggingContainer.SetEnabled(true);
            _progressBar.SetDisplay(false);
            _progressBar.title = "";
        }

        protected virtual void Awake() { }
        protected virtual void OnEnable() { }
        protected virtual void OnDestroy()
        {
            if(_executingCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(_executingCoroutine);
                _executingCoroutine = null;
            }
        }

        #region LoggingSystem
        private StringBuilder _logBuilder = new StringBuilder();

        public void Log(object data, bool onlyShowInVerbose = false)
        {
            if (!onlyShowInVerbose || verboseLogging)
            {
                _logBuilder.AppendLine(FormatDataForLogging(data, onlyShowInVerbose, MessageType.Info));
            }
        }

        protected void LogWarning(object data, bool onlyShowInVerbose = false)
        {
            if (!onlyShowInVerbose || verboseLogging)
            {
                _logBuilder.AppendLine(FormatDataForLogging(data, onlyShowInVerbose, MessageType.Warning));
            }
        }

        protected void LogError(object data, bool onlyShowInVerbose = false)
        {
            if (!onlyShowInVerbose || verboseLogging)
            {
                _logBuilder.AppendLine(FormatDataForLogging(data, onlyShowInVerbose, MessageType.Error));
            }
        }

        private string FormatDataForLogging(object data, bool isVerbose, MessageType messageType)
        {
            return string.Format("[{0}{1}-{2}]: {3}", wizardName, (isVerbose ? "VERBOSE" : string.Empty), messageType.ToString(), data);
        }

        private void DumpLog()
        {
            string logOutput = _logBuilder.ToString();
            _logBuilder.Clear();

            Debug.Log(logOutput);
            if (string.IsNullOrEmpty(logFilePath))
            {
                return;
            }

            using (var writer = File.CreateText(logFilePath))
            {
                writer.Write(logOutput);
            }

            var projectRelative = FileUtil.GetProjectRelativePath(logFilePath.Replace('\\', '/'));
            if (projectRelative.StartsWith("Assets"))
            {
                AssetDatabase.ImportAsset(projectRelative);
            }
        }
        #endregion
    }
}