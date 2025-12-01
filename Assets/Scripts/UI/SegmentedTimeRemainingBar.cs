using UnityEngine;
using UnityEngine.UI;

public class SegmentedTimeRemainingBar : MonoBehaviour
{
    public HorizontalLayoutGroup timeRemainingHorizontalLayoutGroup;
    public GameObject segmentPrefab;
    public GameObject segmentBarTimeMultiplierIconPrefab;

    private float[] segmentOrigin;
    private Image[] segmentTimeBarImages;
    private int _currentSegmentIndex;
    public int CurrentSegmentIndex => _currentSegmentIndex;

    public void InitializeSegmentedTimeRemainingBar(Client.SegmentData[] segmentData, float timeLimit,
        bool reverseArrangement = false)
    {
        // 清空旧 segment
        for (int i = timeRemainingHorizontalLayoutGroup.transform.childCount - 1; i >= 0; i--)
            Destroy(timeRemainingHorizontalLayoutGroup.transform.GetChild(i).gameObject);

        segmentOrigin = new float[segmentData.Length + 1];
        segmentTimeBarImages = new Image[segmentData.Length];

        float step = timeLimit / segmentData.Length;

        // 生成 segment
        for (int i = 0; i < segmentData.Length; i++)
        {
            var img = Instantiate(segmentPrefab, timeRemainingHorizontalLayoutGroup.transform).GetComponent<Image>();
            img.fillOrigin = reverseArrangement ? 1 : 0;
            img.color = segmentData[i].segmentColor;
            var timeMultiplierIconImg =
                Instantiate(segmentBarTimeMultiplierIconPrefab, img.transform).GetComponent<Image>();
            timeMultiplierIconImg.sprite = segmentData[i].timeScaleMultiplierSprite;
            if (!reverseArrangement)
            {
                timeMultiplierIconImg.transform.localScale = new Vector3(-timeMultiplierIconImg.transform.localScale.x,
                    timeMultiplierIconImg.transform.localScale.y, 1);
            }

            segmentTimeBarImages[i] = img;
            segmentOrigin[i] = step * i;
        }

        segmentOrigin[segmentData.Length] = timeLimit;

        timeRemainingHorizontalLayoutGroup.reverseArrangement = reverseArrangement;
    }

    public void UpdateTimeRemainingBar(float timeRemaining)
    {
        if (segmentOrigin == null || segmentOrigin.Length == 0) return;

        // 找当前所在 segment
        int index = -1;
        for (int i = 0; i < segmentTimeBarImages.Length; i++)
        {
            if (timeRemaining >= segmentOrigin[i] && timeRemaining < segmentOrigin[i + 1])
            {
                index = i;
                break;
            }
        }
        
        _currentSegmentIndex = index;

        // 更新 segment 显示
        for (int i = 0; i < segmentTimeBarImages.Length; i++)
        {
            Image img = segmentTimeBarImages[i];

            if (i < index)
            {
                img.fillAmount = 1;
            }
            else if (i == index)
            {
                img.fillAmount =
                    (timeRemaining - segmentOrigin[i]) /
                    (segmentOrigin[i + 1] - segmentOrigin[i]);
            }
            else
            {
                img.fillAmount = 0;
            }
        }
    }
}