using UnityEngine;

public class UnitMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5f;

    public float MoveSpeed => moveSpeed;

    public void Configure(float speed)
    {
        moveSpeed = speed;
    }

    Vector3 targetPosition;
    bool hasTarget;
    UnitFacing facing;

    void Awake()
    {
        facing = GetComponent<UnitFacing>();
    }

    void Start()
    {
        if (GetComponent<Unit>() == null)
            moveSpeed = UnitVisuals.EnemyMoveSpeed;
    }

    public void MoveTo(Vector3 worldPosition)
    {
        targetPosition = worldPosition;
        targetPosition.y = transform.position.y;
        hasTarget = true;
    }

    public void Stop()
    {
        hasTarget = false;
    }

    void Update()
    {
        if (!hasTarget)
            return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        facing?.FaceToward(targetPosition);

        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
            hasTarget = false;
    }
}
