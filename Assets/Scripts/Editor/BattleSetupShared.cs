#if UNITY_EDITOR
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;

public static class BattleSetupShared
{
    public const string BattlefieldName = "Battlefield";

    public static Vector3 UnitPosition(float x, float z)
    {
        return new Vector3(x, 1f, z);
    }

    public static Vector3 FormationPosition(int index, int countInRow, float z)
    {
        float x = UnitVisuals.LineX(index, countInRow, UnitVisuals.FormationCenterSpacing);
        return UnitPosition(x, z);
    }

    public static Vector3 EnemySpawnPosition(int index, int count)
    {
        int unitsPerRow = BattlefieldLayout.MaxFormationUnitsPerRow;
        int row = index / unitsPerRow;
        int rowStart = row * unitsPerRow;
        int unitsInRow = Mathf.Min(unitsPerRow, count - rowStart);
        int column = index - rowStart;
        float z = BattlefieldLayout.EnemyLineZ + row * BattlefieldLayout.FormationRowSpacing;
        return FormationPosition(column, unitsInRow, z);
    }

    public static void ClearChildren(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
    }

    public static GameObject EnsureBattlefieldRoot()
    {
        GameObject legacyGround = GameObject.Find("Ground");
        if (legacyGround != null)
            Object.DestroyImmediate(legacyGround);

        GameObject battlefield = GameObject.Find(BattlefieldName);
        if (battlefield == null)
            battlefield = new GameObject(BattlefieldName);

        ClearChildren(battlefield.transform);
        return battlefield;
    }

