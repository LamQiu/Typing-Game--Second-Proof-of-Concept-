using TMPro;
using UnityEngine;

namespace UI
{
    public class WaitingScreenUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text ROOMIDText;

        public void Show(string roomID)
        {
            ROOMIDText.text = $"room id: <color=white>{roomID}</color>";
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}