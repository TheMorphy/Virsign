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
    [SerializeField] private float minAlpha = 0.7f;

    private Tween blinkTween;
    private bool isBlinking;

    private void Start()
    {
        if (fuelSystem == null || indicatorImage == null)
        {
            enabled = false;
            return;
        }

        SetNormalState();

        fuelSystem.IsLowFuel
            .DistinctUntilChanged()
            .Subscribe(OnLowFuelChanged)
            .AddTo(this);
    }

    private void OnLowFuelChanged(bool isLowFuel)
    {
        if (isLowFuel)
            SetLowFuelState();
        else
            SetNormalState();
    }

    private void SetLowFuelState()
    {
        if (indicatorImage == null || isBlinking)
            return;

        isBlinking = true;
        blinkTween?.Kill();

        indicatorImage.color = new Color(lowFuelColor.r, lowFuelColor.g, lowFuelColor.b, 1f);

        blinkTween = indicatorImage
            .DOFade(minAlpha, blinkDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void SetNormalState()
    {
        if (indicatorImage == null)
            return;

        isBlinking = false;
        blinkTween?.Kill();
        blinkTween = null;

        indicatorImage.color = new Color(normalColor.r, normalColor.g, normalColor.b, 1f);
    }

    private void OnDestroy()
    {
        blinkTween?.Kill();
    }
}