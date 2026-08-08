using UnityEngine;

namespace e23.VehicleController
{
    [CreateAssetMenu(fileName = nameof(VehicleBehaviourSettings), menuName = "e23/AVC/Vehicle Settings", order = 3)]
    public class VehicleBehaviourSettings : ScriptableObject
    {
        [Header("Parameters")]
        [Tooltip("How fast the vehicle should speed up.")]
        [Range(1f, 12f)] public float acceleration = 5f;
        [Tooltip("The maximum speed the vehicle can achieve.")]
        [Range(1f, 100f)] public float maxSpeed = 30f;
        [Tooltip("How quickly the vehicle should slow down when breaking.")]
        [Range(1f, 15f)] public float breakSpeed = 5f;
        [Tooltip("Sets the maximum speed whilst boosting.")]
        [Range(5f, 200f)] public float boostSpeed = 60f;
        [Tooltip("When to change from breaking, to reversing using the Vehicle Velocity Square Magnitude.")]
        [Range(1f, 500f)] public float maxSpeedToStartReverse = 150f;
        [Tooltip(" Controls the turning angle of the vehicle, how tight the vehicle can turn. The lower the value, the larger the turning arch.")]
        [Range(20f, 160f)] public float steering = 80f;
        [Tooltip("The maximum speed the vehicle strafes (moves sideways left/ right).")]
        [Range(1f, 40f)] public float maxStrafingSpeed = 15f;
        [Tooltip("Setting the gravity value changes how quickly the vehicle falls to the ground when in the air.")]
        [Range(0f, 20f)] public float gravity = 10f;
        [Tooltip("How easily the vehicle will slide around a corner, the lower the value the harder it is to drift.")]
        [Range(0f, 1f)] public float drift = 1f;
        [Tooltip("How much the vehicle will tilt on the Z axis when turning.")]
        [Range(0f, 3f)] public float vehicleBodyTilt = 0f;
        [Tooltip("Set to 0 for no tilt. Any value between 0 and 1 will give an unwanted rotation. Higher values will result in a more subtle tilt.")]
        [Range(0f, 10f)] public float forwardTilt = 8f;
        [Tooltip("Set to 0 for no tilt. Higher values will result in a more subtle tilt.")]
        [Range(0f, 10f)] public float strafeTilt = 8f;
        [Tooltip("Sets the angular drag of the Rigidbody. This slows the vehicle down when going up an incline. -1 will leave the value at what is set on the Rigidbody.")]
        public float angularDrag = -1f;

        [Header("Switches")]
        [Tooltip("Should the engine already be running? If set to false you will have to have a way for the player to turn on the engine.")]
        public bool autoStartEngine = true;
        [Tooltip("Enabling looser ground follow allows the vehicle to come off the ground with smaller ramps. See documentation for illustration.")]
        public bool looserGroundFollow = true;
        [Tooltip("Enable this flag to keep the vehicle flat when driving off a cliff. Leave disabled to have the vehicle rotate and point towards the ground as it falls.")]
        public bool stayFlatInAir = false;
        [Tooltip("Allow the vehicle to turn whilst in the air.")]
        public bool turnInAir = true;
        [Tooltip("Allow the vehicle to turn when there is no movement.")]
        public bool turnWhenStationary = true;
        [Tooltip("Adds an extra side tilt for turning. Best used with motorbikes.")]
        public bool twoWheelTilt = false;
        [Tooltip("Enabling this will stop the vehicle from sliding down a slope, when it is perpendicular to the slope.")]
        public bool stopSlopeSlide = true;
        [Tooltip("A normalized value to tweak when a vehicle can start to roll down a slope. 1 is the default setting, which stops sliding at 90 degrees, 0.1 will require the vehicle to be pointing directly up/down the slope to roll.")]
        [Range(0.1f, 1.0f)] public float slideThreshold = 1f;
        [Header("Ground Layer")]
        [Tooltip("The layer required for the vehicle to be able to move.")]
        public LayerMask groundMask = 1 << 0;
    }
}