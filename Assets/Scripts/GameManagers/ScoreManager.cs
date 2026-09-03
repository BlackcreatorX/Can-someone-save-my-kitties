using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance;

    [Header("Dinero")]
    public int dineroActual;
    public int dineroMaximo;

    private const string RECORD_KEY = "DineroMaximo";

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        dineroMaximo = PlayerPrefs.GetInt(RECORD_KEY, 0);
    }

    /// <summary>
    /// Agrega dinero al jugador.
    /// </summary>
    public void AgregarDinero(int cantidad)
    {
        dineroActual += cantidad;

        if (dineroActual > dineroMaximo)
        {
            dineroMaximo = dineroActual;
            PlayerPrefs.SetInt(RECORD_KEY, dineroMaximo);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// Reinicia el dinero de la partida.
    /// </summary>
    public void ReiniciarDinero()
    {
        dineroActual = 0;
    }

    public int ObtenerDinero()
    {
        return dineroActual;
    }

    public int ObtenerRecord()
    {
        return dineroMaximo;
    }
}