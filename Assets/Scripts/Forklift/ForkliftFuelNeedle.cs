using UnityEngine;
using R3;

public class ForkliftFuelNeedle : MonoBehaviour
{
    public enum RotationAxis
    {
        X,
        Y,
        Z
    }

    [Header("References")]
    [SerializeField] private ForkliftFuelSystem fuelSystem;
    [SerializeField] private Transform needleTransform;

    [Header("Rotation")]
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Z;
    [SerializeField] private float emptyAngle = -90f;
    [SerializeField] private float fullAngle = 90f;

    [Header("Smoothing")]
    [SerializeField] private float smoothSpeed = 6f;

    private Quaternion baseLocalRotation;
    private float currentAngle;
    private float targetAngle;

    private void Awake()
    {
        if (needleTransform == null)
            needleTransform = transform;

        baseLocalRotation = needleTransform.localRotation;
    }

    private void Start()
    {
        if (fuelSystem == null || needleTransform == null)
        {
            enabled = false;
            return;
        }

        targetAngle = GetTargetAngle();
        currentAngle = targetAngle;
        ApplyRotation(currentAngle);

        fuelSystem.CurrentFuel
            .Subscribe(_ => targetAngle = GetTargetAngle())
            .AddTo(this);
    }

    private void Update()
    {
        if (needleTransform == null)
            return;

        currentAngle = Mathf.Lerp(currentAngle, targetAngle, smoothSpeed * Time.deltaTime);
        ApplyRotation(currentAngle);
    }

    private float GetTargetAngle()
    {
        float normalizedFuel = Mathf.Clamp01(fuelSystem.FuelNormalized);
        return Mathf.Lerp(emptyAngle, fullAngle, normalizedFuel);
    }

    private void ApplyRotation(float angle)
    {
        Quaternion offset = rotationAxis switch
        {
            RotationAxis.X => Quaternion.Euler(angle, 0f, 0f),
            RotationAxis.Y => Quaternion.Euler(0f, angle, 0f),
            _ => Quaternion.Euler(0f, 0f, angle)
        };

        needleTransform.localRotation = baseLocalRotation * offset;
    }
}