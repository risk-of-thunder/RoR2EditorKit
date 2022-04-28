### 1.0.0

* First Risk of Thunder release
* Rewrote readme a bit
* Added missing XML documentation to methods
* Added a property drawer for PrefabReference (Used on anything that uses RendererInfos)
* Added the MaterialEditor
    * The material editor is used for making modifying and working with HG shaders easier.
    * Works with both stubbed and non stubbed shaders
    * Entire system can be disabled on settings
* Properly added an Extended Property Drawer
* Added Inspector for CharacterBody
* Added Inspector for Child Locator
* Added Inspector for Object Scale Curve
* Added Inspector for BuffDef
* Fixed the enum mask drawer not working with uint based enum flags

### 0.2.4

* Made sure the Assembly Definition is Editor Only.

### 0.2.3

* Added the ability for the EntityStateConfiguration inspector to ignore fields with HideInInspector attribute.

### 0.2.2

* Added 2 new Extended Inspector inheriting classes
    * Component Inspector: Used for creating inspectors for components.
    * ScriptableObject Inspector: Used for creating inspectors for Scriptable Objects.
* Modified the existing inspectors to inherit from these new inspectors.
* Added an inspector for HGButton
* Moved old changelogs to new file

### 0.2.1

* Renamed UnlockableDefCreator to ScriptableCreators
* All the uncreatable skilldefs in the namespace RoR2.Skills can now be created thanks to the ScriptableCreator
* Added an EditorGUILayoutProperyDrawer
    * Extends from property drawer.
    * Should only be used for extremely simple property drawer work.
    * It's not intended as a proper extension to the PropertyDrawer system.
* Added Utility methods to the ExtendedInspector

### 0.2.0

* Added CreateRoR2PrefabWindow, used for creating prefabs.
* Added a window for creating an Interactable prefab.
* Fixed an issue where the Serializable System Type Drawer wouldn't work properly if the inspected type had mode than 1 field.
* Added a fallback on the Serializable System Type Drawer
* Added a property drawer for EnumMasks, allowing proper usage of Flags on RoR2 Enums with the Flags attribute.

### 0.1.4

* Separated the Enabled and Disabled inspector settings to its own setting file. allowing projects to git ignore it.
* The Toggle for enabling and disabling the inspector is now on its header GUI for a more pleasant experience.

### 0.1.2

* Fixed no assembly definition being packaged with the toolkit, whoops.

### 0.1.1

- RoR2EditorKitSettings:
    * Removed the "EditorWindowsEnabled" setting.
    * Added an EnabledInspectors setting.
        * Lets the user choose what inspectors to enable/disable.
    * Added a MainManifest setting.
        * Lets RoR2EditorKit know the main manifest it'll work off, used in the SerializableContentPackWindow.

- Inspectors:
    * Added InspectorSetting property
        * Automatically Gets the inspector's settings, or creates one if none are found.
    * Inspectors can now be toggled on or off at the top of the inspector window.
    
- Editor Windows: 
    * Cleaned up and documented the Extended Editor Window class.
    * Updated the SerializableContentPack editor window:
        * Restored function for Drag and Dropping multiple files
        * Added a button to each array to auto-populate the arrays using the main manifest of the project.

### 0.1.0

- Reorganized CreateAsset Menu
- Added EntityStateConfiguration creator, select state type and hit create. Optional checkbox for setting the asset name to the state's name.
- Added SurvivorDef creator, currently halfway implemented.
- Added BuffDef creator, can automatically create a networked sound event for the start sfx.
- Removed EntityStateConfiguration editor window.
- Implemented a new EntityStateConfiguration inspector
- Internal Changes

### 0.0.1

- Initial Release