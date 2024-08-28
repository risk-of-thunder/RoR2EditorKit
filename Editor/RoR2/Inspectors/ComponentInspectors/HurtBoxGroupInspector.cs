using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(HurtBoxGroup))]
    public class HurtBoxGroupInspector : VisualElementComponentInspector<HurtBoxGroup>
    {
        public override bool canReuseInstance => true;
        private SerializedProperty _hurtBoxesProperty;
        protected override void InitializeVisualElement(VisualElement templateInstanceRoot)
        {
            var foldout = templateInstanceRoot.Q<PropertyField>();

            foldout.AddSimpleContextMenu(new ContextMenuData
            {
                menuName = "Auto Populate Array",
                menuAction = (_) =>
                {
                    var values = targetType.gameObject.GetComponentsInChildren<HurtBox>();
                    _hurtBoxesProperty = serializedObject.FindProperty(nameof(HurtBoxGroup.hurtBoxes));

                    _hurtBoxesProperty.arraySize = values.Length;
                    for (int i = 0; i < _hurtBoxesProperty.arraySize; i++)
                    {
                        _hurtBoxesProperty.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
                    }
                    serializedObject.ApplyModifiedProperties();
                }
            });
        }
    }
}