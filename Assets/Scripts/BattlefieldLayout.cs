using UnityEngine;

public static class BattlefieldLayout
{
    public const float BridgeHalfWidth = 1.25f;
    public const float BridgeCenterZ = 0f;
    public const float PlayerBackZ = -7f;
    public const float PlayerDefenderZ = -0.5f;
    public const float PlayerAttackerZ = -3.5f;
    public const float EnemyLineZ = 7f;

    public const float WallHeight = 3f;
    public const float WallThickness = 2f;
    public const float WallLength = 24f;
    public const float LeftWallX = -2f;
    public const float RightWallX = 2f;

    public const float GroundPlaneScale = 5f;
    public const float BridgeCameraOrthographicSize = 7f;

    // Compact spawn grid — matches player unit spacing (~0.6–0.8 apart).
    public const float FormationColumnSpacing = 0.65f;
    public const float FormationRowSpacing = 0.75f;
    public const int EnemyColumns = 5;
}
