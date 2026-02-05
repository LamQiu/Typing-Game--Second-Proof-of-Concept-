using System;
using Blocks.Sessions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ConnectionScreenUI : MonoBehaviour
    {
        [SerializeField] private TMP_InputField CreateSessionCodeInputField;
        [SerializeField] private GameObject CreateSessionWidget;
        [SerializeField] private TMP_InputField CreateSessionWidgetInputField;
        [SerializeField] private Button CreateSessionButton;
        [SerializeField] private GameObject QuickJoinWidget;
        [SerializeField] private Button QuickJoinButton;
        
        private void OnEnable()
        {
            CreateSessionCodeInputField.onEndEdit.AddListener(OnCreateSessionCodeInputFieldEndEdit);
        }

        private void OnDisable()
        {
            CreateSessionCodeInputField.onEndEdit.RemoveListener(OnCreateSessionCodeInputFieldEndEdit);
        }
        public void Show()
        {
            gameObject.SetActive(true);
            CreateSessionCodeInputField.ActivateInputField();
            CreateSessionWidget.SetActive(true);
            QuickJoinWidget.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
            CreateSessionWidget.SetActive(false);
            QuickJoinWidget.SetActive(false);
        }

        private void Update()
        {
            if (gameObject.activeSelf)
            {
                //CreateSessionCodeInputField.Select();
            }
        }
        
        private void OnCreateSessionCodeInputFieldEndEdit(string content)
        {
            if(content.Length == 0)
                return;
            
            CreateSessionWidgetInputField.text = content;
            CreateSessionButton.onClick.Invoke();
            CreateSessionWidget.SetActive(false);
            UIManager.Instance.EnterWaitingScreen();
        }
    }
}