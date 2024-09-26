using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEditorInternal;

namespace RoR2.Editor.GameMaterialSystem
{
    [CustomEditor(typeof(SerializableMaterialDataImporter))]
    public class SerializableMaterialDataImporterEditor : ScriptedImporterEditor
    {
        SerializedMaterialDataEditor inspector;
        public override void OnEnable()
        {
            base.OnEnable();
            var assetPath = AssetDatabase.GetAssetPath(target);
            var mainAsset = AssetDatabase.LoadMainAssetAtPath(assetPath);
            inspector = CreateEditor(mainAsset) as SerializedMaterialDataEditor;
            InternalEditorUtility.SetIsInspectorExpanded(mainAsset, true);
        }
        public override void OnDisable()
        {
            if (!inspector) return;
            inspector.draw = true;
            inspector = null;
            base.OnDisable();
        }
        protected override void OnHeaderGUI()
        {
            if (!inspector) return;
            inspector.draw = true;
            inspector.DrawHeader();
        }
        public override void OnInspectorGUI()
        {
            if (!inspector) return;
            inspector.OnInspectorGUI();
            inspector.draw = false;
        }
    }
}