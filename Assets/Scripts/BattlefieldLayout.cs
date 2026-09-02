using UnityEngine;

public static class BattlefieldLayout
{
    public const float BridgeCenterZ = 0f;
    public const float PlayerBackZ = -7f;
    public const float PlayerDefenderZ = -0.5f;
    public const float EnemyLineZ = 7f;

    public const int MaxFormationUnitsPerRow = 3;
    public const int ArcherCount = 4;

    public static float PlayerArcherSecondRowZ => PlayerBackZ + FormationRowSpacing;
    public static float PlayerDefenderReserveZ => PlayerDefenderZ - FormationRowSpacing;
    public static float PlayerAttackerReserveZ => PlayerDefenderReserveZ - FormationRowSpacing;

    public const float WallHeight = 3f;
    public const float WallThickness = 2f;
    public const float WallLength = 24f;

    // Bridge corridor: n·D + (n+1)·offset between wall inner faces.
    public const int BridgeDefenderCount = 3;
    public const int ReserveDefenderCount = 3;

    public static float BridgeWidth =>
        UnitVisuals.FormationBridgeWidth(BridgeDefenderCount);

    public static float BridgeCorridorInnerHalfWidth => BridgeWidth * 0.5f;

    public static float LeftWallX => -BridgeCorridorInnerHalfWidth - WallThickness * 0.5f;
    public static float RightWallX => BridgeCorridorInnerHalfWidth + WallThickness * 0.5f;

    public static float BridgeHalfWidth => BridgeCorridorInnerHalfWidth;

    public static float LeftCorridorOuterEdgeX => LeftWallX - WallThickness * 0.5f;
    public static float RightCorridorOuterEdgeX => RightWallX + WallThickness * 0.5f;
    public static float PlayfieldHalfExtent => GroundPlaneScale * 5f;
    public static float FlankBarrierLength => PlayfieldHalfExtent * 2f;
    public static float FlankBarrierCenterZ => (PlayerBackZ + EnemyLineZ) * 0.5f;

    public static float LeftFlankBarrierCenterX =>
        (LeftCorridorOuterEdgeX - PlayfieldHalfExtent) * 0.5f;

    public static float RightFlankBarrierCenterX =>
        (RightCorridorOuterEdgeX + PlayfieldHalfExtent) * 0.5f;

    public static float FlankBarrierWidth =>
        PlayfieldHalfExtent - BridgeCorridorInnerHalfWidth - WallThickness;

    /// <summary>Walkable X half-width when approaching through the bridge (agent radius inset).</summary>
    public static float BridgeApproachHalfWidth =>
        BridgeCorridorInnerHalfWidth - UnitVisuals.CapsuleWorldRadius;

    public const float GroundPlaneScale = 5f;
    public const float BridgeCameraOrthographicSize = 7f;

    public const float FormationRowSpacing = 0.75f;
    public const int EnemyCount = 18;
}
