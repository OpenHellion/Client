#if UNITY_EDITOR
[UnityEditor.InitializeOnLoad]
public class AkLinuxPluginActivator
{
	static AkLinuxPluginActivator()
	{
#if !UNITY_2019_2_OR_NEWER
		AkPluginActivator.BuildTargetToPlatformName.Add(UnityEditor.BuildTarget.StandaloneLinuxUniversal, "Linux");
		AkBuildPreprocessor.BuildTargetToPlatformName.Add(UnityEditor.BuildTarget.StandaloneLinuxUniversal, "Linux");
#endif
		AkPluginActivator.BuildTargetToPlatformName.Add(UnityEditor.BuildTarget.StandaloneLinux64, "Linux");
		AkBuildPreprocessor.BuildTargetToPlatformName.Add(UnityEditor.BuildTarget.StandaloneLinux64, "Linux");
	}
}
#endif