#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BattleSolitaire.EditorTools
{
    public static class BattleSolitaireAndroidBuild
    {
        private const string ScenePath =
            "Assets/BattleSolitaire/Scenes/Battle.unity";

        private const string OutputDirectory =
            "Builds/Android";

        private const string PackageIdentifier =
            "com.rgdevelops.battlesolitaire";

        // Headless build with an optional externally installed Android toolchain.
        public static void BuildBatch()
        {
            string[] args=System.Environment.GetCommandLineArgs();
            int toolsIndex=System.Array.IndexOf(args,"-androidToolsRoot");
            if(toolsIndex>=0)
            {
                string root=Path.GetFullPath(args[toolsIndex+1]);
                var type=System.Type.GetType("UnityEditor.Android.AndroidExternalToolsSettings, UnityEditor.Android.Extensions");
                if(type==null) throw new BuildFailedException("Android editor extension is not installed.");
                foreach(var name in new[]{"sdkRootPath","ndkRootPath","jdkRootPath"})
                {
                    string child=name=="sdkRootPath"?"SDK":name=="ndkRootPath"?"NDK":"OpenJDK";
                    var property=type.GetProperty(name,System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.Static);
                    if(property==null) throw new BuildFailedException("Missing Android tools property: "+name);
                    property.SetValue(null,Path.Combine(root,child));
                    Debug.Log(name+" = "+property.GetValue(null));
                }
            }
            if(!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android,BuildTarget.Android))
                throw new BuildFailedException("Android Build Support is unavailable.");
            PrepareAndroidSettings();
            if(!File.Exists(ScenePath)) BattleSolitaireProjectSetup.SetupPlayablePrototype();
            int outputIndex=System.Array.IndexOf(args,"-apkOutput");
            string output=outputIndex>=0?Path.GetFullPath(args[outputIndex+1]):Path.GetFullPath(OutputDirectory+"/BattleSolitaire-M10-Playtest.apk");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions {
                scenes=new[]{ScenePath},locationPathName=output,target=BuildTarget.Android,
                options=BuildOptions.Development|BuildOptions.AllowDebugging
            });
            if(result.summary.result!=BuildResult.Succeeded) throw new BuildFailedException("Android build failed: "+result.summary.result);
            Debug.Log("M10_APK_BUILD_PASS: "+output);
        }

        [MenuItem("Battle Solitaire/Android/Prepare Android Settings")]
        public static void PrepareAndroidSettings()
        {
            if (!BuildPipeline.IsBuildTargetSupported(
                    BuildTargetGroup.Android,
                    BuildTarget.Android))
            {
                EditorUtility.DisplayDialog(
                    "Android Build Support Missing",
                    "Install Android Build Support, Android SDK/NDK tools, " +
                    "and OpenJDK for this Unity version in Unity Hub.",
                    "OK");
                return;
            }

            PlayerSettings.productName = "Battle Solitaire";
            PlayerSettings.companyName = "RG Develops";
            PlayerSettings.bundleVersion = "0.10.0";
            PlayerSettings.Android.bundleVersionCode = 10;
            PlayerSettings.defaultInterfaceOrientation =
                UIOrientation.Portrait;

            PlayerSettings.SetApplicationIdentifier(
                NamedBuildTarget.Android,
                PackageIdentifier);

            PlayerSettings.SetScriptingBackend(
                NamedBuildTarget.Android,
                ScriptingImplementation.IL2CPP);

            PlayerSettings.Android.targetArchitectures =
                AndroidArchitecture.ARM64;

            PlayerSettings.Android.minSdkVersion =
                AndroidSdkVersions.AndroidApiLevel26;

            PlayerSettings.Android.targetSdkVersion =
                AndroidSdkVersions.AndroidApiLevelAuto;

            PlayerSettings.allowedAutorotateToPortrait = true;
            PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
            PlayerSettings.allowedAutorotateToLandscapeLeft = false;
            PlayerSettings.allowedAutorotateToLandscapeRight = false;

            EditorUserBuildSettings.buildAppBundle = false;

            AssetDatabase.SaveAssets();

            Debug.Log(
                "Battle Solitaire Android settings prepared: " +
                PackageIdentifier + " / ARM64 / min API 26 / IL2CPP.");
        }

        [MenuItem("Battle Solitaire/Android/Build Play-Test APK")]
        public static void BuildPlayTestApk()
        {
            if (!BuildPipeline.IsBuildTargetSupported(
                    BuildTargetGroup.Android,
                    BuildTarget.Android))
            {
                EditorUtility.DisplayDialog(
                    "Android Build Support Missing",
                    "Install Android Build Support in Unity Hub before building.",
                    "OK");
                return;
            }

            if (!File.Exists(ScenePath))
            {
                EditorUtility.DisplayDialog(
                    "Battle Scene Missing",
                    "Run Battle Solitaire > Setup Playable Prototype first.",
                    "OK");
                return;
            }

            PrepareAndroidSettings();

            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(
                    BuildTargetGroup.Android,
                    BuildTarget.Android))
            {
                EditorUtility.DisplayDialog(
                    "Android Switch Failed",
                    "Unity could not switch the active build target to Android.",
                    "OK");
                return;
            }

            Directory.CreateDirectory(OutputDirectory);

            string outputPath =
                OutputDirectory + "/BattleSolitaire-M10-Playtest.apk";

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = outputPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options =
                    BuildOptions.Development |
                    BuildOptions.AllowDebugging
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);

            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log(
                    "Battle Solitaire APK built successfully: " +
                    report.summary.outputPath);

                EditorUtility.DisplayDialog(
                    "APK Built",
                    "Play-test APK created at:\n\n" +
                    report.summary.outputPath,
                    "OK");

                EditorUtility.RevealInFinder(report.summary.outputPath);
            }
            else
            {
                Debug.LogError(
                    "Battle Solitaire APK build failed: " +
                    report.summary.result);

                EditorUtility.DisplayDialog(
                    "APK Build Failed",
                    "Open the Unity Console for the build error details.",
                    "OK");
            }
        }
    }
}
#endif
