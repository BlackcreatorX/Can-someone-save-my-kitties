using System.Collections;
using UnityEngine;

namespace e23.VehicleController
{
    [RequireComponent(typeof(ParticleSystem))]
    public class AutoDestroyEffect : MonoBehaviour
    {
        private ParticleSystem _particleSystem;
        private WaitForSeconds _lifeTime;

        private void Awake() => GetRequiredComponents();
        private void Start() => StartCoroutine(DestroyAfterDelay());

        private void GetRequiredComponents()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _lifeTime = new WaitForSeconds(_particleSystem.main.duration);
        }

        private IEnumerator DestroyAfterDelay()
        {
            yield return _lifeTime;
            
            Destroy(gameObject);
        }
    }
}