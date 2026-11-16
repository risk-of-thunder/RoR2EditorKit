using RoR2.UI;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace RoR2.Editor.Inspectors
{
    [CustomEditor(typeof(SimpleSpriteAnimator))]
    public class SimpleSpriteAnimatorInspector : ComponentIMGUIEditor<SimpleSpriteAnimator>
    {
        private EditorCoroutine testCoroutine;
        protected override void DrawIMGUI()
        {
            DrawDefaultInspector();

            if(GUILayout.Button(testCoroutine != null ? "Stop Testing" : "Start Testing") && targetType && targetType.animation)
            {
                if(testCoroutine == null)
                {
                    testCoroutine = EditorCoroutineUtility.StartCoroutine(TestingCoroutine(), this);
                }
                else
                {
                    StopCoroutine();
                }
            }
        }

        private void StopCoroutine()
        {
            if(testCoroutine != null)
            {
                EditorCoroutineUtility.StopCoroutine(testCoroutine);
                testCoroutine = null;
            }

            if(targetType && targetType.TryGetComponent<Image>(out var img))
            {
                img.sprite = null;
            }
        }

        protected override void OnDisable()
        {
            StopCoroutine();
            base.OnDisable();
        }

        protected override void OnEditorDisabled()
        {
            StopCoroutine();
            base.OnEditorDisabled();
        }

        private IEnumerator TestingCoroutine()
        {
            Image imageComponent = null;
            if(!targetType.TryGetComponent<Image>(out imageComponent))
            {
                EditorCoroutineUtility.StopCoroutine(testCoroutine);
                testCoroutine = null;
                yield break;
            }

            float timeBetweenTicks = 1f / targetType.animation.frameRate;
            int tick = 0;
            int frame = 0;
            while(targetType && imageComponent)
            {
                yield return new EditorWaitForSeconds(timeBetweenTicks);

                SceneView.RepaintAll();

                tick++;
                if(frame > targetType.animation.frames.Length)
                {
                    frame = 0;
                    tick = 0;
                }

                if (targetType.animation.frames[frame].duration <= tick)
                {
                    frame++;
                    if(frame >= targetType.animation.frames.Length)
                    {
                        frame = 0;
                        tick = 0;
                    }

                    SimpleSpriteAnimation.Frame reference = targetType.animation.frames[frame];
                    imageComponent.sprite = reference.sprite;
                }
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return testCoroutine != null;
        }
    }
}