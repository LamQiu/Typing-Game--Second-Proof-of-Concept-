using System;
using System.Text.RegularExpressions;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class Client : NetworkBehaviour
{
    private WordChecker _wordChecker;
    public TMP_InputField inputField;
    public TMP_Text scoreText;
    public TMP_Text prompt;

    public NetworkVariable<int> Score = new NetworkVariable<int>();
    public override void OnNetworkSpawn()
    {
        if (!IsOwner)
        {
            // 非本地玩家 → 禁止编辑
            inputField.interactable = false;
        }
        else
        {
            // 本地玩家 → 可以编辑
            inputField.interactable = true;
        }

        Score.OnValueChanged += OnScoreChanged;
        
        var promptGenerator = FindAnyObjectByType<PromptGenerator>();
        if (promptGenerator != null)
        {
            promptGenerator.CurrentPrompt.OnValueChanged += OnPromptChanged;
        }
    }

    private void OnPromptChanged(PromptGenerator.Prompt previousValue, PromptGenerator.Prompt newValue)
    {
        UpdatePrompt(newValue);
    }

    private void UpdatePrompt(PromptGenerator.Prompt value)
    {
        prompt.text = value.ToString();
    }
    private void OnScoreChanged(int previousValue, int newValue)
    {
        UpdateScore(newValue);
        Debug.Log($"Score Changed from {previousValue} to {newValue}");
    }

    private void UpdateScore(int value)
    {
        scoreText.text = value.ToString();
    }
    [Rpc(SendTo.Server)]
    private void ChangeScoreServerRpc(int amt)
    {
        Score.Value += amt;
    }

    private void Start()
    {
        if (IsOwner)
        {
            inputField.onValueChanged.AddListener(OnLocalInputChanged);
            _wordChecker = new WordChecker();
        }
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            Check();
        }
    }

    #region Check & Score

    private void Check()
    {
        if (_wordChecker.CheckWord(inputField.text))
        {
            Debug.Log("Word is valid");

            ChangeScoreServerRpc(5);
        }
    }

    #endregion

    #region UI

    private string sharedText = "";

    private void OnLocalInputChanged(string newValue)
    {
        // 客户端向服务器发送文字
        SubmitTextServerRpc(newValue);
    }

    [ServerRpc]
    private void SubmitTextServerRpc(string value, ServerRpcParams rpcParams = default)
    {
        sharedText = value;
        UpdateAllClientsClientRpc(value);
    }

    [ClientRpc]
    private void UpdateAllClientsClientRpc(string value)
    {
        sharedText = value;

        // 所有人更新 UI，包括 host
        if (inputField != null)
            inputField.text = value;
    }

    #endregion
}