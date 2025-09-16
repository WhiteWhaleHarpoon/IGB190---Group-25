using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// GameAssets class handles the storage and retrieval of key game assets.
/// </summary>
[System.Serializable]
public class GameAssets
{
    public GoldPickup goldPickup;
    public ItemPickup itemPickup;
    public HealthPickup healthPickup;
    public UnitUI unitUI;
    public StatusMessageUI statusMessageUI;
    

    public CircleEffectGuide circleEffectGuide;
    public LineEffectGuide lineEffectGuide;
    public ArcEffectGuide arcEffectGuide;

    public GameFeedback notificationReceivedFeedback;
    public GameFeedback questReceivedFeedback;
    public GameFeedback questCompletedFeedback;

    public GameObject empoweredEffect;
    public GameObject stunEffect;

    public AnimationCurve smoothInOutCurve;

    public GameObject fog;

    public LayerMask floorMask;
    public LayerMask wallMask;
    public LayerMask monsterMask;

}
