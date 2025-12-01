using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : NetworkBehaviour
{
    public NetworkVariable<bool> GameStartedState = new NetworkVariable<bool>();

    private void Start()
    {
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void StartGameServerRpc()
    {
        GameStartedState.Value = true;
        Debug.Log("Game Started!");
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public void EndGameServerRpc()
    {
        GameStartedState.Value = false;
    }
    // public override void OnNetworkSpawn()
    // {
    //     if (IsServer)
    //     {
    //         NetworkManager.SceneManager.OnLoadComplete += OnSceneLoaded;
    //     }
    // }
    //
    // private void OnSceneLoaded(ulong clientId, string sceneName, LoadSceneMode mode)
    // {
    //     if (clientId != NetworkManager.LocalClientId)
    //         return;
    //
    //     Debug.Log("Scene Loaded. Now start game.");
    //     StartGameServerRpc();
    // }
    //
    // public override void OnNetworkDespawn()
    // {
    //     if (IsServer && NetworkManager != null)
    //     {
    //         NetworkManager.SceneManager.OnLoadComplete -= OnSceneLoaded;
    //     }
    // }


    private void Update()
    {
        if (!IsServer) return;

        if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            //StartCoroutine(RestartGame());
            StartCoroutine(GameRestart());

            ResetClientRpc();
            
            Debug.Log("Game Reset!");

        }
    }
    [Rpc(SendTo.ClientsAndHost)]
    private void ResetClientRpc()
    {
        var roundManager = FindAnyObjectByType<RoundManager>();
        if (roundManager)
        {
            roundManager.ResetRoundManager();
        }

        var clients = FindObjectsByType<Client>(FindObjectsSortMode.InstanceID);
        foreach (var client in clients)
        {
            client.ResetClient();
        }
    }

    private IEnumerator GameRestart()
    {
        GameStartedState.Value = false;
        yield return null;
        GameStartedState.Value = true;
    }

    // public IEnumerator RestartGame()
    // {
    //     NetworkManager.Singleton.Shutdown();
    //     yield return null;
    //     RestartNetworkClientRpc();
    // }
    // [Rpc(SendTo.ClientsAndHost)]
    // private void RestartNetworkClientRpc()
    // {
    //     SceneManager.Instance.LoadTitleScene();
    // }
}