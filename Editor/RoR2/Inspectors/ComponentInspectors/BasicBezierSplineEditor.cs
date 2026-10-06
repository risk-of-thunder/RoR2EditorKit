using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(BasicBezierSpline))]
    public sealed class BasicBezierSplineEditor : IMGUIComponentInspector<BasicBezierSpline>
    {
        protected override void DrawIMGUI()
        {
            DrawDefaultInspector();
            if(GUILayout.Button("Set control points from children"))
            {
                Undo.RecordObject(target, "Set control points from children");
                targetType.controlPoints = targetType.GetComponentsInChildren<BasicBezierSplineControlPoint>();
                EditorUtility.SetDirty(target);
            }
        }
    }
}