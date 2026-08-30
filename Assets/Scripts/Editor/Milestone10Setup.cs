#if UNITY_EDITOR
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

public static class Milestone10Setup
{
    const string MenuPath = "Battle/Setup Milestone 10 (Tactical Terrain)";
    const string BattlefieldName = "Battlefield";

    static readonly (string Name, UnitType Type, Vector3 Position)[] PlayerUnits =
    {
        ("Archer_1", UnitType.Archer, ArcherPosition(0)),
        ("Archer_2", UnitType.Archer, ArcherPosition(1)),
        ("Archer_3", UnitType.Archer, ArcherPosition(2)),
        ("Archer_4", UnitType.Archer, ArcherPosition(3)),
        ("Defender_Bridge_1", UnitType.Defender, DefenderBridgePosition(0)),
        ("Defender_Bridge_2", UnitType.Defender, DefenderBridgePosition(1)),
        ("Defender_Bridge_3", UnitType.Defender, DefenderBridgePosition(2)),
        ("Defender_Bridge_4", UnitType.Defender, DefenderBridgePosition(3)),
        ("Defender_Back", UnitType.Defender, UnitPosition(0f, BattlefieldLayout.PlayerAttackerZ)),
        ("Attacker_1", UnitType.Attacker, UnitPosition(-0.6f, BattlefieldLayout.PlayerAttackerZ - 1f)),
        ("Attacker_2", UnitType.Attacker, UnitPosition(0.6f, BattlefieldLayout.PlayerAttackerZ - 1f)),
    };

    [MenuItem(MenuPath)]
    public static void Setup()
    {
        EnsurePlayerController();
        EnsureBattleManager();
        SetupBattlefield();
        CleanupLegacyUnits();
        SetupPlayerUnits();
        SetupEnemies();
        EnsureUnitComponents();
        BakeNavMesh();
        BattleSceneSetup.ConfigureMainCamera(BattlefieldLayout.BridgeCameraOrthographicSize);

        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log("Milestone 10 ready! Bridge chokepoint, NavMesh pathing, 11 vs 18 unfair battle.");
    }

    static Vector3 UnitPosition(float x, float z)
    {
        return new Vector3(x, 1f, z);
    }

    static Vector3 ArcherPosition(int index)
    {
        const int archerCount = 4;
        float x = UnitVisuals.LineX(index, archerCount, BattlefieldLayout.FormationColumnSpacing);
        return UnitPosition(x, BattlefieldLayout.PlayerBackZ);
    }

    static Vector3 DefenderBridgePosition(int index)
    {
        const int defenderCount = 4;
        float x = UnitVisuals.LineX(index, defenderCount, BattlefieldLayout.FormationColumnSpacing);
        return UnitPosition(x, BattlefieldLayout.PlayerDefenderZ);
    }

    static void SetupBattlefield()
    {
        GameObject legacyGround = GameObject.Find("Ground");
        if (legacyGround != null)
            Object.DestroyImmediate(legacyGround);

        GameObject battlefield = GameObject.Find(BattlefieldName);
        if (battlefield == null)
            battlefield = new GameObject(BattlefieldName);

        ClearChildren(battlefield.transform);

        CreateGround(battlefield.transform);
        CreateWall(battlefield.transform, "Wall_Left",
            new Vector3(BattlefieldLayout.LeftWallX, BattlefieldLayout.WallHeight * 0.5f, BattlefieldLayout.BridgeCenterZ),
            new Vector3(BattlefieldLayout.WallThickness, BattlefieldLayout.WallHeight, BattlefieldLayout.WallLength));
        CreateWall(battlefield.transform, "Wall_Right",
            new Vector3(BattlefieldLayout.RightWallX, BattlefieldLayout.WallHeight * 0.5f, BattlefieldLayout.BridgeCenterZ),
            new Vector3(BattlefieldLayout.WallThickness, BattlefieldLayout.WallHeight, BattlefieldLayout.WallLength));
    }

