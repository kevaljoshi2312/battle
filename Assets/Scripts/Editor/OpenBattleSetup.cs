#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class OpenBattleSetup
{
    const string CreateSceneMenu = "Battle/Create Open Field Scene";
    const string SetupMenu = "Battle/Setup Milestone 11 (Open Field)";
    const string ScenePath = "Assets/Scenes/OpenBattle.unity";

    static readonly (string Name, UnitType Type, Vector3 Position)[] PlayerUnits =
    {
        ("Defender_1", UnitType.Defender, BattleSetupShared.FormationPosition(0, BattlefieldLayout.OpenFieldSquadSize, BattlefieldLayout.OpenFieldPlayerDefenderZ)),
        ("Defender_2", UnitType.Defender, BattleSetupShared.FormationPosition(1, BattlefieldLayout.OpenFieldSquadSize, BattlefieldLayout.OpenFieldPlayerDefenderZ)),
        ("Defender_3", UnitType.Defender, BattleSetupShared.FormationPosition(2, BattlefieldLayout.OpenFieldSquadSize, BattlefieldLayout.OpenFieldPlayerDefenderZ)),
        ("Attacker_1", UnitType.Attacker, BattleSetupShared.FormationPosition(0, BattlefieldLayout.OpenFieldSquadSize, BattlefieldLayout.OpenFieldPlayerAttackerZ)),
        ("Attacker_2", UnitType.Attacker, BattleSetupShared.FormationPosition(1, BattlefieldLayout.OpenFieldSquadSize, BattlefieldLayout.OpenFieldPlayerAttackerZ)),
        ("Attacker_3", UnitType.Attacker, BattleSetupShared.FormationPosition(2, BattlefieldLayout.OpenFieldSquadSize, BattlefieldLayout.OpenFieldPlayerAttackerZ)),
        ("Archer_1", UnitType.Archer, BattleSetupShared.FormationPosition(0, BattlefieldLayout.OpenFieldSquadSize, BattlefieldLayout.OpenFieldPlayerArcherZ)),
        ("Archer_2", UnitType.Archer, BattleSetupShared.FormationPosition(1, BattlefieldLayout.OpenFieldSquadSize, BattlefieldLayout.OpenFieldPlayerArcherZ)),
        ("Archer_3", UnitType.Archer, BattleSetupShared.FormationPosition(2, BattlefieldLayout.OpenFieldSquadSize, BattlefieldLayout.OpenFieldPlayerArcherZ)),
    };

    [MenuItem(CreateSceneMenu)]
    public static void CreateOpenFieldScene()
    {
        EnsureScenesFolder();
        OpenDedicatedScene();
        SetupOpenField();
    }

    [MenuItem(SetupMenu)]
    public static void SetupOpenFieldInCurrentScene()
    {
        SetupOpenField();
    }

    static void EnsureScenesFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            AssetDatabase.CreateFolder("Assets", "Scenes");
    }

    static void OpenDedicatedScene()
    {
        if (File.Exists(ScenePath))
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            EditorSceneManager.OpenScene(ScenePath);
            return;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene(), ScenePath);
        AddSceneToBuildSettings(ScenePath);
    }

    static void AddSceneToBuildSettings(string scenePath)
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        foreach (EditorBuildSettingsScene scene in scenes)
        {
            if (scene.path == scenePath)
                return;
        }

        EditorBuildSettingsScene[] updated = new EditorBuildSettingsScene[scenes.Length + 1];
        for (int i = 0; i < scenes.Length; i++)
            updated[i] = scenes[i];

        updated[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
        EditorBuildSettings.scenes = updated;
    }

    static void SetupOpenField()
    {
        BattleSetupShared.EnsurePlayerController();
        BattleSetupShared.EnsureBattleManager();
        BattleSetupShared.RemoveDebugComponents();
        BattleSetupShared.RemoveScenarioBattle();
        SetupBattlefield();
        BattleSetupShared.CleanupLegacyUnits();
        BattleSetupShared.SetupPlayerUnits(PlayerUnits);
        BattleSetupShared.SetupOpenFieldEnemies();
        BattleSetupShared.EnsureUnitComponents();
        BattleSetupShared.FinishSetup(
            BattlefieldLayout.OpenBattleCameraOrthographicSize,
            "Milestone 11 ready! Open field — 9 vs 10, frontline / mid / back, no terrain tricks.");
    }

    static void SetupBattlefield()
    {
        GameObject battlefield = BattleSetupShared.EnsureBattlefieldRoot();
        BattleSetupShared.CreateGround(battlefield.transform, BattlefieldLayout.OpenFieldGroundPlaneScale);
        BattleSetupShared.EnsureBattlefieldConfig(battlefield, bridgeChokepointEnabled: false);
    }
}
#endif
