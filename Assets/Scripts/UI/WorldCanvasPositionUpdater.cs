using System;
using Unity.Netcode;
using UnityEngine;

public class WorldCanvasPositionUpdater : NetworkBehaviour
{
    public NetworkVariable<Vector3> Position = new NetworkVariable<Vector3>();
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            Debug.Log("I am the owner of " + OwnerClientId);
            UpdatePositionRpc();
        }
    }
    [Rpc(SendTo.Server)]
    private void UpdatePositionRpc(RpcParams rpcParams = default)
    {
        var xOffset = OwnerClientId == 0 ? -3 : 3;
        Position.Value = new Vector3(xOffset, 0, 0);
    }
    private void Update()
    {
        transform.position = Position.Value;
    }
}
