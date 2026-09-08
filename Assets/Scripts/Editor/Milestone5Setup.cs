#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Milestone5Setup
{
    const string MenuPath = "Battle/Setup Milestone 5 (Unit Types)";

    static readonly (string Name, UnitType Type, Vector3 Position)[] PlayerUnits =
    {
        ("Defender_1", UnitType.Defender, SpawnPosition(0, 5)),
        ("Defender_2", UnitType.Defender, SpawnPosition(1, 5)),
        ("Attacker_1", UnitType.Attacker, SpawnPosition(2, 5)),
        ("Attacker_2", UnitType.Attacker, SpawnPosition(3, 5)),
        ("Archer_1", UnitType.Archer, SpawnPosition(4, 5)),
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

    static Vector3 SpawnPosition(int index, int count)
    {
        return new Vector3(UnitVisuals.LineX(index, count), 1f, UnitVisuals.PlayerLineZ);
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
        unit.transform.localScale = UnitVisuals.CapsuleScale;

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

        if (unit.GetComponent<PlayerUnitAI>() == null)
            unit.AddComponent<PlayerUnitAI>();

        Unit unitProfile = unit.GetComponent<Unit>();
        if (unitProfile == null)
            unitProfile = unit.AddComponent<Unit>();

        EnsureTeam(unit, Team.Player);
        UnitFacing.EnsureNose(unit, Team.Player);
        unitProfile.Configure(type);
        EnsureUnitFacing(unit);
        SnapFacingToward(unit, new Vector3(unit.transform.position.x, 0f, UnitVisuals.EnemyLineZ));
        EnsureHealthBar(unit);
        EnsureSelectionRing(unit);
        EnsureDamageFlash(unit);
        EditorUtility.SetDirty(unitProfile);
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
        UnitFacing.EnsureNose(enemy, Team.Enemy);
        EnsureHealth(enemy, 80);
        EnsureCombat(enemy, damage: 15, attackRange: 2f, autoAttack: false);
        EnsureHealthBar(enemy);
        EnsureDamageFlash(enemy);

        if (enemy.GetComponent<UnitMovement>() == null)
            enemy.AddComponent<UnitMovement>();

        if (enemy.GetComponent<EnemyAI>() == null)
            enemy.AddComponent<EnemyAI>();

        EnsureUnitFacing(enemy);
        SnapFacingToward(enemy, new Vector3(enemy.transform.position.x, 0f, UnitVisuals.PlayerLineZ));

        UnitSelection selection = enemy.GetComponent<UnitSelection>();
        if (selection != null)
            Object.DestroyImmediate(selection);

        Unit unitProfile = enemy.GetComponent<Unit>();
        if (unitProfile != null)
            Object.DestroyImmediate(unitProfile);

        PlayerUnitAI playerAi = enemy.GetComponent<PlayerUnitAI>();
        if (playerAi != null)
            Object.DestroyImmediate(playerAi);
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

    static void EnsureHealthBar(GameObject unit)
    {
        if (unit.GetComponent<HealthBar>() == null)
            unit.AddComponent<HealthBar>();
    }

    static void EnsureSelectionRing(GameObject unit)
    {
        if (unit.GetComponent<SelectionRing>() == null)
            unit.AddComponent<SelectionRing>();
    }

    static void EnsureDamageFlash(GameObject unit)
    {
        if (unit.GetComponent<DamageFlash>() == null)
            unit.AddComponent<DamageFlash>();
    }

    static void EnsureUnitFacing(GameObject unit)
    {
        if (unit.GetComponent<UnitFacing>() == null)
            unit.AddComponent<UnitFacing>();
    }

    static void SnapFacingToward(GameObject unit, Vector3 worldPoint)
    {
        UnitFacing facing = unit.GetComponent<UnitFacing>();
        if (facing != null)
            facing.SnapToward(worldPoint);
    }
}
#endif
