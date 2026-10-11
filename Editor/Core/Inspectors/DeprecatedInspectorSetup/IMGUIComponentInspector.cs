using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// A <see cref="ComponentInspector{T}"/> that allows you to implement the UI using IMGUI, instead of directly interfacing with VisualElements.
    /// <br>Ideal for simple inspectors.</br>
    /// </summary>
    /// <typeparam name="T">The type of Component being inspected</typeparam>
    public abstract class IMGUIComponentInspector<T> : ComponentInspector<T> where T : Component
    {
        /// <summary>
        /// <inheritdoc cref="Inspector{T}.CreateInspectorUI"/>
        /// <para>For <see cref="IMGUIComponentInspector{T}"/>, utilize <see cref="DrawIMGUI"/> instead to create your IMGUI inspector</para>
        /// </summary>
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

        /// <summary>
        /// Implement your IMGUI inspector using this method
        /// </summary>
        protected abstract void DrawIMGUI();
    }
}