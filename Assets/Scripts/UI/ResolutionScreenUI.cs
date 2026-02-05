using TMPro;
using UnityEngine;

namespace UI
{
    public class ResolutionScreenUI : MonoBehaviour
    {
        public TMP_Text ResolutionPressSpaceHintText;
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
