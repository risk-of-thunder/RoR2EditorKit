using System.Collections;
using System.Collections.Generic;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// The <see cref="EqualLabelWidthContainer"/> is a VisualElement that ensures all the labels within it that are from Basefields are elided and cropped so the controls are aligned properly.
    /// 
    /// Only labels that are decorated with the R2EKGlobalSetting's ElideLabel class will be modified.
    /// </summary>
    public class EqualLabelWidthContainer : VisualElement
    {
        private static ObservableHashSet<EqualLabelWidthContainer> _attachedInstances = new ObservableHashSet<EqualLabelWidthContainer>();
        private static EditorCoroutine _queryCoroutine;

        private List<Label> _labelsToEqualize = new List<Label>();
        private float _maxWidth;

        private void OnAttached(AttachToPanelEvent evt)
        {
            RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _attachedInstances.Add(this);
            QueryLabels();
            RecalculateMaxSize();
            ResizeLabels();
        }

        private void OnDetached(DetachFromPanelEvent evt)
        {
            UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _attachedInstances.Remove(this);
            _labelsToEqualize.Clear();
        }

        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            ResizeLabels();
        }

        private void QueryLabels()
        {
            _labelsToEqualize.Clear();
            _labelsToEqualize.AddRange(this.Query<Label>(null, "unity-base-field__label").ToList());
        }

        private void RecalculateMaxSize()
        {
            if (_labelsToEqualize.Count == 0)
            {
                _maxWidth = -1;
                return;
            }

            _maxWidth = 0;
            foreach (var label in _labelsToEqualize)
            {
                var textSize = label.MeasureTextSize(label.text, 0, MeasureMode.Undefined, 0, MeasureMode.Undefined);
                textSize.x += 20;
                _maxWidth = Mathf.Max(_maxWidth, textSize.x);
            }
        }

        private void ResizeLabels()
        {
            if (_labelsToEqualize.Count == 0)
                return;

            foreach (var label in _labelsToEqualize)
            {
                label.style.maxWidth = _maxWidth;
            }
        }

        private static IEnumerator QueryCoroutine()
        {
            while (true)
            {
                yield return new EditorWaitForSeconds(3f);
                foreach (var instance in _attachedInstances)
                {
                    instance.QueryLabels();
                    instance.RecalculateMaxSize();
                    instance.ResizeLabels();
                }
            }
        }

        /// <summary>
        /// Creates a new instance of an EqualLabelWidthContainer
        /// </summary>
        public EqualLabelWidthContainer()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttached);
            RegisterCallback<DetachFromPanelEvent>(OnDetached);
        }

        static EqualLabelWidthContainer()
        {
            _attachedInstances = new ObservableHashSet<EqualLabelWidthContainer>();
            _attachedInstances.CollectionChanged += _attachedInstances_CollectionChanged;
        }

        private static void _attachedInstances_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (_attachedInstances.Count == 0)
            {
                if (_queryCoroutine != null)
                {
                    EditorCoroutineUtility.StopCoroutine(_queryCoroutine);
                    _queryCoroutine = null;
                }
            }
            else if (_queryCoroutine == null)
            {
                _queryCoroutine = EditorCoroutineUtility.StartCoroutineOwnerless(QueryCoroutine());
            }
        }

        ~EqualLabelWidthContainer()
        {
            UnregisterCallback<AttachToPanelEvent>(OnAttached);
            UnregisterCallback<DetachFromPanelEvent>(OnDetached);
        }

        public new class UxmlFactory : UxmlFactory<EqualLabelWidthContainer, UxmlTraits> { }
        public new class UxmlTraits : VisualElement.UxmlTraits { }
    }
}