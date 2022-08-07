using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2EditorKit.Utilities;
using RoR2EditorKit.Core.EditorWindows;
using UnityEditor;
using RoR2EditorKit.Common;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public class ModCreatorWizard : CreatorWizardWindow
    {
        public string authorName;
        public string modName;

        [MenuItem(Constants.RoR2EditorKitScriptableRoot + "Wizards/Mod", priority = ThunderKit.Common.Constants.ThunderKitMenuPriority)]
        private static void OpenWindow()
        {
            OpenEditorWindow<ModCreatorWizard>();
        }

        protected override bool RunWizard()
        {
            if(authorName.IsNullOrEmptyOrWhitespace() || modName.IsNullOrEmptyOrWhitespace())
            {
                Debug.LogError("authorName or modName is null, empty or whitespace!");
                return false;
            }
            return true;
        }
    }
}