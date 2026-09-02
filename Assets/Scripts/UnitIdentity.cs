using UnityEngine;

public class UnitIdentity : MonoBehaviour
{
    [SerializeField] int displayId;
    [SerializeField] Team team;

    public int DisplayId => displayId;
    public Team Team => team;
    public string Label => team == Team.Player ? $"P{displayId}" : $"E{displayId}";

    public void Configure(Team unitTeam, int id)
    {
        team = unitTeam;
        displayId = id;
    }

    public static string GetLabel(Health health)
    {
        if (health == null)
            return "-";

        UnitIdentity identity = health.GetComponent<UnitIdentity>();
        return identity != null ? identity.Label : health.gameObject.name;
    }
}
