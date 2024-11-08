using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2.Editor
{
    /// <summary>
    /// The WizardCoroutineHelper is an <see cref="IEnumerator"/> that can be used for creating complex Wizard setups for a <see cref="EditorWizardWindow"/>
    /// 
    /// <para>Coroutines added as steps using <see cref="AddStep(IEnumerator, string)"/> can yield return floats, these floats represent the individual percentage of completion of said step. this value should be between 0 and 1.</para>
    /// </summary>
    public class WizardCoroutineHelper : IEnumerator
    {
        /// <summary>
        /// The EditorWizardWindow that created this instance.
        /// </summary>
        public EditorWizardWindow wizardInstance { get; }

        /// <summary>
        /// The current object in the coroutine
        /// </summary>
        public object Current => _internalCoroutine.Current;

        private Queue<Step> _stepsQueue = new Queue<Step>();

        private IEnumerator _internalCoroutine;
        private int _maxProgress;

        /// <summary>
        /// Adds a new step to the coroutine helper.
        /// </summary>
        /// <param name="coroutine">The coroutine itself, this coroutine can yield return floats to represent the individual progress of the step</param>
        /// <param name="stepName">The name of the step, used during the calling of <see cref="wizardInstance"/>'s <see cref="EditorWizardWindow.UpdateProgress(float, string)"/></param>
        public void AddStep(IEnumerator coroutine, string stepName)
        {
            _stepsQueue.Enqueue(new Step
            {
                subroutine = coroutine,
                subroutineName = stepName,
            });
            _maxProgress++;
        }

        /// <summary>
        /// Processes the Coroutine helper
        /// </summary>
        /// <returns>True if the Coroutine helper is NOT finished, otherwise false.</returns>
        public bool MoveNext()
        {
            return _internalCoroutine.MoveNext();
        }

        /// <summary>
        /// Not supported
        /// </summary>
        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }

        private IEnumerator InternalCoroutine()
        {
            //Makes sure it doesnt execute the proper while loop.
            yield return null;

            int completedSubroutines = 0;
            while (_stepsQueue.TryDequeue(out var step))
            {
                var subroutine = step.subroutine;
                var stepName = step.subroutineName;
                wizardInstance.UpdateProgress(R2EKMath.Remap(completedSubroutines, 0, _maxProgress, 0, 1), stepName);
                yield return null;

                //Progress the subroutine to completion
                while (subroutine.MoveNext())
                {
                    //Handle yielded object.
                    if (subroutine.Current is float f) //If float, update progress
                    {
                        wizardInstance.UpdateProgress(CalculateProgress(completedSubroutines, f), stepName);
                        yield return null;
                    }
                    yield return subroutine.Current; //yield current object, might be a wait for seconds.
                }
                completedSubroutines++;
            }
        }

        private float CalculateProgress(int completedSubroutines, float subroutineProgress)
        {
            //remap the subroutine's progress
            var subroutineMaxProgress = Mathf.Min(completedSubroutines + 1, _maxProgress);
            var val = R2EKMath.Remap(subroutineProgress, 0, 1, completedSubroutines, Mathf.Min(completedSubroutines + 1, subroutineMaxProgress));
            return R2EKMath.Remap(val, 0, _maxProgress, 0, 1);
        }

        /// <summary>
        /// Creates a new instance of <see cref="WizardCoroutineHelper"/>
        /// </summary>
        /// <param name="wizardInstance">The wizard that created this coroutine helper.</param>
        public WizardCoroutineHelper(EditorWizardWindow wizardInstance)
        {
            this.wizardInstance = wizardInstance;
            _internalCoroutine = InternalCoroutine();
        }

        private struct Step
        {
            public IEnumerator subroutine;
            public string subroutineName;
        }
    }
}