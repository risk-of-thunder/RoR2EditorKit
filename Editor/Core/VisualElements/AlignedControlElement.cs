using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    public class AlignedControlElement : VisualElement
    {
        public float minimumLabelWidth
        {
            get => _minimumLabelWidth;
            set
            {
                if(_minimumLabelWidth != value)
                {
                    _minimumLabelWidth = value;
                    ResizeLabelWidth(null);
                }
            }
        }
        private float _minimumLabelWidth;

        public float minimumControlWidth
        {
            get => _minimumControlWidth;
            set
            {
                if(_minimumControlWidth != value)
                {
                    _minimumControlWidth = value;
                    ResizeLabelWidth(null);
                }
            }
        }
        private float _minimumControlWidth;
        
        public float labelRightPadding
        {
            get => _labelRightPadding;
            set
            {
                if(_labelRightPadding != value)
                {
                    _labelRightPadding = value;
                    ResizeLabelWidth(null);
                }
            }
        }
        private float _labelRightPadding;

        public Func<AlignedControlElement, Label, bool> labelPredicate = DefaultWhereCheck;

        private int _previousHierarchyCount;
        private List<LabelWidth> _labelsAndWidths = new List<LabelWidth>();
        private float _largestLabelWidth;
        private float _shortestLabelWidth;
        private void OnAttached(AttachToPanelEvent evt)
        {
            RegisterCallback<GeometryChangedEvent>(ResizeLabelWidth);
        }

        private void OnDetach(DetachFromPanelEvent evt)
        {
            UnregisterCallback<GeometryChangedEvent>(ResizeLabelWidth);
        }

        private void ResizeLabelWidth(GeometryChangedEvent _)
        {
            if(_previousHierarchyCount != hierarchy.childCount)
            {
                _previousHierarchyCount = hierarchy.childCount;
                UpdateLabelCollection();

                if (_labelsAndWidths.Count == 0)
                    return;

                _largestLabelWidth = _labelsAndWidths[_labelsAndWidths.Count - 1].idealWidth;
                _shortestLabelWidth = _labelsAndWidths[0].idealWidth;
            }

            var rectForLabels = contentRect;
            rectForLabels.width -= minimumControlWidth;

            var newWidthForLabels = GetIdealWidthForLabels(rectForLabels);
            foreach(var labelWidth in _labelsAndWidths)
            {
                labelWidth.label.style.width = newWidthForLabels + _labelRightPadding;
            }
        }

        private float GetIdealWidthForLabels(Rect rectForLabels)
        {
            if (rectForLabels.width > _largestLabelWidth)
                return _largestLabelWidth;

            var width = Mathf.Clamp(rectForLabels.width, minimumLabelWidth, _largestLabelWidth);
            return R2EKMath.Remap(width, minimumLabelWidth, _largestLabelWidth, _shortestLabelWidth, _largestLabelWidth);
        }

        private void UpdateLabelCollection()
        {
            _labelsAndWidths.Clear();

            var collection = this.Query<Label>(null, "unity-base-field__label")
                .Where(label => labelPredicate(this, label))
                .ToList()
                .Select(ve => new LabelWidth
                {
                    label = ve,
                    idealWidth = ve.MeasureTextSize(ve.text, 0, MeasureMode.Undefined, 0, MeasureMode.Undefined).x
                })
                .OrderBy(ve => ve.idealWidth);

            foreach (var labelWidth in collection)
            {
                labelWidth.label.style.overflow = Overflow.Hidden;
                _labelsAndWidths.Add(labelWidth);
            }
        }

        public static bool DefaultWhereCheck(AlignedControlElement instance, Label label)
        {
            VisualElement currentElement = label;
            while(currentElement.parent != null)
            {
                //parent is aligned control element
                if(currentElement.parent is AlignedControlElement alignedControlElement)
                {
                    //element is being controled by this instance, if it is another instance it shouldnt control it.
                    if (alignedControlElement == instance)
                        return true;
                }
                currentElement = currentElement.parent;
            }
            return false;
        }

        public AlignedControlElement()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttached);
            RegisterCallback<DetachFromPanelEvent>(OnDetach);
        }

        private struct LabelWidth
        {
            public Label label;
            public float idealWidth;
        }
        public new class UxmlFactory : UxmlFactory<AlignedControlElement, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits
        {
            private UxmlFloatAttributeDescription m_MinimumLabelWidth = new UxmlFloatAttributeDescription
            {
                name = VisualElementUtil.NormalizeNameForUXMLTrait(nameof(minimumLabelWidth)),
                defaultValue = 150f
            };
            private UxmlFloatAttributeDescription m_MinimumControlWidth = new UxmlFloatAttributeDescription
            {
                name = VisualElementUtil.NormalizeNameForUXMLTrait(nameof(minimumControlWidth)),
                defaultValue = 150f
            };
            private UxmlFloatAttributeDescription m_LabelRightPadding = new UxmlFloatAttributeDescription
            {
                name = VisualElementUtil.NormalizeNameForUXMLTrait(nameof(labelRightPadding)),
                defaultValue = 20
            };

            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
            {
                base.Init(ve, bag, cc);
                var element = (AlignedControlElement)ve;
                element._minimumControlWidth = m_MinimumControlWidth.GetValueFromBag(bag, cc);
                element._minimumLabelWidth = m_MinimumLabelWidth.GetValueFromBag(bag, cc);
                element._labelRightPadding = m_LabelRightPadding.GetValueFromBag(bag, cc);
            }
        }

    }
}