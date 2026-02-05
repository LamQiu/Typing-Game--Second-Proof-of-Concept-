using System;
using UnityEngine;

namespace UI
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private MainMenuUI MainMenuUI;
        [SerializeField] private ConnectionScreenUI ConnectionScreenUI;
        public string MainMenuCommandInputFieldEnterPlayKey = "play";

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            MainMenuUI.Show();
            ConnectionScreenUI.Hide();
        }

        public void EnterConnectionScreen()
        {
            MainMenuUI.Hide();
            ConnectionScreenUI.Show();
        }

        private void Update()
        {
        }
    }
}