using System.Collections.Generic;
using UnityEngine;

namespace e23.VehicleController
{
    public class CollisionEffects : MonoBehaviour
    {
        [SerializeField] private List<CollisionEffectsData> effectPrefabs;

        private Dictionary<CollisionType, CollisionEffectsData> _effects;
        private Dictionary<string, CollisionEffectsData> _customEffects;

        private CollisionManager _collisionManager;
        
        private void Awake()
        {
            GetRequiredComponents();
            RegisterActions(true);
            AddEffects();
        }

        private void GetRequiredComponents() => _collisionManager = GetComponentInParent<CollisionManager>();

        private void RegisterActions(bool register)
        {
            _collisionManager.OnVehicleCollisionEnter -= PlayEffectOnCollision;

            if (register == false) { return; }

            _collisionManager.OnVehicleCollisionEnter += PlayEffectOnCollision;
        }

        private void AddEffects()
        {
            _effects ??= new Dictionary<CollisionType, CollisionEffectsData>();
            _customEffects ??= new Dictionary<string, CollisionEffectsData>();
            
            effectPrefabs.ForEach(data =>
            {
                if (data.CollisionType == CollisionType.Custom)
                {
                    _customEffects.Add(data.ID, data);
                }
                else
                {
                    _effects.Add(data.CollisionType, data);
                }
            });
        }
        
        private void PlayEffectOnCollision(Collision collision)
        {
            if (collision.gameObject.TryGetComponent(out CollisionTag collisionTag))
            {
                if (collision.relativeVelocity.magnitude < _customEffects[collisionTag.ID].RequiredSpeed)
                { return; }
                SpawnEffect(_customEffects[collisionTag.ID].Prefab, collision.contacts[0].point, collision.contacts[0].normal);
                return;
            }
            
            if (collision.relativeVelocity.magnitude < _effects[CollisionType.Default].RequiredSpeed)
            { return; }
            
            SpawnEffect(_effects[CollisionType.Default].Prefab, collision.contacts[0].point, collision.contacts[0].normal);
        }

        private void SpawnEffect(GameObject prefab, Vector3 position, Vector3 rotation) => Instantiate(prefab, position, Quaternion.LookRotation(rotation));

        public void AddEffectsData(CollisionEffectsData newData)
        {
            if (effectPrefabs == null) { effectPrefabs = new List<CollisionEffectsData>(); }
            effectPrefabs.Add(newData);
        }
    }
}