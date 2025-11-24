using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RoundManager : NetworkBehaviour
{
    // ----------------------------
    // Public Inspector Variables
    // ----------------------------
    public float[] roundTimes;
    public float roundTime = 15f;
    public float resoluteTime = 5f;

    public Image hostTimerImage;
    public Image clientTimerImage;

    public TMP_Text resolutionText;
    public TMP_Text winText;
    public GameObject winImage;
    public GameObject titleImage;

    // ----------------------------
    // Networked Variables
    // ----------------------------
    public NetworkVariable<bool> IsResolutionPhase = new NetworkVariable<bool>();
    [HideInInspector] public NetworkVariable<float> TimeRemaining = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<float> ResolutionTimeRemaining = new NetworkVariable<float>();

    // ----------------------------
    // Private State
    // ----------------------------
    private bool _started;
    private bool _promptGenerated;
    private bool _startResolute;

    private int _currentRound = -1;

    private readonly List<ulong> submittedAnswerClients = new List<ulong>();
    private readonly List<ulong> confirmedResolutionClients = new List<ulong>();

    // ============================================================
    // Unity Lifecycle
    // ============================================================
    public void ResetRoundManager()
    {
        if (IsServer)
        {
            // Reset timers
            roundTime = roundTimes.Length > 0 ? roundTimes[0] : 15f;
            TimeRemaining.Value = roundTime;
            ResolutionTimeRemaining.Value = resoluteTime;

            // Reset networked state
            IsResolutionPhase.Value = false;
        }

        // Reset internal state
        _started = false;
        _promptGenerated = false;
        _startResolute = false;

        _currentRound = -1;

        submittedAnswerClients.Clear();
        confirmedResolutionClients.Clear();

        // Reset UI elements
        resolutionText.gameObject.SetActive(false);
        resolutionText.text = "";

        winImage.SetActive(false);
        winText.text = "";

        hostTimerImage.gameObject.SetActive(false);
        clientTimerImage.gameObject.SetActive(false);

        titleImage.gameObject.SetActive(true);

        Debug.Log("RoundManager has been reset.");
    }

    private void Start()
    {
        // resolutionText.gameObject.SetActive(false);
        // hostTimerImage.gameObject.SetActive(false);
        // clientTimerImage.gameObject.SetActive(false);
        ResetRoundManager();
    }
    public override void OnNetworkSpawn()
    {
        if (FindAnyObjectByType<GameManager>() is GameManager gm)
        {
            gm.GameStartedState.OnValueChanged += OnGameStartedStateChanged;
        }

        TimeRemaining.OnValueChanged += OnTimeChanged;
        titleImage.gameObject.SetActive(false);
    }

    public override void OnDestroy()
    {
        TimeRemaining.OnValueChanged -= OnTimeChanged;
    }


    // ============================================================
    // Update Loop (Server Only)
    // ============================================================

    private void Update()
    {
        if (!IsServer || !_started) return;

        if (IsResolutionPhase.Value)
        {
            HandleResolutionPhase();
            return;
        }

        HandleRoundPhase();
    }


    // ============================================================
    // Round & Phase Flow
    // ============================================================

    private void HandleRoundPhase()
    {
        if (!_promptGenerated)
        {
            GeneratePrompt();
            _promptGenerated = true;
        }

        TimeRemaining.Value -= Time.deltaTime;

        if (TimeRemaining.Value <= 0)
        {
            OnRoundTimeOutClientRpc();
            EnterResolutionPhase();
        }
    }

    private void HandleResolutionPhase()
    {
        if (_startResolute)
        {
            _startResolute = false;
            ResolutionTimeRemaining.Value = resoluteTime;
            StartCoroutine(DelayResolve());
        }

        ResolutionTimeRemaining.Value -= Time.deltaTime;

        // logic unchanged - original did nothing when <= 0
    }


    private IEnumerator DelayEnterNextRound()
    {
        yield return new WaitForSeconds(0.1f);
        EnterNextRound();
    }

    private IEnumerator DelayResolve()
    {
        yield return null;
        ResoluteServerRpc();
    }


    private void EnterNextRound()
    {
        Debug.Log("Round Ended!");

        _currentRound = Mathf.Clamp(_currentRound + 1, 0, roundTimes.Length - 1);

        roundTime = roundTimes[_currentRound];
        TimeRemaining.Value = roundTime;

        EnterNextRoundClientRpc();
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
        confirmedResolutionClients.Clear();
        IsResolutionPhase.Value = false;

        _promptGenerated = false;
        UpdateResolutionTextClientRpc("");

        EnterNextRound();
        EndResolutionPhaseClientRpc();
    }


    // ============================================================
    // Game Start / End
    // ============================================================

    private void OnGameStartedStateChanged(bool previousStartState, bool start)
    {
        // Start
        if (start)
        {
            _started = true;

            if (IsServer)
                StartCoroutine(DelayEnterNextRound());
        }
        // End
        else
        {
            if (!IsServer) return;

            string winner =
                PlayerManager.Instance.GetHost().Health.Value <= 0 ? "Player 2" : "Player 1";

            EndGameClientRpc(winner);
        }
    }


    // ============================================================
    // Player Submissions / Confirmations
    // ============================================================

    [Rpc(SendTo.Server)]
    public void SubmitAnswerServerRpc(ulong clientId)
    {
        if (!submittedAnswerClients.Contains(clientId))
            submittedAnswerClients.Add(clientId);

        if (submittedAnswerClients.Count >= 2)
            EnterResolutionPhase();
    }

    [Rpc(SendTo.Server)]
    public void ConfirmResolutionServerRpc(ulong clientId)
    {
        if (!confirmedResolutionClients.Contains(clientId))
            confirmedResolutionClients.Add(clientId);

        PlayerManager.Instance.GetHost().UpdateConfirmClientRpc(clientId);
        PlayerManager.Instance.GetClient(1).UpdateConfirmClientRpc(clientId);

        if (confirmedResolutionClients.Count >= 2)
            EndResolutionPhase();
    }
    
    // ============================================================
    // Prompt + Result Resolution
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

            string comparison = difference > 0 ? ">" : difference < 0 ? "<" : "=";

            text = $"Player 1 Letter Count {hostScore}    {comparison}    Player 2 Letter Count {clientScore}";

            if (difference > 0)
                pm.GetClient(1).Health.Value -= difference;
            else if (difference < 0)
                pm.GetHost().Health.Value += difference;
        }

        UpdateResolutionTextClientRpc(text);
    }


    // ============================================================
    // Client RPC Calls
    // ============================================================

    [Rpc(SendTo.ClientsAndHost)]
    private void OnRoundTimeOutClientRpc()
    {
        foreach (var c in FindObjectsByType<Client>(FindObjectsSortMode.InstanceID))
            c.Check(true);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EnterResolutionPhaseClientRpc()
    {
        foreach (var c in FindObjectsByType<Client>(FindObjectsSortMode.InstanceID))
            c.OnEnterResolutionPhase();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EndResolutionPhaseClientRpc()
    {
        foreach (var c in FindObjectsByType<Client>(FindObjectsSortMode.InstanceID))
            c.OnEndResolutionPhase();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EnterNextRoundClientRpc()
    {
        resolutionText.gameObject.SetActive(false);
        hostTimerImage.gameObject.SetActive(true);
        clientTimerImage.gameObject.SetActive(true);

        foreach (var c in FindObjectsByType<Client>(FindObjectsSortMode.InstanceID))
            c.OnEnterNextRound();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateResolutionTextClientRpc(string text)
    {
        resolutionText.gameObject.SetActive(true);
        resolutionText.text = text;
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EndGameClientRpc(string playerID)
    {
        winImage.SetActive(true);
        winText.text = playerID + " Wins";
    }
    // ============================================================
    // UI Updates
    // ============================================================

    private void OnTimeChanged(float oldValue, float newValue)
    {
        if (hostTimerImage != null)
            hostTimerImage.fillAmount = newValue / roundTime;

        if (clientTimerImage != null)
            clientTimerImage.fillAmount = newValue / roundTime;
    }
}