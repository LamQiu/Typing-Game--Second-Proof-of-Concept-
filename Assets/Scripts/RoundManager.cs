using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class RoundManager : NetworkBehaviour
{
    public float[] roundTimes;
    private int _currentRound = -1;
    public float roundTime = 15f;
    public Image hostTimerImage;
    public Image clientTimerImage;

    public float resoluteTime = 5f;
    private bool _startResolute;
    public TMP_Text resolutionText;
    public GameObject winImage;
    public TMP_Text winText;
    public string sharedResoluteText;

    private bool _started;
    private bool _promptGenerated;

    public NetworkVariable<bool> IsResolutionPhase = new NetworkVariable<bool>();
    [HideInInspector] public NetworkVariable<float> TimeRemaining = new NetworkVariable<float>();
    [HideInInspector] public NetworkVariable<float> ResolutionTimeRemaining = new NetworkVariable<float>();

    private void Start()
    {
        resolutionText.gameObject.SetActive(false);
        hostTimerImage.gameObject.SetActive(false);
        clientTimerImage.gameObject.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
        }

        var gm = FindAnyObjectByType<GameManager>();
        if (gm)
        {
            gm.GameStarted.OnValueChanged += OnGameStartedChanged;
        }

        TimeRemaining.OnValueChanged += OnTimeChanged;
    }

    private void OnGameStartedChanged(bool previousValue, bool newValue)
    {
        if (newValue)
        {
            _started = true;
            if (IsServer)
            {
                StartCoroutine(DelayEnterNextRound());
            }
        }
        else
        {
            winImage.SetActive(true);
            var playerID = "";
            if (PlayerManager.Instance.GetHost().Health.Value <= 0)
            {
                playerID = "Player 1";
            }
            else
            {
                playerID = "Player 2";
            }

            winText.text = playerID + " Wins";
        }
    }

    private IEnumerator DelayEnterNextRound()
    {
        yield return new WaitForSeconds(0.1f);
        EnterNextRound();
    }

    public override void OnDestroy()
    {
        TimeRemaining.OnValueChanged -= OnTimeChanged;
    }

    private void Update()
    {
        if (!IsServer) return;
        if (!_started) return;

        if (IsResolutionPhase.Value)
        {
            if (_startResolute)
            {
                _startResolute = false;
                ResolutionTimeRemaining.Value = resoluteTime;
                ResoluteServerRpc();
            }

            ResolutionTimeRemaining.Value -= Time.deltaTime;
            if (ResolutionTimeRemaining.Value <= 0)
            {
                //EndResolutionPhase();
            }

            return;
        }

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

    private void GeneratePrompt()
    {
        var pg = FindAnyObjectByType<PromptGenerator>();
        if (pg)
        {
            Debug.Log($"Generating prompt for round");
            pg.TryUpdatePrompt();
        }
    }

    List<ulong> submittedClients = new List<ulong>();

    [Rpc(SendTo.Server)]
    public void SubmitAnswerServerRpc(ulong clientId)
    {
        if (!submittedClients.Contains(clientId))
        {
            submittedClients.Add(clientId);
        }

        if (submittedClients.Count >= 2)
        {
            EnterResolutionPhase();
        }
    }

    List<ulong> confirmedClients = new List<ulong>();

    [Rpc(SendTo.Server)]
    public void ConfirmResolutionServerRpc(ulong clientId)
    {
        if (!confirmedClients.Contains(clientId))
        {
            confirmedClients.Add(clientId);
        }

        PlayerManager.Instance.GetHost().UpdateConfirmClientRpc(clientId);
        PlayerManager.Instance.GetClient(1).UpdateConfirmClientRpc(clientId);

        if (confirmedClients.Count >= 2)
        {
            EndResolutionPhase();
        }
    }

    private void EnterResolutionPhase()
    {
        submittedClients.Clear();
        IsResolutionPhase.Value = true;
        _startResolute = true;

        EnterResolutionPhaseClientRpc();
    }

    private void EndResolutionPhase()
    {
        confirmedClients.Clear();
        IsResolutionPhase.Value = false;
        _promptGenerated = false;
        UpdateResolutionTextClientRpc("");
        EnterNextRound();

        EndResolutionPhaseClientRpc();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void OnRoundTimeOutClientRpc()
    {
        var clients = FindObjectsByType<Client>(FindObjectsSortMode.InstanceID);
        foreach (var client in clients)
        {
            client.Check();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EnterResolutionPhaseClientRpc()
    {
        var clients = FindObjectsByType<Client>(FindObjectsSortMode.InstanceID);
        foreach (var client in clients)
        {
            client.OnEnterResolutionPhase();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EndResolutionPhaseClientRpc()
    {
        var clients = FindObjectsByType<Client>(FindObjectsSortMode.InstanceID);
        foreach (var client in clients)
        {
            client.OnEndResolutionPhase();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void EnterNextRoundClientRpc()
    {
        resolutionText.gameObject.SetActive(false);
        hostTimerImage.gameObject.SetActive(true);
        clientTimerImage.gameObject.SetActive(true);
        var clients = FindObjectsByType<Client>(FindObjectsSortMode.InstanceID);
        foreach (var client in clients)
        {
            client.OnEnterNextRound();
        }
    }

    [Rpc(SendTo.Server)]
    private void ResoluteServerRpc()
    {
        string text = "";
        var pm = FindAnyObjectByType<PlayerManager>();
        if (pm)
        {
            var hostScore = pm.GetHost().LetterCount.Value;
            var clientScore = pm.GetClient(1).LetterCount.Value;
            var difference = hostScore - clientScore;
            var biggerOrSmallerOrEqual = difference > 0 ? ">" : "<";
            if (difference == 0) biggerOrSmallerOrEqual = "=";
            text = "Player 1 Letter Count " + hostScore + "    " + //"\n" +
                   biggerOrSmallerOrEqual + "    " + //"\n" +
                   "Player 2 Letter Count " + clientScore; //+ "\n";

            if (difference > 0)
            {
                pm.GetClient(1).Health.Value -= difference;
            }
            else if (difference < 0)
            {
                pm.GetHost().Health.Value += difference;
            }
        }

        UpdateResolutionTextClientRpc(text);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateResolutionTextClientRpc(string text)
    {
        resolutionText.gameObject.SetActive(true);
        resolutionText.text = text;
    }

    private void EnterNextRound()
    {
        Debug.Log("Round Ended!");
        _currentRound++;
        if (_currentRound >= roundTimes.Length)
        {
            _currentRound = roundTimes.Length - 1;
        }

        roundTime = roundTimes[_currentRound];
        TimeRemaining.Value = roundTime;
        EnterNextRoundClientRpc();
    }

    private void OnTimeChanged(float oldValue, float newValue)
    {
        if (hostTimerImage != null && clientTimerImage != null)
        {
            hostTimerImage.fillAmount = newValue / roundTime;
            clientTimerImage.fillAmount = newValue / roundTime;
        }
    }
}