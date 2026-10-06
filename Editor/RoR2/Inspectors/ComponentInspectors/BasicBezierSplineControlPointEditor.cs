using System.Runtime.Remoting.Contexts;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(BasicBezierSplineControlPoint))]
    public sealed class BasicBezierSplineControlPointEditor : IMGUIComponentInspector<BasicBezierSplineControlPoint>
    {
        private struct ControlPointContext
        {
            public BasicBezierSplineControlPoint previous;
            public BasicBezierSplineControlPoint next;

            public bool hasPreviousAndNext => previous && next;

            public ControlPointContext(BasicBezierSplineControlPoint controlPoint)
            {
                next = FindNextControlPoint(controlPoint);
                previous = FindPreviousControlPoint(controlPoint);
            }

            private static bool TryFindCurve(BasicBezierSplineControlPoint controlPoint, out BasicBezierSpline curve)
            {
                return curve = controlPoint.GetComponentInParent<BasicBezierSpline>();
            }

            private static BasicBezierSplineControlPoint FindNextControlPoint(BasicBezierSplineControlPoint controlPoint) => FindControlPointByOffset(controlPoint, 1);

            private static BasicBezierSplineControlPoint FindPreviousControlPoint(BasicBezierSplineControlPoint controlPoint) => FindControlPointByOffset(controlPoint, -1);

            private static BasicBezierSplineControlPoint FindControlPointByOffset(BasicBezierSplineControlPoint controlPoint, int offset)
            {
                if (!TryFindCurve(controlPoint, out BasicBezierSpline curve))
                {
                    return null;
                }

                var controlPoints = curve.controlPoints;
                if (controlPoints == null)
                    return null;

                return HG.ArrayUtils.GetSafe(controlPoints, FindControlPointIndex(curve, controlPoint) + offset);
            }

            private static int FindControlPointIndex(BasicBezierSpline curve, BasicBezierSplineControlPoint controlPoint)
            {
                if (curve == null)
                    return -1;

                for (int i = 0; i < curve.controlPoints.Length; i++)
                {
                    if (curve.controlPoints[i] == controlPoint)
                    {
                        return i;
                    }
                }

                return -1;
            }
        }
        protected override void DrawIMGUI()
        {
            DrawDefaultInspector();

            ControlPointContext context = new ControlPointContext(targetType);
            using (new EditorGUILayout.HorizontalScope())
            {
                DrawSelectPrevious(context);
                DrawSelectNext(context);
            }

            DrawFlatten(context);
            DrawPointToNext(context);
            DrawPointFromPrevious(context);
        }

        private void DrawSelectPrevious(ControlPointContext context)
        {
            var previous = context.previous;
            using(new EditorGUI.DisabledGroupScope(previous == null))
            {
                if(GUILayout.Button("Select Previous"))
                {
                    Selection.SetActiveObjectWithContext(previous, null);
                }
            }
        }

        private void DrawSelectNext(ControlPointContext context)
        {
            var next = context.next;
            using (new EditorGUI.DisabledGroupScope(next == null))
            {
                if (GUILayout.Button("Select Next"))
                {
                    Selection.SetActiveObjectWithContext(next, null);
                }
            }
        }

        private void DrawFlatten(ControlPointContext context)
        {
            using(new EditorGUI.DisabledGroupScope(context.hasPreviousAndNext == false))
            {
                if(GUILayout.Button("Flatten"))
                {
                    Undo.RecordObject(targetType, "Flatten");

                    Vector3 direction = context.next.transform.position - context.previous.transform.position;
                    targetType.transform.forward = direction;
                }
            }
        }

        private void DrawPointToNext(ControlPointContext context)
        {
            var next = context.next;
            using(new EditorGUI.DisabledGroupScope(next == null))
            {
                if(GUILayout.Button("Point to Next"))
                {
                    Undo.RecordObject(targetType, "Point to Next");
                    targetType.transform.forward = next.transform.position - targetType.transform.position;
                }
            }
        }

        private void DrawPointFromPrevious(ControlPointContext context)
        {
            var previous = context.previous;
            using (new EditorGUI.DisabledGroupScope(previous == null))
            {
                if (GUILayout.Button("Point to Previous"))
                {
                    Undo.RecordObject(targetType, "Point to Previous");
                    targetType.transform.forward = previous.transform.position - targetType.transform.position;
                }
            }
        }
    }
}