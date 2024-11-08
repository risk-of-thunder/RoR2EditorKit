using HG;
using RoR2.Skills;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using IOPath = System.IO.Path;

namespace RoR2.Editor.Windows
{
    public class CharacterBodyWizard : EditorWizardWindow
    {
        [FilePickerPath(FilePickerPath.PickerType.OpenFolder, title = "Folder for created Assets")]
        public string folderPath;

        [Tooltip("The Template to use for the creation of this body.\n\n1. Standing - A Character that can stand and move around normally, based off CommandoBody.\n2. Flying - A Character that's not affected by gravity and flies, based off WispBody.\n3. Stationary - A Character that does not move and stays in place, based off the MinorConstructBody.\n4. Boss - A Character that works as a Boss, based off the TitanBody")]
        public TemplateChoice template;
        public string characterName;
        public GameObject modelFBX;
        public bool simpleHurtBox;
        public List<string> stateMachines = new List<string>
        {
            "Body",
            "Weapon"
        };
        public List<SkillSlot> genericSkills = new List<SkillSlot>()
        {
            SkillSlot.Primary,
            SkillSlot.Secondary,
            SkillSlot.Utility,
            SkillSlot.Special
        };
        [SerializableSystemType.RequiredBaseType(typeof(MonoBehaviour))]
        public List<SerializableSystemType> extraComponents;
        public bool ignoreExtraComponentDuplicates;

        protected override string wizardTitleTooltip =>
@"The CharacterBodyWizard is a Wizard that creates a fully working CharacterBody from a Template.

The resulting prefab contains the necesary components for it's specified type, a minimum of 3 EntityStateMachines and a minimum of 4 GenericSkills, alongside an instantiated model for the FBX. This wizard does not set up complex hurtBoxes or ragdoll controllers.";

        protected override bool requiresTokenPrefix => true;

        private Dictionary<TemplateChoice, GameObject> _choiceToTemplate = new Dictionary<TemplateChoice, GameObject>();

        private WizardCoroutineHelper _wizardCoroutineHelper;
        private (Func<IEnumerator> subroutine, string step)[] _steps;

        private string _bodyTokenFormat;
        private string _skillDefTokenFormat;
        private GameObject _copiedBody;
        private List<SkillFamily> _createdSkillFamilies = new List<SkillFamily>();
        private List<SkillDef> _createdSkillDefs = new List<SkillDef>();

        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Wizards/Character Body")]
        private static void Open() => Open<CharacterBodyWizard>(null);

        protected override void OnEnable()
        {
            base.OnEnable();
            _choiceToTemplate.Add(TemplateChoice.Standing, R2EKConstants.AssetGUIDs.standingTemplateBody);
            _choiceToTemplate.Add(TemplateChoice.Flying, R2EKConstants.AssetGUIDs.flyingTemplateBody);
            _choiceToTemplate.Add(TemplateChoice.Stationary, R2EKConstants.AssetGUIDs.stationaryTemplateBody);
            _choiceToTemplate.Add(TemplateChoice.Boss, R2EKConstants.AssetGUIDs.bossTemplateBody);

            _wizardCoroutineHelper = new WizardCoroutineHelper(this);
            _wizardCoroutineHelper.AddStep(CreateTokenFormat(), "Creating Token Format");
            _wizardCoroutineHelper.AddStep(InstantiateTemplateAndUnpack(), "Instantiating Template and Unpacking");
            _wizardCoroutineHelper.AddStep(SetNameAndTokens(), "Setting Name and Tokens");
            _wizardCoroutineHelper.AddStep(AddStateMachines(), "Adding State Machines");
            _wizardCoroutineHelper.AddStep(AddGenericSkills(), "Creating Skills");
            _wizardCoroutineHelper.AddStep(AddComponents(), "Adding Components");
            _wizardCoroutineHelper.AddStep(SetupModelGameObject(), "Setting up Model");
            _wizardCoroutineHelper.AddStep(CreateAssets(), "Creating Assets");
        }

