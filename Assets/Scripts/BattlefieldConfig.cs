using UnityEngine;

public class BattlefieldConfig : MonoBehaviour
{
    [SerializeField] bool bridgeChokepointEnabled = true;

    static BattlefieldConfig instance;

    public bool BridgeChokepointEnabled => bridgeChokepointEnabled;

    public static bool IsBridgeChokepointEnabled =>
        instance == null || instance.bridgeChokepointEnabled;

    public void Configure(bool useBridgeChokepoint)
    {
        bridgeChokepointEnabled = useBridgeChokepoint;
    }

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
