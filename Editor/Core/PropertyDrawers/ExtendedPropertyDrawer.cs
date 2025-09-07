using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace RoR2.Editor
{
    /// <summary>
    /// Represents an extension of the default property drawer, used by all the PropertyDrawers in R2EK
    /// <para>The extended property drawer is used for creating  new drawers for both VisualElements and IMGUI property drawers</para>
    /// 
    /// <para>If you want to build a property drawer using visual elements, utilize <see cref="VisualElementPropertyDrawer{T}"/>, otherwise you can use <see cref="IMGUIPropertyDrawer{T}"/></para>
    /// </summary>
    /// <typeparam name="T">The type used for this property drawer, this is usually the same value that's given to the <see cref="CustomPropertyDrawer"/> attribute.</typeparam>
    public abstract class ExtendedPropertyDrawer<T> : PropertyDrawer
    {
        /// <summary>
        /// Returns the PropertyDrawer's ProjectSettings's EditorSetting.
        /// <para>Keep in mind that these settings are used for all instances of the property drawer</para>
        /// </summary>
        public EditorSettingCollection propertyDrawerProjectSettings
        {
            get
            {
                if (_propertyDrawerProjectSettings == null)
                {
                    _propertyDrawerProjectSettings = EditorSettingManager.GetOrCreateSettingsFor(this.GetType(), EditorSettingManager.SettingType.ProjectSetting);
                }
                return _propertyDrawerProjectSettings;
            }
        }
        private EditorSettingCollection _propertyDrawerProjectSettings;

        /// <summary>
        /// Returns the PropertyDrawer's UserSetting's EditorSetting
        /// <para>Keep in mind that these settings are used for all instances of the property drawer</para>
        /// </summary>
        public EditorSettingCollection propertyDrawerPreferenceSettings
        {
            get
            {
                if (_propertyDrawerPreferenceSettings == null)
                {
                    _propertyDrawerPreferenceSettings = EditorSettingManager.GetOrCreateSettingsFor(this.GetType(), EditorSettingManager.SettingType.UserSetting);
                }
                return _propertyDrawerPreferenceSettings;
            }
        }
        private EditorSettingCollection _propertyDrawerPreferenceSettings;

        /// <summary>
        /// The data that this property drawer is modifying, the type of data returned depends wether <typeparamref name="T"/> inherits from <see cref="PropertyAttribute"/> or not.
        /// 
        /// <para>If it does inherit from <see cref="PropertyAttribute"/>, then this property throws an exception when the attribute value is trying to be modified, as attributes are read only</para>
        /// </summary>
        public T propertyDrawerData
        {
            get
            {
                if (typeof(PropertyAttribute).IsAssignableFrom(typeof(T)))
                {
                    return (T)(object)attribute;
                }
                return serializedProperty.GetValue<T>();
            }
            set
            {
                if (typeof(PropertyAttribute).IsAssignableFrom(typeof(T)))
                {
                    throw new NotSupportedException("Cannot modify attribute values.");
                }
                serializedProperty.SetValue<T>(value);
            }
        }

        /// <summary>
        /// The Target unity object this property drawer is drawing
        /// </summary>
        public UnityEngine.Object targetUnityObject => serializedObject.targetObject;

        /// <summary>
        /// The SerializedObject that owns the SerializedProperty this property drawer is drawing
        /// </summary>
        public SerializedObject serializedObject => serializedProperty.serializedObject;

        /// <summary>
        /// The SerializedProperty we're drawing
        /// </summary>
        public SerializedProperty serializedProperty { get; private set; }

        /// <summary>
        /// Override this method to implement the property drawer using IMGUI
        /// <br>Other classes that inherit from this may abstract this functionality further</br>
        /// </summary>
        /// <param name="position">The total position for the property drawer</param>
        /// <param name="property">The property drawer we're drawing</param>
        /// <param name="label">The label for the property drawer</param>
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            serializedProperty = property;
        }

        /// <summary>
        /// Override this emthod to implement the property drawer using VisualElements
        /// <br>Other classes that inherit from this may abstract this functionality further</br>
        /// </summary>
        /// <param name="property">The property we're drawing</param>
        /// <returns>The visual element that represents the control for the serialized property</returns>
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            serializedProperty = property;
            return null;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            serializedProperty = property;
            return base.GetPropertyHeight(property, label);
        }

        /// <summary>
        /// <inheritdoc cref="EditorSettingCollection.GetOrCreateSetting{T}(string, T)"/>
        /// 
        /// <para>Keep in mind that the value used in <paramref name="settingType"/> will make it so either the <see cref="EditorSettingCollection>"/> from <see cref="propertyDrawerProjectSettings"/> or <see cref="propertyDrawerPreferenceSettings"/> is used. If you want to store it in a different provider, utilize <see cref="GetOrCreateSetting{T1}(string, T1, string)"/></para>
        /// </summary>
        /// <typeparam name="T1"><inheritdoc cref="EditorSettingCollection.GetOrCreateSetting{T}(string, T)"/></typeparam>
        /// <param name="settingName">The name of the setting</param>
        /// <param name="defaultValue">The default value of this setting, used when the setting is being created</param>
        /// <param name="settingType">The type of the setting, this cannot be <see cref="EditorSettingManager.SettingType.Custom"/></param>
        /// <returns><inheritdoc cref="EditorSettingCollection.GetOrCreateSetting{T}(string, T)"/></returns>
        /// <exception cref="System.ArgumentException"></exception>
        protected T1 GetOrCreateSetting<T1>(string settingName, T1 defaultValue, EditorSettingManager.SettingType settingType)
        {

            EditorSettingCollection src = null;
            switch (settingType)
            {
                case EditorSettingManager.SettingType.ProjectSetting:
                    src = propertyDrawerProjectSettings;
                    break;
                case EditorSettingManager.SettingType.UserSetting:
                    src = propertyDrawerPreferenceSettings;
                    break;
                default:
                    throw new System.ArgumentException("Setting Type is Invalid", nameof(settingType));
            }

            return src.GetOrCreateSetting<T1>(settingName, defaultValue);
        }

        /// <summary>
        /// <inheritdoc cref="EditorSettingCollection.GetOrCreateSetting{T}(string, T)"/> 
        /// <typeparam name="T1"><inheritdoc cref="EditorSettingCollection.GetOrCreateSetting{T}(string, T)"/></typeparam>
        /// <param name="settingName">The name of the setting</param>
        /// <param name="defaultValue">The default value of this setting, used when the setting is being created</param>
        /// <param name="providerName">The name of the provider that'll store the setting.</param>
        /// <returns><inheritdoc cref="EditorSettingCollection.GetOrCreateSetting{T}(string, T)"/></returns>
        protected T1 GetOrCreateSetting<T1>(string settingName, T1 defaultValue, string providerName)
        {
            var provider = EditorSettingManager.GetEditorSettingProvider(providerName);

            var settings = EditorSettingManager.GetOrCreateSettingsFor(GetType(), provider);

            return settings.GetOrCreateSetting(settingName, defaultValue);
        }


        /// <summary>
        /// <inheritdoc cref="EditorSettingCollection.SetSettingValue(string, object)"/>
        /// 
        /// <para>Keep in mind that the value used in <paramref name="settingType"/> will make it so either the <see cref="EditorSettingCollection>"/> from <see cref="propertyDrawerProjectSettings"/> or <see cref="propertyDrawerPreferenceSettings"/> is used. If you want to store it in a different provider, utilize <see cref="GetOrCreateSetting{T1}(string, T1, string)"/></para>
        /// </summary>
        /// <param name="settingName">The setting to change it's value</param>
        /// <param name="value">The new value for the setting</param>
        /// <param name="settingType">The type of the setting, this cannot be <see cref="EditorSettingManager.SettingType.Custom"/></param>
        protected void SetSetting(string settingName, object value, EditorSettingManager.SettingType settingType)
        {
            EditorSettingCollection dest = null;

            switch (settingType)
            {
                case EditorSettingManager.SettingType.ProjectSetting:
                    dest = propertyDrawerProjectSettings;
                    break;
                case EditorSettingManager.SettingType.UserSetting:
                    dest = propertyDrawerPreferenceSettings;
                    break;
                default:
                    throw new System.ArgumentException("Setting Type is Invalid", nameof(settingType));
            }

            dest.SetSettingValue(settingName, value);
        }

        /// <summary>
        /// <inheritdoc cref="EditorSettingCollection.SetSettingValue(string, object)"/>
        /// </summary>
        /// <param name="settingName">The setting to change it's value</param>
        /// <param name="value">The new value for the setting</param>
        /// <param name="providerName">The name of the provider that'll store the setting.</param>
        protected void SetSetting(string settingName, object value, string providerName)
        {
            var provider = EditorSettingManager.GetEditorSettingProvider(providerName);

            var settings = EditorSettingManager.GetOrCreateSettingsFor(GetType(), provider);

            settings.SetSettingValue(settingName, value);
        }
    }
}