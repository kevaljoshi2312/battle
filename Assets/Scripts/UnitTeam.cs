using UnityEngine;

public class UnitTeam : MonoBehaviour
{
    [SerializeField] Team team = Team.Player;

    public Team Team => team;

    public void SetTeam(Team value) => team = value;
}
