using System.Collections.Generic;
using ThunderKit.Core.Data;
using ThunderKit.Core.Manifests;
using ThunderKit.Markdown;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Data
{
    /// <summary>
    /// The main settings file of RoR2EditorKit
    /// </summary>
    public sealed class RoR2EditorKitSettings : ThunderKitSetting
    {
        private SerializedObject ror2EditorKitSettingsSO;

        /// <summary>
        /// The tokenPrefix thats used for this project
        /// </summary>
        public string tokenPrefix;

        /// <summary>
        /// The main manifest of this project
        /// </summary>
        public Manifest mainManifest;

        /// <summary>
        /// Used to check if the assets of ror2EK have been made ineditable
        /// </summary>
        public bool madeRoR2EKAssetsNonEditable = false;

        /// <summary>
        /// Direct access to RoR2EditorKit's EditorSettings
        /// </summary>
        public R2EditorSettings EditorSettings => GetOrCreateSettings<R2EditorSettings>();

        /// <summary>
        /// Direct access to RoR2EditorKit's MaterialEditorSettings
        /// </summary>
        public MaterialEditorSettings MaterialEditorSettings => GetOrCreateSettings<MaterialEditorSettings>();

        public override void Initialize() => tokenPrefix = "";

        public override void CreateSettingsUI(VisualElement rootElement)
        {
            MarkdownElement markdown = null;
            if (string.IsNullOrEmpty(this.tokenPrefix))
            {
                markdown = new MarkdownElement
                {
                    Data = $@"**__Warning:__** No Token Prefix assigned. Assign a token prefix before continuing.",

                    MarkdownDataType = MarkdownDataType.Text
                };
                markdown.RefreshContent();
                rootElement.Add(markdown);
            }

            var tokenPrefix = CreateStandardField(nameof(RoR2EditorKitSettings.tokenPrefix));
            tokenPrefix.tooltip = $"RoR2EK has tools to automatically set the tokens for certain objects." +
                $"\nThis token is also used when fixing the naming conventions set in place by certain editors." +
                $"\nToken should not have Underscores (_), can be lower or uppercase.";
            rootElement.Add(tokenPrefix);

            var mainManifest = CreateStandardField(nameof(RoR2EditorKitSettings.mainManifest));
            mainManifest.tooltip = $"The main manifest of this unity project, used for certain windows and utilities";
            rootElement.Add(mainManifest);

            if (ror2EditorKitSettingsSO == null)
                ror2EditorKitSettingsSO = new SerializedObject(this);

            rootElement.Bind(ror2EditorKitSettingsSO);
        }

        /// <summary>
        /// Returns the token prefix with all Chars uppercase.
        /// <para>Example: "myToken" => "MYTOKEN"</para>
        /// </summary>
        /// <returns>The token prefix on all uppercase</returns>
        public string GetPrefixUppercase()
        {
            if (tokenPrefix.IsNullOrEmptyOrWhitespace())
            {
                throw ErrorShorthands.NullTokenPrefix();
            }
            return tokenPrefix.ToUpperInvariant();
        }
        /// <summary>
        /// Returns the token prefix with all Chars lowercase
        /// <para>Example: "MyToken" => "mytoken"</para>
        /// </summary>
        /// <returns></returns>
        public string GetPrefixLowercase()
        {
            if (tokenPrefix.IsNullOrEmptyOrWhitespace())
            {
                throw ErrorShorthands.NullTokenPrefix();
            }
            return tokenPrefix.ToLowerInvariant();
        }

        /// <summary>
        /// Returns the token prefix with the first char Uppercase and the rest lowerCase
        /// <para>Example: "MyToken" => "Mytoken"</para>
        /// </summary>
        /// <returns></returns>
        public string GetPrefix1stUpperRestLower()
        {
            List<char> prefix = new List<char>();
            for (int i = 0; i < tokenPrefix.Length; i++)
            {
                char letter = tokenPrefix[i];
                if (i == 0)
                {
                    prefix.Add(char.ToUpperInvariant(letter));
                }
                else
                {
                    prefix.Add(char.ToLowerInvariant(letter));
                }
            }
            return new string(prefix.ToArray());
        }
    }
}