    public static void CreateGround(Transform parent, float planeScale = -1f)
    {
        if (planeScale < 0f)
            planeScale = BattlefieldLayout.GroundPlaneScale;

        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.SetParent(parent, false);
        ground.transform.position = Vector3.zero;
        ground.transform.localScale = new Vector3(planeScale, 1f, planeScale);

        Renderer renderer = ground.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.45f, 0.45f, 0.45f);
            renderer.sharedMaterial = material;
        }
    }

    public static void CreateWall(Transform parent, string name, Vector3 position, Vector3 scale)
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

    public static void CreateFlankBarrier(Transform parent, string name, Vector3 position, Vector3 scale)
    {
        GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barrier.name = name;
        barrier.transform.SetParent(parent, false);
        barrier.transform.position = position;
        barrier.transform.localScale = scale;

        Renderer renderer = barrier.GetComponent<Renderer>();
        if (renderer != null)
            renderer.enabled = false;

        NavMeshModifier modifier = barrier.AddComponent<NavMeshModifier>();
        modifier.overrideArea = true;
        modifier.area = 1;
    }

    public static void EnsureBattlefieldConfig(GameObject battlefield, bool bridgeChokepointEnabled)
    {
        BattlefieldConfig config = battlefield.GetComponent<BattlefieldConfig>();
        if (config == null)
            config = battlefield.AddComponent<BattlefieldConfig>();

        config.Configure(bridgeChokepointEnabled);
        EditorUtility.SetDirty(config);
    }

    public static void BakeNavMesh()
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

    public static void CleanupLegacyUnits()
    {
        GameObject legacySoldier = GameObject.Find("Soldier");
        if (legacySoldier != null)
            Object.DestroyImmediate(legacySoldier);

        foreach (Health health in Object.FindObjectsByType<Health>(FindObjectsSortMode.None))
            Object.DestroyImmediate(health.gameObject);
    }

    public static void SetupPlayerUnits((string Name, UnitType Type, Vector3 Position)[] playerUnits)
    {
        int displayId = 1;
        foreach ((string name, UnitType type, Vector3 position) in playerUnits)
        {
            CreatePlayerUnit(name, type, position, displayId);
            displayId++;
        }
    }

    public static void SetupEnemies()
    {
        for (int i = 0; i < BattlefieldLayout.EnemyCount; i++)
            CreateEnemy($"Enemy_{i + 1}", EnemySpawnPosition(i, BattlefieldLayout.EnemyCount), i + 1);
    }

    public static void SetupOpenFieldEnemies()
    {
        int displayId = 1;

        for (int i = 0; i < BattlefieldLayout.OpenFieldEnemyFrontCount; i++)
        {
            CreateEnemy(
                $"Enemy_Def_{i + 1}",
                FormationPosition(i, BattlefieldLayout.OpenFieldEnemyFrontCount, BattlefieldLayout.OpenFieldEnemyDefenderZ),
                displayId++,
                maxHealth: 80,
                faceTowardZ: BattlefieldLayout.OpenFieldPlayerDefenderZ);
        }

        for (int i = 0; i < BattlefieldLayout.OpenFieldEnemyMidCount; i++)
        {
            CreateEnemy(
                $"Enemy_Atk_{i + 1}",
                FormationPosition(i, BattlefieldLayout.OpenFieldEnemyMidCount, BattlefieldLayout.OpenFieldEnemyAttackerZ),
                displayId++,
                faceTowardZ: BattlefieldLayout.OpenFieldPlayerDefenderZ);
        }

        for (int i = 0; i < BattlefieldLayout.OpenFieldEnemyBackCount; i++)
        {
            CreateEnemyRanged(
                $"Enemy_Arc_{i + 1}",
                FormationPosition(i, BattlefieldLayout.OpenFieldEnemyBackCount, BattlefieldLayout.OpenFieldEnemyArcherZ),
                displayId++);
        }
    }

    public static void RemoveScenarioBattle()
    {
        GameObject manager = GameObject.Find("BattleManager");
        if (manager == null)
            return;

        BattleScenario scenario = manager.GetComponent<BattleScenario>();
        if (scenario != null)
            Object.DestroyImmediate(scenario);

        EnemySpawner spawner = manager.GetComponent<EnemySpawner>();
        if (spawner != null)
            Object.DestroyImmediate(spawner);

        BattleWarningUI warning = manager.GetComponent<BattleWarningUI>();
        if (warning != null)
            Object.DestroyImmediate(warning);

        EditorUtility.SetDirty(manager);
    }

    public static void SetupScenarioBattle()
    {
        GameObject manager = GameObject.Find("BattleManager");
        if (manager == null)
            return;

        EnemySpawner existingSpawner = manager.GetComponent<EnemySpawner>();
        if (existingSpawner != null)
            Object.DestroyImmediate(existingSpawner);

        BattleScenario existingScenario = manager.GetComponent<BattleScenario>();
        if (existingScenario != null)
            Object.DestroyImmediate(existingScenario);

        BattleWarningUI existingWarning = manager.GetComponent<BattleWarningUI>();
        if (existingWarning != null)
            Object.DestroyImmediate(existingWarning);

        manager.AddComponent<EnemySpawner>();
        manager.AddComponent<BattleWarningUI>();
        manager.AddComponent<BattleScenario>();
        EditorUtility.SetDirty(manager);
    }

    public static void EnsurePlayerController()
    {
        GameObject controller = GameObject.Find("PlayerController");
        if (controller == null)
            controller = new GameObject("PlayerController");

        if (controller.GetComponent<PlayerController>() == null)
            controller.AddComponent<PlayerController>();
    }

    public static void EnsureBattleManager()
    {
        GameObject manager = GameObject.Find("BattleManager");
        if (manager == null)
            manager = new GameObject("BattleManager");

        if (manager.GetComponent<BattleManager>() == null)
            manager.AddComponent<BattleManager>();
    }

    public static void RemoveDebugComponents()
    {
        foreach (TargetingDebugUI debugUi in Object.FindObjectsByType<TargetingDebugUI>(FindObjectsSortMode.None))
            Object.DestroyImmediate(debugUi);

        if (BattleDebug.ShowWorldLabels)
            return;

        foreach (UnitWorldLabel label in Object.FindObjectsByType<UnitWorldLabel>(FindObjectsSortMode.None))
            Object.DestroyImmediate(label);

        foreach (GameObject root in EditorSceneManager.GetActiveScene().GetRootGameObjects())
        {
            if (root.name.EndsWith("_WorldLabel"))
                Object.DestroyImmediate(root);
        }
    }

    public static void FinishSetup(float cameraOrthographicSize, string logMessage)
    {
        BakeNavMesh();
        BattleSceneSetup.ConfigureMainCamera(cameraOrthographicSize);
        EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
        Debug.Log(logMessage);
    }

    static void CreatePlayerUnit(string name, UnitType type, Vector3 position, int displayId)
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
        unit.AddComponent<SquadAbility>();
        Unit unitProfile = unit.AddComponent<Unit>();

        EnsureTeam(unit, Team.Player);
        UnitFacing.EnsureNose(unit, Team.Player);
        unitProfile.Configure(type);
        EnsureUnitIdentity(unit, Team.Player, displayId);
        EnsureUnitFacing(unit);
        SnapFacingToward(unit, new Vector3(unit.transform.position.x, 0f, BattlefieldLayout.OpenFieldEnemyDefenderZ));
        EnsureHealthBar(unit);
        EnsureUnitWorldLabel(unit);
        EnsureSelectionRing(unit);
        EnsureDamageFlash(unit);
        EnsureDeathEffect(unit);
        EditorUtility.SetDirty(unitProfile);
    }

    static void CreateEnemy(string name, Vector3 position, int displayId, int maxHealth = 70, float faceTowardZ = float.NaN)
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
        UnitFacing.EnsureNose(enemy, Team.Enemy);
        EnsureHealth(enemy, maxHealth);
        EnsureCombat(enemy, damage: 14, attackRange: 2f, autoAttack: true);
        EnsureUnitIdentity(enemy, Team.Enemy, displayId);
        EnsureHealthBar(enemy);
        EnsureUnitWorldLabel(enemy);
        EnsureDamageFlash(enemy);
        EnsureDeathEffect(enemy);

        enemy.AddComponent<UnitMovement>();
        enemy.AddComponent<EnemyAI>();
        EnsureUnitFacing(enemy);
        float targetZ = float.IsNaN(faceTowardZ) ? BattlefieldLayout.PlayerBackZ : faceTowardZ;
        SnapFacingToward(enemy, new Vector3(enemy.transform.position.x, 0f, targetZ));
    }

    static void CreateEnemyRanged(string name, Vector3 position, int displayId)
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = name;
        enemy.transform.position = position;
        enemy.transform.localScale = UnitVisuals.CapsuleScale;

        Renderer renderer = enemy.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = new Color(0.95f, 0.35f, 0.35f);
            renderer.sharedMaterial = material;
        }

        EnsureTeam(enemy, Team.Enemy);
        UnitFacing.EnsureNose(enemy, Team.Enemy);
        EnsureHealth(enemy, 60);
        EnsureCombat(enemy, damage: 12, attackRange: 8f, autoAttack: true);
        EnsureUnitIdentity(enemy, Team.Enemy, displayId);
        EnsureHealthBar(enemy);
        EnsureUnitWorldLabel(enemy);
        EnsureDamageFlash(enemy);
        EnsureDeathEffect(enemy);

        enemy.AddComponent<UnitMovement>();
        enemy.AddComponent<EnemyAI>();
        EnsureUnitFacing(enemy);
        SnapFacingToward(enemy, new Vector3(enemy.transform.position.x, 0f, BattlefieldLayout.OpenFieldPlayerDefenderZ));
    }

    public static void EnsureUnitComponents()
    {
        int nextPlayerId = 1;
        int nextEnemyId = 1;

        foreach (Health health in Object.FindObjectsByType<Health>(FindObjectsSortMode.None))
        {
            if (health.GetComponent<HealthBar>() == null)
                health.gameObject.AddComponent<HealthBar>();

            EnsureUnitWorldLabel(health.gameObject);

            if (health.GetComponent<UnitFacing>() == null)
                health.gameObject.AddComponent<UnitFacing>();

            if (health.GetComponent<DeathEffect>() == null)
                health.gameObject.AddComponent<DeathEffect>();

            if (health.GetComponent<DamageFlash>() == null)
                health.gameObject.AddComponent<DamageFlash>();

            UnitTeam unitTeam = health.GetComponent<UnitTeam>();
            if (unitTeam != null)
            {
                UnitFacing.EnsureNose(health.gameObject, unitTeam.Team);

                if (unitTeam.Team == Team.Player)
                {
                    if (health.GetComponent<SelectionRing>() == null)
                        health.gameObject.AddComponent<SelectionRing>();

                    if (health.GetComponent<SquadAbility>() == null)
                        health.gameObject.AddComponent<SquadAbility>();
                }
            }

            if (health.GetComponent<UnitIdentity>() == null && unitTeam != null)
            {
                int id = unitTeam.Team == Team.Player ? nextPlayerId++ : nextEnemyId++;
                EnsureUnitIdentity(health.gameObject, unitTeam.Team, id);
            }
        }
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

    static void EnsureUnitWorldLabel(GameObject unit)
    {
        if (!BattleDebug.ShowWorldLabels)
            return;

        if (unit.GetComponent<UnitWorldLabel>() == null)
            unit.AddComponent<UnitWorldLabel>();
    }

    static void EnsureUnitIdentity(GameObject unit, Team team, int displayId)
    {
        UnitIdentity identity = unit.GetComponent<UnitIdentity>();
        if (identity == null)
            identity = unit.AddComponent<UnitIdentity>();

        identity.Configure(team, displayId);
        EditorUtility.SetDirty(identity);
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
