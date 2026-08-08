using System.Collections.Generic;
using UnityEngine;

namespace e23.VehicleController
{
    public class ExtraWheelsSpin : MonoBehaviour
    {
#pragma warning disable 0649
        [Tooltip("If left empty an attempt to find a VehicleBehaviour on this GameObject is made.")]
        [SerializeField] private VehicleBehaviour vehicleBehaviour;
        [SerializeField] private List<Transform> extraWheels;
#pragma warning restore 0649
        private float wheelRadius = 1f;

        private void Awake()
        {
            if (vehicleBehaviour == null) { GetRequiredComponents(); }
            if (extraWheels.Count == 0) { Debug.LogWarning($"No wheels have been assigned to the Extra Wheels Brain.", gameObject); return; }

            GetWheelRadius();
        }

        private void GetRequiredComponents()
        {
            vehicleBehaviour = GetComponent<VehicleBehaviour>();

            if (vehicleBehaviour == null) { Debug.LogWarning($"VehicleBehaviour not assigned or found. Please assign a VehicleBehaviour", gameObject); }
        }

        private void GetWheelRadius()
        {
            Bounds wheelBounds = extraWheels[0].GetComponentInChildren<Renderer>().bounds;
            wheelRadius = wheelBounds.size.y;
        }

        private void Update()
        {
            if (vehicleBehaviour.EngineRunning == false) { return; }

            SpinWheels();
        }

        private void SpinWheels()
        {
            float distanceTraveled = vehicleBehaviour.CurrentSpeed * Time.deltaTime;
            float rotationInRadians = distanceTraveled / wheelRadius;
            float rotationInDegrees = rotationInRadians * Mathf.Rad2Deg;

            extraWheels.ForEach(wheel => wheel.Rotate(rotationInDegrees, 0, 0));
        }
    }
}