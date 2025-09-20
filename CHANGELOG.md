# '5.5.2'

### RoR2ScriptsChanges

* Fixed an issue where the AssetReferenceT drawer would throw an exception when no asset types are returned.

# '5.5.1'

## Core Changes:

* Added back the old ``GetValue`` and ``SetValue`` ``SerializedProperty`` extensions.
	* This retroactively fixes [#23](https://github.com/risk-of-thunder/RoR2EditorKit/issues/23).

### RoR2ScriptsChanges

* The AddressablesPathDictionary can now return all GUIDS/Paths that conform to multiple types.
* Added a PropertyDrawer for ItemDisplayRules, reducing information shown when applicable and adding a button for pasting the values taken from the ItemDisplayPlacementHelper. (Fixes [#24](https://github.com/risk-of-thunder/RoR2EditorKit/issues/24))
* Added a PropertyDrawer for IDRSKeyAssetReference, allowing you to easily select an IDRS Key Asset address. 

# '5.5.0'

### Core Changes:

* All the setting providers have been consolidated into their respective setting classes, removing some files as a result.
* The ExtendedPropertyDrawer and IMGUIPropertyDrawer now set the serializedProperty when ``GetPropertyHeight()`` is called 
* Added the R2EKPreferences preference setting.
  * Used to store the game's executable path, this lays the foundation for more features in the future.

### RoR2Scripts Changes

* Added the AddressablesPathDictionary, which is a struct that contains the deserialized data of the game's "lrapi_returns.json" file.
* Added a the dropdown, the ``AddressablesPathDropdown``, used to obtain a base game's guid via an address path.
* Added a default drawer for ``AssetReferenceT``, which uses the ``AddressablesPathDropdown``
  * this drawer is disabled if the Addressables Package is installed.

### R2APIScripts Changes

* The AddressReferencedAssetDrawer has been revamped to include new functionality.
  * Added the ability to pick an asset's guid address via a dropdown. which is filtered to accept only valid assets.
  * You can now specify wether an asset can be loaded from the catalog or not via the context menu.

# '5.4.1'

### Core Changes:

* FilePickerPath drawer's extension property now properly accounts wether the attribute included the extension dot or not.

### RoR2Scripts changes:

* Fixed the Hitbox Gizmo not using Lossy Scale
* The assembly now enables as long as any version of RoR2 is installed.

### R2APIScripts Changes

* Fixed the StageSerde drawer not having the necesary precompile directive clauses
* Fixed the AddressableFamilyDirectorCardCategorySelection drawer not working.

# '5.4.0'

### General:

* Reformatted the changelog so it has less indentation

### Core Changes:

* The ``CodeGeneratorValidator`` now re-imports any modified scripts.
* Implemented an override for ``AssetGUID<T>``'s ``ToString``
	* By default it returns the path of the asset it represents, a boolean can be specified to return the GUID as a hexadecimal string instead.
* The R2EKSettings window opens until the welcome HelpBox is Dismissed

### RoR2Scripts Changes:

* Implemented a Decorator Drawer for ``ShowFieldObsoleteAttribute``
* Reduced the overall real estate for ``NamedObjectReferencePropertyDrawer``'s labels
* Removed ``Remove Node Excess`` and replaced it with ``Remove Unconnected Nodes`` (Thanks Villiger)
* Fixed an issue where the ``EntityStateConfiguration`` inspector would fail to properly display its fields (Thanks I-Eye)

### ThunderKitSupport Changes

* Implemented multiple changes to the Mod Creation Wizard
	* Mod can now specify a URL, which is used to populate a ``ThunderstoreData`` manifest datum
	* Fixed an issue where sometimes referenced assemblies could have a null path or be misreferenced, causing exceptions that caused either code dependency or manifest dependencies to not apply properly
	* Written BepInDependencies on the generated main class now only writes the top level dependencies
	* Fixed issue where the generated Content class would have parts of it commented out
	* Wizard now creates a ReadMe, ChangeLog and Icon assets.
	* Wizard now adds a Files datum and a ThunderstoreData datum
	* Added the following default references:
		* Unity.Collections
		* Unity.Mathematics
		* Unity.Burst
		* BurstsOfRain

# '5.3.0'

### RoR2Scripts Changes:

* Made multiple changes and improvements to the ``MapNodeGroup`` inspector
	* Fixed the SceneGUI not updating as often as necesary, causing a choppy experience.
	* Fixed certain utilities that where supposed to be connected to editor settings not being properly updated.
	* Added a "HitPoint position" scene field to display the position where the cursor is.
	* Re-implemented the node painter utility.
		* Allows you to select a size for the brush
		* Pressing ``B`` by default paints nodes on the selected area
			* This utility will not be directly supported by the main developer
	* Added a button to bake the nodes "asynchronously"
		* Uses a Coroutine for baking the nodes
		* Coroutine yields more often with less node counts, specially noticeable when the node count jumps by magnitudes.
		* Added a ProgressBar that's used when the inspector is baking nodes asynchronously

# '5.2.2'

### Core Changes:

* The ``InheritingTypeSelectDropdown`` can now use the type's full name instead of the type name
	* These are preference settings tied to the drawers that use this feature.
* Fixed an issue where the ``EditorSettingsElement`` would sometimes display settings that are orphaned in the current editor context.
* Project Settings are now consolidated under a foldout for ease of access.
* Added menu items for the following actions:
	* Refreshing the Asset Database (``Tools/RoR2EditorKit/Utility/Refresh Asset Database``)
	* Saving the Asset Database (``Tools/RoR2EditorKit/Utility/Save Assets``)
	* Requesting a Script Reload (``Tools/RoR2EditorKit/Utility/Reload Domain``)
* ``IOUtils.GenerateUniqueFileName`` now has a fallback check in the case that the end user doesnt provide the ``.`` char for the ``extension`` parameter.

### RoR2Scripts Changes:

* Added new PropertyDrawers for both EntityStateMachines and GenericSkills (Thanks TheTimeSweeper!)
	* As a result for the EntityStateMachine drawer, the ``NetworkEntityStateMachineInspector`` has been removed.
* The ``SerializableEntityStateTypeDrawer`` and the ``SerializableSystemTypeDrawer`` now fully qualify the base game's types.
* Fixed bug where the ``SerializableEntityStateTypeDrawer`` or the ``SerializableSystemTypeDrawer`` would change values without the user's consent.

### R2APISupport Changes:

* Added support for the ``AddressReferencedFamilyDirectorCardCategorySelection``.

# '5.2.1'

### Core Changes:

* Fixed the Editor Wizard Windows binding to a serialized object too early, causing controls built from property fields to not display.

### R2APISupport Changes:

* Fixed the StageSerde drawer not showing correct values for its label under specific circumstances.

### RoR2Scripts Changes:

* Fixed the Node Graph inspector not working due to a null reference exception on enable


# '5.2.0'

### R2APIEditorScripts Changes:

* Renamed the assembly to follow the naming convention of the rest of the assemblies.
* Added a property drawer for the struct ``DirectorAPI.StageSerde``
		* The property drawer showcases the value as a mask of the Stage enum, this was done because unity cannot serialize enums greater than 32 bits.

# '5.1.1'

### General

* Updated to use version 1.3.4 of Risk of Rain 2
* RoR2EditorKit now loads as long as a version of the game greater than 1.3.1 is loaded, instead of being a strict version rule.

### Core Changes:

* Removed the code bits of the EditorSettings pertraining to the ThunderKit integration

# '5.1.0'

### General Changes:

* Non Core Assemblies (RoR2.Editor.Scripts, RoR2.Editor.R2API, RoR2.Editor.Thunderkit) will only load when the required packages are installed utilizing the Version Defines system of AssemblyDefinitions.

### Core Changes:

* Removed logs from ``AssetDatabase.LoadAssetFromGUID`` to avoid generating multiple logs when being called by IMGUI related methods.
* Removed "Enable ThunderKit Integration" option from the main settings, as the ThunderKit integration now becomes enabled and disabled automatically depending on the AssemblyDef's Version Defines.

# '5.0.4'

### Core Changes:

* ``SerializableShaderWrapperDrawer`` now uses ``nameof`` to avoid not finding the correct properties.

# '5.0.3'

### Core Changes:

* Fixed ``SerializableShaderWrapperDrawer`` still using the old property names for finding the ``SerializedProperties``, causing null reference exceptions.
* Added back ``DrawCheckableProperty`` functionality to IMGUIUtil
* Added a ``SerializedProperty`` extension to generate it's GUIContent,

### ThunderKitSupport Changes:

* Re-Added the ``MarkdownUtility`` class

# '5.0.2'

### RoR2EditorScripts Changes:

* Fixed the assembly not referencing the DLL version of HLAPI alongside the AsmDef version of HLAPI

### R2APIEditorScripts Changes:

* Fixed the AddressReferencedAsset drawer not being updated to use the new R2EKConstants class

# '5.0.1'

### Core Changes:

* Fixed an issue where the VisualElementTemplateDictionary wouldnt properly serialize GUIDs.
* Added proper toggle for automatic purging of project settings.
* user preferences will no longer be purged of settings.
* The naming conventions toggle is now disabled and defaults to false, since naming convention systems havent been implemented yet.
* Deleted the "Legacy.zip" file from the project. Version control exists for a reason.

# '5.0.0'
 
### General Changes

* Updated to use Unity version 2021.3.33
* Rewrote most of the package from scratch.
* ThunderKit is no longer a dependency
* Added a dependency to the EditorCoroutines package.
* Some Types have been renamed, but their remains functionality unchanged.
* Added Bombardier font, which can be used in UI
* Namespaces have been consolidated into the ``RoR2.Editor`` namespace

### Core Changes:

* Core assembly has been renamed to ``RoR2.Editor``
* Added the EditorSetting System
	* This system can be used to generically save settings from inspectors, such as colors, floats, etc.
	* These are stored within classes that implement ``EditorSettingManager.IEditorSettingProvider`` interfaces. (Usually Scriptable Singletons)
	* By default, both Project wide and User specific settings are available.
	* Inspectors being enabled are now considered a Use specific setting
	* There's an ``EditorSettingElement`` which can be used to display the settings within a setting provider, and edit with custom controls.
	* You can tie an element that implements ``INotifyValueChanged<T>`` to a specific setting, useful for complex inspectors
* Added a Code Generation system
	* The code generation does not use Roslyn Analizers, instead it works via the ``Writer`` struct, and the ``CodeGeneratorValidator`` and ``CodeGeneratorHelper`` classes.
* Added a ``InheritingTypeSelect`` advanced dropdown.
* Implemented a solution for finding and caching Visual Element Templates.
* Inspectors and PropertyDrawers can now be created using IMGUI or Visual Elements.
* Added a unity package which contains custom script icons for RoR2 Scripts (Contribution by Groove Salad)
* HelpBoxes can now be Dismissed
* Added ``AssetGUID<T>``, which is a struct used to save references to assets using their guids.
* The HG.GeneralSerializer.SerializeFieldCollection can now be drawn using a custom Visual Element.
	* This allows easy management of custom serialization setups for mods.
* Added the SerializationMediator
	* The SerializationMediator works as a Facade and Mediator for the internal EditorStringSerializer
	* It ensures that only specific types can be serialized under the current project's context.
		* This allows the EntityStateConfiguration inspector to only serialize valid fields, if R2API.StringSerializerExtensions is added, the support is expanded to the new handlers added by said module.
	* The SerializationMediator can be bypassed by utilizing the ``SafetyBypass`` class
		* This class allows you to directly serialize with the internal Editor String Serializer.
* Fields for Types that can be serialized can now be created using IMGUI or Visual Elements
* Editor Wizards now run as a Coroutine instead of async tasks.
	* This allows you to use the C++ side of the engine without fear of calling the methods outside of the main thread.
* Editor Wizards now have a progress bar, which can be used to judge how long until the wizard finishes execution.
* Removed all obsolete members
* Removed the following publicly available classes:
	* AddressablesUtilities
	* ErrorShorthands
	* IObjectNameConvention
	* Markdown Utils
	* ScriptableObjectUtils
	* TypeCacheRequester
	* ALL ManifestDatums
	* ALL Pipeline Jobs
	* Element Resizer Manipulator
	* Extended List View
	* Validating Field
	* Validating Object Field
	* IMGUI to Visual Element Inspector
		* These removals may or may not be added back in a future update, some of these have been replaced by incredible advancements in the editor between 2019 and 2021.

### RoR2EditorScripts changes:

* Assembly has been renamed to ``RoR2.Editor.Scripts``
* Assembly is only loaded if the game was imported via ThunderKit.
* Fixed various issues related to the CharacterBody wizard.
* StageWizard can now create node graphs, DCCS and DCCSPools
* Various preferences have been added to the NodeGraph inspector
* Removed the following classes:
	* Characterbody Inspector
	* CharacterModel inspector
	* ObjectScaleCurve Inspector
	* SkillLocator Inspector
	* ArtifactCompoundDef Inspector
	* AssetCollectionInspector
	* BuffDef Inspector
	* EliteDef Inspector
	* EquipmentDef Inspector
	* SurivvorDef Inspector
	* ScaleHelper Window
	* Survivor Creator Wizard
		* These removals may or may not be added back in a future update.
* ThunderKit related features, such as the ModWizard, has been moved to a separate assembly.

### R2APIEditorScripts changes:

* Assembly has been renamed to ``RoR2.Editor.R2API``
* Removed the R2APISerializableContentPack inspector

# '4.1.0'

### General Changes:

* Assemblies no longer compile if the ``RISKOFRAIN2`` definition is missing in the project
* Added editor scripts for R2API
	* Added inspector for R2APISerializableContentPack
	* Added property drawers for AddressReferencedAssets
	* Assembly only compiles if either ``RiskOfThunder-R2API_ContentManagement`` or ``RiskOfThunder-R2API_Addressables`` are present in the project.

### Core Changes:

* Fixed issue where a null reference exception gets thrown if the XMLDoc becomes missing
* Fixed minor issues on the ValidatingObjectField VisualElement

### RoR2EditorScripts Changes

* Added a new script to avoid having NGSS_DIRECTIONAL saving an Addressable Texture, causing issues with lighting

# '4.0.4'

* Fixed the assembly definition for AssetRipper's YAML shader patches not being editor only

# '4.0.3'

* Made the ROR2EK release pipelines path components more uniquely named

# '4.0.2'

### Core Changes

* Re-implemented the AssetRipper YAML shader patches, as they where removed from the repository on accident

### RoR2EditorScripts Changes

* Hitbox Gizmo Drawer now uses the DrawWireMesh gizmo to draw a cube that properly represents the hitbox's rotation

# '4.0.1'

### Core Changes:

* Fixed NullReferenceException caused by setting the ExtendedListView's collectionProperty to 0
* Implemented "Duplicate Element" context menu for ExtendedListView's entries.

# '4.0.0'

### General Changes:

* A general namespace changing and type moving has been made, types have been moved to different namespaces, the Namespace system now looks to encapsulate the most useful aspects of RoR2EK into the RoR2EditorKit namespace, while maintaining specific systems in different namespaces.
* The AssetRipper patch that fixes YAML shaders getting their data corrupted is now a separate assemblyDef to ensure it always loads.

### Core Changes:

* Added Extended versions of PropertyDrawers
	* The ExtendedPropertyDrawer which is the base class for the others, it has a property to directly Get or Set the value of the serialzied property directly.
	* IMGUIPropertyDrawer, which is used for creating universal PropertyDrawers using IMGUI
	* VisualElementPropertyDrawer, much like the rest classes that use PropertyDrawers, it uses UXML Templates that are fetched by looking for a UXML file with the same name as it's type.
* Deprecated ListViewHelper and PropertyValidator
* Deprecated ExtendedInspector's CreateHelpBox() and AddSimpleContextMenu() methods
* ExtendedEditorWindows can now be dynamically bound and unbound to SerializedObjects
* Changed the method arguments for opening ExtendedEditorWindows
* Added new custom VisualElement Controls.
	* ContextualMenuWrapper
		* Create Easy to see and interact ContextMenus for any VisualElement, the ContextMenu is stored inside a small VisualElement Icon that is easy to access and find, the icon can be changed to anything you need.
	* ExtendedListView
		* A Replacement for the ListViewHelper, the ExtendedListView is used for creating complex ListViews that use handmade binding elements, alongside the same functions that the original ListViewHelper had, the ExtendedListView also has new features, such as resizable height and inherent ContextMenu support
	* HelpBox
		* A Replacement for creating IMGUIContainers and calling EditorGUILayout.HelpBox(), the HelpBox element relies information to the end user, the message of the help box can either be Explicit (shown on a label), or Implicit (shown as a tooltip when hovering over the help box icon)
	* ValidatingField
		* A Replacement for the PropertyValidator, the ValidatingField is used for creating Validation Methods for fields, using the new HelpBox elements, the messages created by a ValidatingField are now stored inside a ScrollView element on top of the field.
		* Added ValidatingPropertyField and ValidatingObjectField
* Moved the VisualElement related extensions to VisualElementUtil
* Implemented KingEnderBrine's FixedConditionalWeakTable
* Added a TypeCacheRequester, which allows you to request a collection of types in the current AppDomain

### RoR2EditorScripts changes:

* Added a PropertyDrawer for TypeRestrictedReferenceAttribute
* Updated EnumMaskDrawer, PrefabReferenceDrawer, and SkillFamilyVariantDrawer to use the new extended versions of property drawers.
* SerializedEntityState and SerializedSystemType drawers now sort their types alphabetically
* Updated all occurances of using PropertyValidator, ListViewHelper and EditorGUILayout.HelpBox() to use the ValidatingPropertyField, ExtendedListView and HelpBox elements instead.
	
# '3.5.2'

### RoR2EditorScripts changes:

* Fixed the R2APIMigrationWizard and ModCreatorWizard failing to find the split R2API assemblies.

# '3.5.1'

### RoR2EditorScripts changes:

* Fixed missing binding paths on R2APIMigrationWizard

# '3.5.0'

### Core Changes:

* ExtendedMaterialEditor now attempts to get the action for a material, if it doesnt find any, it'll use the default inspector.
* ObjectEditingWindow's TargetType is now just a getter, obtaining the target type directly from the SerializedObject.

### RoR2EditorScripts changes:

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

# '3.4.0'

### RoR2EditorScripts Changes:

* Added a Scaling Tool window, used to roughly see how big something is compared to ror2 bodies and stages.
* Changed the EntityStateConfiguration inspector to use the old IMGUI version instead of visual elements.
	* All improvements made are also in the IMGUI inspector.
	* EntityStateConfiguration inspector no longer tries to serialize fields marked as constant
* Added a Gizmo for visualizing the scale of a HitBox

# '3.3.1'

### Core Changes:

* Added SerializableShaderWrapper
	* Serializes shaders by serializing their shader.name and their GUID's
* The material editor now works properly by using the SerializableShaderWrapper
* Added new utilities to the AssetDatabase utils class
* Added AssetRipper's Shader asset post processors.
	* This in turn should fix unity destroying YAML shader assets.

### RoR2EditorScripts changes:

* Stage Creator wizard now prefixes the scene's name with the token prefix
* Fixed an issue where having mutliple inspectors would break the Skill Locator inspector.
* Fixed an issue where the CanBeRandomlyTriggered bool for EquipmentDefs wouldnt show
* Exposed HGCloudRemap's Internal Simple Blend Mode property

# '3.3.0'

### Core Changes:

* Improved material editor so it doesnt wipe itself
* Added wizard systems
	* A wizard allows you to create complex jobs for your project.
* All the asset GUID contants are now under Constants.AssetGUIDS class.
	* Added a QuickLoad and GetPath methods for AssetGUIDS class.
* Added FormatPathForUnity and GetCurrentDirectory to IOUtils.
* Extended inspector's object name conventions have been extended.
	*  Can now specify both a custom help box message and custom name validation function
* Component Inspectors now always have the fancy enable toggle visual element.
	
### RoR2EditorScripts changes:

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
	* SerializableSystemType no longer fails to populate if a Type does not have an enclosing namespace.# '3.2.2'

### Core Changes:

* XML documentation file now always appears when ROR2EK is installed via the UPM
* Extended editor window's opening methods now return the instance

### RoR2EditorScripts changes:

* ESC asset name now updates to the target type when the enable naming conventions is on
* ESC now displays a tooltip for serializable fields if the FieldInfo had the Tooltip attribute
* Updated Skill Locator for new bonus stock override skills
* Characterbody's crosshair prefab now properly shows in the inspector

# '3.2.1'

### Core Changes:

* Cleaned up the code
* Added XML documentation file
* ListViewHelper now has a refresh method

### RoR2EditorScripts changes:

* Cleaned up the code

# '3.2.0'

### Core Changes:

* Added "GetParentProperty" extension for SerializedProperty
* Added "SetDisplay" extension for VisualElements
* ListViewHelper's SerializedProperty can now be changed, allowing for dynamic use of a ListView
* ListViewHelper's created elements now have the name "elementN", a substring can be used to get the index of the serialized property
* Improved the ExtendedEditorWindow:
	* Now works like pre 2.0.0 ExtendedEditorWindow
	* Still uses VisualElements
	* ExtendedEditorWindows can load their UI via TemplateHelpers
	* Contains a SerializedObject that points to the instance of the ExtendedEditorWindow
* Added ObjectEditingEditorWindow
	* ObjectEditingEditorWindow's main usage is for constructing more complex editing tools for objects
	* ObjectEditingEditorWindow's SerializedObject points to the inspected/editing object

### RoR2EditorScripts changes:

* Added an AssetCollectionInspector

# '3.1.0'

### Core Changes:

* Added Missing XML Documentation
* Added "HasDoneFirstDrawing" property to ExtendedInspector
* Added "ListViewHelper" class
* PropertyValidator now works on PropertyFields, as well as any VisualElement that implements "INotifyValueChanged"
* Made the returning value of the PropertyValidator's Functions nullable (Returning null skips the container drawing process)
* Removed UtilityMethods from ExtendedEditorWindow
* Improved the look of the MaterialEditorSettings and EditorInspectorSettings inspectors & settings window

### RoR2EditorScripts changes:

* Redid the following inspectors to use VisualElements:
	* ChildLocator
	* EntityStateConfiguration
	* ObjectScaleCurve
* Readded Tooltip and Labeling from NetworkStateMachine feature
* Added SerializableContentPack inspector


# '3.0.2'

### RoR2EditorScripts changes:

* Made assembly Editor Only

# '3.0.1'

### Core Changes:

* Fixed ScriptalbeObjectInspector drawing multiple InspectorEnabled toggles (3 inspectors = 3 toggles)
* Added ReflectionUtils class

### RoR2EditorScripts changes:

* Changed the method that the EntityStateDrawer and SerializableSystemTypeDrawer uses for getting the current Types in the AppDomain

# '3.0.0'

### General Changes:

* Transformed the main repository from a __Project Repository__ to a __Package Repository__ (This change alone justifies the major version change)

### Core Changes:

* Improvements to the Exnteded Inspector:
	* Reworked the naming convention system into the IObjectNameConvention interface
	* Made HasVisualTreeAsset virtual
	* Removed "Find" and "FindAndBind" methods
	* Added AddSimpleContextMenu, simplified version for creating context menus for VisualElements
	* Added PropertyValidator class, used for validating PropertyFields. Evaluate the states of the property fields and append helpBoxes for end users
* Removed most USS files, added ComponentInspector.uss and ScriptableObjectInspector.uss
* Added a ComponentInspectorBase.UXML
* Added the following Extensions and Utilities:
	- KeyValuePair deconstructor from R2API
	- UpdateAndApply extension for ScriptableObject
	- QContainer for Foldout Elements
	- GetRootObject for GameObjects
	- Added AddressableUtils

### RoR2EditorScripts changes:

* Updated the aspect of the following inspectors:
	- ItemDef
	- EquipmentDef
	- EliteDef
	- BuffDef
	- SkillLocator
	- NetworkStateMachine
	- CharacterBody
* Added HurtBoxGroup inspector (Auto population of array)
* Added CharacterModel inspector (Auto population of renderers and lights)
* Standarized the naming conventions of certain scriptableObjects to be truly in line with Hopoo's naming conventions
* Most buttons in various inspectors are now replaced by ContextMenus
* Removed Tooltip and Labeling from NetworkStateMachine for now
* Token Setting actions now take into consideration objects with Whitespaces by removing them
* EliteDef cannow set the health and damage boost coefficient to pre SOTV tiers (T1Honor, T1 & T2)
* CharacterBody's baseVision field can now be set to infinity
* BaseStats can now be set to common vanilla body stats:
	* Commando
	* Lemurian
	* Golem
	* BeetleQueen
	* Mithrix

# '2.2.1'

### General Changes:

* Updated AssemblyDefinitions to reference via name instead of GUIDs (Thanks Passive Picasso!)

# '2.2.0'

* General Changes
* Updated to use TK 5.0
* Updated to use RoR2 1.2.3
* The RoR2 scripts are now in the "RoR2EditorScripts" assembly

### Core Changes:

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

### RoR2EditorScripts changes:

* Added an ArtifactCompoundDef Inspector
* Added an ItemDef Inspector
* Reimplemented the SkillFamilyVariant property drawer
* Made all the classes in the "RoR2EditorScripts" assembly sealed
* Removed the HGButton Inspector, this removes the unstated dependency on unity's UI package, Deleting the UIPackage's button editor is a good and simple workaround to make HGButton workable.

# '2.1.0'

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

# '2.0.2'

* Fixed an issue where ExtendedInspectors would not display properly due to incorrect USS paths.
* Added ValidateUXMLPath to ExtendedInspector, used to validate the UXML's file path, override this if youre making an ExtendedInspector for a package that depends on RoR2EK's systems.
* Added ValidateUXMLPath to ExtendedEditorWindow
* Hopefully fixed the issue where RoR2EK assets can be edited.

# '2.0.1'

* Fixed an issue where ExtendedInspectors would not work due to incorrect path management.

# '2.0.0'

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

# 1.0.0

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

# 0.2.4

* Made sure the Assembly Definition is Editor Only.

# 0.2.3

* Added the ability for the EntityStateConfiguration inspector to ignore fields with HideInInspector attribute.

# 0.2.2

* Added 2 new Extended Inspector inheriting classes
    * Component Inspector: Used for creating inspectors for components.
    * ScriptableObject Inspector: Used for creating inspectors for Scriptable Objects.
* Modified the existing inspectors to inherit from these new inspectors.
* Added an inspector for HGButton
* Moved old changelogs to new file

# 0.2.1

* Renamed UnlockableDefCreator to ScriptableCreators
* All the uncreatable skilldefs in the namespace RoR2.Skills can now be created thanks to the ScriptableCreator
* Added an EditorGUILayoutProperyDrawer
    * Extends from property drawer.
    * Should only be used for extremely simple property drawer work.
    * It's not intended as a proper extension to the PropertyDrawer system.
* Added Utility methods to the ExtendedInspector

# 0.2.0

* Added CreateRoR2PrefabWindow, used for creating prefabs.
* Added a window for creating an Interactable prefab.
* Fixed an issue where the Serializable System Type Drawer wouldn't work properly if the inspected type had mode than 1 field.
* Added a fallback on the Serializable System Type Drawer
* Added a property drawer for EnumMasks, allowing proper usage of Flags on RoR2 Enums with the Flags attribute.

# 0.1.4

* Separated the Enabled and Disabled inspector settings to its own setting file. allowing projects to git ignore it.
* The Toggle for enabling and disabling the inspector is now on its header GUI for a more pleasant experience.

# 0.1.2

* Fixed no assembly definition being packaged with the toolkit, whoops.

# 0.1.1

### RoR2EditorKitSettings:

* Removed the "EditorWindowsEnabled" setting.
* Added an EnabledInspectors setting.
	* Lets the user choose what inspectors to enable/disable.
* Added a MainManifest setting.
	* Lets RoR2EditorKit know the main manifest it'll work off, used in the SerializableContentPackWindow.

### Inspectors:

* Added InspectorSetting property
	* Automatically Gets the inspector's settings, or creates one if none are found.
* Inspectors can now be toggled on or off at the top of the inspector window.
    
### Editor Windows: 

* Cleaned up and documented the Extended Editor Window class.
* Updated the SerializableContentPack editor window:
	* Restored function for Drag and Dropping multiple files
	* Added a button to each array to auto-populate the arrays using the main manifest of the project.

# 0.1.0

* Reorganized CreateAsset Menu
* Added EntityStateConfiguration creator, select state type and hit create. Optional checkbox for setting the asset name to the state's name.
* Added SurvivorDef creator, currently halfway implemented.
* Added BuffDef creator, can automatically create a networked sound event for the start sfx.
* Removed EntityStateConfiguration editor window.
* Implemented a new EntityStateConfiguration inspector
* Internal Changes

# 0.0.1

* Initial Release