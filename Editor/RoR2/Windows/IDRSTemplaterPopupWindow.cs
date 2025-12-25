using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RoR2.Editor.Windows
{
    public class IDRSTemplaterPopupWindow : PopupWindowContent
    {
        private enum DisplayKind
        {
            //We want to add the displays for a survivor (take from commando)
            Survivor,
            //We want to add the displays for a monster (only elite crowns)
            Monster,
            //We want to add the displays for a drone (take from gunner drone)
            Drone
        }

        [MenuItem("CONTEXT/ItemDisplayRuleSet/Add vanilla displays in bulk")]
        private static void Open(MenuCommand command)
        {
            ItemDisplayRuleSet idrs = (ItemDisplayRuleSet)command.context;

            Event current = Event.current;
            Vector2 mousePos = Vector2.zero;
            if(current != null)
            {
                mousePos = current.mousePosition;
            }
            else
            {
                mousePos = Input.mousePosition;
            }

            PopupWindow.Show(new Rect(mousePos, Vector2.zero), new IDRSTemplaterPopupWindow(idrs));
        }
        private SerializedObject _targetIDRSSerializedObject;
        private ItemDisplayRuleSet _targetIDRS;

        private DisplayKind _displayKind = DisplayKind.Survivor;

        public override void OnGUI(Rect rect)
        {
            _displayKind = (DisplayKind)EditorGUILayout.EnumPopup("Bulk Type", _displayKind);
            string infoStringFormat = "Adding displays found within {0}";
            string bodyName = "";
            switch (_displayKind)
            {
                case DisplayKind.Survivor: bodyName = "Commando"; break;
                case DisplayKind.Monster: bodyName = "Lesser Wisp"; break;
                case DisplayKind.Drone: bodyName = "Gunner Drone"; break;
            }
            EditorGUILayout.LabelField(string.Format(infoStringFormat, bodyName));

            if(GUILayout.Button("Add in Bulk"))
            {
                AddEntriesInBulk();
            }
        }

        private void AddEntriesInBulk()
        {
            string addressToUse = "";
            switch(_displayKind)
            {
                case DisplayKind.Survivor: addressToUse = "7d6fc2aa6fd5d5b47b3b39a7abf9c951"; break;
                case DisplayKind.Monster: addressToUse = "de4337c3785ce044a97fcc0f95115b6e"; break;
                case DisplayKind.Drone: addressToUse = "b885310f48d366643a35aa3d29dddfda"; break;
            }


            ItemDisplayRuleSet source = Addressables.LoadAssetAsync<ItemDisplayRuleSet>(addressToUse).WaitForCompletion();

            ReadOnlyCollection<string> vanillaDisplayPrefabGUIDS = new AddressablesPathDictionary.EntryLookup()
                .WithLookupType(AddressablesPathDictionary.EntryType.Guid)
                .WithComponentRequirement(typeof(ItemDisplay), false)
                .WithTypeRestriction(typeof(GameObject))
                .PerformLookup();

            HashSet<string> vanillaKeyAssetGUIDS = new HashSet<string>(new AddressablesPathDictionary.EntryLookup()
                .WithLookupType(AddressablesPathDictionary.EntryType.Guid)
                .WithTypeRestriction(typeof(ItemDef), typeof(EquipmentDef))
                .PerformLookup());

            int totalKeyAssetRuleGroups = source.keyAssetRuleGroups.Length;
            int currentKeyAssetRuleGroupIndex = 0;
            using (var progressBar = new DisposableProgressBar($"Adding displays for {_displayKind}", $"Adding KeyAssetRuleGroups ({currentKeyAssetRuleGroupIndex}/{totalKeyAssetRuleGroups})", 0))
            {
                for (; currentKeyAssetRuleGroupIndex < totalKeyAssetRuleGroups; currentKeyAssetRuleGroupIndex++)
                {
                    progressBar.Update((float)currentKeyAssetRuleGroupIndex / (float)totalKeyAssetRuleGroups, null, $"Adding KeyAssetRuleGroups({currentKeyAssetRuleGroupIndex}/{totalKeyAssetRuleGroups})");
                    Thread.Sleep(1);

                    //Get the key asset we need to ensure it's existence.
                    ItemDisplayRuleSet.KeyAssetRuleGroup currentKeyAssetRuleGroup = source.keyAssetRuleGroups[currentKeyAssetRuleGroupIndex];

                    //If the key asset is already in the target IDRS, ignore it.
                    if (IsKeyAssetInTarget(_targetIDRS.keyAssetRuleGroups, currentKeyAssetRuleGroup))
                    {
                        continue;
                    }

                    string keyAssetGUID = ResolveKeyAssetGUID(GetKeyAssetFromRuleGroup(currentKeyAssetRuleGroup), vanillaKeyAssetGUIDS);
                    string[] displayPrefabGUIDS = ResolveDisplayPrefabGUIDS(currentKeyAssetRuleGroup.displayRuleGroup, vanillaDisplayPrefabGUIDS);

                    if(string.IsNullOrWhiteSpace(keyAssetGUID) || displayPrefabGUIDS == null)
                    {
                        continue;
                    }

                    ItemDisplayRuleSet.KeyAssetRuleGroup ruleGroup = new ItemDisplayRuleSet.KeyAssetRuleGroup
                    {
                        keyAssetAddress = new AddressableAssets.IDRSKeyAssetReference(keyAssetGUID)
                    };

                    for(int i = 0; i < displayPrefabGUIDS.Length; i++)
                    {
                        string displayPrefabGUID = displayPrefabGUIDS[i];

                        if(displayPrefabGUID == null)
                        {
                            System.Diagnostics.Debugger.Break();
                        }

                        ItemDisplayRule newRule;
                        if(displayPrefabGUID.StartsWith("LIMB_MASK"))
                        {
                            string limbNames = displayPrefabGUID.Split(':')[1];
                            LimbFlags flag = Enum.Parse<LimbFlags>(limbNames);

                            newRule = new ItemDisplayRule
                            {
                                limbMask = flag,
                                ruleType = ItemDisplayRuleType.LimbMask
                            };
                        }
                        else
                        {
                            newRule = new ItemDisplayRule
                            {
                                followerPrefabAddress = new AssetReferenceGameObject(displayPrefabGUID)
                            };
                        }

                        ruleGroup.displayRuleGroup.AddDisplayRule(newRule);
                    }

                    HG.ArrayUtils.ArrayAppend(ref _targetIDRS.keyAssetRuleGroups, ruleGroup);
                }
            }

            _targetIDRSSerializedObject.Update();
        }

        private bool IsKeyAssetInTarget(ItemDisplayRuleSet.KeyAssetRuleGroup[] targetKeyAssetRuleGroups, ItemDisplayRuleSet.KeyAssetRuleGroup toMatch)
        {
            foreach(var entry in targetKeyAssetRuleGroups)
            {
                //Ignore entries that have direct references
                if (entry.keyAsset)
                    continue;

                //Ignore entries that have invalid addresses, because we cant load that key asset manually.
                if(!entry.keyAssetAddress.RuntimeKeyIsValid())
                {
                    continue;
                }

                //Load the entry's KeyAsset
                var entryKeyAssetOp = Addressables.LoadAssetAsync<ScriptableObject>(entry.keyAssetAddress.RuntimeKey);
                while(!entryKeyAssetOp.IsDone)
                {
                    Thread.Sleep(1);
                }
                var entryKeyAsset = entryKeyAssetOp.Result;

                //Guard against somehow invalid entry key asset
                if(!entryKeyAsset)
                {
                    continue;
                }

                //Now we get the "toMatch"'s keyAsset.
                ScriptableObject toMatchKeyAsset = GetKeyAssetFromRuleGroup(toMatch);

                //Guard against somehow invalid toMatchKeyAsset.
                if(!toMatchKeyAsset)
                {
                    continue;
                }

                //If the entry key asset is the same as the "to match", then its a match, and as such, is in the target.
                if(entryKeyAsset == toMatch.keyAsset)
                {
                    return true;
                }
            }

            //No match was found, need to add it.
            return false;
        }

        private ScriptableObject GetKeyAssetFromRuleGroup(ItemDisplayRuleSet.KeyAssetRuleGroup keyAssetRuleGroup)
        {
            if(keyAssetRuleGroup.keyAsset)
            {
                return (ScriptableObject)keyAssetRuleGroup.keyAsset;
            }

            if(!keyAssetRuleGroup.keyAssetAddress.RuntimeKeyIsValid())
            {
                return null;
            }

            var operation = Addressables.LoadAssetAsync<ScriptableObject>(keyAssetRuleGroup.keyAssetAddress.RuntimeKey);
            while(!operation.IsDone)
            {
                Thread.Sleep(1);
            }

            return operation.Result;
        }

        private GameObject GetDisplayPrefabFromItemDisplayRule(ItemDisplayRule itemDisplayRule)
        {
            if (itemDisplayRule.followerPrefab)
            {
                return itemDisplayRule.followerPrefab;
            }

            if (!itemDisplayRule.followerPrefabAddress.RuntimeKeyIsValid())
            {
                return null;
            }

            var operation = Addressables.LoadAssetAsync<GameObject>(itemDisplayRule.followerPrefabAddress.RuntimeKey);
            while(!operation.IsDone)
            {
                Thread.Sleep(1);
            }

            return operation.Result;
        }

        private string ResolveKeyAssetGUID(ScriptableObject keyAsset, HashSet<string> keyAssetGUIDS)
        {
            AddressablesPathDictionary instance = AddressablesPathDictionary.instance;
            string guidMatch = null;
            //We need to iterate thru the known guids
            foreach(var guid in keyAssetGUIDS)
            {
                //do a path check, the path shoudl contain at the very least the key asset's name.
                var path = instance.GetPathFromGUID(guid);
                if(!path.Contains(keyAsset.name))
                {
                    continue;
                }

                //Load the asset
                var result = Addressables.LoadAssetAsync<ScriptableObject>(guid).WaitForCompletion();

                //Key asset match, store it's GUID then break
                if(result && result == keyAsset)
                {
                    guidMatch = guid;
                    break;
                }
            }

            //remove the guid from the hash set.
            keyAssetGUIDS.Remove(guidMatch);

            return guidMatch;
        }

        private string[] ResolveDisplayPrefabGUIDS(DisplayRuleGroup displayRuleGroup, ReadOnlyCollection<string> displayPrefabGUIDS)
        {
            if (displayRuleGroup.isEmpty)
            {
                return null;
            }

            AddressablesPathDictionary instance = AddressablesPathDictionary.instance;
            string[] resolvedGUIDS = new string[displayRuleGroup.rules.Length];

            for (int i = 0; i < displayRuleGroup.rules.Length; i++)
            {
                ItemDisplayRule itemDisplayRule = displayRuleGroup.rules[i];
                if(itemDisplayRule.ruleType == ItemDisplayRuleType.LimbMask)
                {
                    resolvedGUIDS[i] = string.Format("LIMB_MASK:{0}", itemDisplayRule.limbMask.ToString());
                    continue;
                }

                var displayPrefab = GetDisplayPrefabFromItemDisplayRule(itemDisplayRule);
                string bestMatch = "";

                foreach(var guid in displayPrefabGUIDS)
                {
                    //do a path check, the path should contain at the very least the displayPrefab's name.
                    var path = instance.GetPathFromGUID(guid);
                    if (!path.Contains(displayPrefab.name))
                    {
                        continue;
                    }

                    //Store the best match for now, in case for whatever reason the object load fails.
                    bestMatch = guid;

                    //Load the asset
                    var result = Addressables.LoadAssetAsync<GameObject>(guid).WaitForCompletion();

                    //Key asset match, store it's GUID then break
                    if (result && result == displayPrefab)
                    {
                        bestMatch = guid;
                        break;
                    }
                }

                resolvedGUIDS[i] = bestMatch;
            }
            return resolvedGUIDS;
        }

        public override Vector2 GetWindowSize()
        {
            Vector2 size = base.GetWindowSize();
            size.x = 400;
            return size;
        }

        public IDRSTemplaterPopupWindow(ItemDisplayRuleSet targetIDRS)
        {
            _targetIDRSSerializedObject = new SerializedObject(targetIDRS);
            _targetIDRS = targetIDRS;
        }
    }
}