using RoR2EditorKit.Data;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2EditorKit.PropertyDrawers
{
    /// <summary>
    /// Represents an extension of the default property drawer, used by all the PropertyDrawers in RoR2EditorKit
    /// <para>The extended property drawer is used for creating new drawers for both VisualElements and IMGUI property drawers</para>
    /// <para>Regardless of this extensions, it is highly recommended to use <see cref="VisualElementPropertyDrawer{T}"/> and <see cref="IMGUIPropertyDrawer{T}"/> respectively instead of the ExtendedProeprtyDrawer directly</para>
    /// </summary>
    /// <typeparam name="T">The type used for this property drawer, this can also be of type <see cref="PropertyAttribute"/></typeparam>
    public abstract class ExtendedPropertyDrawer<T> : PropertyDrawer
    {
        /// <summary>
        /// Access to the main RoR2EditorKit Settings file
        /// </summary>
        public static RoR2EditorKitSettings Settings { get => ThunderKit.Core.Data.ThunderKitSetting.GetOrCreateSettings<RoR2EditorKitSettings>(); }

        /// <summary>
        /// Returns the field value of the SerializedProperty thats being drawn with this PropertyDrawer
        /// </summary>
        public T SerializedPropertyFieldValue
        {
            get
            {
                if (typeof(PropertyAttribute).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)attribute;
                }
                return SerializedProperty.GetValue<T>();
            }
            set
            {
                SerializedProperty.SetValue(value);
            }
        }

        /// <summary>
        /// Returns the Target unity object of the serialized property that's being drawn
        /// </summary>
        public UnityEngine.Object TargetUnityObject => SerializedObject.targetObject;
        /// <summary>
        /// Returns the Serialized Object of the Serialized Property that's being drawn.
        /// </summary>
        public SerializedObject SerializedObject => SerializedProperty.serializedObject;
        /// <summary>
        /// The Serialized Property for this property drawer
        /// </summary>
        public SerializedProperty SerializedProperty { get; private set; }

        /// <summary>
        /// OnGUI method of the ExtendedPropertyDrawer
        /// <para>Always call the base method, as the base method sets <see cref="SerializedProperty"/> to <paramref name="property"/></para>
        /// </summary>
        /// <param name="position">The position used for this property drawer</param>
        /// <param name="property">The property drawer thats being drawn</param>
        /// <param name="label">The label for this property drawer</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty = property;
        }
        /// <summary>
        /// Method for creating the VisualElement drawer for this PropertyDrawer
        /// <para>Always call the base method, it returns null but it sets <see cref="SerializedProperty"/> to <paramref name="property"/></para>
        /// <para>Keep in mind that if you overwrite this method and don't retun null, Any and all inspectors that try to draw this property drawer must draw everything using VisualElements, otherwise, no gui will be shown.</para>
        /// </summary>
        /// <param name="property">The property drawer that's being drawn</param>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            SerializedProperty = property;
            return null;
        }
    }
}