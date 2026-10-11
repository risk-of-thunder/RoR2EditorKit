using UnityEngine.UIElements;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="BaseIMGUIEditor{T}"/> is a <see cref="BaseEditor{T}"/> built for creating UI using IMGUI with <see cref="EditorGUI"/>, <see cref="EditorGUILayout"/>, and <see cref="GUILayout"/>
    /// <para></para>
    /// This editor doesn't support <see cref="VisualElement"/>s. If you want to make an editor using <see cref="VisualElement"/>s, utilize <see cref="BaseVisualElementEditor{T}"/> instead.
    /// <para></para>
    /// RoR2EK comes bundled with two <see cref="BaseIMGUIEditor{T}"/> types, which are the <see cref="ComponentIMGUIEditor{T}"/> and the <see cref="ScriptableObjectIMGUIEditor{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of Object being edited</typeparam>
    public abstract class BaseIMGUIEditor<T> : BaseEditor<T> where T : UnityEngine.Object
    {

        /// <summary>
        /// Checks if the inspector has done it's first drawing call
        /// </summary>
        protected bool hasDoneFirstDrawing { get; private set; }

        /// <summary>
        /// Unavailable on IMGUI editors, if you wish to use VisualElements, utilize <see cref="BaseVisualElementEditor{T}"/> instead
        /// </summary>
        public sealed override VisualElement CreateInspectorGUI() => null;

        /// <summary>
        /// Method utilized to draw this editor's UI. You cannot override this.
        /// <para></para>
        /// To create your own Inspector UI when this editor's enabled, override <see cref="DrawIMGUI"/>.
        /// <br></br>
        /// If you need to draw UI that's always present regardless of editor enabled status, implement <see cref="BeforeInspectorDrawing"/>.
        /// </summary>
        public sealed override void OnInspectorGUI()
        {
            serializedObject.Update();

            BeforeInspectorDrawing();

            if(editorEnabled)
            {
                DrawIMGUI();
            }
            else
            {
                DrawDefaultInspector();
            }

            AfterInspectorDrawing();
            serializedObject.ApplyModifiedProperties();
            hasDoneFirstDrawing = true;
        }

        /// <summary>
        /// Method that's called before <see cref="DrawIMGUI"/> or <see cref="UnityEditor.Editor.DrawDefaultInspector"/>
        /// <br></br>
        /// Either method is called depending on the value of <see cref="BaseEditor.editorEnabled"/>
        /// </summary>
        protected virtual void BeforeInspectorDrawing() { }

        /// <summary>
        /// Method that's called after <see cref="DrawIMGUI"/> or <see cref="UnityEditor.Editor.DrawDefaultInspector"/>
        /// <br></br>
        /// Either method is called depending on the value of <see cref="BaseEditor.editorEnabled"/>
        /// </summary>
        protected virtual void AfterInspectorDrawing() { }

        /// <summary>
        /// Implement this method to draw your custom editor UI
        /// </summary>
        protected abstract void DrawIMGUI();
    }
}