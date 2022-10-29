using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using RoR2EditorKit.Core;
using RoR2EditorKit.Core.EditorWindows;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public sealed class StageScaleHelper : ExtendedEditorWindow
    {
        public static readonly Vector3 commandoAproxSize = new Vector3(1, 2, 1);
        public static readonly Vector3 commandoAccurateSize = new Vector3(0.520f, 1.970f, 0.474f);
        public GameObject cube;


        private VisualElement header;
        private VisualElement center;
        private VisualElement footer;
        private VisualElement resultsContainer;
        [MenuItem(RoR2EditorKit.Common.Constants.RoR2EditorKitMenuRoot + "Stage Scale Helper")]
        private static void OpenWindow()
        {
            var window = OpenEditorWindow<StageScaleHelper>();
            window.Focus();
        }

        protected override void CreateGUI()
        {
            base.CreateGUI();
            header = rootVisualElement.Q<VisualElement>("Header");
            center = rootVisualElement.Q<VisualElement>("Center");
            footer = rootVisualElement.Q<VisualElement>("Footer");
            resultsContainer = footer.Q<VisualElement>("Results");
        }
        protected override void OnWindowOpened()
        {
            base.OnWindowOpened();
            center.Q<Button>().clickable.clicked += Calculate;
        }

        private void Calculate()
        {
            resultsContainer.Clear();
            if (!cube)
                return;

            var cubeScale = cube.transform.localScale;

            var heightAprox = cubeScale.y / commandoAproxSize.y;
            var heightAccurate = cubeScale.y / commandoAccurateSize.y;
            var label = new Label($"Around {heightAprox} commando(s) tall");
            label.tooltip = $"{heightAccurate} commando(s) tall";
            resultsContainer.Add(label);

            var widthAprox = cubeScale.x / commandoAproxSize.x;
            var widthAccurate = cubeScale.x / commandoAccurateSize.x;
            label = new Label($"Around {widthAprox} commando(s) wide");
            label.tooltip = $"{widthAccurate} commando(s) wide";
            resultsContainer.Add(label);

            var depthAprox = cubeScale.z / commandoAproxSize.z;
            var depthAccurate = cubeScale.z / commandoAccurateSize.z;
            label = new Label($"Around {depthAprox} commando(s) deep");
            label.tooltip = $"{depthAccurate} commando(s) deep";
            resultsContainer.Add(label);
        }
    }
}