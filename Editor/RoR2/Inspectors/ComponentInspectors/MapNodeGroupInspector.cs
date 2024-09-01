using RoR2.Navigation;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using System;
using System.Collections.Generic;

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
        private bool _roundHitPointToNearestGrid;
        private bool _drawallNodes;
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

        protected override void OnInspectorEnabled()
        {
            base.OnInspectorEnabled();
            _parentObject = targetType.transform.Find(parentGameObjectName)?.gameObject ?? null;
            _parentObjectField.SetValueWithoutNotify(_parentObject);
            _offsetUpVector = inspectorProjectSettings.GetOrCreateSetting(nameof(_offsetUpVector), new Vector3(0, 15, 0));
            _raycastMask = inspectorProjectSettings.GetOrCreateSetting(nameof(_raycastMask), LayerIndex.CommonMasks.bullet);

            _nodeCylinderSize = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_nodeCylinderSize), 1f);
            _addNodeKeyCode = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_addNodeKeyCode), KeyCode.B);
            _deleteNearestKeyCode = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_deleteNearestKeyCode), KeyCode.M);
            _addOnCamPosKeyCode = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_addOnCamPosKeyCode), KeyCode.V);
            _linkPreviewColor = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_linkPreviewColor), Color.yellow);
            _previewNodeInRangeColor = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_previewNodeInRangeColor), Color.yellow);
            _previewNodeOutOfRangeColor = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_previewNodeOutOfRangeColor), Color.red);
            _placedNodeWithNoLinksColor = inspectorPreferenceSettings.GetOrCreateSetting(nameof(_placedNodeWithNoLinksColor), (Color) new Color32
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

            _nodePlacerContainer = templateInstanceRoot.Q<VisualElement>("NodePlacerContainer");

            var property = serializedObject.FindProperty("nodeGraph");
            var pField = templateInstanceRoot.Q<PropertyField>("NodeGraph");
            pField.TrackPropertyValue(property, OnNodeGraphChange);
            OnNodeGraphChange(property);

            _parentObjectField = templateInstanceRoot.Q<ObjectField>("ParentGameObject");
            _parentObjectField.RegisterValueChangedCallback(UpdateParentGameObjectSetting);

            var drawAllNodes = templateInstanceRoot.Q<Toggle>("DrawAllNodes");
            drawAllNodes.RegisterValueChangedCallback(evt => _drawallNodes = evt.newValue);
            drawAllNodes.ConnectWithSetting(inspectorPreferenceSettings, nameof(drawAllNodes), false);

            var roundHitPointToNearestGrid = templateInstanceRoot.Q<Toggle>("RoundPointToGrid");
            roundHitPointToNearestGrid.RegisterValueChangedCallback(evt => _roundHitPointToNearestGrid = evt.newValue);
            roundHitPointToNearestGrid.ConnectWithSetting(inspectorPreferenceSettings, nameof(roundHitPointToNearestGrid), false);

            var gateName = templateInstanceRoot.Q<TextField>("GateName");
            gateName.RegisterValueChangedCallback(evt => _gateName = evt.newValue);
            gateName.ConnectWithSetting(inspectorProjectSettings, nameof(gateName), "");

            templateInstanceRoot.Q<Button>("UpdateNoCeilingMasks").clicked += UpdateNoCeilingMasks;
            templateInstanceRoot.Q<Button>("ClearNodes").clicked += ClearNodes;
            templateInstanceRoot.Q<Button>("UpdateTeleporterMasks").clicked += UpdateTeleporterMasks;
            templateInstanceRoot.Q<Button>("BakeNodeGraph").clicked += BakeNodeGraph;
            templateInstanceRoot.Q<Button>("UpdateHullMasks").clicked += UpdateHullMasks;
            templateInstanceRoot.Q<Button>("RemoveNodeExcess").clicked += RemoveNodeExcess;
            templateInstanceRoot.Q<Button>("MakeAirNodes").clicked += MakeAirNodes;
            templateInstanceRoot.Q<Button>("MakeGroundNodes").clicked += MakeGroundNodes;
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
                    //List<MapNode.Link> buffer = new List<MapNode.Link>();
                    for (int i = 0; i < mapNode.links.Count; i++)
                    {
                        //Make sure the other node exists and hasn't been deleted before.
                        if (mapNode.links[i].nodeB)
                        {
                            //Too friccin close, get off
                            if ((Vector3.Distance(mapNode.links[i].nodeB.gameObject.transform.position, mapNode.gameObject.transform.position) <= _maxDistance / 1.5))
                            {
                                DestroyImmediate(mapNode.gameObject);
                                c++;
                                break;
                            }
                        }
                    }
                }
            }
            Debug.Log($"Removed {c} nodes that were way too close to others.");
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

        private void UpdateTeleporterMasks()
        {
            targetType.UpdateTeleporterMasks();
        }

        private void ClearNodes()
        {
            if(EditorUtility.DisplayDialog("WARNING: Clear All Nodes", "Clicking this button will delete EVERY node. Are you sure you want to do this? (YOU CANNOT UNDO THIS OPERATION)", "Yes, Im sure", "No, Take me back"))
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
            if(evt.newValue)
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
            if(inspectorEnabled && targetType.nodeGraph)
            {
                DrawPlacer();

                Handles.BeginGUI();
                EditorGUILayout.BeginVertical("box", GUILayout.Width(400));
                EditorGUILayout.BeginVertical("box", GUILayout.Width(400));
                EditorGUILayout.BeginVertical("box", GUILayout.Width(400));

                EditorGUILayout.LabelField($"Camera Position: {Camera.current.transform.position}", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Press {_addNodeKeyCode} to add a map node at the current mouse position (raycast)", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Press {_addOnCamPosKeyCode} to add a map node at the current camera's position", EditorStyles.boldLabel);
                EditorGUILayout.LabelField($"Press {_deleteNearestKeyCode} to delete the nearest map node at current mouse position", EditorStyles.boldLabel);

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                Handles.EndGUI();
            }
        }

        private void DrawPlacer()
        {
            UnityEngine.Cursor.visible = true;

            int controlID = GUIUtility.GetControlID(FocusType.Keyboard | FocusType.Passive);
            _cachedMapNodeList = targetType.GetNodes();
            _maxDistance = MapNode.maxConnectionDistance;

            if (Event.current.GetTypeForControl(controlID) == EventType.KeyDown)
            {
                var keyCode = Event.current.keyCode;
                if (keyCode == _addNodeKeyCode)
                {
                    AddNode(targetType, _currentHitInfo);
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

        private void DeleteNearestNode(Vector3 currentHitInfo)
        {
            float minDist = Mathf.Infinity;
            MapNode nearestNode = null;
            foreach (var mapNode in targetType.GetNodes())
            {
                float dist = Vector3.Distance(mapNode.transform.position, currentHitInfo);
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

        private void AddNode(MapNodeGroup targetType, Vector3 currentHitInfo)
        {
            GameObject node = targetType.AddNode(currentHitInfo);

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

