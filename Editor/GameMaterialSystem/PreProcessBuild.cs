using UnityEditor.Build.Reporting;
using UnityEditor.Build;
using UnityEngine;

public class MyCustomBuildProcessor : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }
    public void OnPreprocessBuild(BuildReport report)
    {
        foreach(var thing in report.packedAssets)
        {
            Debug.Log(thing.shortPath);
        }
        Debug.Log("MyCustomBuildProcessor.OnPreprocessBuild for target " + report.summary.platform + " at path " + report.summary.outputPath);
    }
}