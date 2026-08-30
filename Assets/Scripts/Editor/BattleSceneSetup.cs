#if UNITY_EDITOR
using UnityEngine;

public static class BattleSceneSetup
{
    public const float CameraHeight = 10f;
    public const float CameraBackOffset = 8f;
    public const float CameraPitch = 45f;
    public const float OrthographicSize = 7f;

    public static void ConfigureMainCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            Debug.LogWarning("No Main Camera found in scene.");
            return;
        }

        camera.transform.position = new Vector3(0f, CameraHeight, -CameraBackOffset);
        camera.transform.rotation = Quaternion.Euler(CameraPitch, 0f, 0f);
        camera.orthographic = true;
        camera.orthographicSize = OrthographicSize;
    }
}
#endif
