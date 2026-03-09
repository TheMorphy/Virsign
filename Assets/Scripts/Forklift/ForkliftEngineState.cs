using UnityEngine;
using UnityEngine.InputSystem;
using R3;
using Zenject;

public class ForkliftEngineState : MonoBehaviour
{
    private readonly ReactiveProperty<bool> engineStarted = new(false);
    public ReadOnlyReactiveProperty<bool> EngineStarted => engineStarted;

    private ForkliftInput input;

    [Inject]
    public void Construct(ForkliftInput input)
    {
        this.input = input;
    }

    private void OnEnable()
    {
        input?.SubscribeEngineToggle(OnEngineToggle);
    }

    private void OnDisable()
    {
        input?.UnsubscribeEngineToggle(OnEngineToggle);
    }

    private void OnEngineToggle(InputAction.CallbackContext _)
    {
        ToggleEngine();
    }

    public void ToggleEngine()
    {
        engineStarted.Value = !engineStarted.Value;
    }
}