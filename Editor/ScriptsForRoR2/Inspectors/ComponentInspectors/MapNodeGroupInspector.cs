using RoR2;
using RoR2.Navigation;
using RoR2EditorKit.Core.Inspectors;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Cursor = UnityEngine.Cursor;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(MapNodeGroup))]
    public class MapNodeGroupInspector : ComponentInspector<MapNodeGroup>
    {
        private Vector3 currentHitInfo = default;
        private static Vector3 offsetUpVector = new Vector3(0, 15, 0);

        private VisualElement inspectorData;
        private PropertyValidator<UnityEngine.Object> nodeGraphValidator;

        protected override void OnEnable()
        {
            base.OnEnable();
            OnVisualTreeCopy += () =>
            {
                inspectorData = DrawInspectorElement.Q<VisualElement>("InspectorDataContainer");
                nodeGraphValidator = new PropertyValidator<Object>(inspectorData.Q<PropertyField>("nodeGraph"), DrawInspectorElement);
            };
        }
        protected override void DrawInspectorGUI()
        {
            SetupValidator();
            nodeGraphValidator.ForceValidation();
        }

        private void SetupValidator()
        {
            nodeGraphValidator.AddValidator(() =>
            {
                var nodeGraph = GetNodeGraph();
                return !nodeGraph;
            },
            $"Cannot display node placing utilities without a NodeGraph asset!");

            NodeGraph GetNodeGraph() => nodeGraphValidator.ChangeEvent == null ? TargetType.nodeGraph : (NodeGraph)nodeGraphValidator.ChangeEvent.newValue;
        }

        private void OnSceneGUI()
        {
            if(InspectorEnabled && TargetType.nodeGraph)
            {
                MapNodeGroup mapNodeGroup = (MapNodeGroup)target;

                Cursor.visible = true;

                // You'll need a control id to avoid messing with other tools!
                int controlID = GUIUtility.GetControlID(FocusType.Keyboard | FocusType.Passive);

                if (Event.current.GetTypeForControl(controlID) == EventType.KeyDown)
                {
                    if (Event.current.keyCode == KeyCode.B)
                    {
                        Cursor.visible ^= true;
                    }

                    if (Event.current.keyCode == KeyCode.N)
                    {
                        Debug.Log(currentHitInfo);
                        mapNodeGroup.AddNode(currentHitInfo);
                        // Causes repaint & accepts event has been handled
                        Event.current.Use();
                    }
                }

                var mapNodes = mapNodeGroup.GetNodes();

                Vector2 guiPosition = Event.current.mousePosition;
                Ray ray = HandleUtility.GUIPointToWorldRay(guiPosition);
                if (Physics.Raycast(ray, out var hitInfo, 99999999999f, LayerIndex.CommonMasks.bullet, QueryTriggerInteraction.Collide))
                {
                    currentHitInfo = mapNodeGroup.graphType == MapNodeGroup.GraphType.Air ? hitInfo.point + offsetUpVector : hitInfo.point;
                    if (mapNodes.Count > 0)
                    {
                        var inRange = false;

                        foreach (var mapNode in mapNodes)
                        {
                            if (Vector3.Distance(mapNode.transform.position, currentHitInfo) <= MapNode.maxConnectionDistance)
                            {
                                Handles.color = Color.yellow;
                                Handles.DrawLine(mapNode.transform.position, currentHitInfo);
                                inRange = true;
                            }
                        }

                        if (inRange)
                        {
                            Handles.color = Color.yellow;
                        }
                        else
                        {
                            Handles.color = Color.red;
                        }
                    }
                    else
                    {
                        Handles.color = Color.yellow;
                    }

                    Handles.CylinderHandleCap(controlID, currentHitInfo, Quaternion.Euler(90, 0, 0), 1, EventType.Repaint);
                }

                foreach (var mapNode in mapNodes)
                {
                    Handles.color = Color.green;

                    Handles.CylinderHandleCap(controlID, mapNode.transform.position, Quaternion.Euler(90, 0, 0), 1, EventType.Repaint);

                    Handles.color = Color.magenta;
                    foreach (var link in mapNode.links)
                    {
                        Handles.DrawLine(mapNode.transform.position, link.nodeB.transform.position);
                    }
                }

                Handles.BeginGUI();

                EditorGUILayout.BeginVertical();

                EditorGUILayout.LabelField($"Camera Position: {Camera.current.transform.position}");
                EditorGUILayout.LabelField($"Press N to add map node at cursor position (raycast)");

                if (GUILayout.Button("Clear"))
                {
                    mapNodeGroup.Clear();
                }

                if (GUILayout.Button("Add Map Node at current camera position"))
                {
                    var position = Camera.current.transform.position;
                    mapNodeGroup.AddNode(position);
                }

                if (GUILayout.Button("Update No Ceiling Masks"))
                {
                    mapNodeGroup.UpdateNoCeilingMasks();
                }

                if (GUILayout.Button("Update Teleporter Masks"))
                {
                    mapNodeGroup.UpdateTeleporterMasks();
                }

                if (GUILayout.Button("Bake Node Graph"))
                {
                    EditorUtility.SetDirty(mapNodeGroup.nodeGraph);
                    mapNodeGroup.Bake(mapNodeGroup.nodeGraph);
                    AssetDatabase.SaveAssets();
                }

                EditorGUILayout.EndVertical();

                Handles.EndGUI();
            }
        }
    }
}