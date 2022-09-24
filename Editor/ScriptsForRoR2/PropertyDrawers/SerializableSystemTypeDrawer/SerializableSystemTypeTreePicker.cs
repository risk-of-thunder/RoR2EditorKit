
using RoR2EditorKit.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace RoR2EditorKit.RoR2Related.PropertyDrawers
{
    public sealed class SerializableSystemTypeTreePicker : EditorWindow
    {
        private static SerializableSystemTypeTreePicker typeTreePicker;

        private readonly SerializableSystemTypeTreeView treeView = new SerializableSystemTypeTreeView();
        private bool close;
        private SerializableSystemTypeTreeView.SystemTypeTreeInfo selectedType;
        private SerializedProperty serializableSystemTypeReference;
        private SerializedObject serializedObject;

        public static EditorWindow LastFocusedWindow = null;

        private void Update()
        {
            if (close)
            {
                Close();

                if (LastFocusedWindow)
                {
                    EditorApplication.delayCall += LastFocusedWindow.Repaint;
                    LastFocusedWindow = null;
                }
            }
        }

        private void OnGUI()
        {
            using (new GUILayout.VerticalScope())
            {
                treeView.DisplayTreeView(TreeListControl.DisplayTypes.USE_SCROLL_VIEW);

                using (new GUILayout.HorizontalScope("box"))
                {
                    if (GUILayout.Button("Ok"))
                    {
                        //Get the selected item
                        var selectedItem = treeView.GetSelectedItem();
                        var data = (SerializableSystemTypeTreeView.SystemTypeTreeInfo)selectedItem?.DataContext;
                        if (selectedItem != null && data.itemType == SerializableSystemTypeTreeView.ItemType.Type)
                            SetType(selectedItem);

                        //The window can now be closed
                        close = true;
                    }
                    else if (GUILayout.Button("Cancel"))
                        close = true;
                    else if (GUILayout.Button("Reset"))
                    {
                        ResetType();
                        close = true;
                    }
                    else if (Event.current.type == EventType.Used && treeView.LastDoubleClickedItem != null)
                    {
                        //We must be in 'used' mode in order for this to work
                        SetType(treeView.LastDoubleClickedItem);
                        close = true;
                    }
                }
            }
        }

        private void SetType(TreeListItem in_item)
        {
            serializedObject.Update();

            selectedType = in_item.DataContext as SerializableSystemTypeTreeView.SystemTypeTreeInfo;
            serializableSystemTypeReference.stringValue = selectedType.fullName;
            serializedObject.ApplyModifiedProperties();
        }

        private void ResetType()
        {
            serializedObject.Update();
            serializableSystemTypeReference.stringValue = null;
            selectedType = null;
            serializedObject.ApplyModifiedProperties();
        }


        public class PickerCreator
        {
            public SerializedProperty parentProperty;
            public SerializedProperty systemTypeReference;
            public Rect pickerPosition;
            public SerializedObject serializedObject;

            internal PickerCreator()
            {
                EditorApplication.delayCall += DelayCall;
            }

            private void DelayCall()
            {
                if (typeTreePicker != null)
                    return;

                typeTreePicker = CreateInstance<SerializableSystemTypeTreePicker>();

                //position the window below the button
                var pos = new Rect(pickerPosition.x, pickerPosition.yMax, 0, 0);

                //If the window gets out of the screen, we place it on top of the button instead
                if (pickerPosition.yMax > Screen.currentResolution.height / 2)
                    pos.y = pickerPosition.y - Screen.currentResolution.height / 2;

                //We show a drop down window which is automatically destroyed when focus is lost
                typeTreePicker.ShowAsDropDown(pos,
                    new Vector2(pickerPosition.width >= 250 ? pickerPosition.width : 250,
                        Screen.currentResolution.height / 2));

                typeTreePicker.serializableSystemTypeReference = systemTypeReference;
                typeTreePicker.serializedObject = serializedObject;

                Type requiredBaseType = GetRequiredBaseType();

                typeTreePicker.treeView.AssignDefaults();
                typeTreePicker.treeView.SetRootItem(requiredBaseType != null ? $"Types subclassing {requiredBaseType}" : "Types");

                List<Type> types = new List<Type>();
                foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    Utilities.ReflectionUtils.GetTypesSafe(assembly, out Type[] ts);
                    types.AddRange(ts);
                }

                var finalTypes = types.Where((t) => !t.IsAbstract);

                if (requiredBaseType != null)
                    finalTypes = finalTypes.Where(t => t.IsSubclassOf(requiredBaseType));

                finalTypes.ToList().ForEach(t => typeTreePicker.treeView.PopulateItem(t));
            }

            private Type GetRequiredBaseType()
            {
                Type typeOfObject = serializedObject.targetObject.GetType();
                Type[] assemblyTypes = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(asm => RoR2EditorKit.Utilities.ReflectionUtils.GetTypesSafe(asm))
                    .ToArray();

                Type requiredBaseType = null;

                requiredBaseType = GetBaseTypeFromFieldInfo(typeOfObject);
                if (requiredBaseType != null)
                    return requiredBaseType;

                requiredBaseType = GetBaseTypeFromStructFieldInfo();
                if (requiredBaseType != null)
                    return requiredBaseType;

                requiredBaseType = GetBaseTypeFromArrayFieldInfo(typeOfObject);
                if (requiredBaseType != null)
                    return requiredBaseType;

                throw new Exception("Could not find required base type.");
            }


            private Type GetBaseTypeFromFieldInfo(Type typeOfObject)
            {
                try
                {
                    var field = typeOfObject.GetFields()
                                             .Where(fieldInfo => fieldInfo.GetCustomAttributes(typeof(HG.SerializableSystemType.RequiredBaseTypeAttribute), true) != null)
                                             .Where(fieldInfo => fieldInfo.Name == parentProperty.name)
                                             .FirstOrDefault();

                    Type requiredBaseType = null;
                    if (field != null)
                    {
                        HG.SerializableSystemType.RequiredBaseTypeAttribute attribute = (HG.SerializableSystemType.RequiredBaseTypeAttribute)field.GetCustomAttributes(typeof(HG.SerializableSystemType.RequiredBaseTypeAttribute), true).First();
                        if (attribute != null)
                        {
                            requiredBaseType = attribute.type;
                        }
                    }

                    return requiredBaseType;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Could not find base type from field info, resorting to other methods... \n\n\n{e}");
                    return null;
                }
            }

            private Type GetBaseTypeFromStructFieldInfo()
            {
                try
                {
                    Type requiredBaseType = null;

                    var path = parentProperty.propertyPath;
                    path = path.Substring(0, path.LastIndexOf("."));
                    var pProp = serializedObject.FindProperty(path);
                    var parentType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.GetTypes()).FirstOrDefault(t => t.Name == pProp.type);

                    var fieldInfo = parentType.GetField(parentProperty.name);

                    if (fieldInfo == null)
                    {
                        path = path.Substring(0, path.LastIndexOf("."));
                        pProp = serializedObject.FindProperty(path);
                        parentType = AppDomain.CurrentDomain.GetAssemblies().SelectMany(asm => asm.GetTypes()).FirstOrDefault(t => t.Name == pProp.type);

                        fieldInfo = parentType.GetField(parentProperty.name);
                    }
                    var requiredBaseTypeAttribute = fieldInfo.GetCustomAttributes(typeof(HG.SerializableSystemType.RequiredBaseTypeAttribute), true).FirstOrDefault();

                    if (fieldInfo != null)
                    {
                        if (requiredBaseTypeAttribute != null)
                        {
                            HG.SerializableSystemType.RequiredBaseTypeAttribute attribute = (HG.SerializableSystemType.RequiredBaseTypeAttribute)requiredBaseTypeAttribute;
                            if (attribute != null)
                            {
                                requiredBaseType = attribute.type;
                            }
                        }
                    }

                    return requiredBaseType;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Could not find base type from struct field info, resorting to other methods... \n\n\n{e}");
                    return null;
                }
            }

            private Type GetBaseTypeFromArrayFieldInfo(Type typeOfObject)
            {
                var path = parentProperty.propertyPath;
                if (!path.Contains("Array.data["))
                {
                    return null;
                }
                //parent property is an array, find it.
                string modifiedPath = path;
                SerializedProperty arrayProp = parentProperty.serializedObject.FindProperty(modifiedPath);
                int tries = 0;
                while (!arrayProp.isArray)
                {
                    modifiedPath = path.Substring(0, path.LastIndexOf("."));
                    arrayProp = parentProperty.serializedObject.FindProperty(modifiedPath);
                    tries++;

                    if (tries > 10)
                    {
                        Debug.LogWarning("Took over ten tries to attempt to find the base type from array field info, breaking.");
                        break;
                    }
                }

                string fieldName = modifiedPath.Substring(0, modifiedPath.LastIndexOf("."));

                var field = typeOfObject.GetFields()
                                             .Where(fieldInfo => fieldInfo.GetCustomAttributes(typeof(HG.SerializableSystemType.RequiredBaseTypeAttribute), true) != null)
                                             .Where(fieldInfo => fieldInfo.Name == fieldName)
                                             .FirstOrDefault();

                Type requiredBaseType = null;
                if (field != null)
                {
                    HG.SerializableSystemType.RequiredBaseTypeAttribute attribute = (HG.SerializableSystemType.RequiredBaseTypeAttribute)field.GetCustomAttributes(typeof(HG.SerializableSystemType.RequiredBaseTypeAttribute), true).First();
                    if (attribute != null)
                    {
                        requiredBaseType = attribute.type;
                    }
                }

                return requiredBaseType;
            }
        }
    }
}