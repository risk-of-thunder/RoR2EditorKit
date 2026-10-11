using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor.Deprecated
{
    [Obsolete(OBSOLETE_MESSAGE)]
    public abstract class IMGUIComponentInspector<T> : ComponentInspector<T> where T : Component
    {
        protected sealed override VisualElement CreateInspectorUI()
        {
            IMGUIContainer container = new IMGUIContainer(() =>
            {
                serializedObject.UpdateIfRequiredOrScript();
                DrawIMGUI();
                serializedObject.ApplyModifiedProperties();
            });
            return container;
        }
        protected abstract void DrawIMGUI();
    }
}