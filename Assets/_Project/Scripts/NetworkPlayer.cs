using Fusion;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    [Header("Avatar Bones")]
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    [Header("XR Rig References")]
    public Transform xrHead;
    public Transform xrLeftHand;
    public Transform xrRightHand;

    public override void FixedUpdateNetwork()
    {
        if (Object.HasInputAuthority)
        {
            head.position = xrHead.position;
            head.rotation = xrHead.rotation;

            leftHand.position = xrLeftHand.position;
            leftHand.rotation = xrLeftHand.rotation;

            rightHand.position = xrRightHand.position;
            rightHand.rotation = xrRightHand.rotation;
        }
    }
}