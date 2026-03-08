using TMPro;
using UnityEngine;
using R3;
using UnityEngine.InputSystem;

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
    [SerializeField] private string hintTemplate = "Нажмите <br><size=300%>{0}</size><br>Чтобы завестить";

    private PlayerInput input;

    private void Awake()
    {
        input = new PlayerInput();
        UpdateHintText();
    }

    private void Start()
    {
        if (engineState == null)
            return;

        engineState.EngineStarted
            .Subscribe(OnEngineStateChanged)
            .AddTo(this);

        OnEngineStateChanged(engineState.EngineStarted.CurrentValue);
    }

    private void OnDestroy()
    {
        input?.Dispose();
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

        string bindText = GetEngineToggleBindingDisplayString();
        startHintText.text = string.Format(hintTemplate, bindText);
    }

    private string GetEngineToggleBindingDisplayString()
    {
        var action = input?.Player.EngineToggle;

        if (action == null)
            return "T";

        string bindingText = action.GetBindingDisplayString();

        if (string.IsNullOrWhiteSpace(bindingText))
            bindingText = "T";

        return bindingText;
    }
}