#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Milestone7Setup
{
    const string MenuPath = "Battle/Setup Milestone 7 (Health Bars and Win-Lose)";

    [MenuItem(MenuPath)]
    public static void Setup()
    {
        Milestone6Setup.Setup();
        EnsureHealthBars();
        EnsureBattleManager();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Milestone 7 ready! Health bars visible; battle ends on victory or defeat.");
    }

    static void EnsureHealthBars()
    {
        foreach (Health health in Object.FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            CleanupLegacyBarObjects(health.transform);

            if (health.GetComponent<HealthBar>() == null)
                health.gameObject.AddComponent<HealthBar>();

            if (health.GetComponent<UnitFacing>() == null)
                health.gameObject.AddComponent<UnitFacing>();
        }
    }

    static void CleanupLegacyBarObjects(Transform unitTransform)
    {
        Transform legacyCanvas = unitTransform.Find("HealthBar");
        if (legacyCanvas != null)
            Object.DestroyImmediate(legacyCanvas.gameObject);

        Transform legacyRoot = unitTransform.Find("HealthBarRoot");
        if (legacyRoot != null)
            Object.DestroyImmediate(legacyRoot.gameObject);
    }

    static void EnsureBattleManager()
    {
        GameObject manager = GameObject.Find("BattleManager");
        if (manager == null)
            manager = new GameObject("BattleManager");

        if (manager.GetComponent<BattleManager>() == null)
            manager.AddComponent<BattleManager>();
    }
}
#endif
