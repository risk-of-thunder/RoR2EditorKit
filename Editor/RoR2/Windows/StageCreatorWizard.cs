using RoR2.Navigation;
using System.Collections;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using IOPath = System.IO.Path;

namespace RoR2.Editor.Windows
{
    public class StageCreatorWizard : EditorWizardWindow
    {
        [FilePickerPath(FilePickerPath.PickerType.OpenFolder, title = "Folder for created Assets")]
        public string folderPath;

        public string stageName;
        public int stageOrder;

        protected override string wizardTitleTooltip =>
@"The StageCreatorWizard is a Wizard that creates a stage that can be later loaded into the game.
It'll create a basic scene asset with necesary components to be used as a stage in a run, alongside a SceneDef for the scene.

It'll also create the NodeGraphs, DCCS and DCCSPool for the stage.";

        protected override bool requiresTokenPrefix => true;

        private WizardCoroutineHelper _wizardCoroutineHelper;

        private string _folderOutput;
        private string _upperToken;
        private string _lowerToken;
        private Scene _scene;
        private DirectorCardCategorySelection _monsterDCCS;
        private DirectorCardCategorySelection _interactableDCCS;
        private GameObject _sceneInfoGO;

        [MenuItem(R2EKConstants.ROR2EK_MENU_ROOT + "/Wizards/Stage")]
        private static void Open() => Open<StageCreatorWizard>(null);

        protected override void OnEnable()
        {
            base.OnEnable();
            _wizardCoroutineHelper = new WizardCoroutineHelper(this);
            _wizardCoroutineHelper.AddStep(CreateTokens(), "Creating Tokens");
            _wizardCoroutineHelper.AddStep(DuplicateSceneAsset(), "Creating Scene Asset");
            _wizardCoroutineHelper.AddStep(OpenScene(), "Opening Scene Asset");
            _wizardCoroutineHelper.AddStep(CreateNodeGraphs(), "Creating Node Graphs");
            _wizardCoroutineHelper.AddStep(CreateDCCS(), "Creating DCCS");
            _wizardCoroutineHelper.AddStep(CreateDCCSPool(), "Creating DCCSPool");
            _wizardCoroutineHelper.AddStep(SaveScene(), "Saving Scene Changes");
            _wizardCoroutineHelper.AddStep(CreateSceneDef(), "Creating Scene Def");
        }

        protected override void Cleanup()
        {
            base.Cleanup();
            EditorSceneManager.CloseScene(_scene, true);
            _folderOutput = "";
            _upperToken = "";
            _lowerToken = "";
            _scene = default;
            _monsterDCCS = null;
            _interactableDCCS = null;
            _sceneInfoGO = null;
        }

