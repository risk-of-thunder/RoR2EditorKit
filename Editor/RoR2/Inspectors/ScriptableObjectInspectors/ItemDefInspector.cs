using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(ItemDef))]
    public class ItemDefInspector : IMGUIScriptableObjectInspector<ItemDef>
    {
        protected override void DrawIMGUI()
        {
            EditorGUILayout.LabelField("Custom UI goes here");
            DrawDefaultInspector();
        }
    }
}