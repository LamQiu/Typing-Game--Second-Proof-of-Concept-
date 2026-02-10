using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static readonly int s_GamePlayerCount = 2;
    
    public Vector2 hostOffset;
    public Vector2 clientOffset;
    
    public static PlayerManager Instance;

    // 存所有玩家的引用：key = clientId
    public Dictionary<ulong, Client> Players = new Dictionary<ulong, Client>();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} connected.");
        // 在 player prefab 的 Client.cs 里 OnNetworkSpawn 会自动注册，因此这里通常不用做事
    }

    void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected.");

        Players.Remove(clientId);
    }

    public void RegisterPlayer(ulong clientId, Client clientObj)
    {
        Players[clientId] = clientObj;
        var isHost = clientId == 0;
        clientObj.name = isHost ? "Host" : $"Client {clientId}";
        clientObj.WorldCanvasPosition.Value = isHost ? hostOffset : clientOffset;

        // Start game if there's two players
        if (Players.Count >= 2)
        {
            var gm = FindAnyObjectByType<GameManager>();
            if (gm != null)
            {
                gm.StartGameServerRpc();
            }
        }
        
        Debug.Log($"Players Count: {Players.Count}");
        // Update client world canvas pos
        // foreach (var idClient in Players)
        // {
        //     isHost = idClient.Key == 0;
        //     var offset = isHost ? hostOffset : clientOffset;
        //     idClient.Value.UpdateWorldCanvasPosClientRpc(offset);
        // }

        //StartCoroutine(DelayedCanvasUpdate(clientObj, offset));
    }

    // private IEnumerator DelayedCanvasUpdate(Client clientObj, Vector3 offset)
    // {
    //     // 等 1 frame，或者直到 clientObj.IsSpawned
    //     yield return new WaitForSecondsRealtime(0.1f);
    //
    //     clientObj.UpdateWorldCanvasPosClientRpc(offset);
    // }

    public Client GetHost()
    {
        return Players.ContainsKey(0) ? Players[0] : null;
    }

    public Client GetClient(ulong clientId)
    {
        return Players.ContainsKey(clientId) ? Players[clientId] : null;
    }
}