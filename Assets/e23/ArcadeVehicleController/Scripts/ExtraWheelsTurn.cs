using System.Collections.Generic;
using UnityEngine;


namespace e23.VehicleController
{
    public class ExtraWheelsTurn : MonoBehaviour
    {
#pragma warning disable 0649
        [Tooltip("If left empty an attempt to find a VehicleBehaviour on this GameObject is made.")]
        [SerializeField] private VehicleBehaviour vehicleBehaviour;
        [SerializeField] private List<Transform> wheels;
        [Tooltip("Set to true if you want the wheels to rotate in the opposite direction, allowing for Halo Warthog type behaviour.")]
        [SerializeField] private bool rotateOpposite = false;
#pragma warning restore 0649

        private void Awake()
        {
            if (vehicleBehaviour == null) { GetRequiredComponents(); }
        }

        private void GetRequiredComponents()
        {
            vehicleBehaviour = GetComponent<VehicleBehaviour>();

            if (vehicleBehaviour == null) { Debug.LogWarning($"VehicleBehaviour not assigned or found. Please assign a VehicleBehaviour", gameObject); }
        }

        private void FixedUpdate() => Turn();

        private void Turn()
        {
            float rotateTarget = rotateOpposite == false ? vehicleBehaviour.RotateTarget : -vehicleBehaviour.RotateTarget;

            wheels.ForEach(wheel => wheel.localRotation = Quaternion.Euler(wheel.localRotation.x, rotateTarget / 2, 0));
        }
    }
}