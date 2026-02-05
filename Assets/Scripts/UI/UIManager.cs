using System;
using Unity.Netcode;
using UnityEngine;

namespace UI
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private MainMenuUI MainMenuUI;
        [SerializeField] private ConnectionScreenUI ConnectionScreenUI;
        [SerializeField] private WaitingScreenUI WaitingScreenUI;
        [SerializeField] private GameScreenUI GameScreenUI;
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
            WaitingScreenUI.Hide();
            GameScreenUI.Show();
        }
        
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