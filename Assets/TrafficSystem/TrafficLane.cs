using UnityEngine;
using System.Collections.Generic;

public class TrafficLane : MonoBehaviour
{
    [Header("Puntos de la Calle")]
    [Tooltip("Punto 1: Donde aparecen los autos")]
    public Transform startPoint;
    [Tooltip("Punto 2: Donde desaparecen y se reinician")]
    public Transform endPoint;

    [Header("Configuración del Tráfico")]
    [Tooltip("Arrastra aquí tus prefabs de autos. Puedes poner varios para que haya variedad.")]
    public GameObject[] carPrefabs;
    [Tooltip("Cuántos autos habrá circulando al mismo tiempo en esta calle")]
    public int numberOfCars = 5;
    [Tooltip("Velocidad de movimiento de los autos")]
    public float speed = 10f;

    private List<Transform> activeCars = new List<Transform>();
    private Vector3 moveDirection;
    private float laneDistance;

    void Start()
    {
        if (startPoint == null || endPoint == null || carPrefabs.Length == 0 || numberOfCars <= 0)
        {
            Debug.LogWarning("Faltan referencias en el TrafficLane o el número de autos es 0.");
            return;
        }

        // 1. Calcular la dirección y la distancia total de la calle
        moveDirection = (endPoint.position - startPoint.position).normalized;
        laneDistance = Vector3.Distance(startPoint.position, endPoint.position);

        // 2. Calcular la separación exacta entre cada auto
        float spacing = laneDistance / numberOfCars;

        // 3. Generar los autos y distribuirlos a lo largo de la calle
        for (int i = 0; i < numberOfCars; i++)
        {
            // Elegir un modelo de auto al azar de tu lista
            GameObject prefab = carPrefabs[Random.Range(0, carPrefabs.Length)];
            
            // Calcular su posición inicial para que la calle empiece ya llena de tráfico
            Vector3 spawnPos = startPoint.position + (moveDirection * (spacing * i));
            
            // Instanciar el auto mirando hacia el Punto 2
            GameObject newCar = Instantiate(prefab, spawnPos, Quaternion.LookRotation(moveDirection));
            
            // Agruparlos dentro de este objeto para mantener la jerarquía ordenada
            newCar.transform.SetParent(this.transform);
            
            activeCars.Add(newCar.transform);
        }
    }

    void Update()
    {
        for (int i = 0; i < activeCars.Count; i++)
        {
            Transform car = activeCars[i];

            // Mover el auto en línea recta
            car.position += moveDirection * speed * Time.deltaTime;

            // Calcular qué tan lejos está del inicio
            float currentDist = Vector3.Distance(startPoint.position, car.position);
            
            // Si el auto ya recorrió toda la distancia (llegó al Punto 2)
            if (currentDist >= laneDistance)
            {
                // Lo teletransportamos de vuelta al Punto 1
                car.position = startPoint.position;
            }
        }
    }

    // Esta función dibuja líneas visuales en el Editor de Unity (No se ven en el juego final)
    private void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            // Línea cyan para la ruta
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startPoint.position, endPoint.position);
            
            // Esfera verde para el inicio (Punto 1)
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPoint.position, 1f);
            
            // Esfera roja para el final (Punto 2)
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endPoint.position, 1f);
        }
    }
}