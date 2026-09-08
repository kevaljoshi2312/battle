using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    int nextDisplayId = 1;

    public GameObject SpawnGrunt(Vector3 position)
    {
        string name = $"Enemy_{nextDisplayId}";
        GameObject enemy = EnemyUnitFactory.Create(name, position, nextDisplayId);
        nextDisplayId++;
        return enemy;
    }

    public GameObject SpawnCommander(Vector3 position)
    {
        string name = "Enemy_Commander";
        GameObject commander = EnemyUnitFactory.Create(
            name,
            position,
            nextDisplayId,
            maxHealth: 120,
            damage: 18,
            attackRange: 2f,
            isCommander: true);
        nextDisplayId++;
        return commander;
    }

    public void SpawnFormation(int count, Vector3 center, float rowSpacing = BattlefieldLayout.FormationRowSpacing)
    {
        int unitsPerRow = BattlefieldLayout.MaxFormationUnitsPerRow;

        for (int i = 0; i < count; i++)
        {
            int row = i / unitsPerRow;
            int rowStart = row * unitsPerRow;
            int unitsInRow = Mathf.Min(unitsPerRow, count - rowStart);
            int column = i - rowStart;
            float x = center.x + UnitVisuals.LineX(column, unitsInRow, UnitVisuals.FormationCenterSpacing);
            float z = center.z + row * rowSpacing;
            SpawnGrunt(new Vector3(x, 1f, z));
        }
    }
}
