# RoR2EditorKit - Editor Utilities, Inspectors and More for Risk of Rain2

## About

RoR2EditorKit (abbreviated as R2EK) is a Unity Editor Package for developing and inspecting components from Risk of Rain 2 within theUnityEditor, providing a myriad of inspectors, property drawers, editor windows and more.

At it's core, RoR2EditorKit should not have any classes or systems that depend on runtime usage, R2EK works exclusively for speeding up the modding enviroment.

Thunderkit is supported and it has some capabilities with it, however, R2EK's core systems can run completely standalone if desired.

## Manual Installation

To Download RoR2EditorKit to your project, it is recommended that you add it via Unity's PackageManager by utilizing the latest tagged release.

    Example: https://github.com/risk-of-thunder/RoR2EditorKit.git#5.0.0

Once installed, it is heavily recommended to see the RoR2EditorKit Settings section of the ProjectSettings window to modify certain settings that R2EK 
Once installed, it is heavily reccommended to open the ThunderkitSettings window to modify certain settings that R2EK will use while helping you develop the mod.

## Extending RoR2EditorKit's Functionality.

R2EK has it's core assembly which can be used to extend the functionality of the package to create your own inspectors, property drawers and editor windows utilizing the capabilities of R2EK, you can look into this wiki page that explains how to extend the editor's functionality using R2EK's systems.

[link](https://github.com/risk-of-thunder/RoR2EditorKit/wiki/Extending-the-Editor's-Functionality-with-RoR2EditorKit's-Systems.)

## Contributing

Contributing to R2EK is as simple as creating a fork, creating an empty project and cloning the repo into said project's "Packages" folder

A more detailed Contribution guideline can be found [here](https://github.com/risk-of-thunder/RoR2EditorKit/blob/main/CONTRIBUTING.md)