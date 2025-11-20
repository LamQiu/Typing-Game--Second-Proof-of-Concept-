using System;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class ClientTextInput : NetworkBehaviour
{
    private WordChecker _wordChecker;
    public TMP_InputField inputField;

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
    }

    private string sharedText = "";

    private void Start()
    {
        // 只监听 local player 的输入
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

    #region Check Logic

    private void Check()
    {
        if (_wordChecker.CheckWord(inputField.text))
        {
            Debug.Log("Word is valid");
            
            var scoreManager = FindAnyObjectByType<ScoreManager>();
            scoreManager?.AddScoreServerRpc(5);
        }
    }

    #endregion

    #region UI

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