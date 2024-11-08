namespace RoR2.Editor
{
    /// <summary>
    /// A Variation of the <see cref="ExtendedEditorWindow"/>, the <see cref="ObjectEditingEditorWindow{TObject}"/> allows you to create an EditorWindow that's exclusively used for editing an Object. this is specially useful when the inspector window is not big enough to accomodate for complex Objects.
    /// </summary>
    /// <typeparam name="TObject">The type of object the window is editing</typeparam>
    public abstract class ObjectEditingEditorWindow<TObject> : ExtendedEditorWindow where TObject : UnityEngine.Object
    {
        /// <summary>
        /// Direct access to the <see cref="ExtendedEditorWindow.serializedObject"/>'s targetObject, casted to <see cref="TObject"/>
        /// </summary>
        protected TObject targetType => serializedObject.targetObject as TObject;

        protected override void OnDisable()
        {
            base.OnDisable();
            if (serializedObject != null && serializedObject.hasModifiedProperties)
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}