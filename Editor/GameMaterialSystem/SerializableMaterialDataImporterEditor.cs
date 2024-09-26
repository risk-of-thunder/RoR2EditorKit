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
            base.OnDisable();
            if (!inspector) return;
            inspector = null;
        }
        protected override void OnHeaderGUI()
        {
            if (!inspector) return;
            inspector.DrawHeader();
        }
        public override void OnInspectorGUI()
        {
            ApplyRevertGUI();
            if (!inspector) return;
            inspector.OnInspectorGUI();
        }
    }
}