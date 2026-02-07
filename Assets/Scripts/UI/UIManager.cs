using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private MainMenuUI MainMenuUI;
        [SerializeField] private ConnectionScreenUI ConnectionScreenUI;
        [SerializeField] private WaitingScreenUI WaitingScreenUI;
        [SerializeField] private GameScreenUI GameScreenUI;
        [SerializeField] private ResolutionScreenUI ResolutionScreenUI;
        [SerializeField] private WinScreen WinScreen;
        public string MainMenuCommandInputFieldEnterPlayKey = "play";

        private Client m_client;

        public Client Client
        {
            get => m_client;
            set => m_client = value;
        }

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            MainMenuUI.Show();
            ConnectionScreenUI.Hide();
            WaitingScreenUI.Hide();
            GameScreenUI.Hide();
            ResolutionScreenUI.Hide();
            WinScreen.Hide();
        }

        public void EnterConnectionScreen()
        {
            MainMenuUI.Hide();
            ConnectionScreenUI.Show();
        }

        public void EnterWaitingScreen()
        {
            ConnectionScreenUI.Hide();
            WaitingScreenUI.Show();
        }

        public void EnterGameScreen()
        {
            StartCoroutine(DelayEnterGameScreen());
        }
        
        private const float k_delayEnterGameScreenInSeconds = 0.2f;

        private IEnumerator DelayEnterGameScreen()
        {
            yield return new WaitForSeconds(k_delayEnterGameScreenInSeconds);
            ConnectionScreenUI.Hide();
            WaitingScreenUI.Hide();
            ResolutionScreenUI.Hide();
            GameScreenUI.Show();
            GameScreenUI.ClearWordInputField();
        }

        public void EnterResolutionScreen()
        {
            GameScreenUI.Hide();
            ResolutionScreenUI.Show();
        }

        public void EnterWinScreen()
        {
            WinScreen.Show();
        }

        public void UpdateWinText(string text)
        {
            WinScreen.UpdateWinText(text);
        }

        #region GameScreen UI

        public void SetP1()
        {
            GameScreenUI.SetP1();
        }

        public void SetP2()
        {
            GameScreenUI.SetP2();
        }

        public void UpdateP1LettersCountUI(int lettersCount, bool isOwner )
        {
            GameScreenUI.UpdateP1LettersCountUI(lettersCount, isOwner);
        }

        public void UpdateP2LettersCountUI(int lettersCount, bool isOwner)
        {
            GameScreenUI.UpdateP2LettersCountUI(lettersCount, isOwner);
        }

        public void UpdateCurrentPrompt(string prompt)
        {
            GameScreenUI.UpdateCurrentPrompt(GetTextWithTransparentColor(prompt.ToLower()));
        }

        public void UpdateGameScreenTimer(float timeT)
        {
            GameScreenUI.UpdateTimer(timeT);
        }

        public void AddListenerOnWordInputField(UnityAction<string> onWordSubmit)
        {
            GameScreenUI.AddListenerOnWordInputField(onWordSubmit);
        }

        public void UpdateWordInputField(string content)
        {
            GameScreenUI.UpdateWordInputField(content);
        }

        public void UpdateCurrentWordInputFieldInteractability(bool interactable)
        {
            GameScreenUI.UpdateCurrentWordInputFieldInteractability(interactable);
        }

        public void UpdateInvalidLetters(string invalidLetters)
        {
            GameScreenUI.UpdateInvalidLetters(GetTextWithTransparentColor(invalidLetters));
        }

        #endregion


        #region ResolutionScreen UI

        public void UpdateP1AnswerText(string text)
        {
            ResolutionScreenUI.UpdateP1AnswerText(GetTextWithTransparentColor(text));
        }

        public void UpdateP2AnswerText(string text)
        {
            ResolutionScreenUI.UpdateP2AnswerText(GetTextWithTransparentColor(text));
        }

        public void ResolutionScreenSetP1()
        {
            ResolutionScreenUI.SetP1();
        }

        public void ResolutionScreenSetP2()
        {
            ResolutionScreenUI.SetP2();
        }

        public void UpdateResolutionPressSpaceHintText(string content)
        {
            ResolutionScreenUI.UpdateResolutionPressSpaceHintText((content));
        }

        public void UpdatePlayer1FillImage(float value, int currentScore)
        {
            ResolutionScreenUI.UpdatePlayer1FillImage(value, currentScore);
        }

        public void UpdatePlayer2FillImage(float value, int currentScore)
        {
            ResolutionScreenUI.UpdatePlayer2FillImage(value, currentScore);
        }

        #endregion

        private bool m_isGameStarted = false;

        private void Update()
        {
            if (!m_isGameStarted && NetworkManager.Singleton.ConnectedClients.Count == 2)
            {
                m_isGameStarted = true;
                EnterGameScreen();
            }
        }

        private string m_bannedLetters;

        public void UpdateBannedLetters(string bannedLetters)
        {
            m_bannedLetters = bannedLetters;
        }

        public string GetTextWithTransparentColor(string text)
        {
            //return text;
            string result = "";
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (m_bannedLetters != null &&
                    c != ' ' &&
                    m_bannedLetters.ToLower().Contains(char.ToLower(c)))
                {
                    result += $"<color=#CCCCCCAA>{c}</color>";
                }
                else
                {
                    result += c;
                }
            }


            return result;
        }
    }
}