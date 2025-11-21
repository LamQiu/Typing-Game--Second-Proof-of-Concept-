using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Client : NetworkBehaviour
{
    #region ===== Inspector Fields =====

    public int maxHealth;
    public Transform worldCanvas;
    public TMP_Text playerIndex;
    public TMP_InputField inputField;
    public TMP_Text scoreText;
    public TMP_Text prompt;
    public Image healthBar;
    public TMP_Text hintText;

    #endregion

    #region ===== Network Variables =====

    public NetworkVariable<Vector3> WorldCanvasPosition = new NetworkVariable<Vector3>();
    public NetworkVariable<int> LetterCount = new NetworkVariable<int>();
    public NetworkVariable<int> Health = new NetworkVariable<int>();
    public NetworkVariable<bool> ResolutionConfirmed = new NetworkVariable<bool>();

    #endregion

    #region ===== Private Fields =====

    private WordChecker _wordChecker;
    private RoundManager _roundManager;
    private PromptGenerator.Prompt _currentPrompt;
    private string sharedText = "";
    private List<string> usedWords = new List<string>();

    #endregion

    #region ===== Network Spawn =====

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            PlayerManager.Instance.RegisterPlayer(OwnerClientId, this);
            Health.Value = maxHealth;
        }

        // Input field ownership
        if (!IsOwner)
        {
            inputField.interactable = false;
        }
        else
        {
            inputField.interactable = true;
        }

        // Listeners
        LetterCount.OnValueChanged += OnLetterCountChanged;
        Health.OnValueChanged += OnHealthChanged;

        var promptGenerator = FindAnyObjectByType<PromptGenerator>();
        if (promptGenerator != null)
        {
            promptGenerator.CurrentPrompt.OnValueChanged += OnPromptChanged;
        }

        _roundManager = FindAnyObjectByType<RoundManager>();
        playerIndex.text = "Player " + ((int)OwnerClientId + 1);
    }

    #endregion

    #region ===== Resolution Phase Handling =====

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateConfirmClientRpc(ulong id)
    {
        if (OwnerClientId == id)
        {
            hintText.text = "Confirmed";
        }
    }

    public void OnEnterResolutionPhase()
    {
        worldCanvas.gameObject.SetActive(true);
        UpdateInputFieldInteractability(false);
        Debug.Log(OwnerClientId);
        hintText.text = "Press Enter to Confirm";
    }

    public void OnEndResolutionPhase()
    {
        UpdateInputFieldInteractability(true);
        ClearInputField();
    }

    public void OnEnterNextRound()
    {
        Debug.Log(OwnerClientId);
        hintText.text = "Press Enter to Submit";
        if (!IsOwner)
        {
            worldCanvas.gameObject.SetActive(false);
        }
    }

    #endregion

    #region ===== Network Callbacks =====

    private void OnHealthChanged(int previousValue, int newValue)
    {
        if (newValue <= 0)
        {
            var gm = FindAnyObjectByType<GameManager>();
            if (gm != null)
            {
                gm.EndGameServerRpc();
            }
        }

        var healthT = (float)newValue / maxHealth;
        healthBar.fillAmount = healthT;
    }

    private void OnPromptChanged(PromptGenerator.Prompt previousValue, PromptGenerator.Prompt newValue)
    {
        UpdatePrompt(newValue);
    }

    private void OnLetterCountChanged(int previousValue, int newValue)
    {
        UpdateLetterCount(newValue);
        Debug.Log($"Score Changed from {previousValue} to {newValue}");
    }

    #endregion

    #region ===== Prompt & Score UI =====

    private void UpdatePrompt(PromptGenerator.Prompt value)
    {
        _currentPrompt = value;
        prompt.text = value.ToString();
    }

    private void UpdateLetterCount(int value)
    {
        scoreText.text = "Letter Count:\n" + value.ToString();
    }

    private void UpdateInputFieldInteractability(bool interactable)
    {
        if (IsOwner)
            inputField.interactable = interactable;
    }

    private void ClearInputField()
    {
        inputField.text = "";
    }

    #endregion

    #region ===== Input Handling =====

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
        worldCanvas.transform.position = WorldCanvasPosition.Value;

        if (!IsOwner) return;

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (_roundManager.IsResolutionPhase.Value)
            {
                Debug.Log("Confirm Round resolution phase");
                _roundManager.ConfirmResolutionServerRpc(OwnerClientId);
            }
            else
            {
                Check();
            }
        }
    }

    public void Check()
    {
        var validInDictionary = _wordChecker.CheckWordDictionaryValidity(inputField.text);
        if (validInDictionary)
        {
            var validOfPrompt = _wordChecker.CheckWordPromptValidity(inputField.text, _currentPrompt);
            if (validOfPrompt)
            {
                var wordUsed = usedWords.Contains(inputField.text.ToLower());
                if (wordUsed)
                {
                    hintText.text = $"Word already used";
                }
                else
                {
                    ChangeLetterCountServerRpc(inputField.text.Length);
                    hintText.text = $"Valid Word {inputField.text} Submitted";
                    MarkUsedWordsServerRpc(inputField.text);
                    _roundManager.SubmitAnswerServerRpc(OwnerClientId);
                    inputField.interactable = false;
                }
            }
            else
            {
                hintText.text = $"Word {inputField.text} doesn't meet criteria. Try Agin";
            }
        }
        else
        {
            hintText.text = $"Invalid word {inputField.text}. Try Again";
        }

        ChangeLetterCountServerRpc(inputField.text.Length);
    }
    [Rpc(SendTo.Server)]
    private void MarkUsedWordsServerRpc(string word)
    {
        usedWords.Add(word.ToLower());

        // 序列化列表成一个 string
        string packed = string.Join("|", usedWords);

        // 发给所有客户端
        UpdateUsedWordsClientRpc(packed);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateUsedWordsClientRpc(string packedWords)
    {
        // 解包成数组或 list
        usedWords = packedWords.Split('|').ToList();
    }

    // private bool Check()
    // {
    //     if (_wordChecker.CheckWordPromptValidity(inputField.text, _currentPrompt))
    //     {
    //         Debug.Log("Word is valid and valid for prompt");
    //         return true;
    //     }
    //
    //     return false;
    // }

    #endregion

    #region ===== Text Sync (Shared Input Field) =====

    private void OnLocalInputChanged(string newValue)
    {
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

        if (inputField != null && !IsOwner)
            inputField.text = value;
    }

    #endregion

    #region ===== Letter Count Server Update =====

    [Rpc(SendTo.Server)]
    private void ChangeLetterCountServerRpc(int amt)
    {
        LetterCount.Value = amt;
    }

    #endregion
}