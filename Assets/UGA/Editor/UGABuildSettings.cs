using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.Rendering;

public class UGABuildSettings : IPreprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPreprocessBuild(BuildReport report)
    {
        // Enforce that all webgl builds have OpenGLES3 as their only graphics api
        // This ensures compatability with UGA Assets
        if (report.summary.platform == BuildTarget.WebGL)
        {
            PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { GraphicsDeviceType.OpenGLES3 });
        }
    }
}
