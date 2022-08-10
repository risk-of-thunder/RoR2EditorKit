using RoR2EditorKit.Core.EditorWindows;
using RoR2EditorKit.Utilities;
using RoR2EditorKit.Common;
using RoR2;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using UnityEditor.UIElements;
using System.Threading.Tasks;

namespace RoR2EditorKit.RoR2Related.EditorWindows
{
    public class CharacterBodyWizard : CreatorWizardWindow
    {
        protected override Task<bool> RunWizard()
        {
            return (Task<bool>)Task.CompletedTask;
        }
    }
}
