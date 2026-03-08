using UnityEngine;
using UnityEngine.InputSystem;
using R3;

public class ForkliftEngineState : MonoBehaviour
{
    private readonly ReactiveProperty<bool> engineStarted = new(false);
    public ReadOnlyReactiveProperty<bool> EngineStarted => engineStarted;

    private PlayerInput input;

    private void Awake()
    {
        input = new PlayerInput();
    }

    private void OnEnable()
    {
        input.Player.Enable();
        input.Player.EngineToggle.performed += OnEngineToggle;
    }

    private void OnDisable()
    {
        input.Player.EngineToggle.performed -= OnEngineToggle;
        input.Player.Disable();
    }

    private void OnEngineToggle(InputAction.CallbackContext context)
    {
        engineStarted.Value = !engineStarted.Value;
    }
}