using UnityEngine;

public class UnitFacing : MonoBehaviour
{
    [SerializeField] float turnSpeed = 720f;

    public const string NoseName = "FacingNose";
    public const float NoseLocalZ = 0.55f;
    public const float NoseScale = 0.22f;

    void Awake()
    {
        turnSpeed = UnitVisuals.TurnSpeed;
    }

    public void FaceToward(Vector3 worldPosition)
    {
        Vector3 toTarget = worldPosition - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeed * Time.deltaTime);
    }

    public void SnapToward(Vector3 worldPosition)
    {
        Vector3 toTarget = worldPosition - transform.position;
        toTarget.y = 0f;

        if (toTarget.sqrMagnitude < 0.001f)
            return;

        transform.rotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
    }

    public static void EnsureNose(GameObject unit, Team team)
    {
        if (unit == null)
            return;

        Transform existing = unit.transform.Find(NoseName);
        if (existing != null)
            return;

        Transform legacyPivot = unit.transform.Find("FacingPivot");
        if (legacyPivot != null)
            Object.DestroyImmediate(legacyPivot.gameObject);

        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nose.name = NoseName;
        nose.transform.SetParent(unit.transform, false);
        nose.transform.localPosition = new Vector3(0f, 0f, NoseLocalZ);
        nose.transform.localScale = Vector3.one * NoseScale;

        Collider collider = nose.GetComponent<Collider>();
        if (collider != null)
            Object.DestroyImmediate(collider);

        Renderer renderer = nose.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");

            Material material = new Material(shader);
            material.color = GetNoseColor(team);
            renderer.sharedMaterial = material;
        }
    }

    static Color GetNoseColor(Team team)
    {
        return team == Team.Player
            ? new Color(0.05f, 0.15f, 0.45f)
            : new Color(0.55f, 0.05f, 0.05f);
    }
}
