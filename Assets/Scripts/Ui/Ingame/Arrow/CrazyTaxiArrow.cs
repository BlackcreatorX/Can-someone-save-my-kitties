using UnityEngine;

public class CrazyTaxiArrow : MonoBehaviour
{
    [Header("Referencias")]
    public Transform car;

    [Header("Destino Actual")]
    public Transform target;

    [Header("Ajustes")]
    public bool keepFlat = true;
    public Vector3 rotationOffset;

    void LateUpdate()
    {
        if (car == null || target == null)
            return;

        Vector3 dir = target.position - car.position;

        if (keepFlat)
            dir.y = 0f;

        Vector3 localDir = car.InverseTransformDirection(dir);

        if (localDir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(localDir);
            transform.localRotation = rot * Quaternion.Euler(rotationOffset);
        }
    }

    /// <summary>
    /// Llamar desde el GameManager para cambiar el destino.
    /// </summary>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}