using System;
using Unity.Netcode;
using UnityEngine;

public class NetworkTransformTest : NetworkBehaviour
{
    void Update()
    {
        // if (IsServer)
        // {
        //     float theta = Time.frameCount / 10.0f;
        //     transform.position = new Vector3((float) Math.Sin(theta), 0.0f, 0.0f);
        // }
    }
}