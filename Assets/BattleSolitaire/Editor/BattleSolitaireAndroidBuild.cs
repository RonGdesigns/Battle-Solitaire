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
            PlayerSettings.bundleVersion = "0.5.0";
            PlayerSettings.Android.bundleVersionCode = 5;
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
                OutputDirectory + "/BattleSolitaire-M5-Playtest.apk";

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
