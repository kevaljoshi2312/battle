#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Milestone3Setup
{
    const string MenuPath = "Battle/Setup Milestone 3 (Multiple Units)";
    const int UnitCount = 5;

    static readonly Vector3[] SpawnPositions =
    {
        new Vector3(-4f, 1f, 0f),
        new Vector3(-2f, 1f, 0f),
        new Vector3(0f, 1f, 0f),
        new Vector3(2f, 1f, 0f),
        new Vector3(4f, 1f, 0f),
    };

    [MenuItem(MenuPath)]
    public static void Setup()
    {
        Milestone1Setup.Setup();
        EnsurePlayerController();
        EnsureSoldiers();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Milestone 3 ready! Shift+click to multi-select, then click ground to move all.");
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

    static void EnsureSoldiers()
    {
        GameObject legacySoldier = GameObject.Find("Soldier");
        if (legacySoldier != null)
            Object.DestroyImmediate(legacySoldier);

        for (int i = 0; i < UnitCount; i++)
            CreateSoldier($"Soldier_{i + 1}", SpawnPositions[i]);
    }

    static void CreateSoldier(string name, Vector3 position)
    {
        GameObject existing = GameObject.Find(name);
        GameObject soldier = existing != null ? existing : GameObject.CreatePrimitive(PrimitiveType.Capsule);

        soldier.name = name;
        soldier.transform.position = position;

        Renderer renderer = soldier.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.2f, 0.4f, 0.9f);
            renderer.sharedMaterial = material;
        }

        if (soldier.GetComponent<UnitSelection>() == null)
            soldier.AddComponent<UnitSelection>();

        if (soldier.GetComponent<UnitMovement>() == null)
            soldier.AddComponent<UnitMovement>();
    }
}
#endif
