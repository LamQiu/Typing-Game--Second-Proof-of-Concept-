using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(CanvasScaler))]
    public class AdaptiveUIScaler : MonoBehaviour
    {
        private CanvasScaler scaler;

        [Header("Reference Aspect (设计分辨率比例)")]
        public float referenceAspect = 16f / 9f;

        [Header("Match 范围")]
        [Range(0, 1)]
        public float wideScreenMatch = 1f;   // 超宽屏时偏向Height
        [Range(0, 1)]
        public float narrowScreenMatch = 0f; // 偏高屏时偏向Width

        void Awake()
        {
            scaler = GetComponent<CanvasScaler>();
            UpdateScaler();
        }

#if UNITY_EDITOR
        void Update()
        {
            // 编辑器实时更新
            UpdateScaler();
        }
#endif

        void UpdateScaler()
        {
            float currentAspect = (float)Screen.width / Screen.height;

            if (currentAspect > referenceAspect)
            {
                // 比16:9更宽（21:9等）
                scaler.matchWidthOrHeight = wideScreenMatch;
            }
            else if (currentAspect < referenceAspect)
            {
                // 比16:9更高（16:10 / 4:3）
                scaler.matchWidthOrHeight = narrowScreenMatch;
            }
            else
            {
                scaler.matchWidthOrHeight = 0.5f;
            }
        }
    }
}