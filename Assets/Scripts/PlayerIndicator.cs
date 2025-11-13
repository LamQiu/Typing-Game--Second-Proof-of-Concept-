using System;
using DG.Tweening;
using UnityEngine;

public class PlayerIndicator : MonoBehaviour
{
    public Vector2 yOffsetMinMax = new Vector2(0.5f, 1.5f);
    public float animationDurationInSeconds = 1f;
    public Ease ease = Ease.Linear;

    private void Start()
    {
        transform.SetLocalY(yOffsetMinMax.x);
        transform.DOLocalMoveY(yOffsetMinMax.y, animationDurationInSeconds).SetEase(ease).SetLoops(-1, LoopType.Yoyo);
    }
}