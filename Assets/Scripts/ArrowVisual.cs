using UnityEngine;

public static class ArrowVisual
{
    static Material shaftMaterial;
    static Material headMaterial;
    static Material fletchMaterial;
    static Material trailMaterial;

    public static void BuildProjectile(Transform root)
    {
        CreateShaft(root);
        CreateHead(root);
        CreateFletching(root);
        CreateTrail(root);
    }

    static void CreateShaft(Transform root)
    {
        GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        shaft.name = "Shaft";
        shaft.transform.SetParent(root, false);
        shaft.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        shaft.transform.localScale = new Vector3(
            UnitVisuals.ArrowShaftRadius * 2f,
            UnitVisuals.ArrowShaftLength * 0.5f,
            UnitVisuals.ArrowShaftRadius * 2f);
        ApplyMaterial(shaft, GetShaftMaterial());
    }

    static void CreateHead(Transform root)
    {
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Cube);
        head.name = "Head";
        head.transform.SetParent(root, false);
        head.transform.localPosition = new Vector3(0f, 0f, UnitVisuals.ArrowShaftLength * 0.5f + UnitVisuals.ArrowHeadLength * 0.5f);
        head.transform.localScale = new Vector3(
            UnitVisuals.ArrowHeadWidth,
            UnitVisuals.ArrowHeadWidth,
            UnitVisuals.ArrowHeadLength);
        ApplyMaterial(head, GetHeadMaterial());
    }

    static void CreateFletching(Transform root)
    {
        float backZ = -(UnitVisuals.ArrowShaftLength * 0.5f + UnitVisuals.ArrowFletchLength * 0.35f);

        GameObject fletchLeft = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fletchLeft.name = "FletchLeft";
        fletchLeft.transform.SetParent(root, false);
        fletchLeft.transform.localPosition = new Vector3(-UnitVisuals.ArrowFletchSpread, 0f, backZ);
        fletchLeft.transform.localScale = new Vector3(
            UnitVisuals.ArrowFletchWidth,
            UnitVisuals.ArrowFletchHeight,
            UnitVisuals.ArrowFletchLength);
        fletchLeft.transform.localRotation = Quaternion.Euler(0f, 0f, 18f);
        ApplyMaterial(fletchLeft, GetFletchMaterial());

        GameObject fletchRight = GameObject.CreatePrimitive(PrimitiveType.Cube);
        fletchRight.name = "FletchRight";
        fletchRight.transform.SetParent(root, false);
        fletchRight.transform.localPosition = new Vector3(UnitVisuals.ArrowFletchSpread, 0f, backZ);
        fletchRight.transform.localScale = new Vector3(
            UnitVisuals.ArrowFletchWidth,
            UnitVisuals.ArrowFletchHeight,
            UnitVisuals.ArrowFletchLength);
        fletchRight.transform.localRotation = Quaternion.Euler(0f, 0f, -18f);
        ApplyMaterial(fletchRight, GetFletchMaterial());
    }

    static void CreateTrail(Transform root)
    {
        TrailRenderer trail = root.gameObject.AddComponent<TrailRenderer>();
        trail.time = UnitVisuals.ArrowTrailDuration;
        trail.minVertexDistance = 0.02f;
        trail.numCapVertices = 4;
        trail.numCornerVertices = 2;
        trail.startWidth = UnitVisuals.ArrowTrailStartWidth;
        trail.endWidth = 0f;
        trail.material = GetTrailMaterial();
        trail.startColor = UnitVisuals.ArrowTrailStartColor;
        trail.endColor = UnitVisuals.ArrowTrailEndColor;
        trail.emitting = true;
    }

    static void ApplyMaterial(GameObject part, Material material)
    {
        Collider collider = part.GetComponent<Collider>();
        if (collider != null)
            Object.Destroy(collider);

        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null)
            renderer.sharedMaterial = material;
    }

    static Material GetShaftMaterial()
    {
        if (shaftMaterial != null)
            return shaftMaterial;

        shaftMaterial = CreateUnlitMaterial(UnitVisuals.ArrowShaftColor);
        return shaftMaterial;
    }

    static Material GetHeadMaterial()
    {
        if (headMaterial != null)
            return headMaterial;

        headMaterial = CreateUnlitMaterial(UnitVisuals.ArrowHeadColor);
        return headMaterial;
    }

    static Material GetFletchMaterial()
    {
        if (fletchMaterial != null)
            return fletchMaterial;

        fletchMaterial = CreateUnlitMaterial(UnitVisuals.ArrowFletchColor);
        return fletchMaterial;
    }

    static Material GetTrailMaterial()
    {
        if (trailMaterial != null)
            return trailMaterial;

        Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit");
        if (shader == null)
            shader = Shader.Find("Sprites/Default");

        trailMaterial = new Material(shader);
        trailMaterial.color = Color.white;
        return trailMaterial;
    }

    static Material CreateUnlitMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        Material material = new Material(shader);
        material.color = color;
        return material;
    }
}
