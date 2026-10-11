using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public abstract class BaseIMGUIEditor<T> : BaseEditor<T> where T : UnityEngine.Object
    {
        protected bool hasDoneFirstDrawing { get; private set; }
        public sealed override VisualElement CreateInspectorGUI() => null;
        public sealed override void OnInspectorGUI()
        {
            serializedObject.Update();

            OnBeforeInspectorDrawing();

            if(editorEnabled)
            {
                DrawIMGUI();
            }
            else
            {
                DrawDefaultInspector();
            }
            serializedObject.ApplyModifiedProperties();
            hasDoneFirstDrawing = true;
        }

        protected virtual void OnBeforeInspectorDrawing() { }
        protected abstract void DrawIMGUI();
    }
}