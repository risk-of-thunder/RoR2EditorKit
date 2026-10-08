using HG;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(ZiprailController))]
    public sealed class ZiprailInspector : IMGUIComponentInspector<ZiprailController>
    {
        protected override void DrawIMGUI()
        {
            var path = targetType.GetComponent<BasicBezierSpline>();

            DrawPropertiesExcluding(serializedObject, "_nodeVFXInstances", "m_Script");

            bool isNotEditable = Util.IsPrefab(targetType.gameObject) || PrefabStageUtility.GetCurrentPrefabStage().AsValidOrNull()?.prefabContentsRoot == targetType.gameObject;
            isNotEditable |= Util.IsPrefab(targetType.transform.root.gameObject);

            if(isNotEditable)
            {
                EditorGUILayout.LabelField("Cannot add points or bake when Ziprail is not a prefab instance.");
            }

            using(new EditorGUI.DisabledGroupScope(isNotEditable))
            {
                if(GUILayout.Button("Add Point"))
                {
                    AddPoint(path);
                }

                using (new GUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Bake Colliders"))
                    {
                        targetType.BakeColliders();
                    }
                    if (GUILayout.Button("Bake Visuals"))
                    {
                        var instancesProperty = serializedObject.FindProperty("_nodeVFXInstances");
                        for (int i = 0; i < instancesProperty.arraySize; i++)
                        {
                            DestroyImmediate(instancesProperty.GetArrayElementAtIndex(i).objectReferenceValue);
                        }

                        instancesProperty.arraySize = 0;
                        serializedObject.ApplyModifiedProperties();

                        targetType.BakeVisuals();
                    }
                }
            }

            EditorUtility.SetDirty(targetType);
            serializedObject.ApplyModifiedProperties();
        }

        private void AddPoint(BasicBezierSpline path)
        {
            if (!path)
            {
                Debug.LogError("Cannot add new spline point, the ZiprailController's path variable is invalid.");
                return;
            }
            if (!TryGetSplinePointContainer(path, out var pointContainer))
            {
                Debug.LogError("Cannot add new spline point, no splinePointContainer exists.");
                return;
            }
            if (!TryGetStartAndEndPoints(path, out _, out var endPoint))
            {
                Debug.LogError($"Cannot add new spline point, Either endPoint is null, startPoint is null, or both are null.");
                return;
            }

            var point = new GameObject("ZiprailPoint").transform;
            point.SetParent(pointContainer.transform);

            endPoint.transform.SetAsLastSibling();

            //Places this point before the end point.
            point.SetSiblingIndex(endPoint.transform.GetSiblingIndex() - 1);

            var previousPoint = pointContainer.transform.GetChild(point.GetSiblingIndex() - 1);

            point.transform.localPosition = Vector3.Lerp(previousPoint.localPosition, endPoint.transform.localPosition, 0.5f);

            var controlPoint = point.gameObject.AddComponent<BasicBezierSplineControlPoint>();
            controlPoint.backwardVelocity = Vector3.zero;
            controlPoint.forwardVelocity = Vector3.zero;

            Undo.RegisterCreatedObjectUndo(point.gameObject, "Add Point");

            Undo.RecordObject(targetType, "Add Point");
            Undo.RecordObject(path, "Add Point");

            path.controlPoints = pointContainer.GetComponentsInChildren<BasicBezierSplineControlPoint>();

            EditorUtility.SetDirty(path);
        }

        private bool TryGetStartAndEndPoints(BasicBezierSpline path, out BasicBezierSplineControlPoint startPoint, out BasicBezierSplineControlPoint endPoint)
        {

            if (path.controlPoints.Length < 2)
            {
                startPoint = null;
                endPoint = null;
                return false;
            }

            startPoint = path.startControlPoint;
            endPoint = path.endControlPoint;

            return startPoint && endPoint;
        }

        private bool TryGetSplinePointContainer(BasicBezierSpline path, out GameObject container)
        {
            if (path.controlPoints.Length < 0)
            {
                container = null;
                return false;
            }

            var first = HG.ArrayUtils.GetSafe(path.controlPoints, 0);
            container = first.AsValidOrNull()?.transform.parent.gameObject;
            return container;
        }
    }
}
