using UnityEngine;
using R3;

public class ForkliftFuelSystem : MonoBehaviour
{
    [SerializeField] private ForkliftConfig config;
    [SerializeField] private ForkliftEngineState engineState;
    [SerializeField] private Rigidbody rb;

    private readonly ReactiveProperty<float> currentFuel = new(0f);
    private readonly ReactiveProperty<bool> isLowFuel = new(false);
    private readonly ReactiveProperty<bool> isEmpty = new(false);

    public ReadOnlyReactiveProperty<float> CurrentFuel => currentFuel;
    public ReadOnlyReactiveProperty<bool> IsLowFuel => isLowFuel;
    public ReadOnlyReactiveProperty<bool> IsEmpty => isEmpty;

    public float FuelNormalized =>
        config != null && config.FuelCapacity > 0f
            ? currentFuel.Value / config.FuelCapacity
            : 0f;

    public float CurrentSpeedMultiplier
    {
        get
        {
            if (!config)
                return 1f;

            return FuelNormalized <= config.LowFuelThresholdNormalized ? 0.5f : 1f;
        }
    }

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
        engineState = GetComponent<ForkliftEngineState>();
    }

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        currentFuel.Value = config != null ? config.FuelCapacity : 0f;
        RefreshFlags();
    }

    private void Update()
    {
        if (!config || !engineState || !rb)
            return;

        if (!engineState.EngineStarted.CurrentValue)
            return;

        float throttleFactor = EstimateThrottleFactor();
        float burnRate = Mathf.Lerp(
            config.FuelBurnPerSecondAtIdle,
            config.FuelBurnPerSecondAtFullThrottle,
            throttleFactor
        );

        currentFuel.Value = Mathf.Max(0f, currentFuel.Value - burnRate * Time.deltaTime);
        RefreshFlags();
    }

    private float EstimateThrottleFactor()
    {
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
        float speed = horizontalVelocity.magnitude;

        float maxSpeed = Mathf.Max(config.MaxForwardSpeed, config.MaxReverseSpeed, 0.01f);
        return Mathf.Clamp01(speed / maxSpeed);
    }

    private void RefreshFlags()
    {
        float normalized = FuelNormalized;

        isLowFuel.Value = normalized <= config.LowFuelThresholdNormalized;
        isEmpty.Value = currentFuel.Value <= 0f;
    }

    public void RefillFull()
    {
        currentFuel.Value = config.FuelCapacity;
        RefreshFlags();
    }

    public void AddFuel(float amount)
    {
        currentFuel.Value = Mathf.Clamp(currentFuel.Value + amount, 0f, config.FuelCapacity);
        RefreshFlags();
    }
}