#if UNITY_EDITOR
using BattleSolitaire.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BattleSolitaire.EditorTools
{
    public static class BattleSolitaireProjectSetup
    {
        private const string SceneFolder =
            "Assets/BattleSolitaire/Scenes";

        private const string ScenePath =
            "Assets/BattleSolitaire/Scenes/Battle.unity";

        [MenuItem("Battle Solitaire/Setup Playable Prototype")]
        public static void SetupPlayablePrototype()
        {
            EnsureFolder(
                "Assets/BattleSolitaire",
                "Scenes");

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            var root = new GameObject("Battle Solitaire Game");
            root.AddComponent<BattleGameController>();

            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            PlayerSettings.productName = "Battle Solitaire";
            PlayerSettings.companyName = "RG Develops";
            PlayerSettings.defaultInterfaceOrientation =
                UIOrientation.Portrait;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeGameObject = root;

            EditorUtility.DisplayDialog(
                "Battle Solitaire",
                "Playable prototype setup is complete.\n\n" +
                "Press Play to start a local battle against the AI.\n" +
                "The Battle scene is also enabled in Build Settings.",
                "Play");
        }

        [MenuItem("Battle Solitaire/Open Battle Scene")]
        public static void OpenBattleScene()
        {
            if (!System.IO.File.Exists(ScenePath))
            {
                SetupPlayablePrototype();
                return;
            }

            EditorSceneManager.OpenScene(
                ScenePath,
                OpenSceneMode.Single);
        }

        private static void EnsureFolder(
            string parent,
            string child)
        {
            string path = parent + "/" + child;

            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
