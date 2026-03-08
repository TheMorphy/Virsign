using UnityEngine;

[CreateAssetMenu(fileName = "ForkliftConfig", menuName = "Game/Forklift Config")]
public class ForkliftConfig : ScriptableObject
{
    [Header("Drive")]
    [Min(0f)] public float MaxForwardSpeed = 6f;
    [Min(0f)] public float MaxReverseSpeed = 3f;
    [Min(0f)] public float Acceleration = 12f;
    [Min(0f)] public float Braking = 18f;
    [Min(0f)] public float IdleDrag = 3f;
    
    [Header("Fuel")]
    [Min(0f)] public float FuelCapacity = 100f;
    [Min(0f)] public float FuelBurnPerSecondAtIdle = 0.01f;
    [Min(0f)] public float FuelBurnPerSecondAtFullThrottle = 0.2f;
    [Range(0f, 1f)] public float LowFuelThresholdNormalized = 0.5f;
    [Min(0f)] public float EmptyFuelSpeedMultiplier = 0.5f;

    [Header("Coasting / Inertia")]
    [Min(0f)] public float CoastingDrag = 0.8f;
    [Min(0f)] public float ActiveBrakeStrength = 18f;
    [Min(0f)] public float ReverseBrakeStrength = 24f;
    
    [Header("Steering")]
    [Min(0f)] public float MaxSteerAngle = 35f;
    [Min(0f)] public float SteerResponse = 120f;
    [Min(0f)] public float SteerReturnSpeed = 160f;
    [Min(0f)] public float TurnSpeed = 90f;

    [Header("Forks Physics")]
    [Min(-2f)] public float ForkMinHeight = -2f;
    [Min(0f)] public float ForkMaxHeight = 0.3f;
    [Min(0f)] public float ForkMoveSpeed = 1.2f;

    [Header("Fork Joint Drive")]
    [Min(0f)] public float ForkPositionSpring = 8000f;
    [Min(0f)] public float ForkPositionDamper = 1200f;
    [Min(0f)] public float ForkMaxDriveForce = 20000f;

    [Header("Fork Input")]
    [Min(0f)] public float ForkInputDeadZone = 0.01f;
    
    [Header("Visual Steering")]
    [Min(0f)] public float SteeringWheelVisualAngle = 270f;
    [Min(0f)] public float VisualSteerSmoothTime = 0.05f;
    [Min(0.001f)] public float WheelRadius = 0.25f;

    [Header("Ground Check")]
    public float GroundCheckDistance = 0.3f;
    public Vector3 GroundCheckOffset = new Vector3(0f, 0.1f, 0f);

    [Header("Physics")]
    public Vector3 CenterOfMass = new Vector3(0f, 0f, -0.4f);
    [Min(0f)] public float AngularDrag = 4f;

    [Header("Anti Slip")]
    [Min(0f)] public float LateralGrip = 8f;
    public bool ReduceSideSlip = true;

    [Header("Thresholds")]
    [Min(0f)] public float MinTurnSpeed = 0.15f;
    [Min(0f)] public float InputDeadZone = 0.01f;
}