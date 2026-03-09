using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CargoAnimator : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private float moveHeight = 8f;
    [SerializeField] private float moveDuration = 5f;
    [SerializeField] private float scaleDuration = 0.5f;
    [SerializeField] private float rotateDegrees = 720f;
    [SerializeField] private GameObject spawnParticle;
    [SerializeField] private GameObject unloadParticle;
    

    [Header("Ease")]
    [SerializeField] private Ease moveEase = Ease.InOutSine;
    [SerializeField] private Ease scaleInEase = Ease.OutBack;
    [SerializeField] private Ease scaleOutEase = Ease.InBack;
    [SerializeField] private Ease rotateEase = Ease.Linear;

    [Header("Physics")]
    [SerializeField] private Rigidbody cargoRb;

    private Vector3 targetPosition;
    private Vector3 initialScale;

    private Tween moveTween;
    private Tween scaleTween;
    private Tween rotateTween;

    private bool isSpawning;
    private bool isUnloading;

    private void Reset()
    {
        cargoRb = GetComponent<Rigidbody>();
    }

    private void Awake()
    {
        if (cargoRb == null)
            cargoRb = GetComponent<Rigidbody>();

        targetPosition = transform.position;
        initialScale = transform.localScale;
    }

    private void Start()
    {
        PlaySpawnAnimation();
    }

    public void PlaySpawnAnimation()
    {
        KillTweens();

        isSpawning = true;
        isUnloading = false;

        Vector3 startPosition = targetPosition + Vector3.up * moveHeight;

        transform.position = startPosition;
        transform.localScale = Vector3.zero;
        
        spawnParticle.SetActive(true);

        if (cargoRb != null)
            cargoRb.isKinematic = true;

        moveTween = transform
            .DOMove(targetPosition, moveDuration)
            .SetEase(moveEase)
            .OnComplete(FinishSpawn);

        scaleTween = transform
            .DOScale(initialScale, scaleDuration)
            .SetEase(scaleInEase);

        rotateTween = transform
            .DORotate(
                new Vector3(0f, rotateDegrees, 0f),
                moveDuration,
                RotateMode.FastBeyond360
            )
            .SetRelative(true)
            .SetEase(rotateEase);
    }

    public void PlayUnloadAnimation()
    {
        if (isUnloading)
            return;

        KillTweens();

        isSpawning = false;
        isUnloading = true;

        unloadParticle.SetActive(true);
        
        if (cargoRb != null)
            cargoRb.isKinematic = true;

        Vector3 endPosition = transform.position + Vector3.up * moveHeight;

        moveTween = transform
            .DOMove(endPosition, moveDuration)
            .SetEase(moveEase);

        rotateTween = transform
            .DORotate(
                new Vector3(0f, rotateDegrees, 0f),
                moveDuration,
                RotateMode.FastBeyond360
            )
            .SetRelative(true)
            .SetEase(rotateEase);

        scaleTween = transform
            .DOScale(Vector3.zero, scaleDuration)
            .SetDelay(Mathf.Max(0f, moveDuration - scaleDuration))
            .SetEase(scaleOutEase)
            .OnComplete(() =>
            {
                unloadParticle.SetActive(false);
                Destroy(gameObject);
            });
    }

    private void FinishSpawn()
    {
        isSpawning = false;

        if (cargoRb != null)
            cargoRb.isKinematic = false;
        
        spawnParticle.SetActive(false);
    }

    private void InterruptSpawnByCollision()
    {
        if (!isSpawning)
            return;

        isSpawning = false;

        KillTweens();

        if (cargoRb != null)
        {
            cargoRb.isKinematic = false;
            cargoRb.linearVelocity = Vector3.zero;
            cargoRb.angularVelocity = Vector3.zero;
        }
        
        spawnParticle.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isSpawning)
            return;

        InterruptSpawnByCollision();
    }

    private void KillTweens()
    {
        moveTween?.Kill();
        scaleTween?.Kill();
        rotateTween?.Kill();

        moveTween = null;
        scaleTween = null;
        rotateTween = null;
    }

    private void OnDestroy()
    {
        KillTweens();
    }
}