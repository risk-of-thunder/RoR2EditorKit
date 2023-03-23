using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.VisualElements
{
    public class ElementResizerManipulator : MouseManipulator
    {
        private Vector2 start;
        private bool isActive;
        public IStyle elementThatGetsResized;
        public bool resizeHeight;
        public bool resizeWidth;

        public ElementResizerManipulator(IStyle elementThatGetsResized, bool resizeHeight, bool resizeWidth)
        {
            activators.Add(new ManipulatorActivationFilter { button = MouseButton.LeftMouse });
            isActive = false;
            this.elementThatGetsResized = elementThatGetsResized;
            this.resizeHeight = resizeHeight;
            this.resizeWidth = resizeWidth;
        }
        protected override void RegisterCallbacksOnTarget()
        {
            target.RegisterCallback<MouseDownEvent>(OnMouseDown);
            target.RegisterCallback<MouseMoveEvent>(OnMouseMove);
            target.RegisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected override void UnregisterCallbacksFromTarget()
        {
            target.UnregisterCallback<MouseDownEvent>(OnMouseDown);
            target.UnregisterCallback<MouseMoveEvent>(OnMouseMove);
            target.UnregisterCallback<MouseUpEvent>(OnMouseUp);
        }

        protected void OnMouseDown(MouseDownEvent e)
        {
            if (isActive)
            {
                e.StopImmediatePropagation();
                return;
            }

            if (CanStartManipulation(e))
            {
                start = e.localMousePosition;

                isActive = true;
                target.CaptureMouse();
                e.StopPropagation();
            }
        }

        protected void OnMouseMove(MouseMoveEvent e)
        {
            if (!isActive || !target.HasMouseCapture())
                return;

            Vector2 diff = e.localMousePosition - start;
            
            if(resizeHeight)
                elementThatGetsResized.height = new StyleLength(elementThatGetsResized.height.value.value + diff.y);
            if(resizeWidth)
                elementThatGetsResized.width = new StyleLength(elementThatGetsResized.width.value.value + diff.x);

            e.StopPropagation();
        }

        protected void OnMouseUp(MouseUpEvent e)
        {
            if (!isActive || !target.HasMouseCapture() || !CanStopManipulation(e))
                return;

            isActive = false;
            target.ReleaseMouse();
            e.StopPropagation();
        }
    }
}