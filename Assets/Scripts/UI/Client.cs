using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Client : NetworkBehaviour
{
    #region ===== Inspector Fields =====

    public int maxHealth;
    public Transform worldCanvas;
    public TMP_Text playerIndex;
    public TMP_InputField inputField;
    public TMP_Text letterCountText;
    public Image letterCountIndicator;
    public Image letterCountIndicatorBG;
    public Vector3 letterCountIndicatorBGOffsetHost;
    public Vector3 letterCountIndicatorBGOffsetClient;
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
    private bool _checkValid = false;
    private bool _isResoluting;
    private bool _isAnswering;
    private List<string> usedWords = new List<string>();

    #endregion

    private void UpdateLetterCountIndicator(int letterCount)
    {
        var newMat = new Material(letterCountIndicator.material);
        letterCountIndicator.material = newMat;

        newMat.SetFloat("_CurrentCount", letterCount);

    }
    public void ResetClient()
    {
        // --- Reset local states ---
        _checkValid = false;
        _isResoluting = false;
        _isAnswering = false;
        usedWords.Clear();
        sharedText = "";

        // --- Reset UI ---
        hintText.text = "";
        prompt.text = "";
        playerIndex.text = "P" + ((int)OwnerClientId + 1);
        // Host
        if (OwnerClientId == 0)
        {
            letterCountIndicatorBG.transform.localPosition = letterCountIndicatorBGOffsetHost;
            healthBar.fillOrigin = 0;
        }
        // Client
        else
        {
            letterCountIndicatorBG.transform.localPosition = letterCountIndicatorBGOffsetClient;
            healthBar.fillOrigin = 1;
        }
        inputField.text = "";
        inputField.interactable = IsOwner;
        letterCountText.text = "Letter Count:\n0";
        healthBar.fillAmount = 1f;
        UpdateLetterCountIndicator(0);

        // hide world canvas on non-owners if that's your intended behavior
        //worldCanvas.gameObject.SetActive(IsOwner);
        worldCanvas.gameObject.SetActive(true);

        // --- Reset Network Variables ---
        if (IsServer)
        {
            Health.Value = maxHealth;
            LetterCount.Value = 0;
            ResolutionConfirmed.Value = false;
        }

        // --- Reset input activation ---
        if (IsOwner)
        {
            inputField.Select();
            inputField.ActivateInputField();
            inputField.interactable = true;
        }
        else
        {
            inputField.interactable = false;
        }
    }


    #region ===== Network Spawn =====

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            PlayerManager.Instance.RegisterPlayer(OwnerClientId, this);
            //Health.Value = maxHealth;
        }

        ResetClient();
        // // Input field ownership
        // if (!IsOwner)
        // {
        //     inputField.interactable = false;
        // }
        // else
        // {
        //     inputField.interactable = true;
        // }
        //
        // Listeners
        LetterCount.OnValueChanged += OnLetterCountChanged;
        Health.OnValueChanged += OnHealthChanged;
        
        var promptGenerator = FindAnyObjectByType<PromptGenerator>();
        if (promptGenerator != null)
        {
            promptGenerator.CurrentPrompt.OnValueChanged += OnPromptChanged;
        }
        
        _roundManager = FindAnyObjectByType<RoundManager>();
        // playerIndex.text = "Player " + ((int)OwnerClientId + 1);
        // //worldCanvas.gameObject.SetActive(false);
    }

    #endregion

    #region ===== Phase Handling =====

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateConfirmClientRpc(ulong id)
    {
        if (OwnerClientId == id)
        {
            hintText.text = "";
        }
    }

    public void OnEnterResolutionPhase()
    {
        worldCanvas.gameObject.SetActive(true);
        UpdateInputFieldInteractability(false);
        hintText.text = "Press Enter to Continue";
        _isAnswering = false;
        _isResoluting = true;
        
        // Make sure the answer is visible
        if(IsOwner)
        {
            SubmitAnswerDisplayServerRpc(inputField.text);
        }
    }

    public void OnEndResolutionPhase()
    {
        UpdateInputFieldInteractability(true);
        ClearInputField();
        _isResoluting = false;
        inputField.Select();
        inputField.ActivateInputField();
    }

    public void OnEnterNextRound()
    {
        hintText.text = "Press Enter to Submit";
        if (!IsOwner)
        {
            //worldCanvas.gameObject.SetActive(false);
        }
        else
        {
            inputField.interactable = true;
            worldCanvas.gameObject.SetActive(true);
            inputField.Select();
            inputField.ActivateInputField();
        }

        _checkValid = false;
        _isAnswering = true;

        Check(updateHint:false);
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
        UpdateLetterCountUI(newValue);
        Debug.Log($"Letter Count Changed from {previousValue} to {newValue}");
    }

    #endregion

    #region ===== Prompt & Score UI =====

    private void UpdatePrompt(PromptGenerator.Prompt value)
    {
        _currentPrompt = value;
        prompt.text = value.ToString();
    }

    private void UpdateLetterCountUI(int value)
    {
        letterCountText.text = "Letter Count:\n" + value.ToString();
        UpdateLetterCountIndicator(value);
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

        if (EventSystem.current.currentSelectedGameObject != inputField.gameObject && !_isResoluting)
        {
            inputField.Select();
            inputField.ActivateInputField();
        }
    }

    public void Check(bool keepInput = false, bool updateHint = true)
    {
        if (!IsOwner) return;
        if (_checkValid) return;

        var hintText = "";
        var validInDictionary = _wordChecker.CheckWordDictionaryValidity(inputField.text);
        if (validInDictionary)
        {
            var validOfPrompt = _wordChecker.CheckWordPromptValidity(inputField.text, _currentPrompt);
            if (validOfPrompt)
            {
                var wordUsed = usedWords.Contains(inputField.text.ToLower());
                if (wordUsed)
                {
                    hintText = $"Word already used";
                }
                else
                {
                    ChangeLetterCountServerRpc(inputField.text.Length);
                    Debug.Log($"Valid Word \"{inputField.text}\" Submitted");
                    hintText = $"Valid Word \"{inputField.text}\" Submitted";
                    MarkUsedWordsServerRpc(inputField.text);
                    _roundManager.SubmitAnswerServerRpc(OwnerClientId);
                    inputField.interactable = false;
                    _checkValid = true;
                    return;
                }
            }
            else
            {
                hintText = $"Word {inputField.text} doesn't meet criteria. Try Again";
            }
        }
        else
        {
            hintText = $"Invalid word {inputField.text}. Try Again";
        }

        if (updateHint)
        {
            this.hintText.text = hintText;
        }
        
        ChangeLetterCountServerRpc(0);
        if (!keepInput)
        {
            ClearInputField();
            SubmitAnswerDisplayServerRpc("");
            inputField.Select();
            inputField.ActivateInputField();
        }
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
        SubmitAnswerDisplayServerRpc(newValue);
    }

    [ServerRpc]
    private void SubmitAnswerDisplayServerRpc(string value, ServerRpcParams rpcParams = default)
    {
        sharedText = value;
        UpdateAnswerDisplayClientRpc(value);
    }

    [ClientRpc]
    private void UpdateAnswerDisplayClientRpc(string value)
    {
        sharedText = value;

        if (inputField != null && !IsOwner)
        {
            if (_isAnswering)
            {
                var display = "";
                for (int i = 0; i < value.Length; i++)
                {
                    display += "*";
                }
                inputField.text = display;
            }
            else
            {
                inputField.text = value;
            }
        }
        UpdateLetterCountUI(sharedText.Length);
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