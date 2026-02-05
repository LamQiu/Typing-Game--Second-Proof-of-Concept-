using TMPro;
using UnityEngine;

namespace UI
{
    public class ResolutionScreenUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text P1AnswerText;
        [SerializeField] private TMP_Text P2AnswerText;
        [SerializeField] private GameObject P1BG;
        [SerializeField] private GameObject P2BG;
        [SerializeField] private TMP_Text Player1NameText;
        [SerializeField] private TMP_Text Player2NameText;
        [SerializeField] private Color PlayerActiveTextColor;
        [SerializeField] private Color PlayerInactiveTextColor;
        public TMP_Text ResolutionPressSpaceHintText;

        public void UpdateP1AnswerText(string text)
        {
            P1AnswerText.text = text;
        }
        public void UpdateP2AnswerText(string text)
        {
            P2AnswerText.text = text;
        }
        public void SetP1()
        {
            P1BG.gameObject.SetActive(false);
            P2BG.gameObject.SetActive(true);
            Player1NameText.color = PlayerActiveTextColor;
            Player2NameText.color = PlayerInactiveTextColor;
        }
        
        public void SetP2()
        {
            P1BG.gameObject.SetActive(true);
            P2BG.gameObject.SetActive(false);
            Player1NameText.color = PlayerInactiveTextColor;
            Player2NameText.color = PlayerActiveTextColor;
        }
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void UpdateResolutionPressSpaceHintText(string content)
        {
            ResolutionPressSpaceHintText.text = content;
        }
    }
}
