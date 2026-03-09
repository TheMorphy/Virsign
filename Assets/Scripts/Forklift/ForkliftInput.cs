using System;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ForkliftInput : IDisposable
{
    private readonly PlayerInput playerInput;
    private bool isDisposed;

    public ForkliftInput()
    {
        playerInput = new PlayerInput();
        playerInput.Player.Enable();
    }

    public Vector2 ReadMove()
    {
        return isDisposed ? Vector2.zero : playerInput.Player.Move.ReadValue<Vector2>();
    }

    public float ReadLiftUp()
    {
        return isDisposed ? 0f : playerInput.Player.LiftUp.ReadValue<float>();
    }

    public float ReadLiftDown()
    {
        return isDisposed ? 0f : playerInput.Player.LiftDown.ReadValue<float>();
    }

    public string GetEngineToggleDisplayStringForcedLatin()
    {
        if (isDisposed)
            return "T";

        InputAction action = playerInput.Player.EngineToggle;
        if (action == null || action.bindings.Count == 0)
            return "T";

        for (int i = 0; i < action.bindings.Count; i++)
        {
            var binding = action.bindings[i];

            if (binding.isComposite || binding.isPartOfComposite)
                continue;

            string path = binding.effectivePath;
            if (string.IsNullOrWhiteSpace(path))
                continue;

            if (path.StartsWith("<Keyboard>/", StringComparison.OrdinalIgnoreCase))
            {
                string key = path.Substring("<Keyboard>/".Length);

                if (key.Length == 1)
                    return key.ToUpperInvariant();

                return key switch
                {
                    "space" => "SPACE",
                    "enter" => "ENTER",
                    "tab" => "TAB",
                    "escape" => "ESC",
                    _ => key.ToUpperInvariant()
                };
            }
        }

        return "T";
    }

    public void SubscribeEngineToggle(Action<InputAction.CallbackContext> callback)
    {
        if (isDisposed)
            return;

        playerInput.Player.EngineToggle.performed += callback;
    }

    public void UnsubscribeEngineToggle(Action<InputAction.CallbackContext> callback)
    {
        if (isDisposed)
            return;

        playerInput.Player.EngineToggle.performed -= callback;
    }

    public void Dispose()
    {
        if (isDisposed)
            return;

        playerInput.Player.Disable();
        playerInput.Dispose();
        isDisposed = true;
    }
}