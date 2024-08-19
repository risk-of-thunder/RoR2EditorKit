using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    [InitializeOnLoad]
    public abstract class ScriptableObjectInspector<T> : Inspector<T> where T : ScriptableObject
    {
        static ScriptableObjectInspector()
        {
            finishedDefaultHeaderGUI += DrawEnableToggle;
        }
        private static void DrawEnableToggle(UnityEditor.Editor obj)
        {
            if (obj is ScriptableObjectInspector<T> soInspector)
                soInspector.inspectorEnabled = EditorGUILayout.ToggleLeft($"Enable {ObjectNames.NicifyVariableName(soInspector.target.GetType().Name)} Inspector", soInspector.inspectorEnabled);
        }
    }
}