using UnityEngine;
using UnityEngine.AI;

public static class EnemyUnitFactory
{
    public static GameObject Create(
        string name,
        Vector3 position,
        int displayId,
        int maxHealth = 70,
        int damage = 14,
        float attackRange = 2f,
        bool isCommander = false)
    {
        GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        enemy.name = name;
        enemy.transform.position = position;
        enemy.transform.localScale = isCommander
            ? UnitVisuals.CapsuleScale * 1.35f
            : UnitVisuals.CapsuleScale;

        Renderer renderer = enemy.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color = isCommander
                ? new Color(0.95f, 0.75f, 0.1f)
                : new Color(0.9f, 0.2f, 0.2f);
            renderer.sharedMaterial = material;
        }

        UnitTeam unitTeam = enemy.AddComponent<UnitTeam>();
        unitTeam.SetTeam(Team.Enemy);

        UnitFacing.EnsureNose(enemy, Team.Enemy);

        Health health = enemy.AddComponent<Health>();
        health.Configure(maxHealth);

        UnitCombat combat = enemy.AddComponent<UnitCombat>();
        combat.Configure(damage, attackRange, autoAttack: true);

        UnitIdentity identity = enemy.AddComponent<UnitIdentity>();
        identity.Configure(Team.Enemy, displayId);

        enemy.AddComponent<HealthBar>();
        enemy.AddComponent<DamageFlash>();
        enemy.AddComponent<DeathEffect>();
        enemy.AddComponent<UnitMovement>();
        enemy.AddComponent<EnemyAI>();
        UnitFacing facing = enemy.AddComponent<UnitFacing>();
        facing.SnapToward(new Vector3(enemy.transform.position.x, 0f, BattlefieldLayout.PlayerBackZ));

        if (isCommander)
            enemy.AddComponent<EnemyCommander>();

        WarpToNavMesh(enemy);
        return enemy;
    }

    static void WarpToNavMesh(GameObject unit)
    {
        NavMeshAgent agent = unit.GetComponent<NavMeshAgent>();
        if (agent == null)
            return;

        if (NavMesh.SamplePosition(unit.transform.position, out NavMeshHit hit, 4f, NavMesh.AllAreas))
            agent.Warp(hit.position);
    }
}
