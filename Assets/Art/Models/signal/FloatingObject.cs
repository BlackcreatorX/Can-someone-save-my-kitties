using UnityEngine;

public class FloatingObject : MonoBehaviour
{
    [Header("Movimiento")]
    public float floatHeight = 0.25f;
    public float floatSpeed = 1f;

    [Header("Rotación")]
    public float rotationSpeed = 45f; // Grados por segundo

    private Vector3 startPosition;

    void OnEnable()
    {
        startPosition = transform.localPosition;
    }

    void Update()
    {
        // Movimiento vertical
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.localPosition = startPosition + Vector3.up * yOffset;

        // Rotación en Y
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);
    }
}