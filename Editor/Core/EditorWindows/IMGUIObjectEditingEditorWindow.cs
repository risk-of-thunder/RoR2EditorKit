using System;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public abstract class IMGUIObjectEditingEditorWindow<TObject> : ObjectEditingEditorWindow<TObject> where TObject : UnityEngine.Object
    {
        protected sealed override void CreateGUI()
        {
            rootVisualElement.Add(new IMGUIContainer(OnGUI));
        }

        protected abstract void OnGUI();
    }
}