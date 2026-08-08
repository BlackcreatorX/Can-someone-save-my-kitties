using UnityEngine;

namespace e23.VehicleController
{
    [CreateAssetMenu(fileName = nameof(CollisionEffectsData), menuName = "e23/AVC/Collision Effects Data", order = 5)]
    public class CollisionEffectsData : ScriptableObject
    {
        [SerializeField] private string effectsID = "";
        [SerializeField] private CollisionType collisionType;
        [SerializeField] private GameObject effectPrefab = null;
        [SerializeField] private float requiredSpeed;
        public string ID => effectsID;
        public CollisionType CollisionType => collisionType;
        public GameObject Prefab => effectPrefab;
        public float RequiredSpeed => requiredSpeed;
    }
}