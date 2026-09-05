using UnityEngine;
using System.Collections;

public class TrafficSpawner : MonoBehaviour
{
    [Header("Configuración de Autos")]
    [Tooltip("Arrastra aquí tus prefabs de autos.")]
    public GameObject[] carPrefabs;
    
    [Tooltip("El nodo hacia el que irán los autos al nacer.")]
    public TrafficNode nodoDeInicio;

    [Header("Tiempos y Cantidades")]
    public float tiempoEntreAutos = 5f;
    [Tooltip("Cantidad de autos que este spawner mantendrá vivos al mismo tiempo")]
    public int maxAutos = 10;

    private int autosActivos = 0;

    void Start()
    {
        if (carPrefabs.Length > 0 && nodoDeInicio != null)
        {
            StartCoroutine(GenerarTraficoInicial());
        }
        else
        {
            Debug.LogWarning("Faltan prefabs o el nodo de inicio en el Spawner.");
        }
    }

    private IEnumerator GenerarTraficoInicial()
    {
        while (autosActivos < maxAutos)
        {
            CrearAuto();
            yield return new WaitForSeconds(tiempoEntreAutos);
        }
    }

    private void CrearAuto()
    {
        // 1. Elegir modelo y crearlo
        GameObject prefabElegido = carPrefabs[Random.Range(0, carPrefabs.Length)];
        GameObject nuevoAuto = Instantiate(prefabElegido, transform.position, transform.rotation);

        // 2. Inyectar datos al cerebro del auto
        TrafficCar cerebro = nuevoAuto.GetComponent<TrafficCar>();
        if (cerebro != null)
        {
            cerebro.nodoActual = nodoDeInicio;
            cerebro.miSpawner = this; 
        }

        autosActivos++;
    }

    // El auto manda llamar a esta función justo antes de destruirse
    public void SolicitarReemplazo()
    {
        autosActivos--;
        StartCoroutine(GenerarReemplazo());
    }

    private IEnumerator GenerarReemplazo()
    {
        yield return new WaitForSeconds(tiempoEntreAutos); 
        
        if (autosActivos < maxAutos)
        {
            CrearAuto();
        }
    }
}