using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RoundManager : NetworkBehaviour
{
    // ============================================================
    // Inspector Fields
    // ============================================================
    public float[] roundTimes;
    public float roundTimeLimitInSeconds = 20f;
    public int banLetterPerRound = 3;
    private int _currentRoundIndex = 0;
    public List<string> submittedAnswers = new List<string>();
    private List<char> _bannedLetters = new List<char>();
    public float RoundTimeLimitInSeconds => roundTimeLimitInSeconds;
    public float resoluteTimeInSeconds = 5f;

    public TMP_Text resolutionText;
    public Image resolutionBGImage;
    public TMP_Text winText;
    public GameObject winImage;
    public GameObject titleImage;
    public GameObject banLetterBG;
    public GameObject resolutionBG;
    public GameObject BG;
    public TMP_Text bannedLettersText;
    public float defaultTimeScaleMultiplier = 1f;

    public TMP_Text timeMultiplierText;
    public Image timeMultiplierIndicatorImage;
    public Sprite[] timeMultiplierIndicatorSprites;


    // ============================================================
    // Network Variables
    // ============================================================
    public NetworkVariable<bool> IsResolutionPhase = new NetworkVariable<bool>();
    public NetworkVariable<float> RoundTimeRemainingInSeconds = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<float> ResolutionTimeRemaining = new NetworkVariable<float>();

    private float _localRoundTimeRemainingInSeconds;
    public float LocalRoundTimeRemainingInSeconds => _localRoundTimeRemainingInSeconds;

    private float _timeScaleMultiplier;

    // ============================================================
    // Private State
    // ============================================================
    private bool _started;
    private bool _ended;
    private bool _promptGenerated;
    private bool _startResolute;

    private int _currentRound = -1;

    private readonly List<ulong> submittedAnswerClients = new List<ulong>();
    private readonly List<ulong> confirmedResolutionClients = new List<ulong>();

    // ============================================================
    // Unity Lifecycle
    // ============================================================
    private void Start()
    {
        ResetRoundManager();
    }

    public override void OnNetworkSpawn()
    {
        ResetRoundManager();

        if (FindAnyObjectByType<GameManager>() is GameManager gm)
            gm.GameStartedState.OnValueChanged += OnGameStartedStateChanged;

        RoundTimeRemainingInSeconds.OnValueChanged += OnTimeRemainingChanged;
        titleImage.gameObject.SetActive(false);
    }

    public override void OnDestroy()
    {
        RoundTimeRemainingInSeconds.OnValueChanged -= OnTimeRemainingChanged;
    }

    // ============================================================
    // Reset
    // ============================================================
    public void ResetRoundManager()
    {
        _localRoundTimeRemainingInSeconds = roundTimeLimitInSeconds;
        resolutionBG.SetActive(false);
        BG.SetActive(true);
        if (IsServer)
        {
            roundTimeLimitInSeconds = roundTimes.Length > 0 ? roundTimes[0] : 20f;
            RoundTimeRemainingInSeconds.Value = roundTimeLimitInSeconds;
            ResolutionTimeRemaining.Value = resoluteTimeInSeconds;
            IsResolutionPhase.Value = false;
            confirmedResolutionClients.Clear();
            submittedAnswerClients.Clear();
        }

        _started = false;
        _ended = false;
        _promptGenerated = false;
        _startResolute = false;
        _currentRoundIndex = 0; // Real Game Round [1, 2, 3, ...]
        _currentRound = -1;
        _timeScaleMultiplier = 1f;
        _bannedLetters.Clear();


        resolutionBGImage.gameObject.SetActive(false);
        resolutionText.text = "";
        timeMultiplierText.text = "";
        timeMultiplierText.gameObject.SetActive(true);
        timeMultiplierIndicatorImage.gameObject.SetActive(false);
        timeMultiplierIndicatorImage.sprite = timeMultiplierIndicatorSprites[0];
        submittedAnswers.Clear();
        banLetterBG.gameObject.SetActive(false);
        bannedLettersText.text = "";
        

        winImage.SetActive(false);
        winText.text = "";
        titleImage.gameObject.SetActive(true);

        _usedWords.Clear();
        FindAnyObjectByType<PromptGenerator>().UsesPrompts.Clear();

        Debug.Log("RoundManager has been reset.");
    }

    // ============================================================
    // Update Loop (Server Logic Only)
    // ============================================================
    private void Update()
    {
        if (IsResolutionPhase.Value)
        {
            HandleResolutionPhase();
            return;
        }

        HandleRoundPhase();
    }

    // ============================================================
    // Round Phase Logic
    // ============================================================
    private void HandleRoundPhase()
    {
        if (!_started) return;
        
        _localRoundTimeRemainingInSeconds -= Time.deltaTime * _timeScaleMultiplier;

        if (!IsServer) return;

        if (!_promptGenerated)
        {
            GeneratePrompt();
            _promptGenerated = true;
        }

        RoundTimeRemainingInSeconds.Value -= Time.deltaTime * _timeScaleMultiplier;

        if (RoundTimeRemainingInSeconds.Value < 0)
        {
            OnRoundTimeOutClientRpc();
            EnterResolutionPhase();
        }
    }

    // ============================================================
    // Resolution Phase Logic
    // ============================================================
    private void HandleResolutionPhase()
    {
        if (!IsServer || !_started) return;

        if (_startResolute)
        {
            _startResolute = false;
            ResolutionTimeRemaining.Value = resoluteTimeInSeconds;
            StartCoroutine(DelayResolve());
        }

        ResolutionTimeRemaining.Value -= Time.deltaTime;
    }

    // ============================================================
    // Phase Transitions
    // ============================================================
    private IEnumerator DelayEnterNextRound()
    {
        yield return new WaitForSeconds(0.1f);
        EnterNextRound();
    }

    private IEnumerator DelayResolve()
    {
        yield return new WaitForSeconds(0.5f);
        ResoluteServerRpc();
    }

    private void EnterNextRound()
    {
        Debug.Log("Round Ended!");
        
        submittedAnswerClients.Clear();

        // Ban Letter
        _currentRoundIndex++;

        // Set Round Time
        _currentRound = Mathf.Clamp(_currentRound + 1, 0, roundTimes.Length - 1);
        roundTimeLimitInSeconds = roundTimes[_currentRound];

        EnterNextRoundClientRpc();
        
        RoundTimeRemainingInSeconds.Value = roundTimeLimitInSeconds;
    }

    private void EnterResolutionPhase()
    {
        submittedAnswerClients.Clear();
        IsResolutionPhase.Value = true;
        _startResolute = true;

        EnterResolutionPhaseClientRpc();
    }

    private void EndResolutionPhase()
    {
        Debug.Log("confirmedResolutionClients cleared!");
        confirmedResolutionClients.Clear();
        IsResolutionPhase.Value = false;

        _promptGenerated = false;
        UpdateResolutionTextClientRpc("");

        if (_ended)
        {
            _ended = false;
            string winner =
                PlayerManager.Instance.GetHost().Health.Value <= 0 ? "P2" : "P1";

            EndGameClientRpc(winner);
            return;
        }

        EnterNextRound();
        EndResolutionPhaseClientRpc(_bannedLetters.ToArray());
    }

    // ============================================================
    // Game Start & End
    // ============================================================
    private void OnGameStartedStateChanged(bool previousStartState, bool start)
    {
        if (start)
        {
            winImage.SetActive(false);
            titleImage.gameObject.SetActive(false);
            banLetterBG.gameObject.SetActive(true);
            _started = true;

            if (IsServer)
                StartCoroutine(DelayEnterNextRound());

            return;
        }

        if (!IsServer) return;

        _ended = true;
    }

    // ============================================================
    // Player Submissions
    // ============================================================
    [Rpc(SendTo.Server)]
    public void SubmitAnswerServerRpc(ulong clientId, float timeScaleMultiplier, string answer)
    {
        // Mark player as submitted
        if (!submittedAnswerClients.Contains(clientId))
        {
            submittedAnswerClients.Add(clientId);
        }

        if (submittedAnswerClients.Count == 1)
        {
            _timeScaleMultiplier = timeScaleMultiplier;
            OnSubmitAnswerClientRpc(_timeScaleMultiplier);
        }

        // Mark answer in round words list
        submittedAnswers.Add(answer);

        SoundManager.Instance?.PlaySubmitSfxServerRpc();


        if (submittedAnswerClients.Count >= 2)
            EnterResolutionPhase();
    }

    private void BanLetter()
    {
        if (submittedAnswers.Count == 0) return;
        var letterFrequencies = submittedAnswers
            .SelectMany(s => s.ToCharArray())
            .Where(char.IsLetter)
            .GroupBy(c => c)
            .OrderByDescending(g => g.Count())
            .ToList();

        char selectedLetter = '\0';
        submittedAnswers.Clear();

        foreach (var group in letterFrequencies)
        {
            char letter = group.Key;

            if (!_bannedLetters.Contains(letter))
            {
                selectedLetter = letter;
                break;
            }
        }

        if (selectedLetter == '\0')
        {
            Debug.LogWarning("No available letter to ban!");
        }
        else
        {
            _bannedLetters.Add(selectedLetter);
            UpdateBannedLettersTextClientRpc(selectedLetter);
            Debug.Log($"Banned letter {selectedLetter}");
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateBannedLettersTextClientRpc(char bannedLetter)
    {
        var letter = bannedLetter.ToString().ToUpper();
        if (bannedLettersText.text != "")
        {
            bannedLettersText.text += "\n" + letter;
        }
        else
        {
            bannedLettersText.text += letter;
        }
        
        string original = bannedLettersText.text;
        string cleaned = original.Replace("\r", "").Replace("\n", "");
        UIManager.Instance.UpdateInvalidLetters(cleaned);

    }

    [Rpc(SendTo.ClientsAndHost)]
    private void OnSubmitAnswerClientRpc(float timeScaleMultiplier)
    {
        //_timeScaleMultiplier = timeScaleMultiplier;
        _timeScaleMultiplier = 1.0f;
        Debug.Log($"Time Multiplier Text set to 1.0x");
        timeMultiplierText.text = timeScaleMultiplier.ToString("F1") + "x";
        if (timeScaleMultiplier == 1.0)
        {
            timeMultiplierIndicatorImage.sprite = timeMultiplierIndicatorSprites[0];
        }
        else if (timeScaleMultiplier == 2.0)
        {
            timeMultiplierIndicatorImage.sprite = timeMultiplierIndicatorSprites[1];
        }
        else if (timeScaleMultiplier == 3.0)
        {
            timeMultiplierIndicatorImage.sprite = timeMultiplierIndicatorSprites[2];
        }
        
        
    }

    [Rpc(SendTo.Server)]
    public void ConfirmResolutionServerRpc(ulong clientId)
    {
        if (!confirmedResolutionClients.Contains(clientId))
            confirmedResolutionClients.Add(clientId);

        PlayerManager.Instance.GetHost().UpdateConfirmClientRpc(clientId);
        PlayerManager.Instance.GetClient(1).UpdateConfirmClientRpc(clientId);

        SoundManager.Instance?.PlaySubmitSfxServerRpc();

        if (confirmedResolutionClients.Count >= 2)
            EndResolutionPhase();
    }

    // ============================================================
    // Prompt & Resolution
    // ============================================================
    private void GeneratePrompt()
    {
        if (FindAnyObjectByType<PromptGenerator>() is PromptGenerator pg)
        {
            Debug.Log("Generating prompt for round");
            pg.TryUpdatePrompt();
        }
    }

    [Rpc(SendTo.Server)]
    private void ResoluteServerRpc()
    {
        string text = "";

        if (FindAnyObjectByType<PlayerManager>() is PlayerManager pm)
        {
            int hostScore = pm.GetHost().LetterCount.Value;
            int clientScore = pm.GetClient(1).LetterCount.Value;
            int difference = hostScore - clientScore;

            string comparison = difference > 0 ? ">" :
                difference < 0 ? "<" :
                "=";

            text = $"Letter Count {hostScore}  <size=300%>{comparison}</size>  Letter Count {clientScore}";

            if (difference > 0)
                pm.GetClient(1).Health.Value -= difference;
            else if (difference < 0)
                pm.GetHost().Health.Value += difference;
        }

        // Ban Letter
        if (_currentRoundIndex % banLetterPerRound == 0)
        {
            if (IsServer)
                BanLetter();
        }

        UpdateResolutionTextClientRpc(text);
    }

    // ============================================================
    // Client RPCs
    // ============================================================
    [Rpc(SendTo.ClientsAndHost)]
    private void OnRoundTimeOutClientRpc()
    {
        foreach (var c in FindObjectsByType<Client>(FindObjectsSortMode.InstanceID))
            c.Check(true, updateHint: false);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EnterResolutionPhaseClientRpc()
    {
        SoundManager.Instance?.StopBgm();
        resolutionBG.SetActive(true);
        BG.SetActive(false);
        timeMultiplierIndicatorImage.gameObject.SetActive(false);
        timeMultiplierText.text = "";

        foreach (var c in FindObjectsByType<Client>(FindObjectsSortMode.InstanceID))
            c.OnEnterResolutionPhase();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EndResolutionPhaseClientRpc(char[] bannedLetters)
    {
        foreach (var c in FindObjectsByType<Client>(FindObjectsSortMode.InstanceID))
            c.OnEndResolutionPhase(bannedLetters.ToList());
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EnterNextRoundClientRpc()
    {
        SoundManager.Instance?.PlayGameBgm();
        resolutionBG.SetActive(false);
        BG.SetActive(true);

        resolutionBGImage.gameObject.SetActive(false);
        timeMultiplierText.text = "1.0x";
        timeMultiplierIndicatorImage.gameObject.SetActive(true);
        timeMultiplierIndicatorImage.sprite = timeMultiplierIndicatorSprites[0];

        timeMultiplierText.text = defaultTimeScaleMultiplier.ToString("F1") + "x";
        _localRoundTimeRemainingInSeconds = roundTimeLimitInSeconds;

        foreach (var c in FindObjectsByType<Client>(FindObjectsSortMode.InstanceID))
            c.OnEnterNextRound();
        
        _timeScaleMultiplier = 1f;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateResolutionTextClientRpc(string text)
    {
        resolutionBGImage.gameObject.SetActive(true);
        resolutionText.text = text;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EndGameClientRpc(string playerID)
    {
        winImage.SetActive(true);
        timeMultiplierText.text = "";
        //timeMultiplierText.gameObject.SetActive(false);
        banLetterBG.gameObject.SetActive(false);
        timeMultiplierText.text = "";
        timeMultiplierIndicatorImage.gameObject.SetActive(false);
        winText.text = playerID + " Wins";
    }

    // ============================================================
    // UI Updates
    // ============================================================
    private void OnTimeRemainingChanged(float oldValue, float newValue)
    {
        // Disabled fill updates
        _localRoundTimeRemainingInSeconds = newValue;
    }

    private List<string> _usedWords = new List<string>();
    public List<string> UsedWords => _usedWords;

    [Rpc(SendTo.Server)]
    public void MarkUsedWordServerRpc(string word)
    {
        if (!_usedWords.Contains(word))
        {
            _usedWords.Add(word);
            string packedWords = string.Join(",", _usedWords);
            UpdateUsedWordsClientRpc(packedWords);
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateUsedWordsClientRpc(string packedWords)
    {
        _usedWords = packedWords.Split(',').ToList();
    }
}