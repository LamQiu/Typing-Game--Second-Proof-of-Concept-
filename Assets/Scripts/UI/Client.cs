using System;
using System.Collections;
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
    private bool _ignoreInputChange = false;
    private void UpdateInputFieldText(string text)
    {
        _ignoreInputChange = true;

        string result = "";

        foreach (char c in text)
        {
            if (_bannedLetters != null && _bannedLetters.Contains(char.ToLower(c)))
            {
                result += $"<color=#FFFFFF40>{c}</color>";
            }
            else
            {
                result += c;
            }
        }

        answerAreaText.text = result;

        _ignoreInputChange = false;
    }


    private int GetValidLetterCount(string text)
    {
        if(_bannedLetters == null) return text.Length;
        
        int count = 0;

        foreach (char c in text)
        {
            // 只统计字母，并且没被 ban
            if (char.IsLetter(c) && !_bannedLetters.Contains(char.ToLower(c)))
            {
                count++;
            }
        }

        return count;
    }
    private string GetNonTransparentString(string richText)
    {
        string result = "";
        bool insideColorTag = false;

        for (int i = 0; i < richText.Length; i++)
        {
            char c = richText[i];

            // 检测是否进入标签 <color=...>
            if (c == '<')
            {
                insideColorTag = true;
                continue;
            }

            // 检测是否退出标签 </color>
            if (c == '>' && insideColorTag)
            {
                insideColorTag = false;
                continue;
            }

            // 如果在标签里面（透明字符），跳过
            if (insideColorTag)
                continue;

            // 平常字符（未透明）加入结果
            result += c;
        }

        return result;
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

    private char[] _bannedLetters;
    public void OnEndResolutionPhase(List<char> bannedLetters)
    {
        _bannedLetters = bannedLetters.ToArray();
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
        string rawAnswer = answerAreaText.text;
        string answer = GetNonTransparentString(rawAnswer);
        bool validDict = _wordChecker.CheckWordDictionaryValidity(answer);

        if (validDict)
        {
            bool validPrompt = _wordChecker.CheckWordPromptValidity(answer, _currentPrompt);
            if (validPrompt)
            {
                if (usedWords.Contains(answer.ToLower()))
                {
                    hint = "Word already used";
                }
                else
                {
                    ChangeLetterCountServerRpc(GetValidLetterCount(answer));
                    hint = $"\"{answer}\" Submitted";

                    MarkUsedWordsServerRpc(answer);
                    _roundManager.SubmitAnswerServerRpc(OwnerClientId, timeScaleMultiplierAtSegmentClient[timeRemainingSegmentedBar.CurrentSegmentIndex].timeScaleMultiplier, answer);

                    answerAreaText.interactable = false;
                    _checkValid = true;

                    Debug.Log(hint);
                    return;
                }
            }
            else
            {
                hint = $"Word {answer} doesn't meet criteria. Try Again";
            }
        }
        else
        {
            hint = $"Invalid word {rawAnswer}. Try Again";
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
        if (_ignoreInputChange) return;  // ★阻止循环★

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayTypingSfx();

        UpdateInputFieldText(value);
        SubmitAnswerDisplayServerRpc(value);
        StartCoroutine(FixCaret());
    }

    private IEnumerator FixCaret()
    {
        yield return null; // 等待一帧让 TMP 重新排版
        answerAreaText.MoveTextEnd(false);
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
            _ignoreInputChange = true;

            if (_isAnswering)
                answerAreaText.text = new string('*', GetValidLetterCount(value));
            else
                answerAreaText.text = value;

            _ignoreInputChange = false;
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