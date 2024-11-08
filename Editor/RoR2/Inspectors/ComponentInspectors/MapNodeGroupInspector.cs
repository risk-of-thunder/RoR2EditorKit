using RoR2.Navigation;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(MapNodeGroup))]
    public class MapNodeGroupInspector : VisualElementComponentInspector<MapNodeGroup>
    {
        public string parentGameObjectName => inspectorProjectSettings.GetOrCreateSetting(nameof(parentGameObjectName), "NodeContainer");

        private Vector3 _offsetUpVector;
        private ObjectField _parentObjectField;
        private GameObject _parentObject;
        private HullMask _forbiddenHulls;
        private NodeFlags _nodeFlags;
        private string _gateName;
        private LayerMask _raycastMask;

        private float _nodeCylinderSize;
        private float _painterSize;
        private bool _roundHitPointToNearestGrid;
        private bool _drawallNodes;
        private bool _usePainter;
        private KeyCode _addNodeKeyCode;
        private KeyCode _deleteNearestKeyCode;
        private KeyCode _addOnCamPosKeyCode;
        private Color _linkPreviewColor;
        private Color _previewNodeInRangeColor;
        private Color _previewNodeOutOfRangeColor;
        private Color _placedNodeWithNoLinksColor;
        private Color _bakedNodeColor;
        private Color _invalidPlacedNodeColor;
        private Color _bakedLinkColor;

        private Vector3 _currentHitInfo = default;
        private List<MapNode> _cachedMapNodeList = new List<MapNode>();
        private float _maxDistance;
        private VisualElement _nodePlacerContainer;
        private VisualElement _painterContainer;
        private VisualElement _buttonsContainer;

        private EditorCoroutine _bakingCoroutine;
        protected override void OnInspectorEnabled()
        {
            base.OnInspectorEnabled();
            _parentObject = targetType.transform.Find(parentGameObjectName)?.gameObject ?? null;
            _offsetUpVector = inspectorProjectSettings.GetOrCreateSetting(nameof(_offsetUpVector), new Vector3(0, 15, 0));
            _raycastMask = inspectorProjectSettings.GetOrCreateSetting(nameof(_raycastMask), LayerIndex.CommonMasks.bullet);

            _nodeCylinderSize = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_nodeCylinderSize), 1f);
            _addNodeKeyCode = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_addNodeKeyCode), KeyCode.B);
            _deleteNearestKeyCode = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_deleteNearestKeyCode), KeyCode.M);
            _addOnCamPosKeyCode = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_addOnCamPosKeyCode), KeyCode.V);
            _linkPreviewColor = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_linkPreviewColor), Color.yellow);
            _previewNodeInRangeColor = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_previewNodeInRangeColor), Color.yellow);
            _previewNodeOutOfRangeColor = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_previewNodeOutOfRangeColor), Color.red);
            _placedNodeWithNoLinksColor = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_placedNodeWithNoLinksColor), (Color)new Color32
            {
                a = 255,
                r = 255,
                g = 165
            });
            _bakedNodeColor = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_bakedNodeColor), Color.green);
            _invalidPlacedNodeColor = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_invalidPlacedNodeColor), Color.black);
            _bakedLinkColor = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_bakedLinkColor), Color.magenta);
        }

        protected override void InitializeVisualElement(VisualElement templateInstanceRoot)
        {
            AddFlags(templateInstanceRoot);

            _painterContainer = templateInstanceRoot.Q<VisualElement>("PainterContainer");
            _nodePlacerContainer = templateInstanceRoot.Q<VisualElement>("NodePlacerContainer");

            var property = serializedObject.FindProperty("nodeGraph");
            var pField = templateInstanceRoot.Q<PropertyField>("NodeGraph");
            pField.TrackPropertyValue(property, OnNodeGraphChange);
            OnNodeGraphChange(property);

            _parentObjectField = templateInstanceRoot.Q<ObjectField>("ParentGameObject");
            _parentObjectField.RegisterValueChangedCallback(UpdateParentGameObjectSetting);
            _parentObjectField.SetValueWithoutNotify(_parentObject);

            var drawAllNodes = templateInstanceRoot.Q<Toggle>("DrawAllNodes");
            drawAllNodes.RegisterValueChangedCallback(evt => _drawallNodes = evt.newValue);
            drawAllNodes.ConnectWithSetting(inspectorPreferenceSettings, nameof(drawAllNodes), false);
            _drawallNodes = inspectorPreferenceSettings.GetOrCreateSetting<bool>(nameof(drawAllNodes));

            var usePainter = templateInstanceRoot.Q<Toggle>("UsePainter");
            usePainter.RegisterValueChangedCallback(OnUsePainterSet);
            usePainter.ConnectWithSetting(inspectorPreferenceSettings, nameof(usePainter), false);
            _usePainter = inspectorPreferenceSettings.GetOrCreateSetting<bool>(nameof(usePainter));
            _painterContainer.SetDisplay(_usePainter);

            var painterSize = templateInstanceRoot.Q<FloatField>("BrushSize");
            painterSize.RegisterValueChangedCallback(evt => _painterSize = evt.newValue);
            painterSize.ConnectWithSetting(inspectorPreferenceSettings, nameof(painterSize), 4f);
            _painterSize = inspectorPreferenceSettings.GetOrCreateSetting<float>(nameof(painterSize));
            
            var nodeDistance = templateInstanceRoot.Q<FloatField>("NodeDistance");
            nodeDistance.RegisterValueChangedCallback(evt => _maxDistance = evt.newValue);
            nodeDistance.ConnectWithSetting(inspectorPreferenceSettings, nameof(nodeDistance), 15f);
            _maxDistance = inspectorPreferenceSettings.GetOrCreateSetting<float>(nameof(nodeDistance));

            var roundHitPointToNearestGrid = templateInstanceRoot.Q<Toggle>("RoundPointToGrid");
            roundHitPointToNearestGrid.RegisterValueChangedCallback(evt => _roundHitPointToNearestGrid = evt.newValue);
            roundHitPointToNearestGrid.ConnectWithSetting(inspectorPreferenceSettings, nameof(roundHitPointToNearestGrid), false);
            _roundHitPointToNearestGrid = inspectorPreferenceSettings.GetOrCreateSetting<bool>(nameof(roundHitPointToNearestGrid));

            var gateName = templateInstanceRoot.Q<TextField>("GateName");
            gateName.RegisterValueChangedCallback(evt => _gateName = evt.newValue);
            gateName.ConnectWithSetting(inspectorProjectSettings, nameof(gateName), "");
            _gateName = inspectorProjectSettings.GetOrCreateSetting<string>(nameof(gateName));

            templateInstanceRoot.Q<Button>("UpdateNoCeilingMasks").clicked += UpdateNoCeilingMasks;
            templateInstanceRoot.Q<Button>("UpdateTeleporterMasks").clicked += UpdateTeleporterMasks;
            templateInstanceRoot.Q<Button>("UpdateHullMasks").clicked += UpdateHullMasks;
            templateInstanceRoot.Q<Button>("RemoveNodeExcess").clicked += RemoveNodeExcess;
            templateInstanceRoot.Q<Button>("MakeAirNodes").clicked += MakeAirNodes;
            templateInstanceRoot.Q<Button>("MakeGroundNodes").clicked += MakeGroundNodes;
            templateInstanceRoot.Q<Button>("ClearNodes").clicked += ClearNodes;
            templateInstanceRoot.Q<Button>("BakeNodeGraph").clicked += BakeNodeGraph;
            templateInstanceRoot.Q<Button>("BakeGraphAsync").clicked += BakeGraphAsync;
            _buttonsContainer = templateInstanceRoot.Q<VisualElement>("ButtonContainer");
        }

        private void BakeGraphAsync()
        {
            _bakingCoroutine = EditorCoroutineUtility.StartCoroutine(BakeNodeGraphAsync(rootVisualElement.Q<ProgressBar>(), _buttonsContainer), this);
        }

        private void OnUsePainterSet(ChangeEvent<bool> evt)
        {
            _usePainter = evt.newValue;
            _painterContainer.SetDisplay(evt.newValue);
        }

        private void OnNodeGraphChange(SerializedProperty property)
        {
            _nodePlacerContainer.SetDisplay(property.objectReferenceValue);
        }

        private void MakeGroundNodes()
        {
            foreach (MapNode node in _cachedMapNodeList)
            {
                LayerMask mask = _raycastMask;
                RaycastHit hit;
                Vector3 newPos = node.transform.position;
                if (Physics.Raycast(node.transform.position, Vector3.down, out hit, _offsetUpVector.magnitude * 2, mask, QueryTriggerInteraction.Collide))
                {
                    newPos = hit.point;
                }
                else
                {
                    newPos -= _offsetUpVector;
                }
                node.transform.position = _roundHitPointToNearestGrid ? R2EKMath.RoundToNearestGrid(newPos) : newPos;
            }
        }

        private void MakeAirNodes()
        {
            foreach (MapNode node in _cachedMapNodeList)
            {
                LayerMask mask = _raycastMask;
                RaycastHit hit;
                Vector3 newPos = node.transform.position;
                if (Physics.Raycast(node.transform.position, Vector3.up, out hit, _offsetUpVector.magnitude, mask, QueryTriggerInteraction.Collide))
                {
                    newPos += (hit.point - newPos) / 2;
                }
                else
                {
                    newPos += _offsetUpVector;
                }
                node.transform.position = _roundHitPointToNearestGrid ? R2EKMath.RoundToNearestGrid(newPos) : newPos;
            }
        }

        private void RemoveNodeExcess()
        {
            EditorUtility.SetDirty(targetType.nodeGraph);
            int c = 0;
            foreach (MapNode mapNode in _cachedMapNodeList)
            {
                if (mapNode)
                {
                    if (mapNode.links.Count <= 0) //Destroy instantly as there's no links.
                    {
                        DestroyImmediate(mapNode.gameObject);
                        c++;
                        continue;
                    }
                }
            }
            Debug.Log($"Removed {c} nodes that were not connected to anything.");
            AssetDatabase.SaveAssets();
        }

        private void UpdateHullMasks()
        {
            foreach (MapNode node in _cachedMapNodeList)
            {
                node.forbiddenHulls = HullMask.None;
                for (int i = 0; i < (int)HullClassification.Count; i++)
                {
                    HullDef hullClass = HullDef.Find((HullClassification)i);
                    if (Physics.OverlapSphere(node.gameObject.transform.position + (Vector3.up * ((hullClass.radius * 0.7f) + 0.25f)), hullClass.radius * 0.7f, LayerIndex.world.mask | LayerIndex.defaultLayer.mask | LayerIndex.fakeActor.mask).Length != 0)
                    {
                        node.forbiddenHulls |= ((HullMask)(1 << (int)i));
                    }
                }
            }
        }

        private void BakeNodeGraph()
        {
            EditorUtility.SetDirty(targetType.nodeGraph);
            targetType.Bake(targetType.nodeGraph);
            AssetDatabase.SaveAssets();
        }

        private IEnumerator BakeNodeGraphAsync(ProgressBar progressBar, VisualElement buttonContainer)
        {
            progressBar.SetDisplay(true);
            buttonContainer.SetEnabled(false);
            EditorUtility.SetDirty(targetType.nodeGraph);
            List<MapNode> nodes = targetType.GetNodes();
            ReadOnlyCollection<MapNode> readOnlyCollection = nodes.AsReadOnly();

            var modulo = Mathf.Floor((Mathf.Log10(nodes.Count) + 1) * 2);
            for (int i = 0; i < nodes.Count; i++)
            {
                progressBar.title = $"Building Links for Node {i} thru {i + modulo}";
                progressBar.value = R2EKMath.Remap(i, 0, nodes.Count - 1, 0, 0.5f);

                if (i % modulo == 0)
                    yield return null;

                nodes[i].BuildLinks(readOnlyCollection, targetType.graphType);
            }
            List<SerializableBitArray> list = new List<SerializableBitArray>();
            for (int i = 0; i < nodes.Count; i++)
            {
                float nodeIterationProgress = R2EKMath.Remap(i, 0, nodes.Count - 1, 0.5f, 1f);
                progressBar.title = $"Testing node {i} thru {i + modulo}'s LOS with other nodes";
                progressBar.value = nodeIterationProgress;
                if (i % modulo == 0)
                    yield return null;

                MapNode mapNode = nodes[i];
                SerializableBitArray serializableBitArray = new SerializableBitArray(nodes.Count);
                for (int j = 0; j < nodes.Count; j++)
                {
                    MapNode other = nodes[j];
                    serializableBitArray[j] = mapNode.TestLineOfSight(other);
                }
                list.Add(serializableBitArray);
            }
            targetType.nodeGraph.SetNodes(readOnlyCollection, list.AsReadOnly());
            buttonContainer.SetEnabled(true);
            progressBar.SetDisplay(false);
            AssetDatabase.SaveAssetIfDirty(targetType.nodeGraph);
            _bakingCoroutine = null;
        }

        private void UpdateTeleporterMasks()
        {
            targetType.UpdateTeleporterMasks();
        }

        private void ClearNodes()
        {
            if (EditorUtility.DisplayDialog("WARNING: Clear All Nodes", "Clicking this button will delete EVERY node. Are you sure you want to do this? (YOU CANNOT UNDO THIS OPERATION)", "Yes, Im sure", "No, Take me back"))
            {
                targetType.Clear();
            }
        }

        private void UpdateNoCeilingMasks()
        {
            targetType.UpdateNoCeilingMasks();
        }

        private void UpdateParentGameObjectSetting(ChangeEvent<UnityEngine.Object> evt)
        {
            _parentObject = evt.newValue ? evt.newValue as GameObject : null;
            if (evt.newValue)
            {
                inspectorProjectSettings.SetSettingValue(nameof(parentGameObjectName), evt.newValue.name);
            }
        }

        private void AddFlags(VisualElement root)
        {
            var flagContainer = root.Q<VisualElement>("FlagContainer");

            EnumFlagsField flagsField = new EnumFlagsField(HullMask.None);
            flagsField.label = "Forbidden Hulls";
            flagsField.RegisterValueChangedCallback(evt => _forbiddenHulls = (HullMask)evt.newValue);
            flagsField.ConnectWithSetting(inspectorProjectSettings, "forbiddenHullMask", _forbiddenHulls);
            flagContainer.Add(flagsField);

            flagsField = new EnumFlagsField(NodeFlags.None);
            flagsField.label = "Node Flags";
            flagsField.RegisterValueChangedCallback(evt => _nodeFlags = (NodeFlags)evt.newValue);
            flagsField.ConnectWithSetting(inspectorProjectSettings, "nodeFlags", _nodeFlags);
            flagContainer.Add(flagsField);
        }

        private void OnSceneGUI()
        {
            if (inspectorEnabled && targetType.nodeGraph)
            {
                if (_bakingCoroutine == null)
                    DrawPlacerOrPainter();

                Handles.BeginGUI();
                EditorGUILayout.BeginVertical("box", GUILayout.Width(400));
                EditorGUILayout.BeginVertical("box", GUILayout.Width(400));
                EditorGUILayout.BeginVertical("box", GUILayout.Width(400));

                if (_bakingCoroutine != null)
                {
                    EditorGUILayout.LabelField("Baking... Controls are Disabled.", EditorStyles.boldLabel);
                    goto endVertical;
                }

                EditorGUILayout.LabelField($"Camera Position: {Camera.current.transform.position}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Hitpoint Position: {_currentHitInfo}", EditorStyles.boldLabel);

                string placerString = "Press B to ";
                placerString += _usePainter ? "paint nodes at the current mouse position (raycast)" : "add a map node at the current mouse position (raycast)";
                EditorGUILayout.LabelField(placerString, EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Press {_addOnCamPosKeyCode} to add a map node at the current camera's position", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Press {_deleteNearestKeyCode} to delete the nearest map node at current mouse position", EditorStyles.boldLabel);

            endVertical:
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                Handles.EndGUI();
                SceneView.currentDrawingSceneView.Repaint();
            }
        }

        private void DrawPlacerOrPainter()
        {
            UnityEngine.Cursor.visible = true;

            int controlID = GUIUtility.GetControlID(FocusType.Keyboard | FocusType.Passive);
            _cachedMapNodeList = targetType.GetNodes();
            float zPainterOffset = _maxDistance / 2;

            if (Event.current.GetTypeForControl(controlID) == EventType.KeyDown)
            {
                var keyCode = Event.current.keyCode;
                if (keyCode == _addNodeKeyCode)
                {
                    if (_usePainter)
                    {
                        PaintNodes(_maxDistance, zPainterOffset, _cachedMapNodeList);
                    }
                    else
                    {
                        AddNode(targetType, _currentHitInfo);
                    }
                    Event.current.Use();
                }
                if (keyCode == _deleteNearestKeyCode)
                {
                    DeleteNearestNode(_currentHitInfo);
                    Event.current.Use();
                }
                if (keyCode == _addOnCamPosKeyCode)
                {
                    AddNode(targetType, Camera.current.transform.position);
                    Event.current.Use();
                }
            }

            Vector2 guiPos = Event.current.mousePosition;
            Ray ray = HandleUtility.GUIPointToWorldRay(guiPos);
            var length = _cachedMapNodeList.Count;
            var currentNodeList = _cachedMapNodeList;
            var rotation = Quaternion.Euler(90, 0, 0);

            if (Physics.Raycast(ray, out var hitInfo, 9999999999, _raycastMask, QueryTriggerInteraction.Collide))
            {
                _currentHitInfo = hitInfo.point;
                if (_roundHitPointToNearestGrid)
                {
                    _currentHitInfo = R2EKMath.RoundToNearestGrid(_currentHitInfo);
                }
                _currentHitInfo += targetType.graphType == MapNodeGroup.GraphType.Air ? _offsetUpVector : Vector3.zero;

                if (_cachedMapNodeList.Count > 0)
                {
                    var inRange = false;
                    foreach (var mapNode in _cachedMapNodeList)
                    {
                        if (mapNode && Vector3.Distance(mapNode.transform.position, _currentHitInfo) <= MapNode.maxConnectionDistance)
                        {
                            Handles.color = _linkPreviewColor;
                            Handles.DrawLine(mapNode.transform.position, _currentHitInfo);
                            inRange = true;
                        }
                    }

                    if (inRange)
                    {
                        Handles.color = _previewNodeInRangeColor;
                    }
                    else
                    {
                        Handles.color = _previewNodeOutOfRangeColor;
                    }
                }
                else
                {
                    Handles.color = _previewNodeInRangeColor;
                }

                Handles.CylinderHandleCap(controlID, _currentHitInfo, rotation, _nodeCylinderSize, EventType.Repaint);

                if (_usePainter)
                {
                    if (_painterSize <= 0)
                    {
                        Handles.CylinderHandleCap(controlID, _currentHitInfo, Quaternion.Euler(90, 0, 0), _nodeCylinderSize, EventType.Repaint);
                    }
                    else
                    {
                        Handles.CircleHandleCap(controlID, _currentHitInfo, Quaternion.Euler(90, 0, 0), _painterSize, EventType.Repaint);
                    }
                }
            }

            for (int i = 0; i < length; i++)
            {
                var mapNode = currentNodeList[i];
                if (!mapNode || (!_drawallNodes && (mapNode.transform.position - ray.origin).sqrMagnitude > 22500))
                {
                    continue;
                }

                if ((this._nodeFlags == NodeFlags.None || (mapNode.flags & this._nodeFlags) != NodeFlags.None) && (this._forbiddenHulls == HullMask.None || (mapNode.forbiddenHulls & this._forbiddenHulls) == this._forbiddenHulls))
                {
                    if (mapNode.links.Count <= 0)
                    {
                        Handles.color = _placedNodeWithNoLinksColor;
                    }
                    else
                    {
                        Handles.color = _bakedNodeColor;
                    }
                }
                else
                {
                    Handles.color = _invalidPlacedNodeColor;
                }

                Handles.CylinderHandleCap(controlID, mapNode.transform.position, rotation, _nodeCylinderSize, EventType.Repaint);
                Handles.color = _bakedLinkColor;
                foreach (var link in mapNode.links)
                {
                    if (link.nodeB)
                    {
                        Handles.DrawLine(mapNode.transform.position, link.nodeB.transform.position);
                    }
                }
            }
        }

        //N: This is not my code, this is a code of a previous community member that was banned for being abhorrent, i dont have plans to support this tool if it breaks.
        private void PaintNodes(float currentMaxDistance, float zPainterOffset, List<MapNode> cachedMapNodeList)
        {
            for (float x = _currentHitInfo.x - _painterSize, zCount = 0; x <= _currentHitInfo.x; x += currentMaxDistance / 2, zCount++)
            {
                for (float z = _currentHitInfo.z - _painterSize; z <= _currentHitInfo.z; z += currentMaxDistance / 2)
                {
                    //Haven't found a single node that is too close, feel free to spawn.
                    //We lift the pos in case terrain is not flat...
                    //We raycast to ground
                    if ((x - _currentHitInfo.x) * (x - _currentHitInfo.x) + (z - _currentHitInfo.z) * (z - _currentHitInfo.z) <= _painterSize * _painterSize)
                    {
                        float xSym = _currentHitInfo.x - (x - _currentHitInfo.x);
                        float zSym = _currentHitInfo.z - (z - _currentHitInfo.z);

                        float offsetY = targetType.graphType == MapNodeGroup.GraphType.Air ? 0 : 6;
                        Vector3 future1 = (int)zCount % 2 == 0 ? new Vector3(x, _currentHitInfo.y + offsetY, z + zPainterOffset) : new Vector3(x, _currentHitInfo.y + offsetY, z);
                        Vector3 future2 = (int)zCount % 2 == 0 ? new Vector3(x, _currentHitInfo.y + offsetY, zSym + zPainterOffset) : new Vector3(x, _currentHitInfo.y + offsetY, zSym);
                        Vector3 future3 = (int)zCount % 2 == 0 ? new Vector3(xSym, _currentHitInfo.y + offsetY, z + zPainterOffset) : new Vector3(xSym, _currentHitInfo.y + offsetY, z);
                        Vector3 future4 = (int)zCount % 2 == 0 ? new Vector3(xSym, _currentHitInfo.y + offsetY, zSym + zPainterOffset) : new Vector3(xSym, _currentHitInfo.y + offsetY, zSym);

                        if (!Physics.Linecast(_currentHitInfo, future1, out _, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                        {
                            bool canPlace = true;
                            if (targetType.graphType == MapNodeGroup.GraphType.Ground && Physics.Raycast(future1, Vector3.down, out RaycastHit raycastHit, 30, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Collide))
                            {
                                foreach (MapNode node in cachedMapNodeList)
                                {
                                    if (Vector3.Distance(node.transform.position, raycastHit.point) <= currentMaxDistance / 2)
                                    {
                                        canPlace = false;
                                        break;
                                    }
                                }
                                if (canPlace)
                                {
                                    AddNode(targetType, raycastHit.point);
                                }
                            }
                            /*else
                            {
                                foreach (MapNode node in cachedMapNodeList)
                                {
                                    if (Vector3.Distance(node.transform.position, future1) <= currentMaxDistance)
                                    {
                                        canPlace = false;
                                        break;
                                    }
                                }
                                if (canPlace)
                                {
                                    AddNode(targetType, future1);
                                }
                            }*/
                        }
                        if (!Physics.Linecast(_currentHitInfo, future2, out _, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                        {
                            bool canPlace = true;
                            if (targetType.graphType == MapNodeGroup.GraphType.Ground && Physics.Raycast(future2, Vector3.down, out RaycastHit raycastHitto, 30, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Collide))
                            {
                                foreach (MapNode node in cachedMapNodeList)
                                {
                                    if (Vector3.Distance(node.transform.position, raycastHitto.point) <= currentMaxDistance / 2)
                                    {
                                        canPlace = false;
                                        break;
                                    }
                                }
                                if (canPlace)
                                {
                                    AddNode(targetType, raycastHitto.point);
                                }
                            }
                            /*else
                            {
                                foreach (MapNode node in cachedMapNodeList)
                                {
                                    if (Vector3.Distance(node.transform.position, future2) <= currentMaxDistance)
                                    {
                                        canPlace = false;
                                        break;
                                    }
                                }
                                if (canPlace)
                                {
                                    AddNode(targetType, future2);
                                }
                            }*/
                        }
                        if (!Physics.Linecast(_currentHitInfo, future3, out _, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                        {
                            bool canPlace = true;
                            if (targetType.graphType == MapNodeGroup.GraphType.Ground && Physics.Raycast(future3, Vector3.down, out RaycastHit raycastHittoto, 30, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Collide))
                            {
                                foreach (MapNode node in cachedMapNodeList)
                                {
                                    if (Vector3.Distance(node.transform.position, raycastHittoto.point) <= currentMaxDistance / 2)
                                    {
                                        canPlace = false;
                                        break;
                                    }
                                }
                                if (canPlace)
                                {
                                    AddNode(targetType, raycastHittoto.point);
                                }
                            }
                            /*else
                            {
                                foreach (MapNode node in cachedMapNodeList)
                                {
                                    if (Vector3.Distance(node.transform.position, future3) <= currentMaxDistance)
                                    {
                                        canPlace = false;
                                        break;
                                    }
                                }
                                if (canPlace)
                                {
                                    AddNode(targetType, future3);
                                }
                            }*/
                        }
                        if (!Physics.Linecast(_currentHitInfo, future4, out _, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                        {
                            bool canPlace = true;
                            if (targetType.graphType == MapNodeGroup.GraphType.Ground && Physics.Raycast(future4, Vector3.down, out RaycastHit raycastHittototo, 30, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Collide))
                            {
                                foreach (MapNode node in cachedMapNodeList)
                                {
                                    if (Vector3.Distance(node.transform.position, raycastHittototo.point) <= currentMaxDistance / 2)
                                    {
                                        canPlace = false;
                                        break;
                                    }
                                }
                                if (canPlace)
                                {
                                    AddNode(targetType, raycastHittototo.point);
                                }
                            }
                            /*else
                            {
                                foreach (MapNode node in cachedMapNodeList)
                                {
                                    if (Vector3.Distance(node.transform.position, future4) <= currentMaxDistance)
                                    {
                                        canPlace = false;
                                        break;
                                    }
                                }
                                if (canPlace)
                                {
                                    AddNode(targetType, future4);
                                }
                            }*/
                        }
                    }
                }
            }
        }

        private void DeleteNearestNode(Vector3 _currentHitInfo)
        {
            float minDist = Mathf.Infinity;
            MapNode nearestNode = null;
            foreach (var mapNode in targetType.GetNodes())
            {
                float dist = Vector3.Distance(mapNode.transform.position, _currentHitInfo);
                if (dist < minDist)
                {
                    nearestNode = mapNode;
                    minDist = dist;
                }
            }

            if (nearestNode)
            {
                DestroyImmediate(nearestNode.gameObject);
            }
        }

        private void AddNode(MapNodeGroup targetType, Vector3 _currentHitInfo)
        {
            GameObject node = targetType.AddNode(_currentHitInfo);

            if (_parentObject)
            {
                node.transform.parent = _parentObject.transform;
            }

            MapNode mapNode = node.GetComponent<MapNode>();
            mapNode.gateName = _gateName;
            mapNode.forbiddenHulls = _forbiddenHulls;
            mapNode.flags = _nodeFlags;

            Texture2D icon = null;
            switch (targetType.graphType)
            {
                case MapNodeGroup.GraphType.Air:
                    icon = (Texture2D)EditorGUIUtility.IconContent("sv_icon_dot10_pix16_gizmo").image;
                    break;
                case MapNodeGroup.GraphType.Ground:
                    icon = (Texture2D)EditorGUIUtility.IconContent("sv_icon_dot11_pix16_gizmo").image;
                    break;
                case MapNodeGroup.GraphType.Rail:
                    icon = (Texture2D)EditorGUIUtility.IconContent("sv_icon_dot15_pix16_gizmo").image;
                    break;
            }
            EditorGUIUtility.SetIconForObject(node, icon);
        }
    }
}

