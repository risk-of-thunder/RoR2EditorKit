using RoR2EditorKit.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.UIElements;

namespace RoR2EditorKit.Inspectors
{
    [CustomEditor(typeof(R2EditorSettings))]
    internal class EditorSettingsInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawPropertiesExcluding(serializedObject, "_settings");
        }
    }
}
