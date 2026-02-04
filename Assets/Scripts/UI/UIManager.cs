using System;
using UnityEngine;

namespace UI
{
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private MainMenuUI MainMenuUI;
        public string MainMenuCommandInputFieldEnterPlayKey = "play";

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            MainMenuUI.Show();
        }

        public void EnterPlay()
        {
            MainMenuUI.Hide();
        }

        private void Update()
        {
        }
    }
}