    static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }

    static void CreateGround(Transform parent)
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.SetParent(parent, false);
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(
            BattlefieldLayout.GroundPlaneScale,
            1f,
            BattlefieldLayout.GroundPlaneScale);

        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.45f, 0.45f, 0.45f);
            renderer.sharedMaterial = material;
        }
    }

    static void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent, false);
        wall.transform.position = position;
        wall.transform.localScale = scale;

        Renderer renderer = wall.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.3f, 0.3f, 0.32f);
            renderer.sharedMaterial = material;
        }

        NavMeshModifier modifier = wall.AddComponent<NavMeshModifier>();
        modifier.overrideArea = true;
        modifier.area = 1;
    }

    static void BakeNavMesh()
    {
        GameObject battlefield = GameObject.Find(BattlefieldName);
        if (battlefield == null)
            return;

        NavMeshSurface surface = battlefield.GetComponent<NavMeshSurface>();
        if (surface == null)
            surface = battlefield.AddComponent<NavMeshSurface>();

        surface.collectObjects = CollectObjects.Children;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.BuildNavMesh();
        WarpAllUnitsToNavMesh();
    }

    static void WarpAllUnitsToNavMesh()
    {
        foreach (UnitMovement movement in Object.FindObjectsByType<UnitMovement>(FindObjectsSortMode.None))
        {
            NavMeshAgent agent = movement.GetComponent<NavMeshAgent>();
            if (agent == null)
                continue;

            if (NavMesh.SamplePosition(movement.transform.position, out NavMeshHit hit, 4f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }
    }

    static void CleanupLegacyUnits()
    {
        GameObject legacySoldier = GameObject.Find("Soldier");
        if (legacySoldier != null)
            Object.DestroyImmediate(legacySoldier);

        foreach (Health health in Object.FindObjectsByType<Health>(FindObjectsSortMode.None))
            Object.DestroyImmediate(health.gameObject);
    }

    static void SetupPlayerUnits()
    {
        foreach ((string name, UnitType type, Vector3 position) in PlayerUnits)
            CreatePlayerUnit(name, type, position);
    }

    static void SetupEnemies()
    {
        const int enemyCount = 18;
        for (int i = 0; i < enemyCount; i++)
            CreateEnemy($"Enemy_{i + 1}", EnemySpawnPosition(i, enemyCount));
    }

    static Vector3 EnemySpawnPosition(int index, int count)
    {
        int columns = BattlefieldLayout.EnemyColumns;
        int row = index / columns;
        int column = index % columns;
        float x = UnitVisuals.LineX(column, columns, BattlefieldLayout.FormationColumnSpacing);
        float z = BattlefieldLayout.EnemyLineZ + row * BattlefieldLayout.FormationRowSpacing;
        return UnitPosition(x, z);
    }

    static void CreatePlayerUnit(string name, UnitType type, Vector3 position)
    {
        GameObject unit = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        unit.name = name;
        unit.transform.position = position;
        unit.transform.localScale = UnitVisuals.CapsuleScale;

        unit.AddComponent<UnitSelection>();
        unit.AddComponent<UnitMovement>();
        unit.AddComponent<Health>();
        unit.AddComponent<UnitCombat>();
        unit.AddComponent<UnitTeam>();
        unit.AddComponent<PlayerUnitAI>();
        Unit unitProfile = unit.AddComponent<Unit>();

        EnsureTeam(unit, Team.Player);
        unitProfile.Configure(type);
        EnsureUnitFacing(unit);
        SnapFacingToward(unit, new Vector3(unit.transform.position.x, 0f, BattlefieldLayout.EnemyLineZ));
        EnsureHealthBar(unit);
        EnsureSelectionRing(unit);
        EnsureDamageFlash(unit);
        EnsureDeathEffect(unit);
        EditorUtility.SetDirty(unitProfile);
    }

    static void CreateEnemy(string name, Vector3 position)
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
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
        EnsureHealth(enemy, 70);
        EnsureCombat(enemy, damage: 14, attackRange: 2f, autoAttack: false);
        EnsureHealthBar(enemy);
        EnsureDamageFlash(enemy);
        EnsureDeathEffect(enemy);

        enemy.AddComponent<UnitMovement>();
        enemy.AddComponent<EnemyAI>();
        EnsureUnitFacing(enemy);
        SnapFacingToward(enemy, new Vector3(enemy.transform.position.x, 0f, BattlefieldLayout.PlayerBackZ));
    }

    static void EnsureUnitComponents()
    {
        foreach (Health health in Object.FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (health.GetComponent<HealthBar>() == null)
                health.gameObject.AddComponent<HealthBar>();

            if (health.GetComponent<UnitFacing>() == null)
                health.gameObject.AddComponent<UnitFacing>();

            if (health.GetComponent<DeathEffect>() == null)
                health.gameObject.AddComponent<DeathEffect>();

            if (health.GetComponent<DamageFlash>() == null)
                health.gameObject.AddComponent<DamageFlash>();

            UnitTeam unitTeam = health.GetComponent<UnitTeam>();
            if (unitTeam != null && unitTeam.Team == Team.Player && health.GetComponent<SelectionRing>() == null)
                health.gameObject.AddComponent<SelectionRing>();
        }
    }

    static void EnsurePlayerController()
    {
        GameObject controller = GameObject.Find("PlayerController");
        if (controller == null)
            controller = new GameObject("PlayerController");

        if (controller.GetComponent<PlayerController>() == null)
            controller.AddComponent<PlayerController>();
    }

    static void EnsureBattleManager()
    {
        GameObject manager = GameObject.Find("BattleManager");
        if (manager == null)
            manager = new GameObject("BattleManager");

        if (manager.GetComponent<BattleManager>() == null)
            manager.AddComponent<BattleManager>();
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

    static void EnsureDeathEffect(GameObject unit)
    {
        if (unit.GetComponent<DeathEffect>() == null)
            unit.AddComponent<DeathEffect>();
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
