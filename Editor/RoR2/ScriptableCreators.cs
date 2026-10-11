using HG;
using RoR2.Skills;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using IOPath = System.IO.Path;

namespace RoR2.Editor
{
    [InitializeOnLoad]
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

        static ScriptableCreators()
        {
            RoR2ScriptableObjectAdvancedDropdown.AddPair("RoR2/SkillDef/Captain/Orbital", CreateOrbital);
            RoR2ScriptableObjectAdvancedDropdown.AddPair("RoR2/SkillDef/Captain/SupplyDrop", CreateSupplyDrop);
            RoR2ScriptableObjectAdvancedDropdown.AddPair("RoR2/SkillDef/Combo", CreateCombo);
            RoR2ScriptableObjectAdvancedDropdown.AddPair("RoR2/SkillDef/Conditional", CreateConditional);
            RoR2ScriptableObjectAdvancedDropdown.AddPair("RoR2/SkillDef/EngiMineDeployer", CreateEngiMineDeployer);
            RoR2ScriptableObjectAdvancedDropdown.AddPair("RoR2/SkillDef/Grounded", CreateGrounded);
            RoR2ScriptableObjectAdvancedDropdown.AddPair("RoR2/SkillDef/LunarReplacements/Detonator", CreateDetonator);
            RoR2ScriptableObjectAdvancedDropdown.AddPair("RoR2/SkillDef/LunarReplacements/Primary", CreatePrimary);
            RoR2ScriptableObjectAdvancedDropdown.AddPair("RoR2/SkillDef/LunarReplacements/Secondary", CreateSecondary);
            RoR2ScriptableObjectAdvancedDropdown.AddPair("RoR2/SkillDef/ToolbotWeapon", CreateToolbotWeapon);

            var typesWithCreateAssetMenuAttribute = TypeCache.GetTypesWithAttribute(typeof(CreateAssetMenuAttribute));
            foreach(var type in typesWithCreateAssetMenuAttribute)
            {
                if(!type.FullName.Contains("RoR2"))
                {
                    continue;
                }

                CreateAssetMenuAttribute attribute = type.GetCustomAttribute<CreateAssetMenuAttribute>();
                if(attribute != null)
                {
                    RoR2ScriptableObjectAdvancedDropdown.AddPair(attribute.menuName, $"Assets/Create/{attribute.menuName}");
                }
            }
        }

        [MenuItem("Assets/Create/RoR2/Open Scriptable Object Advanced Dropdown", priority = -1000)]
        private static void OpenScrobjDropdown()
        {
            PopupWindow.Show(new Rect(0, 0, 400, 200), new PopupContent());

        }

        private class PopupContent : PopupWindowContent
        {
            private RoR2ScriptableObjectAdvancedDropdown dropdown;
            private bool _showing;
            public override void OnGUI(Rect rect)
            {
                dropdown ??= new RoR2ScriptableObjectAdvancedDropdown(new AdvancedDropdownState());
                dropdown.onItemClicked = () => editorWindow.Close();

                rect.width = 600;
                rect.height = 400;
                rect.x += 10;
                rect.y = 0;

                typeof(AdvancedDropdown).GetProperty("maximumSize", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetSetMethod(true)
                    .Invoke(dropdown, new object[] { new Vector2(600, 400) });

                if(!_showing)
                {
                    dropdown.Show(rect);
                    _showing = true;
                }
            }

            public override Vector2 GetWindowSize()
            {
                return new Vector2(10, 10);
            }
        }

        private class RoR2ScriptableObjectAdvancedDropdown : AdvancedDropdown
        {
            private static Dictionary<string, Either<Action, string>> itemNameToMenuItemOrAction = new Dictionary<string, Either<Action, string>>();

            public Action onItemClicked;
            public static void AddPair(string itemName, string menuItem)
            {
                itemNameToMenuItemOrAction.TryAdd(itemName, menuItem);
            }
            public static void AddPair(string itemName, Action action)
            {
                itemNameToMenuItemOrAction.TryAdd(itemName, action);
            }

            public RoR2ScriptableObjectAdvancedDropdown(AdvancedDropdownState state) : base(state)
            {
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                Item root = new Item("Create Scriptable Object", "");

                root.AddChild(new Item("Cancel Operation", "CANCEL", true));

                foreach(var (itemName, actionOrMenuItemName) in itemNameToMenuItemOrAction)
                {
                    if(actionOrMenuItemName.isA)
                    {
                        root.AddChild(new Item(itemName, actionOrMenuItemName.a));
                    }
                    else
                    {
                        root.AddChild(new Item(itemName, actionOrMenuItemName.b));
                    }
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                if(item is Item i)
                {
                    i.InvokeMenuItem();
                    onItemClicked?.Invoke();
                }
            }

            public class Item : AdvancedDropdownItem
            {
                private string _menuItem;
                private Action _action;
                private bool _isCancel;
                public Item(string name, string menuItem, bool isCancel = false) : base(name)
                {
                    _menuItem = menuItem;
                }

                public Item(string name, Action menuItem, bool isCancel = false) : base(name)
                {
                    _action = menuItem;
                }

                public void InvokeMenuItem()
                {
                    if (_isCancel)
                        return;

                    if(_action != null)
                    {
                        _action.Invoke();
                    }
                    else if(!string.IsNullOrWhiteSpace(_menuItem))
                    {
                        EditorApplication.ExecuteMenuItem(_menuItem);
                    }
                }
            }
        }
    }
}