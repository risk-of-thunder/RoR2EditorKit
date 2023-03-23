using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoR2EditorKit.EditorWindows;
using RoR2EditorKit.VisualElements;
using UnityEditor;
using HG;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public class TypeSearcher : ExtendedEditorWindow
    {

        public SerializableSystemType baseClass;
        public string typeName;
        public bool allowAbstract;

        [MenuItem(Constants.RoR2EditorKitMenuRoot + "Type Searcher")]
        private static void OpenWindow()
        {
            var window = OpenEditorWindow<TypeSearcher>();
            window.Focus();
        }
    }
}