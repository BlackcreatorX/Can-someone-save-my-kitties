using UnityEngine.UI; 
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

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
    [Header("Fin de Partida")]
    public GameObject panelFinPartida;
    public TextMeshProUGUI textoGatosRescatados; 
    public string nombreEscenaMenu = "MainMenu";
    
    private int gatosRescatados = 0;
    
    [Header("Flecha")]
    public CrazyTaxiArrow arrow;
    public bool LlendoAlVeterinario = false;
    
    [Header("Zonas")]
    public List<Zona> zonas = new List<Zona>();

    [Header("Tiempo")]
    public float tiempoPartidaInicial = 600f; // 10 minutos
    public float tiempoMisionInicial = 45f;   // 45 segundos

    private float tiempoRestantePartida;
    private float tiempoRestanteMision;
    private DialogManager dialogManager;
    private CatMinigameManager catMinigameManager;
    private UIManager uiManager;

    private bool partidaActiva;
    public bool misionActiva;

    private Zona zonaActual;

    public Transform ArbolActual { get; private set; }
    public Transform VeterinariaActual { get; private set; }

    void Awake()
    {
        dialogManager = GetComponent<DialogManager>();
        catMinigameManager = GetComponent<CatMinigameManager>();
        uiManager = GetComponent<UIManager>();
    }

    void Start()
    {
        // Garantizar que el tiempo del motor gráfico corre con normalidad
        Time.timeScale = 1f;

        partidaActiva = true;
        gatosRescatados = 0;
        misionActiva = false;
        LlendoAlVeterinario = false;

        // CORRECCIÓN: Se asigna el tiempo inicial correcto, no 3 segundos
        tiempoRestantePartida = tiempoPartidaInicial; 

        // Asegurarnos de que el panel de fin de partida arranque apagado
        if (panelFinPartida != null)
        {
            panelFinPartida.SetActive(false);
        }

        NuevoViaje();
    }

    /// <summary>
    /// Detiene la misión actual.
    /// </summary>
    public void DetenerMision()
    {
        misionActiva = false;
        tiempoRestanteMision = 0f;
    }

    public void LlegadaAlArbol()
    {
        if (misionActiva == true)
        {
            Debug.Log("¡Has llegado al árbol! Rescata al gato.");
            DetenerMision();
        }
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
                MostrarPantallaFinal();
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
                FalloMinijuego();
            }
        }
    }

    /// <summary>
    /// Comienza un nuevo rescate.
    /// </summary>
    public void NuevoViaje()
    {
        if (!misionActiva)
        {
            misionActiva = true;
        }
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

        // Desactivar todos los árboles de la zona
        foreach (Transform arbol in zonaActual.arboles)
        {
            arbol.gameObject.SetActive(false);
        }

        // Elegir uno al azar
        ArbolActual = zonaActual.arboles[Random.Range(0, zonaActual.arboles.Count)];

        // Activar únicamente el seleccionado
        ArbolActual.gameObject.SetActive(true);
        dialogManager.EjecutarDialogo();

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
        {
            Debug.LogWarning("No se puede iniciar el viaje: la zona actual no tiene una veterinaria asignada.");
            return;
        }

        VeterinariaActual = zonaActual.veterinaria;

        if (arrow != null)
        {
            arrow.SetTarget(VeterinariaActual);
        }

        // Llegar al árbol detiene el temporizador y lo deja en cero. El trayecto
        // a la veterinaria es una nueva fase y necesita su propio tiempo.
        tiempoRestanteMision = tiempoMisionInicial;
        misionActiva = true;
        LlendoAlVeterinario = true;

        Debug.Log("¡Gato rescatado! Ahora llévalo a la veterinaria.");
    }

    public void FalloMinijuego()
    {
        StartCoroutine(Fallominigame());
    }

    private IEnumerator Fallominigame()
    {
        yield return new WaitForSeconds(3f);
        
        Debug.Log("¡Fallo en el minijuego! Intenta de nuevo.");
        NuevoViaje();
    }

    /// <summary>
    /// Llamar cuando el jugador entrega el gato.
    /// </summary>
    public void EntregarGatoVet()
    {
        // CORRECCIÓN: Redirigimos esto a EntregarGato() para unificar la lógica
        EntregarGato();
    }

    public void EntregarGato()
    {
        misionActiva = false;
        LlendoAlVeterinario = false;
        
        // Sumamos un gato rescatado
        gatosRescatados++; 
        
        Debug.Log("¡Gato entregado! Rescate completado. Llevas: " + gatosRescatados);
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

    public void Startminigame()
    {
        uiManager.MostrarMinigamePanel();
        catMinigameManager.StartMinigame();
    }

    private void MostrarPantallaFinal()
    {
        // Pausar el juego
        Time.timeScale = 0f; 
        
        // Mostrar el panel y el texto
        panelFinPartida.SetActive(true);
        textoGatosRescatados.text = "Gatitos rescatados: " + gatosRescatados;
    }

    public void ReiniciarJuego()
    {
        // Despausar y recargar la escena actual
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void VolverAlMenu()
    {
        // Despausar y cargar el menú principal
        Time.timeScale = 1f;
        SceneManager.LoadScene(nombreEscenaMenu);
    }
}
