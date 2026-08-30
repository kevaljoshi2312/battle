#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Milestone8Setup
{
    const string MenuPath = "Battle/Setup Milestone 8 (Selection Ring and Damage Flash)";

    [MenuItem(MenuPath)]
    public static void Setup()
    {
        Milestone7Setup.Setup();
        BattleSceneSetup.ConfigureMainCamera();
        EnsureSelectionRings();
        EnsureDamageFlash();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Milestone 8 ready! Selection rings on player units, damage flash on hit, orthographic camera.");
    }

    static void EnsureSelectionRings()
    {
        foreach (UnitSelection selection in Object.FindObjectsByType<UnitSelection>(FindObjectsSortMode.None))
        {
            if (selection.GetComponent<SelectionRing>() == null)
                selection.gameObject.AddComponent<SelectionRing>();
        }
    }

    static void EnsureDamageFlash()
    {
        foreach (Health health in Object.FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (health.GetComponent<DamageFlash>() == null)
                health.gameObject.AddComponent<DamageFlash>();
        }
    }
}
#endif
