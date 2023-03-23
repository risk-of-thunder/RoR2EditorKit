using RoR2;
using RoR2EditorKit.Inspectors;
using RoR2EditorKit.VisualElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace RoR2EditorKit.RoR2Related.Inspectors
{
    [CustomEditor(typeof(AssetCollection))]
    public sealed class AssetCollectionInspector : ScriptableObjectInspector<AssetCollection>
    {

        protected override void OnEnable()
        {
            base.OnEnable();
        }
        protected override void DrawInspectorGUI()
        {
            var extendedListView = DrawInspectorElement.Q<ExtendedListView>("extendedListView");
            extendedListView.BindElement = BindElement;
            extendedListView.CreateElement = () => new ObjectField();
            extendedListView.collectionProperty = serializedObject.FindProperty("assets");
        }

        private void BindElement(VisualElement arg1, SerializedProperty arg2)
        {
            ObjectField objField = arg1 as ObjectField;
            objField.SetObjectType<UnityEngine.Object>();
            objField.label = ObjectNames.NicifyVariableName(objField.name);
            objField.bindingPath = arg2.propertyPath;
        }
    }
}