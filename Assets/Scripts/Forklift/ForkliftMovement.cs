using UnityEngine;
using Zenject;

[RequireComponent(typeof(Rigidbody))]
public class ForkliftMovement : MonoBehaviour
{
    [Header("Config")]
    private ForkliftConfig config;
    [SerializeField] private ForkliftEngineState engineState;
    [SerializeField] private ForkliftFuelSystem fuelSystem;

    [Header("References")]
    [SerializeField] private Transform steeringWheel;
    private Rigidbody rb;

    [Header("Rear Steering Wheels")]
    [SerializeField] private Transform rearLeftWheelVisual;
    [SerializeField] private Transform rearRightWheelVisual;

    [Header("Front Drive Wheels")]
    [SerializeField] private Transform frontWheelsVisual;

    [Header("Ground")]
    [SerializeField] private LayerMask groundMask;

    private ForkliftInput input;
    private Vector2 moveInput;

    private bool isGrounded;

    private float currentSteerAngle;
    private float visualSteerAngle;
    private float visualSteerVelocity;
    private float wheelRollAngle;

    private Quaternion steeringWheelBaseRot;
    private Quaternion rearLeftWheelBaseRot;
    private Quaternion rearRightWheelBaseRot;
    private Quaternion frontWheelsBaseRot;

    [Inject]
    public void Construct(ForkliftConfig config, ForkliftInput input)
    {
        if (this.config == null)
            this.config = config;

        this.input = input;
    }

