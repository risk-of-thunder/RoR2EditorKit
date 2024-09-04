using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RoR2.Editor
{
    public class WizardCoroutineHelper : IEnumerator
    {
        public EditorWizardWindow wizardInstance { get; }
        public object Current => _internalCoroutine.Current;

        private Queue<Step> _stepsQueue = new Queue<Step>();

        private IEnumerator _internalCoroutine;
        private int _maxProgress;

        public void AddStep(IEnumerator coroutine, string stepName)
        {
            _stepsQueue.Enqueue(new Step
            {
                subroutine = coroutine,
                subroutineName = stepName,
            });
            _maxProgress++;
        }

        public bool MoveNext()
        {
            return _internalCoroutine.MoveNext();
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        private IEnumerator InternalCoroutine()
        {
            //Makes sure it doesnt execute the proper while loop.
            yield return null;

            int completedSubroutines = 0;
            while(_stepsQueue.TryDequeue(out var step))
            {
                var subroutine = step.subroutine;
                var stepName = step.subroutineName;
                wizardInstance.UpdateProgress(R2EKMath.Remap(completedSubroutines, 0, _maxProgress, 0, 1), stepName);
                yield return null;

                //Progress the subroutine to completion
                while(subroutine.MoveNext())
                {
                    //Handle yielded object.
                    if(subroutine.Current is float f) //If float, update progress
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