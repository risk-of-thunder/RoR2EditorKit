# RoR2EditorKit - Editor Utilities, Inspectors and More for Risk of Rain2

## About

RoR2EditorKit is a *Thunderkit Extension* for developing mods inside the UnityEditor, providing a myriad of Inspectors, Property Drawers, Editor Windows and more.

At it's core, RoR2EditorKit should not have any classes or systems that depend on runtime usage, RoR2EditorKit works exclusively for speeding up the modding enviroment.

## Manual Installation

To Download RoR2EditorKit to your project, it is recommended that you either add it via the ThunderKit extension store, or adding it via Unity's PackageManager (Downloading a specific tagged version is recommended).

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

### '3.5.0'

* Core Changes:
	* ExtendedMaterialEditor now attempts to get the action for a material, if it doesnt find any, it'll use the default inspector.
	* ObjectEditingWindow's TargetType is now just a getter, obtaining the target type directly from the SerializedObject.

* RoR2EditorScripts changes:
	* Made the EntityStateConfiguration inspector able to draw fields as enums, courtesy of KingEnderBrine
		* This is done by using the field as an int, and marking it with the EnumMask attribute.
	* Fixed a bug where the CharacterBody template would have unfitting entity states assigned.
	* Added 3 new Template options to CharacterBody template, courtesy of HeyImNoop
		* Grounded is now the original prefab template.
		* Flying uses a Wisp as a template.
		* Stationary uses an Alpha construct as a template
		* Boss uses a StoneTitan as a template.
	* Major improvements and fixes to the MapNodeGroup inspector.
		* Painter should now work properly
		* Added a button to update hull masks
		* Added a button that shifts nodes upwards, for creating Air nodes from ground nodes.
		* Added a button that shifts nodes downards, for creating ground nodes from Air nodes
		* Increased visibility of the Scene GUI
		* Removed "Add Node on Cam Pos" keybind
	* Updated ModCreatorWizard window to support R2API's Split assemblies update.
		* Now will scan for all assemblies in the AppDomain, and add all the R2API submodules it finds as dependencies.
		* Automatically adds said dependencies to your Manifest, main class, and assembly definition.
	* Added R2APIMigrationWizard
		* Scans for all assemblies in the app domain to find the R2API submodules loaded in the editor.
		* Used for migrating a mod that uses the old R2API system to the split assemblies system.
		* Replaces the single BepInDependency attribute for multiple attributes, depending on the amount of assemblies it finds.
		* Replaces the assemblyDefinition and Manifest's dependencies from the single dll to the multiple dll's, depending on the amount of assemblies and manifests it finds.

### '3.4.0'

* RoR2EditorScripts Changes:
	* Added a Scaling Tool window, used to roughly see how big something is compared to ror2 bodies and stages.
	* Changed the EntityStateConfiguration inspector to use the old IMGUI version instead of visual elements.
		* All improvements made are also in the IMGUI inspector.
		* EntityStateConfiguration inspector no longer tries to serialize fields marked as constant
	* Added a Gizmo for visualizing the scale of a HitBox

### '3.3.1'

* Core Changes:
	* Added SerializableShaderWrapper
		* Serializes shaders by serializing their shader.name and their GUID's
	* The material editor now works properly by using the SerializableShaderWrapper
	* Added new utilities to the AssetDatabase utils class
	* Added AssetRipper's Shader asset post processors.
		* This in turn should fix unity destroying YAML shader assets.

* RoR2EditorScripts changes:
	* Stage Creator wizard now prefixes the scene's name with the token prefix
	* Fixed an issue where having mutliple inspectors would break the Skill Locator inspector.
	* Fixed an issue where the CanBeRandomlyTriggered bool for EquipmentDefs wouldnt show
	* Exposed HGCloudRemap's Internal Simple Blend Mode property

### '3.3.0'

* Core Changes:
	* Improved material editor so it doesnt wipe itself
	* Added wizard systems
		* A wizard allows you to create complex jobs for your project.
	* All the asset GUID contants are now under Constants.AssetGUIDS class.
		* Added a QuickLoad and GetPath methods for AssetGUIDS class.
	* Added FormatPathForUnity and GetCurrentDirectory to IOUtils.
	* Extended inspector's object name conventions have been extended.
		*  Can now specify both a custom help box message and custom name validation function
	* Component Inspectors now always have the fancy enable toggle visual element.
	
* RoR2EditorScripts changes:
	* Added ModCreator wizard.
		* Creates an Asmdef, manifest, assetbundle folder and main class for a mod
	* Added StageCreator wizard
		* Creates a template stage and scenedef
	* Added CharacterBodyCreator Wizard
		* Creates a valid CharacterBody by supplying minimal data and the FBX game object for the model.
	* Added a SurvivorCreator Wizard
		* Creates a SurvivorDef and DisplayPrefab, the DisplayPrefab is taken directly from the specified CharacterBody's model.
	* Added MapNodeGroup inspector
		* MapNodeGroup inspector allows for easy placement of nodes and utilities (Thanks Anreol & IDeath)
	* EntityStateConfiguration no longer renames the asset file without consent if naming conventions are enabled.
	* Added JumpVolume Inspector
		* Automatically calculates the JumpVelocity using the Time variable and the TargetElevationTransform variable.
	* Added SurvivorDef Inspector
	* SerializableSystemType changes
		* SerializableSystemType can now obtain the required type if the field is an Array.
		* SerializableSystemType no longer fails to populate if a Type does not have an enclosing namespace.



### '3.2.2'

* Core Changes:
	* XML documentation file now always appears when ROR2EK is installed via the UPM
	* Extended editor window's opening methods now return the instance

* RoR2EditorScripts changes:
	* ESC asset name now updates to the target type when the enable naming conventions is on
	* ESC now displays a tooltip for serializable fields if the FieldInfo had the Tooltip attribute
	* Updated Skill Locator for new bonus stock override skills
	* Characterbody's crosshair prefab now properly shows in the inspector