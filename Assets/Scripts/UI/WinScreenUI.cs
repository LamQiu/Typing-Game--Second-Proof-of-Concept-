using TMPro;
using UnityEngine;

namespace UI
{
    public class WinScreenUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text WinText;
        [SerializeField] private TMP_Text WinText1;
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
        
        public void UpdateWinText(string text)
        {
            WinText.text = text;
            WinText1.text = text;
        }
    }
}
