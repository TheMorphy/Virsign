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
        if (input == null)
            return "T";

        var action = input.Player.EngineToggle;
        if (action == null || action.bindings.Count == 0)
            return "T";

        int bindingIndex = action.GetBindingIndexForControl(action.controls.Count > 0 ? action.controls[0] : null);

        if (bindingIndex < 0)
            bindingIndex = 0;

        string effectivePath = action.bindings[bindingIndex].effectivePath;

        if (string.IsNullOrWhiteSpace(effectivePath))
            return "T";

        int slashIndex = effectivePath.LastIndexOf('/');
        if (slashIndex < 0 || slashIndex >= effectivePath.Length - 1)
            return "T";

        string key = effectivePath[(slashIndex + 1)..];

        return key switch
        {
            "space" => "SPACE",
            "leftShift" => "LEFT SHIFT",
            "rightShift" => "RIGHT SHIFT",
            "leftCtrl" => "LEFT CTRL",
            "rightCtrl" => "RIGHT CTRL",
            "leftAlt" => "LEFT ALT",
            "rightAlt" => "RIGHT ALT",
            _ => key.ToUpperInvariant()
        };
    }
}