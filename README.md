# RoR2EditorKit - Editor Utilities, Inspectors and More for Risk of Rain2

## About

RoR2EditorKit is a *Thunderkit Extension* for developing mods inside the UnityEditor, providing a myriad of Inspectors, Property Drawers, Editor Windows and more.

At it's core, RoR2EditorKit should not have any classes or systems that depend on runtime usage, RoR2EditorKit works exclusively for speeding up the modding enviroment.

## Manual Installation

To download *RoR2EditorKit* to your thunderkit project you'll need thunderkit version 3.4.0 or greater in your project.

* 1.- Open the ThunderkitSettings window (Tools/Thunderkit/Settings) and look at the PackageSourceSettings.
* 2.- If a PackageSource that points to https://thunderkit.thunderstore.io exists, skip to step 6, otherwise, continue
* 3.- Click the Add button, and on the dropdown menu, select ThunderstoreSource
* 4.- Select the new ThunderstoreSource, name "Package Source:" to "Thunderkit Extensions" and "Url:" to https://thunderkit.thunderstore.io
* 5.- Close ThunderkitSettings, open the Packages window (Tools/Thunderkit/Packages).
* 6.- Expand the Thunderkit Extensions menu, select RoR2EditorKit. Hit Install.
* 7.- You've now installed RoR2EditorKit to your project.

Once installed, it is heavily reccommended to open the ThunderkitSettings window to modify certain settings that RoR2EditorKit will use while helping you develop the mod.

* RoR2EditorKitSettings: Settings of the extension itself
 * Token Prefix: A prefix for your mod, it's used to generate unique tokens.
 * Main Manifest: The manifest of your mod, used in a myriad of tools to know the assetbundle or the main DLL.

## Extending RoR2EditorKit's Functionality.

* In case you need to extend RoR2EditorKit's functionality for your own purposes (Such as a custom inspector for a mod you're working on), you can look into this wiki page that explains how to extend the editor's functionality using RoR2EditorKit's systems.

[link](https://github.com/risk-of-thunder/RoR2EditorKit/wiki/Extending-the-Editor's-Functionality-with-RoR2EditorKit's-Systems.)

## Contributing

Contributing to RoR2EditorKit is as simple as creating a fork, and cloning the project. the main folder (RoR2EditorKit) is a unity project itself. Simply opening it with the unity version ror2 uses will allow you to edit the project to your heart's content.

A more detailed Contribution guideline can be found [here](https://github.com/risk-of-thunder/RoR2EditorKit/blob/main/CONTRIBUTING.md)

## Changelog

(Old changelogs can be found [here](https://github.com/risk-of-thunder/RoR2EditorKit/blob/main/OldChangelogs.md))

### Current

### '3.0.0'
* General Changes:
	* Transformed the main repository from a __Project Repository__ to a __Package Repository__ (This change alone justifies the major version change)
	

### '2.2.1'

* General Changes:
	* Updated AssemblyDefinitions to reference via name instead of GUIDs (Thanks Passive Picasso!)

### '2.2.0'

* General Changes
* Updated to use TK 5.0
* Updated to use RoR2 1.2.3
* The RoR2 scripts are now in the "RoR2EditorScripts" assembly

* Core Changes:
	* Started to generalize the look of inspectors in RoR2EK, not all inspectors are updated to show this change.
	* Fixed an issue where the RoR2EK AsmDef wouldnt recognize the AsmDef com.Multiplayer.Hlapi-runtime.
	* Fixed an issue where the system to make the RoR2EK assets inedtable wouldnt work properly
	* Reimplemented XML documentation
	* Improvements to the ExtendedInspector system
		* added a bool to define if the inspector being created has a visual tree asset or not
		* Fixed an issue where "VisualElementPropertyDrawers" would draw multiple times when inspecting an object
		* Having a null TokenPrefix no longer stops the inspector from rendering.
	* Improved the IMGUToVisualElementInspector so it no longer throws errors.
	* Removed unfinished "WeaveAssemblies" job
	* Removed PropertyDarawer wrappers

* RoR2EditorScripts changes:
	* Added an ArtifactCompoundDef Inspector
	* Added an ItemDef Inspector
	* Reimplemented the SkillFamilyVariant property drawer
	* Made all the classes in the "RoR2EditorScripts" assembly sealed
	* Removed the HGButton Inspector, this removes the unstated dependency on unity's UI package, Deleting the UIPackage's button editor is a good and simple workaround to make HGButton workable.

### '2.1.0'

* Actually added ValidateUXMLPath to the expended inspector.
* Added IMGUToVisualElementInspector editor. Used to transform an IMGUI inspector into a VisualElement inspector.
* Fixed StageLanguageFiles not working properly
* Fixed StageLanguageFiles not copying the results to the manifest's staging paths.
* Improved StageLanguageFiles' logging capabilities.
* RoR2EK assets can no longer be edited if the package is installed under the "Packages" folder.
* Split Utils.CS into 5 classes
	* Added AssetDatabaseUtils
	* Added ExtensionUtils
	* Added IOUtils
	* Added MarkdownUtils
	* Added ScriptableObjectUtils
* Removed SkillFamilyVariant property drawer

### '2.0.2'

* Fixed an issue where ExtendedInspectors would not display properly due to incorrect USS paths.
* Added ValidateUXMLPath to ExtendedInspector, used to validate the UXML's file path, override this if youre making an ExtendedInspector for a package that depends on RoR2EK's systems.
* Added ValidateUXMLPath to ExtendedEditorWindow
* Hopefully fixed the issue where RoR2EK assets can be edited.

### '2.0.1'

* Fixed an issue where ExtendedInspectors would not work due to incorrect path management.

### '2.0.0'

* Updated to unity version 2019.4.26f1
* Updated to Survivors of The Void
* Added a plethora of Util Methods to Util.CS, including Extensions
* Removed UnlockableDef creation as it's been fixed
* Added "VisualElementPropertyDrawer"
* Renamed "ExtendedPropertyDrawer" to "IMGUIPropertyDrawer"
* Rewrote ExtendedInspector sistem to use VisualElements
* Rewrote CharacterBody inspector
* Rewrote BuffDef inspector
* Rewrote ExtendedEditorWindow to use VisualElements
* Added EliteDef inspector
* Added EquipmentDef inspector
* Added NetworkStateMachine inspector
* Added SkillLocator inspector
* Removed Entirety of AssetCreator systems
* Removed SerializableContentPack window
