namespace RoR2.Editor
{
    public abstract class ObjectEditingEditorWindow<TObject> : ExtendedEditorWindow where TObject : UnityEngine.Object
    {
        protected TObject targetType => serializedObject.targetObject as TObject;

        protected override void OnDisable()
        {
            base.OnDisable();
            if(serializedObject != null && serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}