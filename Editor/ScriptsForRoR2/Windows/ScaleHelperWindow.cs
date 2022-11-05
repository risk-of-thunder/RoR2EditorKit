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
    public sealed class ScaleHelperWindow : ExtendedEditorWindow
    {
        public enum ScaleMeasurement
        {
            CommandoAccurate,
            CommandoBoundingBox,
            Golem,
            ElderLemurian,
            StoneTitan,
            Mithrix,
            GolemPlains,
            GolemPlains2,
            BlackBeach,
            BlackBeach2,
            SkyMeadows,
            VoidStage,
            AncientLoft,
            Moon,
            Moon2
        }

        public static Dictionary<ScaleMeasurement, Vector3> scaleMeasurementToBoundingBox = new Dictionary<ScaleMeasurement, Vector3>
        {
            { ScaleMeasurement.CommandoAccurate, new Vector3(0.520f, 1.970f, 0.474f) },
            { ScaleMeasurement.CommandoBoundingBox, new Vector3(1, 2, 1) },
            { ScaleMeasurement.Golem, new Vector3(3.406f, 4.969f, 1.221f) },
            { ScaleMeasurement.ElderLemurian, new Vector3(11.206f, 18.441f, 13.107f) },
            { ScaleMeasurement.StoneTitan, new Vector3(7.851f, 22.732f, 7.67f) },
            { ScaleMeasurement.Mithrix, new Vector3(0.565f, 2.222f, 0.485f) },
            { ScaleMeasurement.GolemPlains, new Vector3(509, 170, 566) },
            { ScaleMeasurement.GolemPlains2, new Vector3(611, 192, 387) },
            { ScaleMeasurement.BlackBeach, new Vector3(590, 163, 431) },
            { ScaleMeasurement.BlackBeach2, new Vector3(470, 151, 374) },
            { ScaleMeasurement.SkyMeadows, new Vector3(598, 274, 622) },
            { ScaleMeasurement.VoidStage, new Vector3(332, 124, 361) },
            { ScaleMeasurement.AncientLoft, new Vector3(422, 130, 589) },
            { ScaleMeasurement.Moon, new Vector3(3507, 379, 1451) },
            { ScaleMeasurement.Moon2, new Vector3(2234, 973, 2023) }
        };
        public GameObject cube;
        public ScaleMeasurement compareAgainst;

        private VisualElement header;
        private VisualElement center;
        private VisualElement footer;
        private VisualElement resultsContainer;
        [MenuItem(RoR2EditorKit.Common.Constants.RoR2EditorKitMenuRoot + "Scale Helper")]
        private static void OpenWindow()
        {
            var window = OpenEditorWindow<ScaleHelperWindow>();
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
            var comparassionScale = scaleMeasurementToBoundingBox[compareAgainst];
            var comparassionScaleName = System.Enum.GetName(typeof(ScaleMeasurement), compareAgainst);

            var heightAprox = cubeScale.y / comparassionScale.y;
            var label = new Label($"Around {heightAprox} {comparassionScaleName}(s) tall");
            resultsContainer.Add(label);

            var widthAprox = cubeScale.x / comparassionScale.x;
            label = new Label($"Around {widthAprox} {comparassionScaleName}(s) wide");
            resultsContainer.Add(label);

            var depthAprox = cubeScale.z / comparassionScale.z;
            label = new Label($"Around {depthAprox} {comparassionScaleName}(s) deep");

            resultsContainer.Add(label);
        }
    }
}