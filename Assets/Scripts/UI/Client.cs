using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UI;
using UnityEngine.InputSystem;

public class Client : NetworkBehaviour
{
    public string HintText;

    #region ===== Network Variables =====

    public NetworkVariable<int> LetterCount = new NetworkVariable<int>(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    public NetworkVariable<int> CurrentScore = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    #endregion

    private WordChecker _wordChecker;
    private RoundManager _roundManager;
    private PromptGenerator.Prompt _currentPrompt;

    private string m_answer = "";
    public string Answer => m_answer;
    private bool _checkValid = false;
    private Client m_otherClient;

    #region ===== Reset Helpers =====

    private void ResetLocalStates()
    {
        _checkValid = false;
        m_answer = "";
    }

    private void ResetUI()
    {
        HintText = "";
    }

    private void ResetNetworkVariables()
    {
        if (IsOwner)
        {
            LetterCount.Value = 0;
        }

        if (IsServer)
        {
            CurrentScore.Value = 0;
        }
    }

    public void ResetClient()
    {
        ResetLocalStates();
        ResetUI();
        ResetNetworkVariables();
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
    }

    private void UpdatePrompt(PromptGenerator.Prompt value)
    {
        _currentPrompt = value;
        UIManager.Instance.UpdateCurrentPrompt(value.ToString());
    }

    private void ClearCurrentAnswer()
    {
        m_answer = "";
        UIManager.Instance.UpdateAnswerInputField("");
    }

    #endregion


    #region ===== Callbacks =====

    private void OnTimeRemainingChanged(float prev, float value)
    {
        UpdateTimerUI(value);
    }

    private void OnLetterCountChanged(int prev, int value)
    {
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

    private void OnCurrentScoreChanged(int prev, int value)
    {
        if (value >= GameManager.s_WinGameScore)
        {
            value = 0;
        }

        UIManager.Instance.UpdatePlayerFillImage(IsHost, CurrentScore.Value, m_otherClient.CurrentScore.Value);
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


        if (IsOwner)
        {
            CheckWinStateServerRpc(OwnerClientId);
        }

        HintText = "Press Enter to Continue";

        // if (IsOwner)
        //     SubmitAnswerDisplayServerRpc(GetNonTransparentString(answerAreaText.text));

        UIManager.Instance.UpdatePlayerFillImage(IsHost, CurrentScore.Value, m_otherClient.CurrentScore.Value);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateScoreUIClientRpc()
    {
        if (IsOwner)
        {
            if (IsHost)
            {
                UIManager.Instance.UpdatePlayer1FillImage(CurrentScore.Value / (float)GameManager.s_WinGameScore,
                    CurrentScore.Value);
                if (m_otherClient != null)
                {
                    UIManager.Instance.UpdatePlayer2FillImage(
                        m_otherClient.CurrentScore.Value / (float)GameManager.s_WinGameScore,
                        m_otherClient.CurrentScore.Value);
                }
            }
            else if (IsClient)
            {
                UIManager.Instance.UpdatePlayer2FillImage(CurrentScore.Value / (float)GameManager.s_WinGameScore,
                    CurrentScore.Value);
                if (m_otherClient != null)
                {
                    UIManager.Instance.UpdatePlayer1FillImage(
                        m_otherClient.CurrentScore.Value / (float)GameManager.s_WinGameScore,
                        m_otherClient.CurrentScore.Value);
                }
            }
        }
    }

    public void OnEndResolutionPhase(List<char> bannedLetters)
    {
        UIManager.Instance.UpdateAnswerInputFieldInteractability(true);
        ClearCurrentAnswer();
    }

    [Rpc(SendTo.Server)]
    public void CheckWinStateServerRpc(ulong id)
    {
        if (CurrentScore.Value >= GameManager.s_WinGameScore)
        {
            GameManager.Instance.EndGameServerRpc();
        }
    }

    public void OnEnterNextRound()
    {
        if (m_otherClient == null)
        {
            m_otherClient = GetOtherClient();
        }

        HintText = "Press Enter to Submit";

        if (IsOwner)
        {
            UIManager.Instance.UpdateAnswerInputFieldInteractability(true);
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

            LetterCount.Value = 0;
        }

        _checkValid = false;

        //TrySubmitAnswer();
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void UpdateConfirmClientRpc(ulong id)
    {
        if (NetworkManager.Singleton.LocalClientId == id)
        {
            Debug.Log(($"Update Resolution Press Space Hint Text: clientID: {id}"));
            HintText = "";
            UIManager.Instance.UpdateResolutionPressSpaceHintText("");
        }
    }

    #endregion

    #region ===== Input Handling =====

    private void Start()
    {
        if (IsOwner)
        {
            UIManager.Instance.AddListenerToAnswerInputField(OnLocalInputFieldChanged);
            _wordChecker = new WordChecker();
        }
    }

    private void Update()
    {
        UpdateTimerUI(_roundManager.LocalRoundTimeRemainingInSeconds);

        if (!IsOwner) return;

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (_roundManager.IsResolutionPhase.Value)
            {
                _roundManager.ConfirmResolutionServerRpc(OwnerClientId);
            }
        }

        if (Keyboard.current.enterKey.wasPressedThisFrame)
        {
            if (!_roundManager.IsResolutionPhase.Value)
            {
                if (!TrySubmitAnswer())
                {
                    ClearCurrentAnswer();
                }
            }
        }

        if (IsHost)
        {
            UIManager.Instance.UpdateP1LettersCountUI(LetterCount.Value, true);
            if (m_otherClient != null)
            {
                UIManager.Instance.UpdateP2LettersCountUI(m_otherClient.LetterCount.Value, false);
            }
        }
        else if (IsClient)
        {
            if (m_otherClient != null)
            {
                UIManager.Instance.UpdateP1LettersCountUI(m_otherClient.LetterCount.Value, false);
            }

            UIManager.Instance.UpdateP2LettersCountUI(LetterCount.Value, true);
        }
    }

    public override void OnDestroy()
    {
        _roundManager.RoundTimeRemainingInSeconds.OnValueChanged -= OnTimeRemainingChanged;
    }

    #endregion

    #region ===== Word Checking & Submitting =====

    public bool TrySubmitAnswer()
    {
        if (!IsOwner || _checkValid) return false;

        string answer = m_answer;
        bool isAnswerValidInDictionary = _wordChecker.CheckWordDictionaryValidity(answer);
        if (!isAnswerValidInDictionary)
        {
            HintText = $"invalid word {m_answer}. try again";
            return false;
        }

        bool isAnswerValidForCurrentPrompt = _wordChecker.CheckWordPromptValidity(answer, _currentPrompt);
        if (!isAnswerValidForCurrentPrompt)
        {
            HintText = $"word {answer} doesn't meet criteria. try again";
            return false;
        }

        bool isAnswerUsed = _roundManager.IsAnswerUsed(answer);
        if (isAnswerUsed)
        {
            HintText = "word already used";
            return false;
        }

        HintText = $"\"{answer}\" submitted";
        MarkUsedWord(answer.ToLower());

        Debug.Log($"SubmitAnswerServerRpc {OwnerClientId}");
        _roundManager.SubmitAnswerServerRpc(OwnerClientId, answer);
        _checkValid = true;
        UIManager.Instance.UpdateAnswerInputFieldInteractability(false);
        SoundManager.Instance?.PlaySubmitSfxServerRpc();

        return true;
    }

    #endregion

    #region ===== Used Words Sync =====

    private void MarkUsedWord(string word)
    {
        _roundManager.MarkUsedWordServerRpc(word);
    }

    #endregion

    private void OnLocalInputFieldChanged(string value)
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlayTypingSfx();

        m_answer = UIManager.Instance.RemoveColorTags(value);
        UpdateServerAnswerServerRpc(m_answer);
        LetterCount.Value = _roundManager.GetValidLetterCount(m_answer);
        UIManager.Instance.UpdateAnswerInputField(m_answer);
    }

    [Rpc(SendTo.Server)]
    private void UpdateServerAnswerServerRpc(string answer)
    {
        m_answer = answer;
    }
}