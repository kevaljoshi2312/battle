#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Milestone9Setup
{
    const string MenuPath = "Battle/Setup Milestone 9 (Combat Feel)";

    [MenuItem(MenuPath)]
    public static void Setup()
    {
        Milestone8Setup.Setup();
        EnsureDeathEffects();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Milestone 9 ready! Archer arrows on hit, death tilt and shrink. Select archers and attack from range.");
    }

    static void EnsureDeathEffects()
    {
        foreach (Health health in Object.FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (health.GetComponent<DeathEffect>() == null)
                health.gameObject.AddComponent<DeathEffect>();
        }
    }
}
#endif
