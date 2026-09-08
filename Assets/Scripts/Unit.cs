using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] UnitType unitType = UnitType.Attacker;

    public UnitType Type => unitType;

    void Awake()
    {
        ApplyStats();
    }

    public void Configure(UnitType type)
    {
        unitType = type;
        ApplyStats();
    }

    void ApplyStats()
    {
        UnitStats stats = UnitStats.For(unitType);

        Health health = GetComponent<Health>();
        if (health != null)
            health.Configure(stats.MaxHealth, stats.Armor);

        UnitCombat combat = GetComponent<UnitCombat>();
        if (combat != null)
            combat.Configure(stats.Damage, stats.AttackRange, stats.AutoAttack);

        UnitMovement movement = GetComponent<UnitMovement>();
        if (movement != null)
            movement.Configure(stats.MoveSpeed * UnitVisuals.MoveSpeedScale);

        transform.localScale = UnitVisuals.CapsuleScale;

        Renderer renderer = GetComponent<Renderer>();
        if (renderer == null)
            return;

        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        material.color = stats.Color;
        renderer.sharedMaterial = material;
    }
}

public readonly struct UnitStats
{
    public int MaxHealth { get; }
    public int Armor { get; }
    public int Damage { get; }
    public float AttackRange { get; }
    public float MoveSpeed { get; }
    public Color Color { get; }
    public bool AutoAttack { get; }

    UnitStats(
        int maxHealth,
        int armor,
        int damage,
        float attackRange,
        float moveSpeed,
        Color color,
        bool autoAttack)
    {
        MaxHealth = maxHealth;
        Armor = armor;
        Damage = damage;
        AttackRange = attackRange;
        MoveSpeed = moveSpeed;
        Color = color;
        AutoAttack = autoAttack;
    }

    public static UnitStats For(UnitType type)
    {
        return type switch
        {
            UnitType.Defender => new UnitStats(
                maxHealth: 200,
                armor: 0,
                damage: 11,
                attackRange: 2f,
                moveSpeed: 3f,
                color: new Color(0.1f, 0.3f, 0.85f),
                autoAttack: true),
            UnitType.Attacker => new UnitStats(
                maxHealth: 110,
                armor: 0,
                damage: 20,
                attackRange: 2f,
                moveSpeed: 5f,
                color: new Color(0.2f, 0.5f, 1f),
                autoAttack: true),
            UnitType.Archer => new UnitStats(
                maxHealth: 70,
                armor: 0,
                damage: 14,
                attackRange: 8f,
                moveSpeed: 4f,
                color: new Color(0.3f, 0.85f, 1f),
                autoAttack: true),
            _ => new UnitStats(100, 0, 10, 2f, 5f, Color.blue, true)
        };
    }
}
