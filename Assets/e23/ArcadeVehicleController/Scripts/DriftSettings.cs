using UnityEngine;

namespace e23.VehicleController
{
    [CreateAssetMenu(fileName = nameof(DriftSettings), menuName = "e23/AVC/Drift Settings", order = 5)]
    public class DriftSettings : ScriptableObject
    {
        [Tooltip("MaxSpeed is divided by this value, setting this to a lower value will result in having to go faster to active the skids.")]
        [SerializeField] private float skidSpeedThreshold = 1.25f;
        [Tooltip("Set this to the minimum angle you want the vehicle to be turning at for skids to be activated.")]
        [SerializeField] private float skidAngleThreshold = 20.0f;
        [Tooltip("MaxSpeed is divided by this value, setting this to a lower value will result in having to go faster to active the skids.")]
        [SerializeField] private float skidReverseSpeedThreshold = 1f;
        
        public float SkidSpeed => skidSpeedThreshold;
        public float SkidAngle => skidAngleThreshold;
        public float SkidReverseSpeed => skidReverseSpeedThreshold;

        public void SetDriftSettings(float speed, float angle, float reverseSpeed)
        {
            skidSpeedThreshold = speed;
            skidAngleThreshold = angle;
            skidReverseSpeedThreshold = reverseSpeed;
        }
    }
}