        protected override bool ValidateData()
        {
            if (string.IsNullOrEmpty(characterName) || string.IsNullOrWhiteSpace(characterName))
            {
                Debug.LogWarning($"Cannot run wizard because the CharacterName is not Valid.");
                return false;
            }
            return true;
        }

        protected override IEnumerator RunWizardCoroutine()
        {
            while (_wizardCoroutineHelper.MoveNext())
            {
                yield return _wizardCoroutineHelper.Current;
            }
            yield break;
        }

        private IEnumerator CreateTokenFormat()
        {
            _bodyTokenFormat = $"{R2EKSettings.instance.GetTokenAllUpperCase()}_{characterName.ToUpperInvariant()}_BODY_{{0}}";

            _skillDefTokenFormat = $"{R2EKSettings.instance.GetTokenAllUpperCase()}_{characterName.ToUpperInvariant()}_{{0}}_{{1}}";
            yield break;
        }

        private IEnumerator InstantiateTemplateAndUnpack()
        {
            var templateObject = _choiceToTemplate[template];
            _copiedBody = Instantiate(templateObject, Vector3.zero, Quaternion.identity);
            yield break;
        }

        private IEnumerator SetNameAndTokens()
        {
            CharacterBody body = _copiedBody.GetComponent<CharacterBody>();
            body.baseNameToken = string.Format(_bodyTokenFormat, "NAME");
            body.subtitleNameToken = string.Format(_bodyTokenFormat, "SUBTITLE");
            _copiedBody.name = $"{characterName}Body";
            yield break;
        }

