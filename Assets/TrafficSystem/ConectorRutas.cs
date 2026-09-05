using UnityEngine;

public class ConectorRutas : MonoBehaviour
{
    [ContextMenu("Auto Conectar Nodos Hijos")]
    public void ConectarNodos()
    {
        // Obtiene todos los nodos que estén dentro de este objeto, en el orden de la jerarquía
        TrafficNode[] nodos = GetComponentsInChildren<TrafficNode>();

        for (int i = 0; i < nodos.Length - 1; i++)
        {
            // Limpiamos la lista por si la ejecutaste por accidente dos veces
            nodos[i].siguientesNodos.Clear();
            
            // Le asignamos automáticamente el siguiente nodo de la lista
            nodos[i].siguientesNodos.Add(nodos[i + 1]);
        }

        Debug.Log($"¡Se conectaron {nodos.Length} nodos automáticamente en esta calle!");
    }
}