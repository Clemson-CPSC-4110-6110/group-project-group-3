using Fusion;
using UnityEngine;

public class NetworkPlayer : NetworkBehaviour
{
    public Transform head;
    public Transform leftHand;
    public Transform rightHand;

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Debug.Log("This is MY player");
        }
    }
}