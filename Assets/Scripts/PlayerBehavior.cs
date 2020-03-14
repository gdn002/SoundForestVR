using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class PlayerBehavior : MonoBehaviour
{
    public SteamVR_Action_Vector2 SnapTurn;

    public SteamVR_Input_Sources handType;

    private bool SnapTurnReset = true;

    // Start is called before the first frame update
    void Start()
    {
        SnapTurn.AddOnAxisListener(ExecuteTurn, handType);
    }

    public void ExecuteTurn(SteamVR_Action_Vector2 fromAction, SteamVR_Input_Sources fromSource, Vector2 axis, Vector2 axisDelta)
    {
        if (SnapTurnReset && Mathf.Abs(axis.x) > 0.8f)
        {
            SnapTurnReset = false;
            float angle = 30 * (axis.x > 0 ? 1 : -1);
            transform.Rotate(new Vector3(0, angle, 0));
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!SnapTurnReset && Mathf.Abs(SnapTurn.axis.x) < 0.2f)
        {
            SnapTurnReset = true;
        }
    }
}
