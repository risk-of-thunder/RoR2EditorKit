# Contributing

RoR2EditorKit has become a part of Risk of Thunder due to the desire to make it's features community backed. We invite everyone to contribute to R2EK to make the editor experience more smooth for everyone. Here are a few Guidelines in order to make your contribution as easy as possible.

## Guidelines

To be able to mantain the project, we try to stick to the following rules:

* No system in RoR2EditorKit should have any need of implementing a runtime solution, even if it fixes annoyances such as Shaders.
* No need to keep code commented out, there's no point to it and only makes reading thru code harder than it should be.
* Removing release exposed code is only possible on major releases. This is to avoid uneeded issues related to plugins that depend on RoR2EditorKit's systems. In case something needs to be deprecated, it should be marked as obsolete, but not removed.
* Do not ask for Gearbox employees for their editor scripts, since they cannot provide them. however, you *are* allowed to ask for a screenshot of how the feature looks in the editor to have an idea on what you need to implement.
* Do not use Publicized assemblies. If you need to access methods of types that are private or public, use reflection.
* Any script from the assemblies should be within the ``RoR2.Editor`` namespace

## Pull Requests

Pull requests are welcomed but may be rejected for the reasons mentioned above, please try to mantain high code quality.
