using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace UI
{
    public class GameScreenUI : MonoBehaviour
    {
        [SerializeField] private GameObject P1BG;
        [SerializeField] private GameObject P2BG;
        [SerializeField] private TMP_Text Player1NameText;
        [SerializeField] private TMP_Text Player2NameText;
        [SerializeField] private Color PlayerActiveTextColor;
        [SerializeField] private Color PlayerInactiveTextColor;
        [SerializeField] private GameObject P1LettersCountUI;
        [SerializeField] private GameObject P2LettersCountUI;
        [SerializeField] private TMP_Text CurrentPromptText;
        [SerializeField] private Image TimerImage;
        [SerializeField] private TMP_InputField WordInputField;
        [SerializeField] private TMP_Text InvalidLettersText;

        public void Show()
        {
            gameObject.SetActive(true);
            WordInputField.ActivateInputField();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetP1()
        {
            P1BG.gameObject.SetActive(true);
            P2BG.gameObject.SetActive(false);
            Player1NameText.color = PlayerActiveTextColor;
            Player2NameText.color = PlayerInactiveTextColor;
        }
        
        public void SetP2()
        {
            P1BG.gameObject.SetActive(false);
            P2BG.gameObject.SetActive(true);
            Player1NameText.color = PlayerInactiveTextColor;
            Player2NameText.color = PlayerActiveTextColor;
        }

        public void UpdateP1LettersCountUI(int lettersCount)
        {
            UpdateLettersCountUI(P1LettersCountUI, lettersCount);
        }
        
        public void UpdateP2LettersCountUI(int lettersCount)
        {
            UpdateLettersCountUI(P2LettersCountUI, lettersCount);
        }

        private void UpdateLettersCountUI(GameObject letterCountUI, int lettersCount)
        {
            for (int i = 0; i < letterCountUI.transform.childCount; i++)
            {
                if (i < lettersCount)
                {
                    letterCountUI.transform.GetChild(i).gameObject.SetActive(true);
                }
                else
                {
                    letterCountUI.transform.GetChild(i).gameObject.SetActive(false);
                }
            }
        }
        
        public void UpdateCurrentPrompt(string prompt)
        {
            CurrentPromptText.text = prompt;
        }

        public void UpdateTimer(float timeT)
        {
            TimerImage.fillAmount = timeT;
        }

        public void AddListenerOnWordInputField(UnityAction<string> onWordSubmit)
        {
            WordInputField.onValueChanged.AddListener(onWordSubmit);
        }

        public void UpdateCurrentWordInputFieldInteractability(bool interactable)
        {
            WordInputField.interactable = interactable;
        }
        
        public void UpdateInvalidLetters(string invalidLetters)
        {
            InvalidLettersText.text = "invalid letters" + invalidLetters;
        }

        private void Update()
        {
            if (gameObject.activeSelf)
            {
                WordInputField.Select();
            }
        }
    }
}