        protected override bool ValidateData()
        {
            if (stageName.IsNullOrEmptyOrWhiteSpace())
            {
                Debug.LogError($"Cannot run wizard because the Stage Name is null, empty or whitespace.");
                return false;
            }
            if (folderPath.IsNullOrEmptyOrWhiteSpace())
            {
                Debug.LogError($"Cannot run wizard because the Folder Path is null, empty or whitespace.");
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

        private IEnumerator CreateTokens()
        {
            _upperToken = R2EKSettings.instance.GetTokenAllUpperCase();
            _lowerToken = R2EKSettings.instance.GetTokenAllLowerCase();
            _folderOutput = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder(folderPath, stageName));
            yield break;
        }

        private IEnumerator DuplicateSceneAsset()
        {
            var sceneAsset = (SceneAsset)R2EKConstants.AssetGUIDs.stageTemplate;
            var destPath = IOUtils.FormatPathForUnity(IOPath.Combine(_folderOutput, $"{_lowerToken}_{stageName}.unity"));
            var sceneAssetPath = AssetDatabase.GetAssetPath(sceneAsset);
            AssetDatabase.CopyAsset(sceneAssetPath, destPath);
            yield return null;
            yield break;
        }

        private IEnumerator OpenScene()
        {
            _scene = EditorSceneManager.OpenScene(IOUtils.FormatPathForUnity(IOPath.Combine(_folderOutput, $"{_lowerToken}_{stageName}.unity")), OpenSceneMode.Additive);
            yield return null;
        }

        private IEnumerator CreateNodeGraphs()
        {
            var rootGO = _scene.GetRootGameObjects();
            GameObject[] mapNodeGroupObjects = rootGO.Where(go => go.TryGetComponent<MapNodeGroup>(out _)).ToArray();
            SceneInfo sceneInfo = rootGO.Select(go => go.GetComponent<SceneInfo>()).Where(sceneInfo => sceneInfo != null).FirstOrDefault();

            _sceneInfoGO = sceneInfo.gameObject;
            var serializedObject = new SerializedObject(sceneInfo);

            for (int i = 0; i < mapNodeGroupObjects.Length; i++)
            {
                var gameObject = mapNodeGroupObjects[i];
                Debug.Log($"Creating Node Graph for {gameObject}");
                yield return R2EKMath.Remap(i, 0, mapNodeGroupObjects.Length - 1, 0, 1f);
                var nodeGroup = gameObject.GetComponent<MapNodeGroup>();
                var nodeGraph = CreateInstance<NodeGraph>();

                var path = IOUtils.GenerateUniqueFileName(_folderOutput, gameObject.name, ".asset");
                AssetDatabase.CreateAsset(nodeGraph, path);
                nodeGroup.nodeGraph = nodeGraph;


                switch (nodeGroup.graphType)
                {
                    case MapNodeGroup.GraphType.Air:
                        serializedObject.FindProperty("airNodesAsset").objectReferenceValue = nodeGraph;
                        break;
                    case MapNodeGroup.GraphType.Ground:
                        serializedObject.FindProperty("groundNodesAsset").objectReferenceValue = nodeGraph;
                        break;
                }
            }

            serializedObject.ApplyModifiedProperties();
            yield break;
        }

        private IEnumerator CreateDCCS()
        {
            Debug.Log($"Creating interactable and monster DCCS");
            _monsterDCCS = CreateInstance<DirectorCardCategorySelection>();
            var path = IOUtils.GenerateUniqueFileName(_folderOutput, $"dccs{stageName}Monsters", ".asset");
            AssetDatabase.CreateAsset(_monsterDCCS, path);
            yield return null;

            _interactableDCCS = CreateInstance<DirectorCardCategorySelection>();
            path = IOUtils.GenerateUniqueFileName(_folderOutput, $"dccs{stageName}Interactables", ".asset");
            AssetDatabase.CreateAsset(_interactableDCCS, path);
            yield break;
        }

        private IEnumerator CreateDCCSPool()
        {
            Debug.Log($"Creating DCCSPools for Monsters and Interactables");

            ClassicStageInfo stageInfo = _sceneInfoGO.GetComponent<ClassicStageInfo>();
            var stageInfoSerializedObject = new SerializedObject(stageInfo);

            string[] poolNames = new string[] { $"dccspool{stageName}Monsters", $"dccspool{stageName}Interactables" };
            for (int i = 0; i < poolNames.Length; i++)
            {
                var poolName = poolNames[i];
                yield return R2EKMath.Remap(i, 0, poolNames.Length - 1, 0, 1);
                var poolInstance = CreateInstance<DccsPool>();
                var poolInstanceSerializedObject = new SerializedObject(poolInstance);
                var categoriesArray = poolInstanceSerializedObject.FindProperty("poolCategories");

                categoriesArray.arraySize = 1;
                var category = categoriesArray.GetArrayElementAtIndex(0);
                category.FindPropertyRelative("name").stringValue = "Standard";
                category.FindPropertyRelative("categoryWeight").floatValue = 0.98f;

                var ifNoConditionsMet = category.FindPropertyRelative("includedIfNoConditionsMet");
                ifNoConditionsMet.arraySize = 1;
                var poolEntry = ifNoConditionsMet.GetArrayElementAtIndex(0);
                poolEntry.FindPropertyRelative("dccs").objectReferenceValue = poolName.Contains("Monsters") ? _monsterDCCS : _interactableDCCS;
                poolEntry.FindPropertyRelative("weight").floatValue = 1;

                poolInstanceSerializedObject.ApplyModifiedProperties();

                var path = IOUtils.GenerateUniqueFileName(_folderOutput, poolName, ".asset");
                AssetDatabase.CreateAsset(poolInstance, path);

                if (poolName.Contains("Monsters"))
                {
                    stageInfoSerializedObject.FindProperty("monsterDccsPool").objectReferenceValue = poolInstance;
                }
                else
                {
                    stageInfoSerializedObject.FindProperty("interactableDccsPool").objectReferenceValue = poolInstance;
                }
            }
            stageInfoSerializedObject.ApplyModifiedProperties();
            yield break;
        }

        private IEnumerator SaveScene()
        {
            Debug.Log("Saving Scene Changes");
            yield return null;
            EditorSceneManager.SaveScene(_scene);
            yield break;
        }

        private IEnumerator CreateSceneDef()
        {
            Debug.Log("Creating Scene Def");

            var sceneDef = CreateInstance<SceneDef>();

            sceneDef.baseSceneNameOverride = stageName;
            sceneDef.sceneType = SceneType.Stage;
            sceneDef.stageOrder = stageOrder;

            var tokenBase = $"{_upperToken}_MAP_{stageName.ToUpperInvariant()}_{0}";
            sceneDef.nameToken = string.Format(tokenBase, "NAME");
            sceneDef.subtitleToken = string.Format(tokenBase, "SUBTITLE");
            sceneDef.loreToken = string.Format(tokenBase, "LORE");

            sceneDef.portalSelectionMessageString = $"{_upperToken}_BAZAAR_SEER_{stageName.ToUpperInvariant()}";

            sceneDef.shouldIncludeInLogbook = true;

            sceneDef.validForRandomSelection = true;
            yield return null;

            var path = IOUtils.GenerateUniqueFileName(_folderOutput, $"sd{_lowerToken}{stageName}", ".asset");
            AssetDatabase.CreateAsset(sceneDef, path);
            yield break;
        }
    }
}