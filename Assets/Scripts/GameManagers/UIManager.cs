using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    [Header("Textos")]
    [SerializeField] private TMP_Text tiempoPartidaText;
    [SerializeField] private TMP_Text tiempoMisionText;

    private void Update()
    {
        if (gameManager == null)
            return;

        tiempoPartidaText.text = gameManager.ObtenerTiempoPartidaFormateado();
        tiempoMisionText.text = gameManager.ObtenerTiempoMisionFormateado();
    }
}