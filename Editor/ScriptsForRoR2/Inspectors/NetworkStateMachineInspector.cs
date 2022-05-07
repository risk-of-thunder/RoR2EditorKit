using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RoR2;
using RoR2EditorKit.Core.Inspectors;
using RoR2EditorKit.Utilities;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    //Remove foldout of array, Set element's name to ESM's custom name
    [CustomEditor(typeof(NetworkStateMachine))]
    public sealed class NetworkStateMachineInspector : ComponentInspector<NetworkStateMachine>
    {
        protected override void DrawInspectorGUI() { }
    }
}
