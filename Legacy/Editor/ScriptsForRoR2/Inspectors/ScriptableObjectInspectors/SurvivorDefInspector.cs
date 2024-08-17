using RoR2;
using RoR2EditorKit.Inspectors;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using RoR2EditorKit.VisualElements;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(SurvivorDef))]
    public sealed class SurvivorDefInspector : ScriptableObjectInspector<SurvivorDef>
    {
        private VisualElement inspectorDataHolder;
        private Foldout tokenFoldout;

        private ValidatingPropertyField bodyPrefabValidator;
        private ValidatingPropertyField displayPrefabValidator;

        public string Prefix => TargetType.bodyPrefab.name.Replace("Body", string.Empty);
        public bool UsesTokenForPrefix => false;

        protected override void OnEnable()
        {
            base.OnEnable();
            OnVisualTreeCopy += () =>
            {
                var container = DrawInspectorElement.Q<VisualElement>("Container");
                inspectorDataHolder = container.Q<VisualElement>("InspectorDataContainer");
                tokenFoldout = inspectorDataHolder.Q<Foldout>("TokenContainer");

                bodyPrefabValidator = inspectorDataHolder.Q<ValidatingPropertyField>("bodyPrefab");
                displayPrefabValidator = inspectorDataHolder.Q<ValidatingPropertyField>("displayPrefab");
            };
        }

        protected override void DrawInspectorGUI()
        {
            SetupBodyValidator();
            bodyPrefabValidator.ForceValidation();

            SetupDisplayValidator();
            displayPrefabValidator.ForceValidation();

            var primaryColor = inspectorDataHolder.Q<PropertyField>("primaryColor");
            primaryColor.AddSimpleContextMenu(new ContextMenuData
            {
                actionStatusCheck = (dma) => TargetType.bodyPrefab && TargetType.bodyPrefab.GetComponent<CharacterBody>() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled,
                menuAction = SetPrimaryColorToBodyColor,
                menuName = "Set Primary Color to Body Color"
            });
            primaryColor.AddSimpleContextMenu(new ContextMenuData
            {
                actionStatusCheck = (dma) => TargetType.bodyPrefab && TargetType.bodyPrefab.GetComponent<CharacterBody>() ? DropdownMenuAction.Status.Normal : DropdownMenuAction.Status.Disabled,
                menuAction = SetBodyColorToPrimaryColor,
                menuName = "Set Body Color to Primary Color"
            });

            tokenFoldout.AddSimpleContextMenu(new ContextMenuData
            {
                actionStatusCheck = dma =>
                {
                    var tokenPrefix = Settings.tokenPrefix;
                    if (tokenPrefix.IsNullOrEmptyOrWhitespace())
                        return DropdownMenuAction.Status.Disabled;
                    return DropdownMenuAction.Status.Normal;
                },
                menuAction = SetTokens,
                menuName = "Set Tokens",
            });
        }

        private void SetupBodyValidator()
        {
            bodyPrefabValidator.AddValidator(() =>
            {
                GameObject prefab = GetBodyPrefab();
                return !prefab;
            }, "No BodyPrefab assigned!");

            bodyPrefabValidator.AddValidator(() =>
            {
                GameObject prefab = GetBodyPrefab();
                return prefab && !prefab.GetComponent<CharacterBody>();
            }, "The selected prefab in Body Prefab is not a CharacterBody prefab!");

            GameObject GetBodyPrefab() => bodyPrefabValidator.ChangeEvent == null ? TargetType.bodyPrefab : (GameObject)bodyPrefabValidator.ChangeEvent.newValue;
        }

        private void SetupDisplayValidator()
        {
            displayPrefabValidator.AddValidator(() =>
            {
                GameObject prefab = GetDisplayPrefab();
                return !prefab;
            }, "No Display Prefab assigned!");

            displayPrefabValidator.AddValidator(() =>
            {
                GameObject prefab = GetDisplayPrefab();
                return prefab && !prefab.GetComponent<CharacterModel>();
            }, "The selected prefab in Display Prefab is not a CharacterModel prefab!");

            GameObject GetDisplayPrefab() => displayPrefabValidator.ChangeEvent == null ? TargetType.displayPrefab : (GameObject)bodyPrefabValidator.ChangeEvent.newValue;
        }

        private void SetPrimaryColorToBodyColor(DropdownMenuAction dma) => TargetType.primaryColor = TargetType.bodyPrefab.GetComponent<CharacterBody>().bodyColor;
        private void SetBodyColorToPrimaryColor(DropdownMenuAction dma) => TargetType.bodyPrefab.GetComponent<CharacterBody>().bodyColor = TargetType.primaryColor;

        private void SetTokens(DropdownMenuAction dma)
        {
            string tokenBase = $"{Settings.GetPrefixUppercase()}_{TargetType.cachedName.ToUpperInvariant()}_";
            TargetType.displayNameToken = tokenBase + "BODY_NAME";
            TargetType.descriptionToken = tokenBase + "DESCRIPTION";
            TargetType.outroFlavorToken = tokenBase + "OUTRO_FLAVOR";
            TargetType.mainEndingEscapeFailureFlavorToken = tokenBase + "MAIN_ENDING_ESCAPE_FAILURE_FLAVOR";
            serializedObject.Update();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
