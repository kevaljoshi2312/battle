using UnityEngine;

public class Health : MonoBehaviour
{
    [SerializeField] int maxHealth = 100;
    [SerializeField] int armor;

    int currentHealth;

    public bool IsAlive => currentHealth > 0;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public int Armor => armor;

    void Awake()
    {
        currentHealth = maxHealth;
    }

    public void Configure(int health, int armorValue = 0)
    {
        maxHealth = health;
        currentHealth = health;
        armor = armorValue;
    }

    public void TakeDamage(int amount)
    {
        if (!IsAlive || amount <= 0)
            return;

        int damageAfterArmor = Mathf.Max(1, amount - armor);
        currentHealth -= damageAfterArmor;

        GetComponent<DamageFlash>()?.Play();

        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Destroy(gameObject);
    }
}
