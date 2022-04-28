using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;

namespace RoR2EditorKit.Core.Inspectors
{
    public struct PrefixData
    {
        public string tooltipMessage;
        public Action contextMenuAction;

        public PrefixData(Action contextMenuAction, string tooltipMessage = null)
        {
            this.tooltipMessage = tooltipMessage;
            this.contextMenuAction = contextMenuAction;
        }
    }
    public interface IObjectNameConvention
    {
        string Prefix { get; }
        bool UsesTokenForPrefix { get; }

        PrefixData GetPrefixData();
    }
}
