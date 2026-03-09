using TMPro;
using UnityEngine;
using R3;
using Zenject;

public class ForkliftTabletDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ForkliftEngineState engineState;
    [SerializeField] private Renderer screenRenderer;
    [SerializeField] private Camera rearViewCamera;

    [Header("Materials")]
    [SerializeField] private Material engineOffMaterial;
    [SerializeField] private Material engineOnMaterial;

    [Header("UI")]
    [SerializeField] private GameObject startHintRoot;
    [SerializeField] private TMP_Text startHintText;
    [SerializeField] private string hintTemplate = "Нажмите <br><size=300%>{0}</size><br>Чтобы завестись";

    private ForkliftInput input;

    [Inject]
    public void Construct(ForkliftInput input)
    {
        this.input = input;
    }

    private void Awake()
    {
        UpdateHintText();
    }

    private void Start()
    {
        if (engineState == null)
        {
            enabled = false;
            return;
        }

        engineState.EngineStarted
            .DistinctUntilChanged()
            .Subscribe(OnEngineStateChanged)
            .AddTo(this);

        OnEngineStateChanged(engineState.EngineStarted.CurrentValue);
    }

    private void OnEngineStateChanged(bool isStarted)
    {
        if (screenRenderer != null)
        {
            screenRenderer.sharedMaterial = isStarted
                ? engineOnMaterial
                : engineOffMaterial;
        }

        if (startHintRoot != null)
            startHintRoot.SetActive(!isStarted);

        if (rearViewCamera != null)
            rearViewCamera.enabled = isStarted;
    }

    private void UpdateHintText()
    {
        if (startHintText == null)
            return;

        string bindText = input != null
            ? input.GetEngineToggleDisplayStringForcedLatin()
            : "T";

        startHintText.text = string.Format(hintTemplate, bindText);
    }
}