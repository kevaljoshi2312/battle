#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BattleEventsSetup
{
    const string MenuPath = "Battle/Setup Milestone 15 (Battle Events)";

    [MenuItem(MenuPath)]
    public static void Setup()
    {
        BattleSetupShared.EnsureBattleManager();
        RemoveStaticEnemies();
        BattleSetupShared.SetupScenarioBattle();
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Milestone 15 ready! Scripted waves at 0/15/30/45s on the current scene.");
    }

    static void RemoveStaticEnemies()
    {
        foreach (Health health in Object.FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            UnitTeam team = health.GetComponent<UnitTeam>();
            if (team != null && team.Team == Team.Enemy)
                Object.DestroyImmediate(health.gameObject);
        }
    }
}
#endif
