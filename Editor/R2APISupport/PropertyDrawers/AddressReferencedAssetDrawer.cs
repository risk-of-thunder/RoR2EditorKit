#if R2EK_R2API_ADDRESSABLES
using R2API.AddressReferencedAssets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace RoR2.Editor.PropertyDrawers
{
    public abstract class AddressReferencedAssetDrawer<T> : IMGUIPropertyDrawer<T> where T : AddressReferencedAsset
    {
        protected virtual string AddressTooltip { get; } = "The Address to the Asset";
        protected bool usingDirectReference;

        protected override void DrawIMGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            usingDirectReference = GetDirectReferenceValue(property);

            EditorGUI.BeginProperty(position, label, property);
            var fieldRect = new Rect(position.x, position.y, position.width - 16, position.height);
            EditorGUI.PropertyField(fieldRect, usingDirectReference ? property.FindPropertyRelative("_asset") : property.FindPropertyRelative("_address"), new GUIContent(property.displayName, usingDirectReference ? string.Empty : AddressTooltip));

            var contextRect = new Rect(fieldRect.xMax, position.y, 16, position.height);
            EditorGUI.DrawTextureTransparent(contextRect, R2EKConstants.AssetGUIDs.r2ekIcon, ScaleMode.ScaleToFit);
            if (Event.current.type == EventType.ContextClick)
            {
                Vector2 mousePos = Event.current.mousePosition;
                if (contextRect.Contains(mousePos))
                {
                    GenericMenu menu = new GenericMenu();
                    menu.AddItem(new GUIContent($"Use Direct Reference"), GetDirectReferenceValue(property), () =>
                    {
                        SetDirectReferenceValue(property, !GetDirectReferenceValue(property));
                    });
                    ModifyContextMenu(menu);
                    menu.ShowAsContext();
                    Event.current.Use();
                }
            }
            EditorGUI.EndProperty();
        }

        protected virtual void ModifyContextMenu(GenericMenu menu) { }
        private bool GetDirectReferenceValue(SerializedProperty property)
        {
            return property.FindPropertyRelative("_useDirectReference").boolValue;
        }

        private void SetDirectReferenceValue(SerializedProperty property, bool booleanValue)
        {
            property.FindPropertyRelative("_useDirectReference").boolValue = booleanValue;
            property.serializedObject.ApplyModifiedProperties();
        }
    }
    //-----
    [CustomPropertyDrawer(typeof(AddressReferencedBuffDef))]
    public sealed class AddressReferencedBuffDefDrawer : AddressReferencedAssetDrawer<AddressReferencedBuffDef>
    {
        protected override string AddressTooltip => "The Address or Asset Name of the Buff";
    }
    //-----
    [CustomPropertyDrawer(typeof(AddressReferencedEliteDef))]
    public sealed class AddressReferencedEliteDefDrawer : AddressReferencedAssetDrawer<AddressReferencedEliteDef>
    {
        protected override string AddressTooltip => "The Address or Asset Name of the EliteDef";
    }
    //-----
    [CustomPropertyDrawer(typeof(AddressReferencedEquipmentDef))]
    public sealed class AddressReferencedEquipmentDefDrawer : AddressReferencedAssetDrawer<AddressReferencedEquipmentDef>
    {
        protected override string AddressTooltip => "The Address or Asset Name of the EquipmentDef";
    }
    //-----
    [CustomPropertyDrawer(typeof(AddressReferencedExpansionDef))]
    public sealed class AddressReferencedExpansionDefDrawer : AddressReferencedAssetDrawer<AddressReferencedExpansionDef>
    {
        protected override string AddressTooltip => "The Address or Asset Name of the ExpansionDef";
    }
    //-----
    [CustomPropertyDrawer(typeof(AddressReferencedItemDef))]
    public sealed class AddressReferencedItemDefDrawer : AddressReferencedAssetDrawer<AddressReferencedItemDef>
    {
        protected override string AddressTooltip => "The Address or Asset Name of the ItemDef";
    }
    //-----
    [CustomPropertyDrawer(typeof(AddressReferencedPrefab))]
    public sealed class AddressReferencedPrefabDrawer : AddressReferencedAssetDrawer<AddressReferencedPrefab>
    {
    }
    //-----
    [CustomPropertyDrawer(typeof(AddressReferencedSpawnCard))]
    public sealed class AddressReferencedSpawnCardDrawer : AddressReferencedAssetDrawer<AddressReferencedSpawnCard>
    {
    }
    //-----
    [CustomPropertyDrawer(typeof(AddressReferencedUnlockableDef))]
    public sealed class AddressReferencedUnlockableDefDrawer : AddressReferencedAssetDrawer<AddressReferencedUnlockableDef>
    {
        protected override string AddressTooltip => "The Address or Asset Name of the UnlockableDef";
    }
#if R2EK_R2API_DIRECTOR
    //-----
    public sealed class AddressReferencedFamilyDirectorCardCategorySelectionDrawer : AddressReferencedAssetDrawer<AddressReferencedFamilyDirectorCardCategorySelection>
    {

    }
#endif
}
#endif