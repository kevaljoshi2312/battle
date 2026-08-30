#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Milestone6Setup
{
    const string MenuPath = "Battle/Setup Milestone 6 (Tactics)";

    [MenuItem(MenuPath)]
    public static void Setup()
    {
        Milestone5Setup.Setup();
        EnsurePlayerUnitAI();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Milestone 6 ready! H = hold, click enemy = attack, archers shoot from range.");
    }

    static void EnsurePlayerUnitAI()
    {
        foreach (UnitTeam unitTeam in Object.FindObjectsByType<UnitTeam>(FindObjectsSortMode.None))
        {
            if (unitTeam.Team != Team.Player)
                continue;

            if (unitTeam.GetComponent<PlayerUnitAI>() == null)
                unitTeam.gameObject.AddComponent<PlayerUnitAI>();
        }
    }
}
#endif
