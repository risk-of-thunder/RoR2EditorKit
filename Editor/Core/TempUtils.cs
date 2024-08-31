using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    internal class TestUtil
    {
        [UnityEditor.MenuItem("Tools/RoR2EK/Reload Domain")]
        public static void ReloadDomain()
        {
            EditorUtility.RequestScriptReload();
        }
    }
}