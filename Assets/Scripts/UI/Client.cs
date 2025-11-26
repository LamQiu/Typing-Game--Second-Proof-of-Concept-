using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
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

    public TMP_Text playerIndexText;
    public TMP_InputField answerAreaText;

    public Image letterCountIndicatorImage;
    public Image letterCountIndicatorBGImage;
    public Vector3 letterCountIndicatorBGOffsetHost;
    public Vector3 letterCountIndicatorBGOffsetClient;

    public TMP_Text promptText;
    public Image healthBarImage;
    [InfoBox("Timer Multiplier at each segment (segment index 0 is the last segment)")]
    public SegmentData[] timeScaleMultiplierAtSegmentHost;
    public SegmentData[] timeScaleMultiplierAtSegmentClient;
    
    [Serializable]
    public struct SegmentData
    {
        public float timeScaleMultiplier;
        public Color segmentColor;
        public Sprite timeScaleMultiplierSprite;
    }
    public SegmentedTimeRemainingBar timeRemainingSegmentedBar;
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

    #region ===== Reset Helpers =====

    private void ResetLocalStates()
    {
        _checkValid = false;
        _isResoluting = false;
        _isAnswering = false;
        sharedText = "";
        usedWords.Clear();
    }

    private void ResetUI()
    {
        hintText.text = "";
        promptText.text = "";

        playerIndexText.text = "P" + ((int)OwnerClientId + 1);

        // Host / Client UI difference
        if (OwnerClientId == 0)
        {
            letterCountIndicatorBGImage.transform.localPosition = letterCountIndicatorBGOffsetHost;
            healthBarImage.fillOrigin = 0;
            timeRemainingSegmentedBar.InitializeSegmentedTimeRemainingBar(timeScaleMultiplierAtSegmentHost,
                _roundManager.RoundTimeLimitInSeconds, true);
        }
        else
        {
            letterCountIndicatorBGImage.transform.localPosition = letterCountIndicatorBGOffsetClient;
            healthBarImage.fillOrigin = 1;
            timeRemainingSegmentedBar.InitializeSegmentedTimeRemainingBar(timeScaleMultiplierAtSegmentClient,
                _roundManager.RoundTimeLimitInSeconds, false);
        }

        answerAreaText.text = "";
        answerAreaText.interactable = IsOwner;

        healthBarImage.fillAmount = 1f;

        UpdateLetterCountIndicator(0);

        worldCanvas.gameObject.SetActive(true);
    }

    private void ResetNetworkVariables()
    {
        if (IsServer)
        {
            Health.Value = maxHealth;
            LetterCount.Value = 0;
            ResolutionConfirmed.Value = false;
        }
    }

    private void ResetInputActivation()
    {
        if (IsOwner)
        {
            answerAreaText.interactable = true;
            answerAreaText.Select();
            answerAreaText.ActivateInputField();
        }
        else
        {
            answerAreaText.interactable = false;
        }
    }

    public void ResetClient()
    {
        ResetLocalStates();
        ResetUI();
        ResetNetworkVariables();
        ResetInputActivation();
    }

    #endregion

    #region ===== Network Spawn =====

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            PlayerManager.Instance.RegisterPlayer(OwnerClientId, this);
        }

        LetterCount.OnValueChanged += OnLetterCountChanged;
        Health.OnValueChanged += OnHealthChanged;

        var promptGenerator = FindAnyObjectByType<PromptGenerator>();
        if (promptGenerator != null)
        {
            promptGenerator.CurrentPrompt.OnValueChanged += OnPromptChanged;
        }

        _roundManager = FindAnyObjectByType<RoundManager>();
        _roundManager.RoundTimeRemainingInSeconds.OnValueChanged += OnTimeRemainingChanged;

        ResetClient();
    }

    #endregion

    #region ===== UI Updates =====

    private void UpdateTimerUI(float timerRemainingInSeconds)
    {
        timeRemainingSegmentedBar.UpdateTimeRemainingBar(timerRemainingInSeconds);
    }

    private void UpdateLetterCountIndicator(int letterCount)
    {
        Material newMat = new Material(letterCountIndicatorImage.material);
        letterCountIndicatorImage.material = newMat;
        newMat.SetFloat("_CurrentCount", letterCount);
    }

    private void UpdatePrompt(PromptGenerator.Prompt value)
    {
        _currentPrompt = value;
        promptText.text = value.ToString();
    }

    private void UpdateLetterCountUI(int value)
    {
        UpdateLetterCountIndicator(value);
    }

    private void UpdateInputFieldInteractability(bool interactable)
    {
        if (IsOwner) answerAreaText.interactable = interactable;
    }

    private void ClearInputField()
    {
        answerAreaText.text = "";
    }

    #endregion

    #region ===== Callbacks =====

    private void OnTimeRemainingChanged(float prev, float value)
    {
        // UpdateTimerUI(value);
    }

    private void OnLetterCountChanged(int prev, int value)
    {
        UpdateLetterCountUI(value);
        Debug.Log($"Letter Count Changed from {prev} to {value}");
    }

    private void OnHealthChanged(int prev, int value)
    {
        if (value <= 0)
        {
            var gm = FindAnyObjectByType<GameManager>();
            if (gm != null)
                gm.EndGameServerRpc();
        }

        healthBarImage.fillAmount = (float)value / maxHealth;
    }

    private void OnPromptChanged(PromptGenerator.Prompt prev, PromptGenerator.Prompt value)
    {
        UpdatePrompt(value);
    }

    #endregion

    #region ===== Phase Handling =====

    public void OnEnterResolutionPhase()
    {
        worldCanvas.gameObject.SetActive(true);
        UpdateInputFieldInteractability(false);

        hintText.text = "Press Enter to Continue";
        _isResoluting = true;
        _isAnswering = false;

        if (IsOwner)
            SubmitAnswerDisplayServerRpc(answerAreaText.text);
    }

    public void OnEndResolutionPhase()
    {
        UpdateInputFieldInteractability(true);
        ClearInputField();
        _isResoluting = false;

        answerAreaText.Select();
        answerAreaText.ActivateInputField();
    }

    public void OnEnterNextRound()
    {
        hintText.text = "Press Enter to Submit";

        if (IsOwner)
        {
            answerAreaText.interactable = true;
            worldCanvas.gameObject.SetActive(true);
            answerAreaText.Select();
            answerAreaText.ActivateInputField();
        }

        _checkValid = false;
        _isAnswering = true;

        Check(updateHint: false);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateConfirmClientRpc(ulong id)
    {
        if (OwnerClientId == id) hintText.text = "";
    }

    #endregion

    #region ===== Input Handling =====

    private void Start()
    {
        if (IsOwner)
        {
            answerAreaText.onValueChanged.AddListener(OnLocalInputChanged);
            _wordChecker = new WordChecker();
        }
    }

    private void Update()
    {
        worldCanvas.transform.position = WorldCanvasPosition.Value;
        UpdateTimerUI(_roundManager.LocalRoundTimeRemainingInSeconds);

        if (!IsOwner) return;

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (_roundManager.IsResolutionPhase.Value)
            {
                _roundManager.ConfirmResolutionServerRpc(OwnerClientId);
            }
            else
            {
                Debug.Log("SubmitAnswerServerRpc at segment: " + timeRemainingSegmentedBar.CurrentSegmentIndex + "");
                Check();
            }
        }

        if (EventSystem.current.currentSelectedGameObject != answerAreaText.gameObject && !_isResoluting)
        {
            answerAreaText.Select();
            answerAreaText.ActivateInputField();
        }
    }

    public override void OnDestroy()
    {
        _roundManager.RoundTimeRemainingInSeconds.OnValueChanged -= OnTimeRemainingChanged;
    }

    #endregion

    #region ===== Word Checking & Submitting =====

    public void Check(bool keepInput = false, bool updateHint = true)
    {
        if (!IsOwner || _checkValid) return;

        string hint = "";
        string input = answerAreaText.text;
        bool validDict = _wordChecker.CheckWordDictionaryValidity(input);

        if (validDict)
        {
            bool validPrompt = _wordChecker.CheckWordPromptValidity(input, _currentPrompt);
            if (validPrompt)
            {
                if (usedWords.Contains(input.ToLower()))
                {
                    hint = "Word already used";
                }
                else
                {
                    ChangeLetterCountServerRpc(input.Length);
                    hint = $"Valid Word \"{input}\" Submitted";

                    MarkUsedWordsServerRpc(input);
                    _roundManager.SubmitAnswerServerRpc(OwnerClientId, timeScaleMultiplierAtSegmentClient[timeRemainingSegmentedBar.CurrentSegmentIndex].timeScaleMultiplier);

                    answerAreaText.interactable = false;
                    _checkValid = true;

                    Debug.Log(hint);
                    return;
                }
            }
            else
            {
                hint = $"Word {input} doesn't meet criteria. Try Again";
            }
        }
        else
        {
            hint = $"Invalid word {input}. Try Again";
        }

        if (updateHint) this.hintText.text = hint;

        ChangeLetterCountServerRpc(0);

        if (!keepInput)
        {
            ClearInputField();
            SubmitAnswerDisplayServerRpc("");
            answerAreaText.Select();
            answerAreaText.ActivateInputField();
        }
    }

    #endregion

    #region ===== Used Words Sync =====

    [Rpc(SendTo.Server)]
    private void MarkUsedWordsServerRpc(string word)
    {
        usedWords.Add(word.ToLower());
        string packed = string.Join("|", usedWords);
        UpdateUsedWordsClientRpc(packed);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateUsedWordsClientRpc(string packedWords)
    {
        usedWords = packedWords.Split('|').ToList();
    }

    #endregion

    #region ===== Shared Input Display Sync =====

    private void OnLocalInputChanged(string value)
    {
        SubmitAnswerDisplayServerRpc(value);
    }

    [ServerRpc]
    private void SubmitAnswerDisplayServerRpc(string value, ServerRpcParams rpc = default)
    {
        sharedText = value;
        UpdateAnswerDisplayClientRpc(value);
    }

    [ClientRpc]
    private void UpdateAnswerDisplayClientRpc(string value)
    {
        sharedText = value;

        if (answerAreaText != null && !IsOwner)
        {
            if (_isAnswering)
                answerAreaText.text = new string('*', value.Length);
            else
                answerAreaText.text = value;
        }

        UpdateLetterCountUI(sharedText.Length);
    }

    #endregion

    #region ===== Letter Count Sync =====

    [Rpc(SendTo.Server)]
    private void ChangeLetterCountServerRpc(int amt)
    {
        LetterCount.Value = amt;
    }

    #endregion
}