using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CargoUnloadZone : MonoBehaviour
{
    [SerializeField] private GameObject unloadZoneParticle;
    
    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
        unloadZoneParticle.SetActive(true);
    }

    private void OnTriggerEnter(Collider other)
    {
        CargoAnimator cargoAnimator = other.GetComponentInParent<CargoAnimator>();
        if (cargoAnimator == null)
            return;

        cargoAnimator.PlayUnloadAnimation();
        unloadZoneParticle.SetActive(false);
    }
}