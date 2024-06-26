﻿using RoR2;
using RoR2.Navigation;
using RoR2EditorKit.Inspectors;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;
using Object = UnityEngine.Object;
using RoR2EditorKit.VisualElements;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(MapNodeGroup))]
    public sealed class MapNodeGroupInspector : ComponentInspector<MapNodeGroup>
    {
        private Vector3 currentHitInfo = default;
        private static Vector3 offsetUpVector = new Vector3(0, 15, 0);

        private HullMask forbiddenHulls = HullMask.None;
        private NodeFlags nodeFlags = NodeFlags.None;
        private string gateName = String.Empty;
        private bool drawAll = false;
        private bool usePainter = false;
        private float currentPainterSize = 0;

        private VisualElement inspectorData;
        private ValidatingPropertyField nodeGraphValidator;
        private VisualElement nodePlacerContainers;

        private MethodInfo setIconMethod;
        private List<MapNode> cachedMapNodeList = new List<MapNode>();
        private float maxDistance = 0;
        private Transform parentGameObject;

        protected override void OnEnable()
        {
            base.OnEnable();
            var egu = typeof(EditorGUIUtility);
            var flags = BindingFlags.InvokeMethod | BindingFlags.Static | BindingFlags.NonPublic;
            setIconMethod = egu.GetMethod("SetIconForObject", flags, null, new Type[] { typeof(UnityEngine.Object), typeof(Texture2D) }, null);
            OnVisualTreeCopy += () =>
            {
                inspectorData = DrawInspectorElement.Q<VisualElement>("InspectorDataContainer");
                nodeGraphValidator = inspectorData.Q<ValidatingPropertyField>("nodeGraph");
                nodePlacerContainers = inspectorData.Q<VisualElement>("NodePlacerContainers");

                EnumFlagsField hullMaskFlags = new EnumFlagsField(HullMask.None);
                hullMaskFlags.label = "Forbidden Hulls";
                hullMaskFlags.RegisterValueChangedCallback(evt => forbiddenHulls = (HullMask)evt.newValue);
                nodePlacerContainers.Insert(0, hullMaskFlags);
                hullMaskFlags.ConnectWithSetting(EditorSettings, "ForbiddenHullMask", forbiddenHulls);

                EnumFlagsField nodeFlagsField = new EnumFlagsField(NodeFlags.None);
                nodeFlagsField.label = "Flags";
                nodeFlagsField.RegisterValueChangedCallback(evt => nodeFlags = (NodeFlags)evt.newValue);
                nodePlacerContainers.Insert(1, nodeFlagsField);
                nodeFlagsField.ConnectWithSetting(EditorSettings, "NodeFlags", nodeFlags);

                ObjectField parentField = nodePlacerContainers.Q<ObjectField>("parentGameObject");
                parentField.SetObjectType<Transform>();
                parentField.RegisterValueChangedCallback((evt) => parentGameObject = evt.newValue as Transform);
            };
        }
        protected override void DrawInspectorGUI()
        {
            SetupValidator();
            nodeGraphValidator.ForceValidation();

            var gateNameField = nodePlacerContainers.Q<TextField>("gateName");
            gateNameField.RegisterValueChangedCallback(evt =>
            {
                gateName = evt.newValue;
            });
            gateNameField.ConnectWithSetting(EditorSettings, "GateName", string.Empty);

            var toggle = nodePlacerContainers.Q<Toggle>("drawAll");
            toggle.RegisterValueChangedCallback(evt =>
            {
                drawAll = evt.newValue;
            });
            toggle.ConnectWithSetting(EditorSettings, "DrawAllNodes", false);


            var painterSize = nodePlacerContainers.Q<FloatField>("painterSize");
            toggle = nodePlacerContainers.Q<Toggle>("usePainter");
            toggle.RegisterValueChangedCallback(evt =>
            {
                usePainter = evt.newValue;
                painterSize.SetDisplay(usePainter);
            });
            toggle.ConnectWithSetting(EditorSettings, "UseNodePainter", false);

            painterSize.RegisterValueChangedCallback(evt =>
            {
                currentPainterSize = Mathf.Abs(evt.newValue);
                painterSize.value = Mathf.Abs(evt.newValue);
            });
            painterSize.SetDisplay(toggle.value);
            painterSize.ConnectWithSetting(EditorSettings, "NodePainterSize", 5);

            var container = nodePlacerContainers.Q<VisualElement>("ButtonContainer1");
            container.Q<Button>("clearNodes").clicked += ClearNodes;
            container.Q<Button>("updateNoCeilingMasks").clicked += UpdateNoCeilingMasks;

            container = nodePlacerContainers.Q<VisualElement>("ButtonContainer2");
            container.Q<Button>("updateTeleporterMasks").clicked += UpdateTeleporterMasks;
            container.Q<Button>("bakeNodeGraph").clicked += BakeNodeGraph;

            container = nodePlacerContainers.Q<VisualElement>("ButtonContainer3");
            container.Q<Button>("updateHullMasks").clicked += UpdateHullMask;
            container.Q<Button>("removeNodeExcess").clicked += RemoveNodeExcess;

            container = nodePlacerContainers.Q<VisualElement>("ButtonContainer4");
            container.Q<Button>("makeAirNodes").clicked += MakeAirNodes;
            container.Q<Button>("makeGroundNodes").clicked += MakeGroundNodes;
        }

        private void SetupValidator()
        {
            nodeGraphValidator.AddValidator(() =>
            {
                var nodeGraph = GetNodeGraph();
                nodePlacerContainers.SetDisplay(nodeGraph);
                return !nodeGraph;
            },
            $"Cannot display node placing utilities without a NodeGraph asset!");

            NodeGraph GetNodeGraph()
            {
                try
                {
                    if(nodeGraphValidator.ChangeEvent == null)
                    {
                        return TargetType.nodeGraph;
                    }
                    else if(nodeGraphValidator.ChangeEvent.newValue == null)
                    {
                        return null;
                    }
                    if((UnityEngine.Object)nodeGraphValidator.ChangeEvent.newValue)
                    {
                        return (NodeGraph)nodeGraphValidator.ChangeEvent.newValue;
                    }
                    return null;
                }
                catch(InvalidCastException ex)
                {
                    /*gulp
                     * Note to self, i have no idea how to fix it, and i dont care.
                     */
                    return null;
                }
            }
        }

        private void OnSceneGUI()
        {
            if (InspectorEnabled && TargetType.nodeGraph)
            {
                float nodeCylinderSize = EditorSettings.GetSetting("NodeCylinderSize", 1f);
                Cursor.visible = true;

                // You'll need a control id to avoid messing with other tools!
                int controlID = GUIUtility.GetControlID(FocusType.Keyboard | FocusType.Passive);
                cachedMapNodeList = TargetType.GetNodes();
                maxDistance = TargetType.graphType == MapNodeGroup.GraphType.Air ? (MapNode.maxConnectionDistance * 2) - 2 : MapNode.maxConnectionDistance;
                float zPainterOffset = maxDistance / 2;

                if (Event.current.GetTypeForControl(controlID) == EventType.KeyDown)
                {
                    //Paint or add node
                    if (Event.current.keyCode == KeyCode.B)
                    {
                        if (usePainter)
                        {
                            Painter(maxDistance, zPainterOffset, cachedMapNodeList);
                        }
                        else
                        {
                            AddNode(TargetType, currentHitInfo);
                        }
                        Event.current.Use();
                    }
                    //Delete nearest
                    if (Event.current.keyCode == KeyCode.M)
                    {
                        DeleteNearestNode(currentHitInfo);
                        Event.current.Use();
                    }
                    //Add on cam pos
                    if(Event.current.keyCode == KeyCode.V)
                    {
                        AddNode(TargetType, Camera.current.transform.position);
                        Event.current.Use();
                    }
                }

                Vector2 guiPosition = Event.current.mousePosition;
                Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);

                var length = cachedMapNodeList.Count;
                var currentNodeList = cachedMapNodeList;
                var rotation = Quaternion.Euler(90, 0, 0);

                LayerMask mask = EditorSettings.GetSetting("RaycastMask", LayerIndex.CommonMasks.bullet);
                if (Physics.Raycast(ray, out var hitInfo, 99999999999f, mask, QueryTriggerInteraction.Collide))
                {
                    currentHitInfo = hitInfo.point;
                    if(EditorSettings.GetSetting("RoundHitPositionToNearestGrid", false))
                    {
                        currentHitInfo = EditorMath.RoundToNearestGrid(currentHitInfo);
                    }
                    currentHitInfo += TargetType.graphType == MapNodeGroup.GraphType.Air ? offsetUpVector : Vector3.zero;
                    if (cachedMapNodeList.Count > 0)
                    {
                        var inRange = false;

                        foreach (var mapNode in cachedMapNodeList)
                        {
                            if (mapNode && Vector3.Distance(mapNode.transform.position, currentHitInfo) <= MapNode.maxConnectionDistance)
                            {
                                Handles.color = EditorSettings.GetSetting("LinkPreviewColor", Color.yellow);
                                Handles.DrawLine(mapNode.transform.position, currentHitInfo);
                                inRange = true;
                            }
                        }

                        if (inRange)
                        {
                            Handles.color = EditorSettings.GetSetting("PreviewNodeInRangeColor", Color.yellow);
                        }
                        else
                        {
                            Handles.color = EditorSettings.GetSetting("PreviewNodeOutOfRangeColor", Color.red);
                        }
                    }
                    else
                    {
                        Handles.color = EditorSettings.GetSetting("PreviewNodeInRangeColor", Color.yellow);
                    }


                    Handles.CylinderHandleCap(controlID, currentHitInfo, Quaternion.Euler(90, 0, 0), nodeCylinderSize, EventType.Repaint);

                    if (usePainter)
                    {
                        if (currentPainterSize <= 0)
                        {
                            Handles.CylinderHandleCap(controlID, currentHitInfo, Quaternion.Euler(90, 0, 0), nodeCylinderSize, EventType.Repaint);
                        }
                        else
                        {
                            Handles.CircleHandleCap(controlID, currentHitInfo, Quaternion.Euler(90, 0, 0), currentPainterSize, EventType.Repaint);
                        }
                    }
                }
                for (int i = 0; i < length; i++)
                {
                    var mapNode = currentNodeList[i];
                    if (!mapNode || (!drawAll && (mapNode.transform.position - ray.origin).sqrMagnitude > 22500))
                    {
                        continue;
                    }

                    if ((this.nodeFlags == NodeFlags.None || (mapNode.flags & this.nodeFlags) != NodeFlags.None) && (this.forbiddenHulls == HullMask.None || (mapNode.forbiddenHulls & this.forbiddenHulls) == this.forbiddenHulls))
                    {
                        if (mapNode.links.Count <= 0)
                        {
                            Handles.color = EditorSettings.GetSetting("PlacedNodeWithNoLinksColor", (Color)new Color32
                            {
                                a = 255,
                                r = 255,
                                g = 165,
                            });
                        }
                        else
                        {
                            Handles.color = EditorSettings.GetSetting("BakedNodeColor", Color.green);
                        }
                    }
                    else
                    {
                        Handles.color = EditorSettings.GetSetting("InvalidPlacedNode", Color.black);
                    }

                    Handles.CylinderHandleCap(controlID, mapNode.transform.position, rotation, nodeCylinderSize, EventType.Repaint);

                    Handles.color = EditorSettings.GetSetting("BakedLinkColor", Color.magenta);
                    foreach (var link in mapNode.links)
                    {
                        if (link.nodeB)
                            Handles.DrawLine(mapNode.transform.position, link.nodeB.transform.position);
                    }
                }

                Handles.BeginGUI();

                EditorGUILayout.BeginVertical("box", GUILayout.Width(400));
                EditorGUILayout.BeginVertical("box", GUILayout.Width(400));
                EditorGUILayout.BeginVertical("box", GUILayout.Width(400));

                EditorGUILayout.LabelField($"Camera Position: {Camera.current.transform.position}", EditorStyles.boldLabel);
                var placerString = "Press B to ";
                placerString += usePainter ? "paint nodes at current mouse position (raycast)" : "add map node at current mouse position (raycast)";
                EditorGUILayout.LabelField(placerString, EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Press V to add map node at current camera position", EditorStyles.boldLabel);
                EditorGUILayout.LabelField("Press M to remove the nearest map node at cursor position", EditorStyles.boldLabel);

                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();

                Handles.EndGUI();
            }
        }

        private void ClearNodes()
        {
            if (EditorUtility.DisplayDialog("WARNING: Clear All Nodes", "Clicking this button will delete EVERY node. Are you sure you want to do this? (YOU CANNOT UNDO THIS OPERATION)", "Yes, Im sure", "No, Take me back"))
            {
                TargetType.Clear();
            }
        }

        private void UpdateHullMask()
        {
            foreach (MapNode node in cachedMapNodeList)
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

        private void UpdateNoCeilingMasks()
        {
            TargetType.UpdateNoCeilingMasks();
        }

        private void UpdateTeleporterMasks()
        {
            TargetType.UpdateTeleporterMasks();
        }

        private void BakeNodeGraph()
        {
            EditorUtility.SetDirty(TargetType.nodeGraph);
            TargetType.Bake(TargetType.nodeGraph);
            AssetDatabase.SaveAssets();
        }

        private void RemoveNodeExcess()
        {
            EditorUtility.SetDirty(TargetType.nodeGraph);
            int c = 0;
            foreach (MapNode mapNode in cachedMapNodeList)
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
                            if ((Vector3.Distance(mapNode.links[i].nodeB.gameObject.transform.position, mapNode.gameObject.transform.position) <= maxDistance / 1.5))
                            {
                                DestroyImmediate(mapNode.gameObject);
                                c++;
                                break;
                            }
                            //It's not way too close and the link has some value
                            //Make sure the buffer does not contain too many links. More than three should ensure it is connected to other two nodes, as nodes connect back to their linkee
                            //if (buffer.Count < 4 && mapNode.links[i].nodeB.links.Count > 3)
                            //{
                            //    buffer.Add(mapNode.links[i]);
                            //}
                        }
                    }
                    //Assign the new link list, whenever it is empty or not.
                    //mapNode.links = buffer;
                }
            }
            //Go again through each map node to make sure there's no empty nodes after assigning the new buffer
            //foreach (MapNode mapNode in cachedMapNodeList)
            //{
            //    if (mapNode)
            //    {
            //        //Save if its not linking back to the linkee
            //        if (mapNode.links.Count == 1 && mapNode.links[0].nodeB.links.Count == 1 && mapNode.links[0].nodeB.links[0].nodeB != mapNode)
            //        {
            //            continue;
            //        }
            //        //Destroy instantly as it either has no links or its just linking back to the linkee
            //        if (mapNode.links.Count <= 1)
            //        {
            //            //Save as the link isnt with the linker
            //            DestroyImmediate(mapNode.gameObject);
            //            c++;
            //        }
            //    }
            //}
            Debug.Log($"Removed {c} nodes that were way too close to others.");
            AssetDatabase.SaveAssets();
        }

        private void MakeAirNodes()
        {
            foreach (MapNode node in cachedMapNodeList)
            {
                LayerMask mask = EditorSettings.GetSetting("RaycastMask", LayerIndex.CommonMasks.bullet);
                RaycastHit hit;
                Vector3 newPos = node.transform.position;
                if (Physics.Raycast(node.transform.position, Vector3.up, out hit, offsetUpVector.magnitude, mask, QueryTriggerInteraction.Collide))
                {
                    newPos += (hit.point - newPos) / 2;
                }
                else
                {
                    newPos += offsetUpVector;
                }
                node.transform.position = EditorSettings.GetSetting("RoundHitPositionToNearestGrid", false) ? EditorMath.RoundToNearestGrid(newPos) : newPos;
            }
        }

        private void MakeGroundNodes()
        {
            foreach (MapNode node in cachedMapNodeList)
            {
                LayerMask mask = EditorSettings.GetSetting("RaycastMask", LayerIndex.CommonMasks.bullet);
                RaycastHit hit;
                Vector3 newPos = node.transform.position;
                if (Physics.Raycast(node.transform.position, Vector3.down, out hit, offsetUpVector.magnitude * 2, mask, QueryTriggerInteraction.Collide))
                {
                    newPos = hit.point;
                }
                else
                {
                    newPos -= offsetUpVector;
                }
                node.transform.position = EditorSettings.GetSetting("RoundHitPositionToNearestGrid", false) ? EditorMath.RoundToNearestGrid(newPos) : newPos;
            }
        }

        private void Painter(float currentMaxDistance, float zPainterOffset, List<MapNode> cachedMapNodeList)
        {
            LayerMask mask = EditorSettings.GetSetting("RaycastMask", LayerIndex.CommonMasks.bullet);
            for (float x = currentHitInfo.x - currentPainterSize, zCount = 0; x <= currentHitInfo.x; x += currentMaxDistance - 4, zCount++)
            {
                for (float z = currentHitInfo.z - currentPainterSize; z <= currentHitInfo.z; z += currentMaxDistance - 4)
                {
                    //Haven't found a single node that is too close, feel free to spawn.
                    //We lift the pos in case terrain is not flat...
                    //We raycast to ground
                    if ((x - currentHitInfo.x) * (x - currentHitInfo.x) + (z - currentHitInfo.z) * (z - currentHitInfo.z) <= currentPainterSize * currentPainterSize)
                    {
                        float xSym = currentHitInfo.x - (x - currentHitInfo.x);
                        float zSym = currentHitInfo.z - (z - currentHitInfo.z);

                        float offsetY = TargetType.graphType == MapNodeGroup.GraphType.Air ? 0 : 6;
                        Vector3 future1 = (int)zCount % 2 == 0 ? new Vector3(x, currentHitInfo.y + offsetY, z + zPainterOffset) : new Vector3(x, currentHitInfo.y + offsetY, z);
                        Vector3 future2 = (int)zCount % 2 == 0 ? new Vector3(x, currentHitInfo.y + offsetY, zSym + zPainterOffset) : new Vector3(x, currentHitInfo.y + offsetY, zSym);
                        Vector3 future3 = (int)zCount % 2 == 0 ? new Vector3(xSym, currentHitInfo.y + offsetY, z + zPainterOffset) : new Vector3(xSym, currentHitInfo.y + offsetY, z);
                        Vector3 future4 = (int)zCount % 2 == 0 ? new Vector3(xSym, currentHitInfo.y + offsetY, zSym + zPainterOffset) : new Vector3(xSym, currentHitInfo.y + offsetY, zSym);

                        if (!Physics.Linecast(currentHitInfo, future1, out _, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                        {
                            bool canPlace = true;
                            if (TargetType.graphType == MapNodeGroup.GraphType.Ground && Physics.Raycast(future1, Vector3.down, out RaycastHit raycastHit, 30, mask, QueryTriggerInteraction.Collide))
                            {
                                var point = raycastHit.point;
                                foreach (MapNode node in cachedMapNodeList)
                                {
                                    point = EditorSettings.GetSetting("RoundHitPositionToNearestGrid", false) ? EditorMath.RoundToNearestGrid(point) : point;
                                    if (Vector3.Distance(node.transform.position, point) <= currentMaxDistance)
                                    {
                                        canPlace = false;
                                        break;
                                    }
                                }
                                if (canPlace)
                                {
                                    AddNode(TargetType, point);
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
                                    AddNode(TargetType, future1);
                                }
                            }*/
                        }
                        if (!Physics.Linecast(currentHitInfo, future2, out _, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                        {
                            bool canPlace = true;
                            if (TargetType.graphType == MapNodeGroup.GraphType.Ground && Physics.Raycast(future2, Vector3.down, out RaycastHit raycastHit, 30, mask, QueryTriggerInteraction.Collide))
                            {
                                var point = raycastHit.point;
                                foreach (MapNode node in cachedMapNodeList)
                                {
                                    point = EditorSettings.GetSetting("RoundHitPositionToNearestGrid", false) ? EditorMath.RoundToNearestGrid(point) : point;
                                    if (Vector3.Distance(node.transform.position, point) <= currentMaxDistance)
                                    {
                                        canPlace = false;
                                        break;
                                    }
                                }
                                if (canPlace)
                                {
                                    AddNode(TargetType, point);
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
                                    AddNode(TargetType, future2);
                                }
                            }*/
                        }
                        if (!Physics.Linecast(currentHitInfo, future3, out _, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                        {
                            bool canPlace = true;
                            if (TargetType.graphType == MapNodeGroup.GraphType.Ground && Physics.Raycast(future3, Vector3.down, out RaycastHit raycastHit, 30, mask, QueryTriggerInteraction.Collide))
                            {
                                var point = raycastHit.point;
                                foreach (MapNode node in cachedMapNodeList)
                                {
                                    point = EditorSettings.GetSetting("RoundHitPositionToNearestGrid", false) ? EditorMath.RoundToNearestGrid(point) : point;
                                    if (Vector3.Distance(node.transform.position, point) <= currentMaxDistance)
                                    {
                                        canPlace = false;
                                        break;
                                    }
                                }
                                if (canPlace)
                                {
                                    AddNode(TargetType, point);
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
                                    AddNode(TargetType, future3);
                                }
                            }*/
                        }
                        if (!Physics.Linecast(currentHitInfo, future4, out _, LayerIndex.world.mask, QueryTriggerInteraction.Ignore))
                        {
                            bool canPlace = true;
                            if (TargetType.graphType == MapNodeGroup.GraphType.Ground && Physics.Raycast(future4, Vector3.down, out RaycastHit raycastHit, 30, mask, QueryTriggerInteraction.Collide))
                            {
                                var point = raycastHit.point;
                                foreach (MapNode node in cachedMapNodeList)
                                {
                                    point = EditorSettings.GetSetting           ("RoundHitPositionToNearestGrid", false) ? EditorMath.RoundToNearestGrid(point) : point;
                                    if (Vector3.Distance(node.transform.position, raycastHit.point) <= currentMaxDistance)
                                    {
                                        canPlace = false;
                                        break;
                                    }
                                }
                                if (canPlace)
                                {
                                    AddNode(TargetType, raycastHit.point);
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
                                    AddNode(TargetType, future4);
                                }
                            }*/
                        }
                    }
                }
            }
        }

        private void AddNode(MapNodeGroup group, Vector3 pos)
        {
            GameObject node = group.AddNode(pos);

            if (parentGameObject)
            {
                node.transform.parent = parentGameObject;
            }

            MapNode mapNode = node.GetComponent<MapNode>();
            mapNode.gateName = gateName;
            mapNode.forbiddenHulls = forbiddenHulls;
            mapNode.flags = nodeFlags;

            Texture2D icon = null;
            switch (group.graphType)
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
            setIconMethod.Invoke(null, new object[] { node, icon });
        }

        private void DeleteNearestNode(Vector3 hitInfo)
        {
            float minDist = Mathf.Infinity;
            MapNode nearestNode = null;
            foreach (var mapNode in TargetType.GetNodes())
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
    }
}