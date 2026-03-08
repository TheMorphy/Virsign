using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ConfigurableJoint))]
public class ForkliftForkController : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private ForkliftConfig config;

    [Header("References")] 
    [SerializeField] private ForkliftEngineState engineState;
    [SerializeField] private Rigidbody carriageRb;
    [SerializeField] private ConfigurableJoint carriageJoint;

    private PlayerInput input;

    private float liftInput;
    private float targetHeight;

    public float TargetHeight => targetHeight;

    private void Reset()
    {
        carriageRb = GetComponent<Rigidbody>();
        carriageJoint = GetComponent<ConfigurableJoint>();
    }

    private void Awake()
    {
        input = new PlayerInput();

        if (carriageRb == null)
            carriageRb = GetComponent<Rigidbody>();

        if (carriageJoint == null)
            carriageJoint = GetComponent<ConfigurableJoint>();

        SetupRigidbody();
        SetupJoint();

        targetHeight = config != null ? config.ForkMaxHeight : 0f;
        ApplyTargetHeightImmediate();
    }

    private void OnEnable()
    {
        input.Player.Enable();
    }

    private void OnDisable()
    {
        input.Player.Disable();
    }

    private void Update()
    {
        if (config == null || carriageJoint == null)
            return;
        
        if (engineState != null && !engineState.EngineStarted.CurrentValue)
            return;

        float up = input.Player.LiftUp.ReadValue<float>();
        float down = input.Player.LiftDown.ReadValue<float>();

        liftInput = up - down;

        if (Mathf.Abs(liftInput) < config.ForkInputDeadZone)
            liftInput = 0f;
    }

    private void FixedUpdate()
    {
        if (config == null || carriageJoint == null)
            return;

        UpdateTargetHeight(Time.fixedDeltaTime);
        ApplyJointTarget();
    }

    private void SetupRigidbody()
    {
        if (carriageRb == null)
            return;

        carriageRb.useGravity = false;
        carriageRb.interpolation = RigidbodyInterpolation.None;
        carriageRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
    }

    private void SetupJoint()
    {
        if (carriageJoint == null || config == null)
            return;

        carriageJoint.autoConfigureConnectedAnchor = false;
        carriageJoint.configuredInWorldSpace = false;

        carriageJoint.xMotion = ConfigurableJointMotion.Locked;
        carriageJoint.yMotion = ConfigurableJointMotion.Free;
        carriageJoint.zMotion = ConfigurableJointMotion.Locked;

        carriageJoint.angularXMotion = ConfigurableJointMotion.Locked;
        carriageJoint.angularYMotion = ConfigurableJointMotion.Locked;
        carriageJoint.angularZMotion = ConfigurableJointMotion.Locked;

        SoftJointLimit linearLimit = carriageJoint.linearLimit;
        linearLimit.limit = config.ForkMinHeight;
        carriageJoint.linearLimit = linearLimit;

        JointDrive yDrive = carriageJoint.yDrive;
        yDrive.positionSpring = config.ForkPositionSpring;
        yDrive.positionDamper = config.ForkPositionDamper;
        yDrive.maximumForce = config.ForkMaxDriveForce;
        carriageJoint.yDrive = yDrive;
    }

    private void UpdateTargetHeight(float dt)
    {
        targetHeight += liftInput * config.ForkMoveSpeed * dt;
        targetHeight = Mathf.Clamp(targetHeight, config.ForkMinHeight, config.ForkMaxHeight);
    }

    private void ApplyJointTarget()
    {
        Vector3 target = carriageJoint.targetPosition;
        target.y = targetHeight;
        carriageJoint.targetPosition = target;
    }

    private void ApplyTargetHeightImmediate()
    {
        if (carriageJoint == null)
            return;

        Vector3 target = carriageJoint.targetPosition;
        target.y = targetHeight;
        carriageJoint.targetPosition = target;
    }
}