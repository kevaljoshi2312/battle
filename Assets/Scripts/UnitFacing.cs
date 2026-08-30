using UnityEngine;

public class UnitFacing : MonoBehaviour
{
    [SerializeField] float turnSpeed = 720f;
    [SerializeField] float noseLocalZ = 0.55f;
    [SerializeField] float noseScale = 0.22f;

    const string NoseName = "FacingNose";

    void Awake()
    {
        turnSpeed = UnitVisuals.TurnSpeed;
        BuildNose();
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

    void BuildNose()
    {
        Transform existing = transform.Find(NoseName);
        if (existing != null)
            Destroy(existing.gameObject);

        Transform legacyPivot = transform.Find("FacingPivot");
        if (legacyPivot != null)
            Destroy(legacyPivot.gameObject);

        GameObject nose = GameObject.CreatePrimitive(PrimitiveType.Cube);
        nose.name = NoseName;
        nose.transform.SetParent(transform, false);
        nose.transform.localPosition = new Vector3(0f, 0f, noseLocalZ);
        nose.transform.localScale = Vector3.one * noseScale;

        Collider collider = nose.GetComponent<Collider>();
        if (collider != null)
            Destroy(collider);

        Renderer renderer = nose.GetComponent<Renderer>();
        if (renderer != null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
                shader = Shader.Find("Unlit/Color");

            Material material = new Material(shader);
            material.color = GetNoseColor();
            renderer.material = material;
        }
    }

    Color GetNoseColor()
    {
        UnitTeam unitTeam = GetComponent<UnitTeam>();
        if (unitTeam == null)
            return new Color(0.85f, 0.85f, 0.85f);

        return unitTeam.Team == Team.Player
            ? new Color(0.05f, 0.15f, 0.45f)
            : new Color(0.55f, 0.05f, 0.05f);
    }
}
