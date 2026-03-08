using UnityEngine;

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

    private void Awake()
    {
        if (needleTransform == null)
            needleTransform = transform;

        baseLocalRotation = needleTransform.localRotation;
        currentAngle = GetTargetAngle();
        ApplyRotation(currentAngle, true);
    }

    private void Update()
    {
        if (fuelSystem == null || needleTransform == null)
            return;

        float targetAngle = GetTargetAngle();
        currentAngle = Mathf.Lerp(currentAngle, targetAngle, smoothSpeed * Time.deltaTime);
        ApplyRotation(currentAngle, false);
    }

    private float GetTargetAngle()
    {
        float normalizedFuel = Mathf.Clamp01(fuelSystem.FuelNormalized);
        return Mathf.Lerp(emptyAngle, fullAngle, normalizedFuel);
    }

    private void ApplyRotation(float angle, bool instant)
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