using System;
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
            ConnectionScreenUI.Hide();
            WaitingScreenUI.Hide();
            ResolutionScreenUI.Hide();
            GameScreenUI.Show();
        }

        public void EnterResolutionScreen()
        {
            GameScreenUI.Hide();
            ResolutionScreenUI.Show();
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

        public void UpdateP1LettersCountUI(int lettersCount)
        {
            GameScreenUI.UpdateP1LettersCountUI(lettersCount);
        }
        public void UpdateP2LettersCountUI(int lettersCount)
        {
            GameScreenUI.UpdateP2LettersCountUI(lettersCount);
        }

        public void UpdateCurrentPrompt(string prompt)
        {
            GameScreenUI.UpdateCurrentPrompt(prompt);
        }

        public void UpdateGameScreenTimer(float timeT)
        {
            GameScreenUI.UpdateTimer(timeT);
        }

        public void AddListenerOnWordInputField(UnityAction<string> onWordSubmit)
        {
            GameScreenUI.AddListenerOnWordInputField(onWordSubmit);
        }

        public void UpdateCurrentWordInputFieldInteractability(bool interactable)
        {
            GameScreenUI.UpdateCurrentWordInputFieldInteractability(interactable);
        }

        public void UpdateInvalidLetters(string invalidLetters)
        {
            GameScreenUI.UpdateInvalidLetters(invalidLetters);
        }
        
        #endregion


        #region ResolutionScreen UI

        public void UpdateP1AnswerText(string text)
        {
            ResolutionScreenUI.UpdateP1AnswerText(text);
        }
        public void UpdateP2AnswerText(string text)
        {
            ResolutionScreenUI.UpdateP2AnswerText(text);
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
            ResolutionScreenUI.UpdateResolutionPressSpaceHintText(content);
        }
        
        public void UpdatePlayer1FillImage(float value)
        {
            ResolutionScreenUI.UpdatePlayer1FillImage(value);
        }
        public void UpdatePlayer2FillImage(float value)
        {
            ResolutionScreenUI.UpdatePlayer2FillImage(value);
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
    }
}