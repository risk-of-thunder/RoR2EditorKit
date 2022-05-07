using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine.AddressableAssets;

namespace RoR2EditorKit.Utilities
{
    public static class AddressablesUtils
    {
        public static bool AddressableCatalogExists => ContainsDefine("TK_ADDRESSABLE");

        public static async Task<T> LoadAssetFromCatalog<T>(object address) where T : UnityEngine.Object
        {
            if (AddressableCatalogExists)
                throw new InvalidOperationException($"Cannot load asset because ThunderKit has not imported the addressable catalog!");

            using (var pb = new ThunderKit.Common.Logging.ProgressBar("Loading Object"))
            {
                var op = Addressables.LoadAssetAsync<T>(address);
                while(!op.IsDone)
                {
                    await Task.Delay(100);
                    pb.Update($"Loading asset from address {address}, this may take a while", null, op.PercentComplete);
                }
                return op.Result;
            }
        }

        private static bool ContainsDefine(string define)
        {
            foreach (BuildTargetGroup targetGroup in System.Enum.GetValues(typeof(BuildTargetGroup)))
            {
                if (targetGroup == BuildTargetGroup.Unknown || IsObsolete(targetGroup))
                    continue;

                string defineSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

                if (!defineSymbols.Contains(define))
                    return false;
            }

            return true;


            bool IsObsolete(BuildTargetGroup group)
            {
                var attrs = typeof(BuildTargetGroup).GetField(group.ToString()).GetCustomAttributes(typeof(ObsoleteAttribute), false);
                return attrs.Length > 0;
            }
        }
    }
}