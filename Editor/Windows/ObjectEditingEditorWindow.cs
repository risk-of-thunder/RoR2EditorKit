﻿using System;
using UnityEditor;
using Object = UnityEngine.Object;

namespace RoR2EditorKit.EditorWindows
{
    /// <summary>
    /// A variation of the <see cref="ExtendedEditorWindow"/>, this editor window is used for editing objects, allowing for more space and better control over the inspected object
    /// </summary>
    /// <typeparam name="TObject">The type of object being inspected/edited</typeparam>
    public abstract class ObjectEditingEditorWindow<TObject> : ExtendedEditorWindow where TObject : Object
    {
        /// <summary>
        /// Direct access to the SerializedObject's targetObject as it's type
        /// </summary>
        protected TObject TargetType { get => SerializedObject?.targetObject as TObject; }

        protected virtual void OnDisable()
        {
            if(SerializedObject != null && SerializedObject.hasModifiedProperties)
            {
                SerializedObject.ApplyModifiedProperties();
            }
        }

        /// <summary>
        /// Opens the ObjectEditingEditorWindow specified in <typeparamref name="TEditorWindow"/>, and sets the SerializedObject
        /// </summary>
        /// <typeparam name="TEditorWindow">The type of ObjectEditingEditorWindow to open</typeparam>
        /// <param name="obj">The object being edited in the window, cannot be null</param>
        /// <param name="windowName">The name for this window, leaving this null nicifies the <typeparamref name="TEditorWindow"/>'s type name</param>
        /// <exception cref="NullReferenceException">Thrown when <paramref name="obj"/> is null</exception>
        public static TEditorWindow OpenEditorWindow<TEditorWindow>(Object obj, string windowName = null) where TEditorWindow : ObjectEditingEditorWindow<TObject>
        {
            if (!obj)
            {
                throw new NullReferenceException(obj.ToString());
            }

            TEditorWindow window = GetWindow<TEditorWindow>(windowName == null ? ObjectNames.NicifyVariableName(typeof(TEditorWindow).Name) : windowName);
            window.SerializedObject = new SerializedObject(obj);
            window.OnWindowOpened();
            return window;
        }
    }
}