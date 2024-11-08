using RoR2.Skills;
using System;
using UnityEditor;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using IOPath = System.IO.Path;

namespace RoR2.Editor
{
    internal static class ScriptableCreators
    {
        #region skilldefs
        [MenuItem("Assets/Create/RoR2/SkillDef/Captain/Orbital")]
        private static void CreateOrbital()
        {
            CreateNewScriptableObject<CaptainOrbitalSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Captain/SupplyDrop")]
        private static void CreateSupplyDrop()
        {
            CreateNewScriptableObject<CaptainSupplyDropSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Combo")]
        private static void CreateCombo()
        {
            CreateNewScriptableObject<ComboSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Conditional")]
        private static void CreateConditional()
        {
            CreateNewScriptableObject<ConditionalSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/EngiMineDeployer")]
        private static void CreateEngiMineDeployer()
        {
            CreateNewScriptableObject<EngiMineDeployerSkill>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Grounded")]
        private static void CreateGrounded()
        {
            CreateNewScriptableObject<GroundedSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/LunarReplacements/Detonator")]
        private static void CreateDetonator()
        {
            CreateNewScriptableObject<LunarDetonatorSkill>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/LunarReplacements/Primary")]
        private static void CreatePrimary()
        {
            CreateNewScriptableObject<LunarPrimaryReplacementSkill>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/LunarReplacements/Secondary")]
        private static void CreateSecondary()
        {
            CreateNewScriptableObject<LunarSecondaryReplacementSkill>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/Stepped")]
        private static void CreateStepped()
        {
            CreateNewScriptableObject<SteppedSkillDef>();
        }

        [MenuItem("Assets/Create/RoR2/SkillDef/ToolbotWeapon")]
        private static void CreateToolbotWeapon()
        {
            CreateNewScriptableObject<ToolbotWeaponSkillDef>();
        }

        readonly static object[] findTextureParams = new object[1];

        private static void CreateNewScriptableObject<T>() where T : UnityEngine.ScriptableObject
        {
            T asset = ScriptableObject.CreateInstance<T>();

            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (path == "")
            {
                path = "Assets";
            }
            else if (IOPath.GetExtension(path) != "")
            {
                path = path.Replace(IOPath.GetFileName(AssetDatabase.GetAssetPath(Selection.activeObject)), "");
            }
            var name = typeof(T).Name;
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"{path}/{name}.asset");
            Action<int, string, string> action =
                (int instanceId, string pathname, string resourceFile) =>
                {
                    AssetDatabase.CreateAsset(asset, pathname);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                    Selection.activeObject = asset;
                };

            var endAction = ScriptableObject.CreateInstance<SelfDestructingActionAsset>();
            endAction.action = action;
            var findTexture = typeof(EditorGUIUtility).GetMethod(nameof(EditorGUIUtility.FindTexture), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            findTextureParams[0] = typeof(T);
            var icon = (Texture2D)findTexture.Invoke(null, findTextureParams);
            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(asset.GetInstanceID(), endAction, assetPathAndName, icon, null);
        }
        #endregion

        public class SelfDestructingActionAsset : EndNameEditAction
        {
            public Action<int, string, string> action;

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                action(instanceId, pathName, resourceFile);
                CleanUp();
            }
        }
    }
}