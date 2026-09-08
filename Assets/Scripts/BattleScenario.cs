using UnityEngine;

public class BattleScenario : MonoBehaviour
{
    struct TimelineEvent
    {
        public float Time;
        public string Warning;
        public int SpawnCount;
        public SpawnZone Zone;
        public bool SpawnCommander;
    }

    enum SpawnZone
    {
        MainFront,
        LeftFlank,
        BridgeApproach
    }

    static readonly TimelineEvent[] Timeline =
    {
        new TimelineEvent
        {
            Time = 0f,
            Warning = string.Empty,
            SpawnCount = 8,
            Zone = SpawnZone.MainFront
        },
        new TimelineEvent
        {
            Time = 15f,
            Warning = "⚠ ENEMIES APPROACHING FROM LEFT",
            SpawnCount = 4,
            Zone = SpawnZone.LeftFlank
        },
        new TimelineEvent
        {
            Time = 30f,
            Warning = "⚠ REINFORCEMENTS INCOMING",
            SpawnCount = 5,
            Zone = SpawnZone.BridgeApproach
        },
        new TimelineEvent
        {
            Time = 45f,
            Warning = "⚠ ENEMY COMMANDER EXPOSED",
            SpawnCount = 0,
            Zone = SpawnZone.MainFront,
            SpawnCommander = true
        }
    };

    float battleStartTime;
    int nextEventIndex;
    EnemySpawner spawner;
    BattleWarningUI warningUi;

    public bool AllWavesSpawned => nextEventIndex >= Timeline.Length;

    void Awake()
    {
        spawner = GetComponent<EnemySpawner>();
        warningUi = GetComponent<BattleWarningUI>();
    }

    void Start()
    {
        battleStartTime = Time.time;
        ProcessDueEvents();
    }

    void Update()
    {
        ProcessDueEvents();
    }

    void ProcessDueEvents()
    {
        if (spawner == null || nextEventIndex >= Timeline.Length)
            return;

        float elapsed = Time.time - battleStartTime;

        while (nextEventIndex < Timeline.Length && elapsed >= Timeline[nextEventIndex].Time)
        {
            TriggerEvent(Timeline[nextEventIndex]);
            nextEventIndex++;
        }
    }

    void TriggerEvent(TimelineEvent timelineEvent)
    {
        if (!string.IsNullOrEmpty(timelineEvent.Warning))
            warningUi?.Show(timelineEvent.Warning);

        if (timelineEvent.SpawnCommander)
        {
            spawner.SpawnCommander(GetSpawnCenter(SpawnZone.MainFront, 0) + new Vector3(0f, 0f, 1.2f));
            return;
        }

        if (timelineEvent.SpawnCount > 0)
            spawner.SpawnFormation(timelineEvent.SpawnCount, GetSpawnCenter(timelineEvent.Zone, nextEventIndex));
    }

    static Vector3 GetSpawnCenter(SpawnZone zone, int eventIndex)
    {
        return zone switch
        {
            SpawnZone.LeftFlank => new Vector3(
                BattlefieldLayout.LeftFlankSpawnCenterX,
                1f,
                BattlefieldLayout.LeftFlankSpawnCenterZ),
            SpawnZone.BridgeApproach => new Vector3(
                0f,
                1f,
                BattlefieldLayout.BridgeReinforcementCenterZ),
            _ => new Vector3(
                0f,
                1f,
                BattlefieldLayout.EnemyLineZ + (eventIndex > 0 ? 0.5f : 0f))
        };
    }
}
