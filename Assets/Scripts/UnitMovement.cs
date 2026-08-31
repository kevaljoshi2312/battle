using UnityEngine;
using UnityEngine.AI;

public class UnitMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;

    NavMeshAgent agent;
    UnitFacing facing;
    Vector3 targetPosition;
    Vector3 anchoredPosition;
    bool hasTarget;
    bool isPositionAnchored;
    int defaultAvoidancePriority;

    public float MoveSpeed => moveSpeed;
    public bool HasMoveTarget => hasTarget;

    public void Configure(float speed)
    {
        moveSpeed = speed;
        if (agent != null)
            agent.speed = speed;
    }

    public void SetPositionAnchored(bool anchored)
    {
        if (anchored && !isPositionAnchored)
        {
            anchoredPosition = transform.position;
            hasTarget = false;

            if (CanUseNavMesh())
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }

        isPositionAnchored = anchored;

        if (agent == null)
            return;

        agent.avoidancePriority = anchored ? 0 : defaultAvoidancePriority;
    }

    void Awake()
    {
        facing = GetComponent<UnitFacing>();
        agent = GetComponent<NavMeshAgent>();
        if (agent == null)
            agent = gameObject.AddComponent<NavMeshAgent>();

        agent.updateRotation = false;
        agent.updateUpAxis = false;
        agent.stoppingDistance = UnitVisuals.NavAgentStoppingDistance;
        agent.autoBraking = true;
        agent.angularSpeed = 0f;
        agent.radius = UnitVisuals.NavAgentRadius;
        agent.height = UnitVisuals.CapsuleHeight * 2f;
        agent.baseOffset = UnitVisuals.CapsuleHeight;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        defaultAvoidancePriority = 30 + (Mathf.Abs(GetInstanceID()) % 40);
        agent.avoidancePriority = defaultAvoidancePriority;
        agent.speed = moveSpeed;
    }

    void Start()
    {
        if (GetComponent<Unit>() == null)
            Configure(UnitVisuals.EnemyMoveSpeed);

        TryWarpToNavMesh();

        if (GetComponent<Unit>() != null)
            SetPositionAnchored(true);
    }

    public void MoveTo(Vector3 worldPosition)
    {
        isPositionAnchored = false;
        if (agent != null)
            agent.avoidancePriority = defaultAvoidancePriority;

        targetPosition = worldPosition;
        targetPosition.y = transform.position.y;
        hasTarget = true;

        if (CanUseNavMesh())
        {
            agent.isStopped = false;
            agent.SetDestination(targetPosition);
        }
    }

    public void Stop()
    {
        hasTarget = false;

        if (CanUseNavMesh())
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    void Update()
    {
        if (!hasTarget)
            return;

        if (CanUseNavMesh())
        {
            Vector3 faceTarget = agent.steeringTarget;
            if (faceTarget == transform.position)
                faceTarget = targetPosition;

            facing?.FaceToward(faceTarget);

            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                hasTarget = false;
                agent.isStopped = true;
            }

            return;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime);

        facing?.FaceToward(targetPosition);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            hasTarget = false;
    }

    void LateUpdate()
    {
        if (!isPositionAnchored || !CanUseNavMesh())
            return;

        Vector3 delta = transform.position - anchoredPosition;
        delta.y = 0f;
        if (delta.sqrMagnitude > 0.0001f)
            agent.Warp(anchoredPosition);
    }

    bool CanUseNavMesh()
    {
        return agent != null && agent.isOnNavMesh;
    }

    void TryWarpToNavMesh()
    {
        if (agent == null)
            return;

        if (agent.isOnNavMesh)
            return;

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            agent.Warp(hit.position);
    }
}
