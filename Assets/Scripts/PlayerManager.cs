using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static readonly int s_GamePlayerCount = 2;
    public static PlayerManager Instance;

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

        // Start game if there's two players
        if (Players.Count >= s_GamePlayerCount)
        {
            var gm = FindAnyObjectByType<GameManager>();
            if (gm != null)
            {
                gm.StartGameServerRpc();
            }
        }
        
        Debug.Log($"Players Count: {Players.Count}");
    }
    public Client GetHost()
    {
        return Players.ContainsKey(0) ? Players[0] : null;
    }

    public Client GetClient(ulong clientId)
    {
        return Players.ContainsKey(clientId) ? Players[clientId] : null;
    }
}