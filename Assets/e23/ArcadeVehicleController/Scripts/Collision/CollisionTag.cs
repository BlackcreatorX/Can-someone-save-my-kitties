using UnityEngine;

namespace e23.VehicleController
{
    public class CollisionTag : MonoBehaviour
    {
        [SerializeField] private string collisionID = "";

        public string ID => collisionID;
    }
}