        private IEnumerator AddStateMachines()
        {
            var networker = _copiedBody.GetComponent<NetworkStateMachine>();
            SerializedObject networkerSerializedObject = null;
            if (networker)
            {
                networkerSerializedObject = new SerializedObject(networker);
            }

            var stateOnHurt = _copiedBody.GetComponent<SetStateOnHurt>();
            var deathBehaviour = _copiedBody.GetComponent<CharacterDeathBehavior>();

            for (int i = 0; i < stateMachines.Count; i++)
            {
                var stateMachineName = stateMachines[i];
                yield return R2EKMath.Remap(i, 0, stateMachines.Count - 1, 0, 1);
                var stateMachine = _copiedBody.AddComponent<EntityStateMachine>();
                stateMachine.customName = stateMachineName;

                switch (stateMachineName)
                {
                    case "Body":
                        stateMachine.initialStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.GenericCharacterSpawnState).AssemblyQualifiedName);
                        stateMachine.mainStateType = template == TemplateChoice.Flying ? new EntityStates.SerializableEntityStateType(typeof(EntityStates.FlyState).AssemblyQualifiedName) : new EntityStates.SerializableEntityStateType(typeof(EntityStates.GenericCharacterMain).AssemblyQualifiedName);

                        if (stateOnHurt)
                            stateOnHurt.targetStateMachine = stateMachine;

                        if (deathBehaviour)
                            deathBehaviour.deathStateMachine = stateMachine;
                        break;
                    case "Weapon":
                        stateMachine.initialStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Idle).AssemblyQualifiedName);
                        stateMachine.mainStateType = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Idle).AssemblyQualifiedName);

                        if (stateOnHurt)
                            HG.ArrayUtils.ArrayAppend(ref stateOnHurt.idleStateMachine, stateMachine);

                        if (deathBehaviour)
                            HG.ArrayUtils.ArrayAppend(ref deathBehaviour.idleStateMachine, stateMachine);
                        break;
                    default:
                        if (stateOnHurt)
                            HG.ArrayUtils.ArrayAppend(ref stateOnHurt.idleStateMachine, stateMachine);

                        if (deathBehaviour)
                            HG.ArrayUtils.ArrayAppend(ref deathBehaviour.idleStateMachine, stateMachine);
                        break;
                }

                if (networker)
                {
                    SerializedProperty prop = networkerSerializedObject.FindProperty("stateMachines");
                    prop.arraySize++;
                    prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = stateMachine;
                }
                Debug.Log($"Created state machine with name {stateMachineName}");
            }
            yield return 1f;

            if (networker)
            {
                networkerSerializedObject.ApplyModifiedProperties();
            }
            yield break;
        }

        private IEnumerator AddGenericSkills()
        {
            var skillLocator = _copiedBody.GetComponent<SkillLocator>();
            for (int i = 0; i < genericSkills.Count; i++)
            {
                var skillSlot = genericSkills[i];
                var stepProgress = R2EKMath.Remap(i, 0, genericSkills.Count - 1, 0, 1);
                yield return stepProgress;
                var genericSkill = _copiedBody.AddComponent<GenericSkill>();
                genericSkill.skillName = skillSlot.ToString();

                if (skillLocator)
                {
                    switch (skillSlot)
                    {
                        case SkillSlot.Primary:
                            if (skillLocator.primary)
                            {
                                Debug.LogError($"Skill Locator Primary is already assigned!");
                                break;
                            }
                            skillLocator.primary = genericSkill;
                            break;
                        case SkillSlot.Secondary:
                            if (skillLocator.secondary)
                            {
                                Debug.LogError($"Skill Locator Secondary is already assigned!");
                                break;
                            }
                            skillLocator.secondary = genericSkill;
                            break;
                        case SkillSlot.Utility:
                            if (skillLocator.utility)
                            {
                                Debug.LogError($"Skill Locator Utility is already assigned!");
                                break;
                            }
                            skillLocator.utility = genericSkill;
                            break;
                        case SkillSlot.Special:
                            if (skillLocator.special)
                            {
                                Debug.LogError($"Skill Locator Secondary is already assigned!");
                                break;
                            }
                            skillLocator.special = genericSkill;
                            break;
                    }
                }

                Debug.Log($"Creating SkillFamily and SkillDef for generic skill of skillSlot {skillSlot}");
                yield return stepProgress;

                var sf = CreateInstance<SkillFamily>();
                ((ScriptableObject)sf).name = $"sf{characterName}{skillSlot}";
                _createdSkillFamilies.Add(sf);
                var serializedObject = new SerializedObject(genericSkill);
                serializedObject.FindProperty("_skillFamily").objectReferenceValue = sf;
                serializedObject.ApplyModifiedProperties();

                var sd = CreateInstance<SkillDef>();
                ((ScriptableObject)sd).name = $"sd{characterName}{skillSlot}";
                sd.activationState = new EntityStates.SerializableEntityStateType(typeof(EntityStates.Uninitialized).AssemblyQualifiedName);
                sd.activationStateMachineName = "Body";
                sd.skillName = $"sd{characterName}{skillSlot}";
                sd.skillNameToken = string.Format(_skillDefTokenFormat, skillSlot.ToString().ToUpperInvariant(), "NAME");
                sd.skillDescriptionToken = string.Format(_skillDefTokenFormat, skillSlot.ToString().ToUpperInvariant(), "DESC");
                HG.ArrayUtils.ArrayAppend(ref sf.variants, new SkillFamily.Variant
                {
                    skillDef = sd,
                });
                _createdSkillDefs.Add(sd);
            }
            yield break;
        }

        private IEnumerator AddComponents()
        {
            for (int i = 0; i < extraComponents.Count; i++)
            {
                var type = extraComponents[i];
                yield return R2EKMath.Remap(i, 0, extraComponents.Count - 1, 0, 1);
                try
                {
                    Type t = (Type)type;
                    if (!ignoreExtraComponentDuplicates && _copiedBody.TryGetComponent(t, out _))
                    {
                        throw new Exception($"Component of type {t} is already in the body.");
                    }
                    _copiedBody.AddComponent(t);
                    Debug.Log($"Added component {t.FullName}");
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            yield break;
        }

        private IEnumerator SetupModelGameObject()
        {
            GameObject modelBase = _copiedBody.transform.Find("ModelBase").gameObject;
            GameObject mdlGameObject = modelBase.transform.GetChild(0).gameObject;

            if (modelFBX)
            {
                GameObject fbxPrefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(modelFBX, modelBase.transform);

                UnityEditorInternal.ComponentUtility.ReplaceComponentsIfDifferent(mdlGameObject, fbxPrefabInstance, (c) => c is not Transform);

                Debug.Log($"Instantiated {fbxPrefabInstance} and transfered components, ensuring proper references between model and body components...");
                var bodyComponents = _copiedBody.GetComponents<MonoBehaviour>();
                for (int i = 0; i < bodyComponents.Length; i++)
                {
                    var bodyComponent = bodyComponents[i];
                    var componentIterationProgress = R2EKMath.Remap(i, 0, bodyComponents.Length, 0, 0.5f);
                    yield return componentIterationProgress;

                    FieldInfo[] fields = bodyComponent.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(f => f.IsPublic || f.GetCustomAttribute<SerializeField>() != null).ToArray();
                    for (int j = 0; j < fields.Length; j++)
                    {
                        FieldInfo field = fields[j];
                        yield return R2EKMath.Remap(j, 0, fields.Length - 1, 0, 0.1f) + componentIterationProgress;
                        if (field.FieldType == typeof(GameObject) && (GameObject)field.GetValue(bodyComponent) == mdlGameObject)
                        {
                            field.SetValue(bodyComponent, fbxPrefabInstance.gameObject);
                        }
                        else if (field.FieldType.IsSubclassOf(typeof(Component)))
                        {
                            Component referencedComponent = field.GetValue(bodyComponent) as Component;
                            if (referencedComponent && referencedComponent.gameObject == mdlGameObject)
                            {
                                field.SetValue(bodyComponent, fbxPrefabInstance.GetComponent(field.FieldType));
                            }
                        }
                    }
                }
                DestroyImmediate(mdlGameObject);
                mdlGameObject = fbxPrefabInstance;

                mdlGameObject.name = $"mdl{characterName}";
            }
            else
            {
                yield return 0.5f;
                GameObject primitiveCapsule = GameObject.CreatePrimitive(PrimitiveType.Capsule);
                DestroyImmediate(primitiveCapsule.GetComponent<CapsuleCollider>());
                primitiveCapsule.transform.SetParent(mdlGameObject.transform);
                Debug.Log("Created a simple capsule as a character model.");
            }

            if (simpleHurtBox)
            {
                yield return 0.55;
                var hurtBoxGameObject = new GameObject("MainHurtBox");
                hurtBoxGameObject.transform.SetParent(mdlGameObject.transform);
                hurtBoxGameObject.layer = LayerIndex.entityPrecise.intVal;

                var capsuleCollider = hurtBoxGameObject.AddComponent<CapsuleCollider>();
                var hurtBox = hurtBoxGameObject.AddComponent<HurtBox>();
                hurtBox.healthComponent = _copiedBody.GetComponent<HealthComponent>();
                hurtBox.isBullseye = true;

                var hbGroup = mdlGameObject.GetComponent<HurtBoxGroup>();
                hbGroup.mainHurtBox = hurtBox;
                hbGroup.hurtBoxes = new HurtBox[] { hurtBox };

                Debug.Log("Added a simple hurt box");
            }

            var characterModel = mdlGameObject.GetComponent<CharacterModel>();
            var renderers = mdlGameObject.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                var renderer = renderers[i];
                yield return R2EKMath.Remap(i, 0, renderers.Length, 0.55f, 1);

                if (renderer is SkinnedMeshRenderer skinnedMeshRenderer)
                {
                    HG.ArrayUtils.ArrayAppend(ref characterModel.baseRendererInfos, new CharacterModel.RendererInfo
                    {
                        defaultMaterial = skinnedMeshRenderer.sharedMaterial,
                        defaultShadowCastingMode = skinnedMeshRenderer.shadowCastingMode,
                        renderer = skinnedMeshRenderer,
                        hideOnDeath = false,
                        ignoreOverlays = false,
                    });
                }
                else if (renderer is MeshRenderer meshRenderer)
                {
                    HG.ArrayUtils.ArrayAppend(ref characterModel.baseRendererInfos, new CharacterModel.RendererInfo
                    {
                        defaultMaterial = meshRenderer.sharedMaterial,
                        defaultShadowCastingMode = meshRenderer.shadowCastingMode,
                        renderer = meshRenderer,
                        hideOnDeath = false,
                        ignoreOverlays = false,
                    });
                }
            }
            Debug.Log("Populated renderer infos.");
            yield break;
        }

        private IEnumerator CreateAssets()
        {
            var bodyFolder = IOPath.Combine(folderPath, characterName);
            Debug.Log("Creating folder " + bodyFolder);
            AssetDatabase.CreateFolder(folderPath, characterName);
            AssetDatabase.Refresh();
            yield return 0.11f;

            var skillsFolder = IOPath.Combine(bodyFolder, "Skills");
            Debug.Log("Creating Skills folder " + skillsFolder);
            AssetDatabase.CreateFolder(IOUtils.FormatPathForUnity(bodyFolder), "Skills");
            AssetDatabase.Refresh();
            yield return 0.22f;

            Debug.Log($"Creating Skill Defs");
            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < _createdSkillDefs.Count; i++)
                {
                    var skillDef = _createdSkillDefs[i];
                    yield return R2EKMath.Remap(i, 0, _createdSkillDefs.Count - 1, 0.33f, 0.66f);

                    var soName = ((ScriptableObject)skillDef).name;
                    var skillDefPath = IOUtils.GenerateUniqueFileName(skillsFolder, soName, ".asset");
                    Debug.Log($"Creating SkillDef in {skillDefPath}");
                    AssetDatabase.CreateAsset(skillDef, IOUtils.FormatPathForUnity(skillDefPath));
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            AssetDatabase.Refresh();
            yield return 0.66f;

            Debug.Log("Creating Skill Families");
            AssetDatabase.StartAssetEditing();
            try
            {
                for (int i = 0; i < _createdSkillFamilies.Count; i++)
                {
                    var skillFamily = _createdSkillFamilies[i];
                    yield return R2EKMath.Remap(i, 0, _createdSkillFamilies.Count - 1, 0.75f, 0.99f);

                    var soName = ((ScriptableObject)skillFamily).name;
                    var skillFamilyPath = IOUtils.GenerateUniqueFileName(skillsFolder, soName, ".asset");
                    Debug.Log($"Creating SkillFamily in {skillFamilyPath}");
                    AssetDatabase.CreateAsset(skillFamily, IOUtils.FormatPathForUnity(skillFamilyPath));
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }
            AssetDatabase.Refresh();
            yield return 0.99f;


            var bodyPrefabPath = IOPath.Combine(bodyFolder, $"{_copiedBody.name}.prefab");
            Debug.Log("Saving body in " + bodyPrefabPath);
            PrefabUtility.SaveAsPrefabAsset(_copiedBody, IOUtils.FormatPathForUnity(bodyPrefabPath));
            AssetDatabase.ImportAsset(IOUtils.FormatPathForUnity(bodyPrefabPath));
            yield return 1f;
        }

        protected override void Cleanup()
        {
            foreach (var sd in _createdSkillDefs)
            {
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(sd)))
                    DestroyImmediate(sd);
            }

            foreach (var sf in _createdSkillFamilies)
            {
                if (string.IsNullOrEmpty(AssetDatabase.GetAssetPath(sf)))
                    DestroyImmediate(sf);
            }

            DestroyImmediate(_copiedBody);
        }
        public enum TemplateChoice
        {
            Standing,
            Flying,
            Stationary,
            Boss
        }
    }
}