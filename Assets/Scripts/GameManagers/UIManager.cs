using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Header("Textos")]
    [SerializeField] private TMP_Text tiempoPartidaText;
    [SerializeField] private TMP_Text tiempoMisionText;
    [SerializeField] private GameObject Minigamepanel;

    private void Update()
    {
        // Si no hay GameManager, no hacemos nada para evitar errores
        if (gameManager == null)
            return;

        // Candado 1: Verificamos que el texto de misión esté asignado antes de tocarlo
        if (tiempoMisionText != null)
        {
            if (gameManager.misionActiva == false)
            {
                tiempoMisionText.enabled = false; 
            }
            else 
            {
                tiempoMisionText.enabled = true; 
            }
            
            // Actualizamos el texto
            tiempoMisionText.text = gameManager.ObtenerTiempoMisionFormateado();
        }

        // Candado 2: Verificamos que el texto de partida esté asignado antes de tocarlo
        if (tiempoPartidaText != null)
        {
            tiempoPartidaText.text = gameManager.ObtenerTiempoPartidaFormateado();
        }
    }

    public void MostrarMinigamePanel()
    {
        if (Minigamepanel != null)
        {
            Minigamepanel.SetActive(true);
        }
    }

    public void OcultarMinigamePanel()
    {
        if (Minigamepanel != null)
        {
            Minigamepanel.SetActive(false);
        }
    }
}