using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Unity.Netcode;
using UnityEngine;
using TMPro;
using UI;
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
    private const string k_currentHealthTextFormat = "Current Health: {0}";
    public TMP_Text currentHealthText;
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

    public NetworkVariable<int> LetterCount = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> CurrentScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public NetworkVariable<bool> ResolutionConfirmed = new NetworkVariable<bool>();

    #endregion

    #region ===== Private Fields =====

    private WordChecker _wordChecker;
    private RoundManager _roundManager;
    private PromptGenerator.Prompt _currentPrompt;

    private string sharedText = "";
    public string SharedText => sharedText;
    private bool _checkValid = false;
    private bool _isResoluting;
    private bool _isAnswering;
    private List<string> usedWords = new List<string>();

    #endregion

    private Client m_otherClient;

    #region ===== Reset Helpers =====

    private void ResetLocalStates()
    {
        _checkValid = false;
        _isResoluting = false;
        _isAnswering = false;
        sharedText = "";
        usedWords.Clear();
        _bannedLetters = Array.Empty<char>();
    }

    private void ResetUI()
    {
        hintText.text = "";
        promptText.text = "";

        playerIndexText.text = "P" + ((int)OwnerClientId + 1);

        UpdateTimerUI(20);

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

        if (IsClient && !IsOwner)
        {
            worldCanvas.gameObject.SetActive(false);
        }

        answerAreaText.text = "";
        answerAreaText.interactable = IsOwner;

        currentHealthText.text = string.Format(k_currentHealthTextFormat, maxHealth);

        healthBarImage.fillAmount = 1f;

        UpdateLetterCountIndicator(0);

        worldCanvas.gameObject.SetActive(true);
    }

    private void ResetNetworkVariables()
    {
        if (IsServer)
        {
            CurrentScore.Value = 0;
            LetterCount.Value = 0;
            ResolutionConfirmed.Value = false;
        }
    }

    private void ResetInputActivation()
    {
        if (IsOwner)
        {
            answerAreaText.interactable = true;
            //answerAreaText.Select();
            //answerAreaText.ActivateInputField();
        }
        else
        {
            //answerAreaText.interactable = false;
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

        if (IsOwner)
        {
            LetterCount.OnValueChanged += OnLetterCountChanged;
            CurrentScore.OnValueChanged += OnCurrentScoreChanged;

            var promptGenerator = FindAnyObjectByType<PromptGenerator>();
            if (promptGenerator != null)
            {
                promptGenerator.CurrentPrompt.OnValueChanged += OnPromptChanged;
            }
        }

        _roundManager = FindAnyObjectByType<RoundManager>();
        _roundManager.RoundTimeRemainingInSeconds.OnValueChanged += OnTimeRemainingChanged;

        ResetClient();

        if (IsHost || IsClient)
        {
            UIManager.Instance.Client = this;
        }
    }

    #endregion

    #region ===== UI Updates =====

    private void UpdateTimerUI(float timerRemainingInSeconds)
    {
        UIManager.Instance.UpdateGameScreenTimer(timerRemainingInSeconds / _roundManager.RoundTimeLimitInSeconds);
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
        UIManager.Instance.UpdateCurrentPrompt(value.ToString());
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
        if (_bannedLetters == null) return text.Length;

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
        UpdateTimerUI(value);
    }

    private void OnLetterCountChanged(int prev, int value)
    {
        UpdateLetterCountUI(value);


        // if(IsHost)
        // {
        // }
        // else
        // {
        //     //UIManager.Instance.UpdateP2LettersCountUI(value);
        // }
        Debug.Log($"Letter Count Changed from {prev} to {value}");
    }

    private Client GetOtherClient()
    {
        Client result = null;
        Client[] clients = FindObjectsByType<Client>(FindObjectsInactive.Exclude, FindObjectsSortMode.InstanceID);
        foreach (Client client in clients)
        {
            if (client != this && client.OwnerClientId != OwnerClientId)
            {
                result = client;
                break;
            }
        }

        return result;
    }

    private const int k_winScore = 100;
    private const int k_maxScore = 150;

    private void OnCurrentScoreChanged(int prev, int value)
    {
        if (value >= k_winScore)
        {
            var gm = FindAnyObjectByType<GameManager>();
            if (gm != null)
                gm.EndGameServerRpc();
            value = 0;
        }

        currentHealthText.text = string.Format(k_currentHealthTextFormat, value);
        healthBarImage.fillAmount = (float)value / maxHealth;

        if (IsHost)
        {
            UIManager.Instance.UpdatePlayer1FillImage(value / (float)k_maxScore);
            UIManager.Instance.UpdatePlayer2FillImage(m_otherClient.CurrentScore.Value / (float)k_maxScore);
        }
        else if (IsClient)
        {
            UIManager.Instance.UpdatePlayer2FillImage(value / (float)k_maxScore);
            UIManager.Instance.UpdatePlayer1FillImage(m_otherClient.CurrentScore.Value / (float)k_maxScore);
        }
    }

    private void OnPromptChanged(PromptGenerator.Prompt prev, PromptGenerator.Prompt value)
    {
        UpdatePrompt(value);
    }

    #endregion

    #region ===== Phase Handling =====

    public void OnEnterResolutionPhase()
    {
        Debug.Log("Enter Resolution Phase Client");

        if (!IsOwner)
        {
            hintText.gameObject.SetActive(true);
        }

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

        //answerAreaText.Select();
        //answerAreaText.ActivateInputField();
    }

    public void OnEnterNextRound()
    {
        if (m_otherClient == null)
        {
            m_otherClient = GetOtherClient();
        }

        if (!IsOwner)
        {
            hintText.gameObject.SetActive(false);
        }

        hintText.text = "Press Enter to Submit";

        if (IsOwner)
        {
            answerAreaText.interactable = true;
            worldCanvas.gameObject.SetActive(true);
            //answerAreaText.Select();
            //answerAreaText.ActivateInputField();
            if (IsHost)
            {
                UIManager.Instance.SetP1();
                UIManager.Instance.ResolutionScreenSetP1();
            }
            else if (IsClient)
            {
                UIManager.Instance.SetP2();
                UIManager.Instance.ResolutionScreenSetP2();
            }
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
            //answerAreaText.onValueChanged.AddListener(OnLocalInputChanged);
            UIManager.Instance.AddListenerOnWordInputField(OnLocalInputFieldChanged);
            _wordChecker = new WordChecker();
        }
    }

    private void Update()
    {
        worldCanvas.transform.position = WorldCanvasPosition.Value;
        UpdateTimerUI(_roundManager.LocalRoundTimeRemainingInSeconds);

        if (!IsOwner) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (_roundManager.IsResolutionPhase.Value)
            {
                _roundManager.ConfirmResolutionServerRpc(OwnerClientId);
            }
            else
            {
                Debug.Log("SubmitAnswerServerRpc at segment: " + timeRemainingSegmentedBar.CurrentSegmentIndex + "");
                Check(keepInput: false);
            }
        }

        if (IsHost)
        {
            UIManager.Instance.UpdateP1LettersCountUI(LetterCount.Value);
            if (m_otherClient != null)
            {
                UIManager.Instance.UpdateP2LettersCountUI(m_otherClient.LetterCount.Value);
            }
        }
        else if (IsClient)
        {
            if (m_otherClient != null)
            {
                UIManager.Instance.UpdateP1LettersCountUI(m_otherClient.LetterCount.Value);
            }

            UIManager.Instance.UpdateP2LettersCountUI(LetterCount.Value);
        }

        if (IsHost)
        {
            UIManager.Instance.UpdatePlayer1FillImage(CurrentScore.Value / (float)k_maxScore);
            if (m_otherClient != null)
            {
                UIManager.Instance.UpdatePlayer2FillImage(m_otherClient.CurrentScore.Value / (float)k_maxScore);
            }
        }
        else if (IsClient)
        {
            UIManager.Instance.UpdatePlayer2FillImage(CurrentScore.Value / (float)k_maxScore);
            if (m_otherClient != null)
            {
                UIManager.Instance.UpdatePlayer1FillImage(m_otherClient.CurrentScore.Value / (float)k_maxScore);
            }
        }

        if (EventSystem.current.currentSelectedGameObject != answerAreaText.gameObject && !_isResoluting)
        {
            //answerAreaText.Select();
            //answerAreaText.ActivateInputField();
        }

        //UIManager.Instance.UpdateCurrentWordInputFieldInteractability(answerAreaText.interactable);
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
                if (_roundManager.UsedWords != null && _roundManager.UsedWords.Contains(answer.ToLower()))
                {
                    hint = "Word already used";
                }
                else
                {
                    ChangeLetterCount(GetValidLetterCount(answer));
                    hint = $"\"{answer}\" Submitted";

                    MarkUsedWord(answer.ToLower());
                    var index = timeRemainingSegmentedBar.CurrentSegmentIndex;
                    if (timeRemainingSegmentedBar.CurrentSegmentIndex < 0) index = 0;
                    if (updateHint)
                    {
                        this.hintText.text = hint;
                        Debug.Log($"Updated hint: {hint}");
                    }

                    _roundManager.SubmitAnswerServerRpc(OwnerClientId,
                        timeScaleMultiplierAtSegmentClient[index].timeScaleMultiplier, answer);
                    //answerAreaText.interactable = false;
                    _checkValid = true;


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

        if (updateHint)
        {
            this.hintText.text = hint;
            Debug.Log($"Updated hint: {hint}");
        }

        ChangeLetterCount(0);

        if (!keepInput)
        {
            ClearInputField();
            SubmitAnswerDisplayServerRpc("");
            //answerAreaText.Select();
            //answerAreaText.ActivateInputField();
        }
    }

    #endregion

    #region ===== Used Words Sync =====

    private void MarkUsedWord(string word)
    {
        _roundManager.MarkUsedWordServerRpc(word);
    }

    #endregion

    #region ===== Shared Input Display Sync =====

    private Coroutine _inputDisplaySyncCoroutine;

    private void OnLocalInputFieldChanged(string value)
    {
        if (_ignoreInputChange) return;

        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayTypingSfx();

        UpdateInputFieldText(GetNonTransparentString(value));
        SubmitAnswerDisplayServerRpc(value);
        if (_inputDisplaySyncCoroutine != null) StopCoroutine(_inputDisplaySyncCoroutine);
        _inputDisplaySyncCoroutine = StartCoroutine(FixCaret());

        if (IsOwner)
        {
            LetterCount.Value = GetValidLetterCount(GetNonTransparentString(value));
        }

        //UIManager.Instance.UpdateLettersCount(value.Length);
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
        var validLength = GetValidLetterCount(GetNonTransparentString(value));

        if (answerAreaText != null && !IsOwner)
        {
            _ignoreInputChange = true;

            if (_isAnswering)
                answerAreaText.text = new string('*', validLength);
            else
                answerAreaText.text = value;

            _ignoreInputChange = false;
        }

        UpdateLetterCountUI(validLength);
    }

    #endregion

    #region ===== Letter Count Sync =====

    private void ChangeLetterCount(int amt)
    {
        LetterCount.Value = amt;
    }

    #endregion
}