using System;

namespace RoR2EditorKit.Core.Inspectors
{
    /// <summary>
    /// Data that represents a Prefix thats going to be used for the naming convention system
    /// </summary>
    public struct PrefixData
    {
        /// <summary>
        /// The message in the tooltip for the messageBox, can be null
        /// </summary>
        public string tooltipMessage;
        /// <summary>
        /// The message in the Help Box, can be null.
        /// </summary>
        public string helpBoxMessage;
        /// <summary>
        /// The context menu action for the helpBox
        /// </summary>
        public Action contextMenuAction;
        /// <summary>
        /// A function that detemines if the object is following naming conventions
        /// </summary>
        public Func<bool> nameValidatorFunc;

        /// <summary>
        /// PrefixData constructor
        /// </summary>
        /// <param name="contextMenuAction">An action that runs when the message box is right clicked</param>
        /// <param name="tooltipMessage">Optional tooltip message</param>
        public PrefixData(Action contextMenuAction, Func<bool> nameValidatorFunc = null, string tooltipMessage = null, string helpBoxMessage = null)
        {
            this.tooltipMessage = tooltipMessage;
            this.nameValidatorFunc = nameValidatorFunc;
            this.contextMenuAction = contextMenuAction;
            this.helpBoxMessage = helpBoxMessage;
        }
    }

    /// <summary>
    /// An interface that makes an ExtendedInspector inform a user when theyre not following the Object's naming conventions.
    /// </summary>
    public interface IObjectNameConvention
    {
        string Prefix { get; }
        bool UsesTokenForPrefix { get; }

        PrefixData GetPrefixData();

    }
}
