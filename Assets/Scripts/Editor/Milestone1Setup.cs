#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class Milestone1Setup
{
    const string MenuPath = "Battle/Setup Milestone 1 (Click-to-Move)";

    [MenuItem(MenuPath)]
    public static void Setup()
    {
        EnsureGround();
        EnsureSoldier();
        SetupCamera();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Milestone 1 ready! Press Play, then click the ground to move the soldier.");
    }

    static void EnsureGround()
    {
        GameObject ground = GameObject.Find("Ground");
        if (ground == null)
        {
            ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
        }

        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(2f, 1f, 2f);
    }

    static void EnsureSoldier()
    {
        GameObject soldier = GameObject.Find("Soldier");
        if (soldier == null)
        {
            soldier = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            soldier.name = "Soldier";
        }

        soldier.transform.position = new Vector3(0f, 1f, 0f);

        Renderer renderer = soldier.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.2f, 0.4f, 0.9f);
            renderer.sharedMaterial = material;
        }

        if (soldier.GetComponent<UnitMovement>() == null)
            soldier.AddComponent<UnitMovement>();
    }

    static void SetupCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            Debug.LogWarning("No Main Camera found in scene.");
            return;
        }

        camera.transform.position = new Vector3(0f, 10f, -8f);
        camera.transform.rotation = Quaternion.Euler(45f, 0f, 0f);
    }
}
#endif
