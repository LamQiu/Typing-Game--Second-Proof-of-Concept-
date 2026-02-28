using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class ResolutionScreenUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text P1AnswerText;
        [SerializeField] private TMP_Text P2AnswerText;
        [SerializeField] private float AnswerTextPopupIntervalInSeconds;
        [SerializeField] private GameObject P1BG;
        [SerializeField] private GameObject P2BG;
        [SerializeField] private TMP_Text Player1NameText;
        [SerializeField] private TMP_Text Player2NameText;
        [SerializeField] private Image Player1FillImage;
        [SerializeField] private Image Player2FillImage;
        [SerializeField] private Image Player1BGFillImage;
        [SerializeField] private Image Player2BGFillImage;
        [SerializeField] private TMP_Text Player1ScoreText;
        [SerializeField] private TMP_Text Player2ScoreText;
        [SerializeField] private Color PlayerActiveTextColor;
        [SerializeField] private Color PlayerInactiveTextColor;
        [SerializeField] private TMP_Text MaxHpText;
        public TMP_Text ResolutionPressSpaceHintText;

        private void Awake()
        {
            Reset();
        }

        public void Reset()
        {
            Player1FillImage.fillAmount = 0;
            Player2FillImage.fillAmount = 0;
            Player1BGFillImage.fillAmount = 0;
            Player2BGFillImage.fillAmount = 0;
            
            MaxHpText.text = GameManager.Instance.MaxPlayerHp.ToString();
            Player1BGFillImage.fillAmount = 1f;
            Player2BGFillImage.fillAmount = 1f;

            SetPlayer1ScoreTextsAnchoredPos(0);
            SetPlayer2ScoreTextsAnchoredPos(0);
        }
        
        private const float k_playerScoreTextAnchoredPosXMaximum = 520;
        private const float k_playerScoreTextAnchoredPosXOffset = 40;

        private void SetPlayer1ScoreTextsAnchoredPos(float fillAmount)
        {
            Vector2 anchorPos = Player1ScoreText.rectTransform.anchoredPosition;
            float offset = fillAmount == 0 ? 0 : k_playerScoreTextAnchoredPosXOffset;
            anchorPos.x = fillAmount * k_playerScoreTextAnchoredPosXMaximum + offset;
            Player1ScoreText.rectTransform.anchoredPosition = anchorPos;
        }
        
        private void SetPlayer2ScoreTextsAnchoredPos(float fillAmount)
        {
            Vector2 anchorPos = Player2ScoreText.rectTransform.anchoredPosition;
            float offset = fillAmount == 0 ? 0 : k_playerScoreTextAnchoredPosXOffset;
            anchorPos.x = fillAmount * k_playerScoreTextAnchoredPosXMaximum + offset;
            Player2ScoreText.rectTransform.anchoredPosition = anchorPos;
        }

        public void UpdateP1AnswerText(string text)
        {
            StartCoroutine(P1AnswerTextPopupRoutine(text));
        }
        
        private IEnumerator P1AnswerTextPopupRoutine(string answer)
        {
            yield return StartCoroutine(AnswerTextPopupRoutine(P1AnswerText, answer));
        }

        public void UpdateP2AnswerText(string text)
        {
            StartCoroutine(P2AnswerTextPopupRoutine(text));
        }
        
        private IEnumerator P2AnswerTextPopupRoutine(string answer)
        {
            yield return StartCoroutine(AnswerTextPopupRoutine(P2AnswerText, answer));
        }

        private IEnumerator AnswerTextPopupRoutine(TMP_Text answerText, string answer)
        {
            answerText.text = "";
            for (int i = 0; i < answer.Length; i++)
            {
                answerText.text = answer.Substring(0, i + 1);
                yield return new WaitForSeconds(AnswerTextPopupIntervalInSeconds);
            }
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

        private Coroutine m_updateP1FillImageCoroutine;
        private Coroutine m_updateP2FillImageCoroutine;
        private const float k_fillImageDelayInSeconds = 1.8f;
        private const float k_fillImageLerpTimeInSeconds = 1.2f;

        public void UpdatePlayer1FillImage(float fill, int currentHp)
        {
            fill = Mathf.Clamp01(fill);
            Player1FillImage.fillAmount = fill;
            Player1ScoreText.text = currentHp.ToString();
            SetPlayer1ScoreTextsAnchoredPos(fill);

            if (gameObject.activeSelf)
            {
                if (m_updateP1FillImageCoroutine != null) StopCoroutine(m_updateP1FillImageCoroutine);
                m_updateP1FillImageCoroutine = StartCoroutine(UpdateFillImageCoroutine(Player1BGFillImage, fill));
            }
        }

        private IEnumerator UpdateFillImageCoroutine(Image fillImage, float value)
        {
            float startFillAmount = fillImage.fillAmount;
            float timer = 0;
            yield return new WaitForSeconds(k_fillImageDelayInSeconds);
            while (timer < k_fillImageLerpTimeInSeconds)
            {
                timer += Time.deltaTime;
                fillImage.fillAmount = Mathf.Lerp(startFillAmount, value, timer / k_fillImageLerpTimeInSeconds);
                yield return null;
            }
        }

        public void UpdatePlayer2FillImage(float value, int currentScore)
        {
            value = Mathf.Clamp01(value);
            Player2FillImage.fillAmount = value;
            Player2ScoreText.text = currentScore.ToString();
            SetPlayer2ScoreTextsAnchoredPos(value);

            if (gameObject.activeSelf)
            {
                if (m_updateP2FillImageCoroutine != null) StopCoroutine(m_updateP2FillImageCoroutine);
                m_updateP2FillImageCoroutine = StartCoroutine(UpdateFillImageCoroutine(Player2BGFillImage, value));
            }
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