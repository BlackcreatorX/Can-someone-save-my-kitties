using UnityEngine;
using System.Collections.Generic;

public class TrafficNode : MonoBehaviour
{
    [Tooltip("Arrastra aquí los nodos a los que el auto puede ir después de este.")]
    public List<TrafficNode> siguientesNodos;

    [Header("Intersecciones")]
    [Tooltip("¿Es una esquina o cruce donde el auto debe hacer alto?")]
    public bool esAlto = false;
    [Tooltip("Segundos que el auto esperará antes de continuar")]
    public float tiempoDeEspera = 2f;

    // Esto dibuja esferas y líneas amarillas en la ventana de Escena para que 
    // puedas conectar tus calles visualmente. No se verá en el juego final.
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.position, 0.5f);

        if (siguientesNodos != null)
        {
            Gizmos.color = Color.yellow;
            foreach (TrafficNode nodo in siguientesNodos)
            {
                if (nodo != null)
                {
                    Gizmos.DrawLine(transform.position, nodo.transform.position);
                }
            }
        }
    }
}