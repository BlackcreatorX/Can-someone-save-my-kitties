using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Zona
{
    public string nombre;

    [Header("Árboles de la zona")]
    public List<Transform> arboles = new List<Transform>();

    [Header("Veterinaria de la zona")]
    public Transform veterinaria;
}

public class GameManager : MonoBehaviour
{
    [Header("Flecha")]
    public CrazyTaxiArrow arrow;

    [Header("Zonas")]
    public List<Zona> zonas = new List<Zona>();

    [Header("Tiempo")]
    public float tiempoPartidaInicial = 600f; // 10 minutos
    public float tiempoMisionInicial = 45f;   // 45 segundos

    private float tiempoRestantePartida;
    private float tiempoRestanteMision;

    private bool partidaActiva;
    private bool misionActiva;

    private Zona zonaActual;

    public Transform ArbolActual { get; private set; }
    public Transform VeterinariaActual { get; private set; }

    private void Start()
    {
        tiempoRestantePartida = tiempoPartidaInicial;
        partidaActiva = true;

        NuevoViaje();
    }

    private void Update()
    {
        // Temporizador general
        if (partidaActiva)
        {
            tiempoRestantePartida -= Time.deltaTime;

            if (tiempoRestantePartida <= 0)
            {
                tiempoRestantePartida = 0;
                partidaActiva = false;

                Debug.Log("¡Fin de la partida!");
            }
        }

        // Temporizador de misión
        if (misionActiva)
        {
            tiempoRestanteMision -= Time.deltaTime;

            if (tiempoRestanteMision <= 0)
            {
                tiempoRestanteMision = 0;
                misionActiva = false;

                Debug.Log("¡Tiempo de la misión agotado!");
            }
        }
    }

    /// <summary>
    /// Comienza un nuevo rescate.
    /// </summary>
    public void NuevoViaje()
    {
        IniciarRescate();
    }

    /// <summary>
    /// Selecciona una zona y un árbol aleatorios.
    /// </summary>
    public void IniciarRescate()
    {
        if (zonas.Count == 0)
            return;

        zonaActual = zonas[Random.Range(0, zonas.Count)];

        if (zonaActual.arboles.Count == 0)
            return;

        ArbolActual = zonaActual.arboles[Random.Range(0, zonaActual.arboles.Count)];

        arrow.SetTarget(ArbolActual);

        tiempoRestanteMision = tiempoMisionInicial;
        misionActiva = true;

        Debug.Log("Ve a rescatar al gato.");
    }

    /// <summary>
    /// Llamar cuando el jugador rescata al gato.
    /// </summary>
    public void IrAVeterinaria()
    {
        if (zonaActual == null || zonaActual.veterinaria == null)
            return;

        VeterinariaActual = zonaActual.veterinaria;

        arrow.SetTarget(VeterinariaActual);

        tiempoRestanteMision = tiempoMisionInicial;
        misionActiva = true;

        Debug.Log("¡Gato rescatado! Ahora llévalo a la veterinaria.");
    }

    /// <summary>
    /// Llamar cuando el jugador entrega el gato.
    /// </summary>
    public void EntregarGato()
    {
        misionActiva = false;

        Debug.Log("¡Rescate completado!");

        NuevoViaje();
    }

    // ==========================
    // GETTERS
    // ==========================

    public float ObtenerTiempoPartida()
    {
        return tiempoRestantePartida;
    }

    public float ObtenerTiempoMision()
    {
        return tiempoRestanteMision;
    }

    public string ObtenerTiempoPartidaFormateado()
    {
        int minutos = Mathf.FloorToInt(tiempoRestantePartida / 60);
        int segundos = Mathf.FloorToInt(tiempoRestantePartida % 60);

        return $"{minutos:00}:{segundos:00}";
    }

    public string ObtenerTiempoMisionFormateado()
    {
        int minutos = Mathf.FloorToInt(tiempoRestanteMision / 60);
        int segundos = Mathf.FloorToInt(tiempoRestanteMision % 60);

        return $"{minutos:00}:{segundos:00}";
    }
}