using UnityEngine;

public class ArrowNavigation : MonoBehaviour
{
    [Header("Configuración del Sistema")]
    public bool isSystemActive = true;
    [Tooltip("Distancia a la que se considera que el auto llegó al destino")]
    public float reachDistance = 5f;
    public float rotationSpeed = 10f;

    [Header("Referencias")]
    [Tooltip("Coloca aquí la parte visual de la flecha (el modelo 3D)")]
    public GameObject arrowVisuals;
    [Tooltip("Arrastra aquí los puntos de destino en orden (Destino 1, Destino 2, etc.)")]
    public Transform[] targets;

    private int currentTargetIndex = 0;

    void Update()
    {
        // 1. Determinar si la flecha debe verse
        // Se ve si el sistema está activo Y si aún quedan destinos por visitar
        bool shouldShow = isSystemActive && currentTargetIndex < targets.Length;
        arrowVisuals.SetActive(shouldShow);

        // Si no debe verse, detenemos la ejecución de este frame
        if (!shouldShow) return;

        Transform currentTarget = targets[currentTargetIndex];

        // 2. Rotar la flecha hacia el destino (ignorando la altura Y)
        Vector3 direction = currentTarget.position - transform.position;
        direction.y = 0; // Mantiene la flecha plana

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        // 3. Comprobar la distancia en un plano 2D (X y Z)
        Vector2 myPosition2D = new Vector2(transform.position.x, transform.position.z);
        Vector2 targetPosition2D = new Vector2(currentTarget.position.x, currentTarget.position.z);
        
        float distanceToTarget = Vector2.Distance(myPosition2D, targetPosition2D);

        // 4. Si llegamos, pasamos al siguiente destino
        if (distanceToTarget <= reachDistance)
        {
            currentTargetIndex++;
            Debug.Log("¡Destino " + currentTargetIndex + " alcanzado!");
        }
    }

    // Método público para encender/apagar el sistema desde otros scripts (ej. UI o botones)
    public void ToggleSystem(bool state)
    {
        isSystemActive = state;
    }

    // Método opcional por si necesitas reiniciar la ruta
    public void ResetRoute()
    {
        currentTargetIndex = 0;
        isSystemActive = true;
    }
}