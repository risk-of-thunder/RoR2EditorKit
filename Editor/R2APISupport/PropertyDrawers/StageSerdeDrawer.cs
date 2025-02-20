#if R2EK_R2API_DIRECTOR
using R2API;
using System;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.PropertyDrawers
{
    [CustomPropertyDrawer(typeof(DirectorAPI.StageSerde))]
    public class StageSerdeDrawer : IMGUIPropertyDrawer<DirectorAPI.StageSerde>
    {
        private static string[] _optionNames;
        private static DirectorAPI.Stage[] _optionValues;
        private static Vector2 _windowSize;

        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var mask = (long)property.FindPropertyRelative("Value").longValue;
            var popupButtonText = CreateContentFromMaskValue(mask);

            var controlRect = EditorGUI.PrefixLabel(position, label);
            if (EditorGUI.DropdownButton(controlRect, popupButtonText, FocusType.Passive))
            {
                PopupContent content = new PopupContent((DirectorAPI.Stage)mask);
                content.onValueChanged = (newVal) =>
                {
                    mask = (long)newVal;
                    property.FindPropertyRelative("Value").longValue = mask;
                    property.serializedObject.ApplyModifiedProperties();
                };
                PopupWindow.Show(controlRect, content);
            }
        }

        private GUIContent CreateContentFromMaskValue(long mask)
        {
            DirectorAPI.Stage stageValue = (DirectorAPI.Stage)mask;
            string label;
            StringBuilder tooltipBuilder = new StringBuilder();
            int valCount = 0;
            var allEnumValues = Enum.GetValues(typeof(DirectorAPI.Stage));
            foreach (DirectorAPI.Stage value in allEnumValues)
            {
                if (stageValue.HasFlag(value))
                {
                    tooltipBuilder.Append(value.ToString());
                    tooltipBuilder.Append(" | ");
                    valCount++;
                }
            }

            if (valCount == allEnumValues.Length)
            {
                label = "All...";
            }
            else if (valCount > 1)
            {
                label = "Mixed...";
            }
            else if (valCount == 1)
            {
                label = Enum.GetName(typeof(DirectorAPI.Stage), stageValue);
            }
            else
            {
                label = "None...";
            }

            return new GUIContent(label, tooltipBuilder.ToString());
        }

        static StageSerdeDrawer()
        {
            _optionNames = Enum.GetNames(typeof(DirectorAPI.Stage));
            ArrayUtility.Insert(ref _optionNames, 0, "None");
            ArrayUtility.Add(ref _optionNames, "All");

            _optionValues = (DirectorAPI.Stage[])Enum.GetValues(typeof(DirectorAPI.Stage));
            ArrayUtility.Insert(ref _optionValues, 0, default(DirectorAPI.Stage));
            ArrayUtility.Add(ref _optionValues, (DirectorAPI.Stage)~0L);

            float x = 0;
            float y = 0;
            foreach (var option in _optionNames)
            {
                var vector = EditorStyles.label.CalcSize(new GUIContent(option));
                var possiblyBiggerX = vector.x;


                x = Mathf.Max(x, possiblyBiggerX);
                y += EditorGUIUtility.singleLineHeight;
            }
            x += 100;
            _windowSize = new Vector2(x, y);
        }

        public class PopupContent : PopupWindowContent
        {
            private DirectorAPI.Stage _maskValue;
            public Action<DirectorAPI.Stage> onValueChanged;

            public override Vector2 GetWindowSize()
            {
                return _windowSize;
            }

            public override void OnGUI(Rect rect)
            {
                for (int i = 0; i < _optionNames.Length; i++)
                {
                    var optionName = _optionNames[i];
                    bool previousVal = ComputePreviousBoolValForCurrent(i);

                    EditorGUI.BeginChangeCheck();
                    bool newVal = GUILayout.Toggle(previousVal, optionName);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (i == 0) //Handles "None" option
                        {
                            if (newVal == true)
                            {
                                _maskValue = _optionValues[i];
                                onValueChanged?.Invoke(_maskValue);
                                continue;
                            }

                            _maskValue = _optionValues[_optionValues.Length - 1];
                            onValueChanged?.Invoke(_maskValue);
                            continue;
                        }
                        else if (i == _optionNames.Length - 1) //Handles "All" option
                        {
                            if (newVal == true)
                            {
                                _maskValue = _optionValues[i];
                                onValueChanged?.Invoke(_maskValue);
                                continue;
                            }

                            _maskValue = _optionValues[0];
                            onValueChanged?.Invoke(_maskValue);
                            continue;
                        }

                        //It has been selected, and its not already in the mask
                        if (newVal == true && !_maskValue.HasFlag(_optionValues[i]))
                        {
                            _maskValue |= _optionValues[i];
                            onValueChanged?.Invoke(_maskValue);
                            continue;
                        }//It has been deselected, and its in the mask
                        else if (newVal == false && _maskValue.HasFlag(_optionValues[i]))
                        {
                            _maskValue &= ~_optionValues[i];
                            onValueChanged?.Invoke(_maskValue);
                            continue;
                        }
                    }
                }
            }

            private bool ComputePreviousBoolValForCurrent(int i)
            {
                //Do a direct comparasion for All and None options, use HasFlag for the rest.
                if (i == 0)
                {
                    return _maskValue == _optionValues[i];
                }
                else if (i == _optionNames.Length - 1)
                {
                    return _maskValue == _optionValues[i];
                }
                return _maskValue.HasFlag(_optionValues[i]);
            }

            public PopupContent(DirectorAPI.Stage flagsSet)
            {
                _maskValue = flagsSet;
            }
        }
    }
}
#endif