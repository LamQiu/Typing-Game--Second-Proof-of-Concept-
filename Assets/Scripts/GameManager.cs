using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<bool> GameStarted = new NetworkVariable<bool>();

    // 供客户端请求由 host/start server 开始游戏（如果你希望由客户端触发，可以用 ServerRpc）
    [ServerRpc(RequireOwnership = false)]
    public void StartGameServerRpc(ServerRpcParams rpcParams = default)
    {
        GameStarted.Value = true;
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)] 
    public void EndGameServerRpc()
    {
        GameStarted.Value = false;
    }

    private void Update()
    {
        // 只有 host (本地有输入) 可以通过按键触发重启逻辑
        if (!IsHost) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            //RestartGame();
        }
    }

    private void RestartGame()
    {
        // Despawn all player objects
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.PlayerObject != null && client.PlayerObject.IsSpawned)
            {
                client.PlayerObject.Despawn(true); // true = destroy
            }
        }

        // Reload scene using Netcode SceneManager
        NetworkManager.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            UnityEngine.SceneManagement.LoadSceneMode.Single
        );
    }

}