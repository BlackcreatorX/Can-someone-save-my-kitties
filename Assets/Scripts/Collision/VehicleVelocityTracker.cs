using UnityEngine;

/// <summary>
/// Pon este componente en el GameObject del jugador que tiene el Rigidbody.
/// Guarda la velocidad que llevaba el auto justo antes del paso de física,
/// para que Destructible pueda devolverle el impulso tras romper algo
/// (en OnCollisionEnter el motor ya frenó al auto).
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[DefaultExecutionOrder(100)] // Se ejecuta después del controlador del vehículo
public class VehicleVelocityTracker : MonoBehaviour
{
    private Rigidbody _rb;

    public Vector3 LastVelocity { get; private set; }

    private void Awake() => _rb = GetComponent<Rigidbody>();

    private void FixedUpdate() => LastVelocity = _rb.linearVelocity;
}
