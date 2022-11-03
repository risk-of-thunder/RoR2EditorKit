using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related
{
    internal class HitBoxGizmoDrawer
    {
        [DrawGizmo(GizmoType.Selected, typeof(HitBox))]
        private static void DrawGizmos(HitBox hitBox, GizmoType gizmoType)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(hitBox.transform.position, hitBox.transform.localScale);
        }
    }
}