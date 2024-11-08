using RoR2.ContentManagement;
using UnityEditor;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(SerializableContentPack), isFallback = false)]
    public class SerializableContentPackInspector : IMGUIScriptableObjectInspector<SerializableContentPack>
    {
        private ExtendedHelpBox _helpBox;
        protected override void OnEnable()
        {
            base.OnEnable();
            _helpBox = new ExtendedHelpBox("The vanilla SerializableContentPack is no longer supported as it lacks the new fields added to ContentPacks in the DLC Updates.\n\n" +
                "While RiskofThunder recommends utilizing the regular ContentPack's capabilities instead of a SerializableContentPack, one can also Subclass SerializableContentPack to add the new required fields.", MessageType.Info, true, false);


            onRootElementCleared += () =>
            {
                if (target is SerializableContentPack)
                {
                    rootVisualElement.Add(_helpBox);
                }
            };
        }

        protected override void DrawIMGUI()
        {
            DrawDefaultInspector();
        }
    }
}