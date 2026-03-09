using UnityEngine;
using R3;
using Zenject;

public class ForkliftFuelSystem : MonoBehaviour
{
    private ForkliftConfig config;
    [SerializeField] private ForkliftEngineState engineState;
    [SerializeField] private Rigidbody rb;

    private readonly ReactiveProperty<float> currentFuel = new(0f);
    private readonly ReactiveProperty<bool> isLowFuel = new(false);
    private readonly ReactiveProperty<bool> isEmpty = new(false);

    public ReadOnlyReactiveProperty<float> CurrentFuel => currentFuel;
    public ReadOnlyReactiveProperty<bool> IsLowFuel => isLowFuel;

    public float FuelNormalized =>
        config != null && config.FuelCapacity > 0f
            ? currentFuel.Value / config.FuelCapacity
            : 0f;

    public float CurrentSpeedMultiplier
    {
        get
        {
            if (config == null)
                return 1f;

            return FuelNormalized <= config.LowFuelThresholdNormalized
                ? config.EmptyFuelSpeedMultiplier
                : 1f;
        }
    }

    [Inject]
    public void Construct(ForkliftConfig config)
    {
        if (this.config == null)
            this.config = config;
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

        SetFuel(config != null ? config.FuelCapacity : 0f);
    }

    private void Update()
    {
        if (config == null || engineState == null || rb == null)
            return;

        if (!engineState.EngineStarted.CurrentValue || isEmpty.Value)
            return;

        float throttleFactor = EstimateThrottleFactor();
        float burnRate = Mathf.Lerp(
            config.FuelBurnPerSecondAtIdle,
            config.FuelBurnPerSecondAtFullThrottle,
            throttleFactor
        );

        SetFuel(currentFuel.Value - burnRate * Time.deltaTime);
    }

    private float EstimateThrottleFactor()
    {
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
        float speed = horizontalVelocity.magnitude;

        float maxSpeed = Mathf.Max(config.MaxForwardSpeed, config.MaxReverseSpeed, 0.01f);
        return Mathf.Clamp01(speed / maxSpeed);
    }

    private void SetFuel(float value)
    {
        float capacity = config != null ? config.FuelCapacity : 0f;
        currentFuel.Value = Mathf.Clamp(value, 0f, capacity);
        RefreshFlags();
    }

    private void RefreshFlags()
    {
        if (config == null || config.FuelCapacity <= 0f)
        {
            isLowFuel.Value = false;
            isEmpty.Value = true;
            return;
        }

        float normalized = FuelNormalized;
        isLowFuel.Value = normalized <= config.LowFuelThresholdNormalized;
        isEmpty.Value = currentFuel.Value <= 0f;
    }

    public void RefillFull()
    {
        SetFuel(config != null ? config.FuelCapacity : 0f);
    }

    public void AddFuel(float amount)
    {
        SetFuel(currentFuel.Value + amount);
    }
}