#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Milestone2Setup
{
    const string MenuPath = "Battle/Setup Milestone 2 (Unit Selection)";

    [MenuItem(MenuPath)]
    public static void Setup()
    {
        Milestone1Setup.Setup();

        EnsureSoldierComponents();
        EnsurePlayerController();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Milestone 2 ready! Click soldier to select, then click ground to move.");
    }

    static void EnsureSoldierComponents()
    {
        GameObject soldier = GameObject.Find("Soldier");
        if (soldier == null)
        {
            Debug.LogWarning("No Soldier found in scene.");
            return;
        }

        if (soldier.GetComponent<UnitSelection>() == null)
            soldier.AddComponent<UnitSelection>();

        if (soldier.GetComponent<UnitMovement>() == null)
            soldier.AddComponent<UnitMovement>();
    }

    static void EnsurePlayerController()
    {
        GameObject controller = GameObject.Find("PlayerController");
        if (controller == null)
        {
            controller = new GameObject("PlayerController");
            controller.AddComponent<PlayerController>();
        }
        else if (controller.GetComponent<PlayerController>() == null)
        {
            controller.AddComponent<PlayerController>();
        }
    }
}
#endif
