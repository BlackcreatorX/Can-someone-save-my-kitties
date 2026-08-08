using UnityEngine;

namespace e23.VehicleController.Audio
{
    public class AudioTag : MonoBehaviour
    {
        [SerializeField] private string audioID = "";

        public string ID => audioID;
    }
}