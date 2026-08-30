#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Milestone5Setup
{
    const string MenuPath = "Battle/Setup Milestone 5 (Unit Types)";

    static readonly (string Name, UnitType Type, Vector3 Position)[] PlayerUnits =
    {
        ("Defender_1", UnitType.Defender, new Vector3(-4f, 1f, 0f)),
        ("Defender_2", UnitType.Defender, new Vector3(-2f, 1f, 0f)),
        ("Attacker_1", UnitType.Attacker, new Vector3(0f, 1f, 0f)),
        ("Attacker_2", UnitType.Attacker, new Vector3(2f, 1f, 0f)),
        ("Archer_1", UnitType.Archer, new Vector3(4f, 1f, 0f)),
    };

    [MenuItem(MenuPath)]
    public static void Setup()
    {
        Milestone1Setup.Setup();
        EnsurePlayerController();
        CleanupLegacyUnits();
        SetupPlayerUnits();
        SetupEnemies();

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Milestone 5 ready! Defenders (tanky), Attackers (melee DPS), Archers (long range).");
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

    static void CleanupLegacyUnits()
    {
        GameObject legacySoldier = GameObject.Find("Soldier");
        if (legacySoldier != null)
            Object.DestroyImmediate(legacySoldier);

        for (int i = 1; i <= 5; i++)
        {
            GameObject soldier = GameObject.Find($"Soldier_{i}");
            if (soldier != null)
                Object.DestroyImmediate(soldier);
        }
    }

    static void SetupPlayerUnits()
    {
        foreach ((string name, UnitType type, Vector3 position) in PlayerUnits)
            CreatePlayerUnit(name, type, position);
    }

    static void CreatePlayerUnit(string name, UnitType type, Vector3 position)
    {
        GameObject existing = GameObject.Find(name);
        GameObject unit = existing != null ? existing : GameObject.CreatePrimitive(PrimitiveType.Capsule);

        unit.name = name;
        unit.transform.position = position;

        if (unit.GetComponent<UnitSelection>() == null)
            unit.AddComponent<UnitSelection>();

        if (unit.GetComponent<UnitMovement>() == null)
            unit.AddComponent<UnitMovement>();

        if (unit.GetComponent<Health>() == null)
            unit.AddComponent<Health>();

        if (unit.GetComponent<UnitCombat>() == null)
            unit.AddComponent<UnitCombat>();

        if (unit.GetComponent<UnitTeam>() == null)
            unit.AddComponent<UnitTeam>();

        Unit unitProfile = unit.GetComponent<Unit>();
        if (unitProfile == null)
            unitProfile = unit.AddComponent<Unit>();

        EnsureTeam(unit, Team.Player);
        unitProfile.Configure(type);
        EditorUtility.SetDirty(unitProfile);
    }

    static void SetupEnemies()
    {
        Vector3[] spawnPositions =
        {
            new Vector3(0f, 1f, 8f),
            new Vector3(-3f, 1f, 10f),
            new Vector3(3f, 1f, 10f),
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

        Unit unitProfile = enemy.GetComponent<Unit>();
        if (unitProfile != null)
            Object.DestroyImmediate(unitProfile);
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
