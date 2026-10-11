# Upgrading from the Old ``Inspector<T>`` framework to the ``BaseEditor<T>`` framework.

To upgrade, you need to change your inheritance from the _old_ version to the _new_ version.

| Old Version | New Version |
|--|--|
| ``IMGUIComponentInspector`` | ``ComponentIMGUIEditor`` |
| ``VisualElementComponentInspector`` | ``ComponentVisualElementEditor`` |
| ``IMGUIScriptableObjectInspector`` | ``ScriptableObjectIMGUIEditor`` |
| ``VisualElementScriptableObjectInspector`` | ``ScriptableObjectVisualElementEditor`` |

The new version has basically the exact same method names with their exact same purpose, so the migration process should be relatively pain free.

## New framework compared to the old one

The old one had the following structure:

* Inspector: Base class, used VisualElements, had the setting capability to enable or disable itself. Requested child class to implement the UI using VisualElements
    * ScriptableObjectInspector: Added the toggle to disable or enable on the header GUI
        * VisualElementScriptableObjectInspector: Created the UIElement of the editor based off it's TypeName, and returned it to the base Inspector.
        * IMGUIScriptableObjectInspector: Create an IMGUIContainer, and returned it to the base inspector, this worked _fine_
    * ComponentInspector: Added the toggle to disable or enable on the top of the component UI using Visual Elements
        * VisualElementComponentInspector: Created the UIElement of the editor based off it's TypeName, and returned it to the base inspector. DefaultInspector used an IMGUIContainer
        * IMGUIComponentInspector: Create an IMGUIContainer, and returned it to the base inspector, _labels for fields using this where cut off by controls_. Which is the issue the new framework fixes.

The new one has the following structure:

* BaseEditor: Base class, has the setting capability to enable or disable itself, but child classes need to implement it.
    * BaseIMGUIEditor: Specifies that the editor _uses_ IMGUI. Draws the default inspector using ``DrawDefaultInspector``
        * ComponentIMGUIEditor: Used to create an inspector for a component using IMGUI. Draws the toggle for enabling or disabling on the top of the UI. Since it doesnt use an IMGUIContainer internally, it fixed the issue that ``IMGUIComponentInspector`` had.
        * ScriptableObjectIMGUIEditor: Used to create an inspector for a ScriptableObject using IMGUI. Draws the toggle for enabling or disabling on the inspector header
    * BaseVisualElementEditor: Specifies that the editor _uses_ VisualElements. Draws the default inspector using ``InspectorElement.FillDefaultInspector``.
        * ComponentVisualElementEditor: Used to create an inspector for a component using VisualElements. Draws the toggle for enabling or disabling with these as well.
        * ScriptableObjectIMGUIEditor: Used to create an inspector for a ScriptableObject using VisualElements. Draws the toggle for enabling or disabling on the inspector header.