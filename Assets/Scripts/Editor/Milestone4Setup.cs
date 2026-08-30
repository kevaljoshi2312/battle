#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Milestone4Setup
{
    const string MenuPath = "Battle/Setup Milestone 4 (Combat)";

    [MenuItem(MenuPath)]
    public static void Setup()
    {
        Milestone3Setup.Setup();
        SetupPlayerUnits();
        SetupEnemies();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Milestone 4 ready! Red enemies chase and attack blue soldiers.");
    }

    static void SetupPlayerUnits()
    {
        for (int i = 1; i <= 5; i++)
        {
            GameObject soldier = GameObject.Find($"Soldier_{i}");
            if (soldier == null)
                continue;

            EnsureTeam(soldier, Team.Player);
            EnsureHealth(soldier, 100);
            EnsureCombat(soldier, damage: 10, attackRange: 2f, autoAttack: true);
        }
    }

    static void SetupEnemies()
    {
        Vector3[] spawnPositions =
        {
            new Vector3(UnitVisuals.LineX(0, 3), 1f, UnitVisuals.EnemyLineZ),
            new Vector3(UnitVisuals.LineX(1, 3), 1f, UnitVisuals.EnemyLineZ),
            new Vector3(UnitVisuals.LineX(2, 3), 1f, UnitVisuals.EnemyLineZ),
        };

        for (int i = 0; i < spawnPositions.Length; i++)
            CreateEnemy($"Enemy_{i + 1}", spawnPositions[i]);
    }

    static void CreateEnemy(string name, Vector3 position)
    {
        GameObject existing = GameObject.Find(name);
        GameObject enemy = existing != null ? existing : GameObject.CreatePrimitive(PrimitiveType.Capsule);

        enemy.name = name;
        enemy.transform.position = position;
        enemy.transform.localScale = UnitVisuals.CapsuleScale;

        Renderer renderer = enemy.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.9f, 0.2f, 0.2f);
            renderer.sharedMaterial = material;
        }

        EnsureTeam(enemy, Team.Enemy);
        EnsureHealth(enemy, 80);
        EnsureCombat(enemy, damage: 15, attackRange: 2f, autoAttack: false);

        if (enemy.GetComponent<UnitMovement>() == null)
            enemy.AddComponent<UnitMovement>();

        if (enemy.GetComponent<EnemyAI>() == null)
            enemy.AddComponent<EnemyAI>();

        UnitSelection selection = enemy.GetComponent<UnitSelection>();
        if (selection != null)
            Object.DestroyImmediate(selection);
    }

    static void EnsureTeam(GameObject unit, Team team)
    {
        UnitTeam unitTeam = unit.GetComponent<UnitTeam>();
        if (unitTeam == null)
            unitTeam = unit.AddComponent<UnitTeam>();

        unitTeam.SetTeam(team);
    }

    static void EnsureHealth(GameObject unit, int maxHealth)
    {
        Health health = unit.GetComponent<Health>();
        if (health == null)
            health = unit.AddComponent<Health>();

        health.Configure(maxHealth);
        EditorUtility.SetDirty(health);
    }

    static void EnsureCombat(GameObject unit, int damage, float attackRange, bool autoAttack)
    {
        UnitCombat combat = unit.GetComponent<UnitCombat>();
        if (combat == null)
            combat = unit.AddComponent<UnitCombat>();

        combat.Configure(damage, attackRange, autoAttack);
        EditorUtility.SetDirty(combat);
    }
}
#endif
