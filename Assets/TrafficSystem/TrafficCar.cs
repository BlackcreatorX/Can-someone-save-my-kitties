using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrafficCar : MonoBehaviour
{
    [HideInInspector] public TrafficSpawner miSpawner;

    [Header("Navegación")]
    public float velocidadMaxima = 10f;
    public float velocidadRotacion = 5f;
    public TrafficNode nodoActual;

    [Header("Sensores (SphereCast)")]
    public float distanciaRayo = 4f;
    [Tooltip("Pon aquí la capa (Layer) de tus autos y del jugador")]
    public LayerMask capaObstaculos; 
    [Tooltip("Objeto vacío colocado en la defensa delantera del auto")]
    public Transform puntoRayo; 

    private float velocidadActual;
    private bool estaEsperando = false;
    private TrafficNode nodoAnterior; 
    private float tiempoAtascado = 0f;

    void Start()
    {
        velocidadActual = velocidadMaxima;
        
        // Modificador aleatorio para que los autos no vayan a la misma velocidad exacta
        velocidadMaxima *= Random.Range(0.8f, 1.2f);
    }

    void Update()
    {
        if (nodoActual == null) return;

        ManejarSensores();

        // Solo se mueve si no está esperando en un alto y no tiene obstáculos
        if (!estaEsperando && velocidadActual > 0.1f)
        {
            MoverHaciaNodo();
        }
    }

    private void ManejarSensores()
    {
        Vector3 origen = puntoRayo != null ? puntoRayo.position : transform.position;
        RaycastHit hit;
        
        // Sensor de 0.6 de radio para no invadir el carril contrario
        float radioDelSensor = 0.6f; 
        float distanciaDinamica = distanciaRayo + (velocidadMaxima * 0.2f);

        if (Physics.SphereCast(origen, radioDelSensor, transform.forward, out hit, distanciaDinamica, capaObstaculos))
        {
            // Frena suavemente
            velocidadActual = Mathf.Lerp(velocidadActual, 0f, Time.deltaTime * 10f); 
            
            // --- SISTEMA ANTI-EMBOTELLAMIENTO ---
            if (velocidadActual < 0.5f)
            {
                tiempoAtascado += Time.deltaTime;
                
                // Si lleva más de 4 segundos atascado, se elimina y pide reemplazo
                if (tiempoAtascado > 4f)
                {
                    DestruirYReponer();
                }
            }
        }
        else
        {
            // Acelera suavemente y reinicia el cronómetro de atasco
            velocidadActual = Mathf.Lerp(velocidadActual, velocidadMaxima, Time.deltaTime * 2f);
            tiempoAtascado = 0f; 
        }
    }

    private void MoverHaciaNodo()
    {
        Vector3 direccion = nodoActual.transform.position - transform.position;
        direccion.y = 0; // Ignoramos la altura

        // Rotación natural simulando el volante
        if (direccion.magnitude > 0.1f)
        {
            Quaternion rotacionDeseada = Quaternion.LookRotation(direccion);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, Time.deltaTime * velocidadRotacion);
        }

        // Mover hacia adelante
        transform.Translate(Vector3.forward * velocidadActual * Time.deltaTime);

        // Si estamos cerca del nodo, pasamos al siguiente
        if (Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z), 
                             new Vector3(nodoActual.transform.position.x, 0, nodoActual.transform.position.z)) < 1.5f)
        {
            LlegarANodo();
        }
    }

    private void LlegarANodo()
    {
        if (nodoActual.siguientesNodos.Count == 0)
        {
            // Llegó al final de la ciudad
            DestruirYReponer(); 
            return;
        }

        // Revisar si es un cruce con alto
        if (nodoActual.esAlto && !estaEsperando)
        {
            StartCoroutine(EsperarEnCruce(nodoActual.tiempoDeEspera));
        }

        // 1. Filtrar las opciones válidas
        List<TrafficNode> opcionesValidas = new List<TrafficNode>();

        foreach (TrafficNode nodo in nodoActual.siguientesNodos)
        {
            if (nodo != nodoAnterior)
            {
                opcionesValidas.Add(nodo);
            }
        }

        // 2. Seguro contra callejones sin salida
        if (opcionesValidas.Count == 0)
        {
            opcionesValidas.Add(nodoAnterior);
        }

        // 3. Guardar en memoria
        nodoAnterior = nodoActual;

        // 4. Elegir destino
        nodoActual = opcionesValidas[Random.Range(0, opcionesValidas.Count)];
    }

    private IEnumerator EsperarEnCruce(float tiempo)
    {
        estaEsperando = true;
        yield return new WaitForSeconds(tiempo);
        estaEsperando = false;
    }

    private void DestruirYReponer()
    {
        if (miSpawner != null)
        {
            miSpawner.SolicitarReemplazo();
        }
        Destroy(gameObject);
    }
}