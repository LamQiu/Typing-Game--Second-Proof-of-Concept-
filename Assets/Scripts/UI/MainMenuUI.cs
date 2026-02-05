using System;
using TMPro;
using UnityEngine;

namespace UI
{
    public class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField CommandInputField;


        private void OnEnable()
        {
            CommandInputField.onEndEdit.AddListener(OnCommandInputFieldEndEdit);
        }

        private void OnDisable()
        {
            CommandInputField.onEndEdit.RemoveListener(OnCommandInputFieldEndEdit);
        }

        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        private void OnCommandInputFieldEndEdit(string content)
        {
            if(content.ToLower() == UIManager.Instance.MainMenuCommandInputFieldEnterPlayKey)
            {
                UIManager.Instance.EnterConnectionScreen();
            }
        }

        private void Update()
        {
            if(gameObject.activeSelf)
            {
                CommandInputField.Select();
                CommandInputField.ActivateInputField();
            }
        }
    }
}
