using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using R3;

public class ForkliftLowFuelIndicator : MonoBehaviour
{
    [SerializeField] private ForkliftFuelSystem fuelSystem;
    [SerializeField] private Image indicatorImage;

    [Header("Colors")]
    [SerializeField] private Color normalColor = Color.black;
    [SerializeField] private Color lowFuelColor = Color.red;

    [Header("Blink")]
    [SerializeField] private float blinkDuration = 0.35f;
    [SerializeField] private float minAlpha = 0.2f;

    private Tween blinkTween;

    private void Start()
    {
        indicatorImage.color = normalColor;

        fuelSystem.IsLowFuel
            .Subscribe(OnLowFuelChanged)
            .AddTo(this);
    }

    private void OnLowFuelChanged(bool isLowFuel)
    {
        if (isLowFuel)
        {
            indicatorImage.color = lowFuelColor;

            blinkTween?.Kill();

            blinkTween = indicatorImage
                .DOFade(minAlpha, blinkDuration)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutSine);
        }
        else
        {
            blinkTween?.Kill();
            blinkTween = null;

            indicatorImage.color = normalColor;

            Color c = indicatorImage.color;
            c.a = 1f;
            indicatorImage.color = c;
        }
    }

    private void OnDestroy()
    {
        blinkTween?.Kill();
    }
}