    private void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        CacheVisualBaseRotations();
        SetupRigidbody();
    }

    private void Update()
    {
        if (config == null || input == null)
            return;

        moveInput = input.ReadMove();
        UpdateVisualSteering();
        UpdateWheelRollVisual(Time.deltaTime);
    }

    private void FixedUpdate()
    {
        if (config == null || rb == null)
            return;

        CheckGround();
        HandleSteering(Time.fixedDeltaTime);

        if (engineState == null || !engineState.EngineStarted.CurrentValue || !isGrounded)
            return;

        HandleMovement(Time.fixedDeltaTime);

        if (config.ReduceSideSlip)
            ReduceSidewaysSlip(Time.fixedDeltaTime);

        HandleRotation(Time.fixedDeltaTime);
    }

    private void LateUpdate()
    {
        if (config == null)
            return;

        UpdateSteeringVisuals();
        UpdateWheelVisuals();
    }

    private void OnValidate()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    private void SetupRigidbody()
    {
        if (rb == null || config == null)
            return;

        rb.centerOfMass = config.CenterOfMass;
        rb.angularDamping = config.AngularDrag;
    }

    private void CacheVisualBaseRotations()
    {
        if (steeringWheel != null)
            steeringWheelBaseRot = steeringWheel.localRotation;

        if (rearLeftWheelVisual != null)
            rearLeftWheelBaseRot = rearLeftWheelVisual.localRotation;

        if (rearRightWheelVisual != null)
            rearRightWheelBaseRot = rearRightWheelVisual.localRotation;

        if (frontWheelsVisual != null)
            frontWheelsBaseRot = frontWheelsVisual.localRotation;
    }

    private void UpdateVisualSteering()
    {
        visualSteerAngle = Mathf.SmoothDampAngle(
            visualSteerAngle,
            currentSteerAngle,
            ref visualSteerVelocity,
            config.VisualSteerSmoothTime
        );
    }

    private void HandleSteering(float dt)
    {
        float steerInputRaw = moveInput.x;
        float steerInput = Mathf.Abs(steerInputRaw) < config.InputDeadZone ? 0f : Mathf.Sign(steerInputRaw);
        float targetAngle = steerInput * config.MaxSteerAngle;

        if (Mathf.Abs(steerInput) > 0f)
        {
            currentSteerAngle = Mathf.MoveTowards(
                currentSteerAngle,
                targetAngle,
                config.SteerResponse * dt
            );
        }
        else
        {
            currentSteerAngle = Mathf.MoveTowards(
                currentSteerAngle,
                0f,
                config.SteerReturnSpeed * dt
            );
        }
    }

    private void HandleMovement(float dt)
    {
        float throttleInput = ApplyDeadZone(moveInput.y, config.InputDeadZone);

        Vector3 forward = transform.forward;
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
        float currentForwardSpeed = Vector3.Dot(horizontalVelocity, forward);

        if (Mathf.Abs(throttleInput) <= 0f)
        {
            ApplyCoastingDrag(dt);
            return;
        }

        float speedMultiplier = fuelSystem != null ? fuelSystem.CurrentSpeedMultiplier : 1f;

        float targetMaxSpeed = throttleInput > 0f
            ? config.MaxForwardSpeed * speedMultiplier
            : config.MaxReverseSpeed * speedMultiplier;

        float targetSpeed = throttleInput * targetMaxSpeed;

        bool changingDirection =
            Mathf.Abs(currentForwardSpeed) > 0.05f &&
            Mathf.Sign(throttleInput) != Mathf.Sign(currentForwardSpeed);

        float forcePower = changingDirection
            ? config.ActiveBrakeStrength
            : config.Acceleration;

        float speedDelta = targetSpeed - currentForwardSpeed;
        Vector3 force = forward * (speedDelta * forcePower);

        rb.AddForce(force, ForceMode.Acceleration);
    }

    private void HandleRotation(float dt)
    {
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
        float forwardSpeed = Vector3.Dot(horizontalVelocity, transform.forward);
        float absForwardSpeed = Mathf.Abs(forwardSpeed);

        if (absForwardSpeed < config.MinTurnSpeed)
            return;

        float steerNormalized = config.MaxSteerAngle > 0f
            ? currentSteerAngle / config.MaxSteerAngle
            : 0f;

        float speedFactor = config.MaxForwardSpeed > 0f
            ? Mathf.Clamp01(absForwardSpeed / config.MaxForwardSpeed)
            : 0f;

        float direction = Mathf.Sign(forwardSpeed);
        float yawAmount = steerNormalized * config.TurnSpeed * direction * speedFactor * dt;

        Quaternion deltaRotation = Quaternion.Euler(0f, yawAmount, 0f);
        rb.MoveRotation(rb.rotation * deltaRotation);
    }

    private void ReduceSidewaysSlip(float dt)
    {
        Vector3 velocity = rb.linearVelocity;

        Vector3 forwardVelocity = transform.forward * Vector3.Dot(velocity, transform.forward);
        Vector3 lateralVelocity = transform.right * Vector3.Dot(velocity, transform.right);

        lateralVelocity = Vector3.Lerp(
            lateralVelocity,
            Vector3.zero,
            config.LateralGrip * dt
        );

        Vector3 finalVelocity = forwardVelocity + lateralVelocity;
        rb.linearVelocity = new Vector3(finalVelocity.x, velocity.y, finalVelocity.z);
    }

    private void ApplyCoastingDrag(float dt)
    {
        Vector3 velocity = rb.linearVelocity;
        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(velocity, Vector3.up);

        Vector3 dampedHorizontalVelocity = Vector3.Lerp(
            horizontalVelocity,
            Vector3.zero,
            config.CoastingDrag * dt
        );

        rb.linearVelocity = new Vector3(
            dampedHorizontalVelocity.x,
            velocity.y,
            dampedHorizontalVelocity.z
        );
    }

    private void UpdateWheelRollVisual(float dt)
    {
        float wheelRadius = Mathf.Max(0.001f, config.WheelRadius);

        Vector3 horizontalVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
        float forwardSpeed = Vector3.Dot(horizontalVelocity, transform.forward);

        float angularSpeedDeg = (forwardSpeed / wheelRadius) * Mathf.Rad2Deg;
        wheelRollAngle += angularSpeedDeg * dt;

        if (wheelRollAngle > 360f || wheelRollAngle < -360f)
            wheelRollAngle %= 360f;
    }

    private void UpdateSteeringVisuals()
    {
        float normalized = config.MaxSteerAngle > 0f
            ? visualSteerAngle / config.MaxSteerAngle
            : 0f;

        if (steeringWheel != null)
        {
            float wheelY = normalized * config.SteeringWheelVisualAngle;
            steeringWheel.localRotation = steeringWheelBaseRot * Quaternion.Euler(0f, wheelY, 0f);
        }
    }

    private void UpdateWheelVisuals()
    {
        if (frontWheelsVisual != null)
        {
            frontWheelsVisual.localRotation =
                frontWheelsBaseRot * Quaternion.Euler(wheelRollAngle, 0f, 0f);
        }

        if (rearLeftWheelVisual != null)
        {
            rearLeftWheelVisual.localRotation =
                rearLeftWheelBaseRot * Quaternion.Euler(wheelRollAngle, -visualSteerAngle, 0f);
        }

        if (rearRightWheelVisual != null)
        {
            rearRightWheelVisual.localRotation =
                rearRightWheelBaseRot * Quaternion.Euler(wheelRollAngle, -visualSteerAngle, 0f);
        }
    }

    private void CheckGround()
    {
        Vector3 origin = transform.position + config.GroundCheckOffset;

        isGrounded = Physics.Raycast(
            origin,
            Vector3.down,
            config.GroundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private float ApplyDeadZone(float value, float deadZone)
    {
        return Mathf.Abs(value) < deadZone ? 0f : value;
    }
}