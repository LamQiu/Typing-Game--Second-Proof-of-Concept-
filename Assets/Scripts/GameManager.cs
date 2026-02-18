using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Utilities;

public class GameManager : NetworkSingleton<GameManager>
{
    public int WinGameScore = 50;
    public int MaxGameScore = 70;
    public static int s_WinGameScore = 50;
    
    public NetworkVariable<bool> GameStartedState = new NetworkVariable<bool>();
    private SceneReloader m_sceneReloader;

    protected override void Awake()
    {
        base.Awake();
        
        m_sceneReloader = GetComponent<SceneReloader>();
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

    public void NetworkReloadScene()
    {
        m_sceneReloader.ReloadCurrentScene();
        //NetworkReloadSceneClientRpc();
    }
    
    [Rpc(SendTo.ClientsAndHost)]
    public void NetworkReloadSceneClientRpc()
    {
        m_sceneReloader.ReloadCurrentScene();
        StartCoroutine(DelayReloadSceneRoutine());
    }

    private IEnumerator DelayReloadSceneRoutine()
    {
        yield return null;
        UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
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

        if (Keyboard.current != null && Keyboard.current.digit1Key.wasPressedThisFrame && false)
        {
            //StartCoroutine(RestartGame());
            ResetGame();
        }
    }

    public void ResetGame()
    {
        StartCoroutine(GameRestart());
        ResetClientRpc();
        Debug.Log("Game Reset!");
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
        
        UIManager.Instance.ResetUI();
        UIManager.Instance.EnterWinScreen();
        UIManager.Instance.EnterGameScreen();
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