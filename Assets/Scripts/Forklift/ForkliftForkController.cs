using UnityEngine;
using Zenject;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(ConfigurableJoint))]
public class ForkliftForkController : MonoBehaviour
{
    [Header("Config")]
    private ForkliftConfig config;

    [Header("References")]
    [SerializeField] private ForkliftEngineState engineState;
    private Rigidbody carriageRb;
    private ConfigurableJoint carriageJoint;

    private ForkliftInput input;

    private float liftInput;
    private float targetHeight;

    [Inject]
    public void Construct(ForkliftConfig config, ForkliftInput input)
    {
        if (this.config == null)
            this.config = config;

        this.input = input;
    }

    private void Reset()
    {
        carriageRb = GetComponent<Rigidbody>();
        carriageJoint = GetComponent<ConfigurableJoint>();
    }

    private void Awake()
    {
        if (carriageRb == null)
            carriageRb = GetComponent<Rigidbody>();

        if (carriageJoint == null)
            carriageJoint = GetComponent<ConfigurableJoint>();

        SetupRigidbody();
        SetupJoint();

        targetHeight = config != null ? config.ForkMaxHeight : 0f;
        ApplyTargetHeightImmediate();
    }

    private void Update()
    {
        if (config == null || carriageJoint == null || input == null)
            return;

        if (engineState != null && !engineState.EngineStarted.CurrentValue)
        {
            liftInput = 0f;
            return;
        }

        float up = input.ReadLiftUp();
        float down = input.ReadLiftDown();

        liftInput = down - up;

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
        linearLimit.limit = Mathf.Abs(config.ForkMinHeight);
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