using UnityEngine;

public class DetectorDestinos : MonoBehaviour
{
    [Header("Referencia al Manager")]
    public GameManager gameManager;

    private void OnTriggerEnter(Collider other)
    {
        // Verificamos si la misión de ir al veterinario está activa
        if (gameManager != null && gameManager.misionActiva && gameManager.LlendoAlVeterinario)
        {
            // Comprobamos si el objeto con el que chocamos es EXACTAMENTE la veterinaria actual
            if (other.transform == gameManager.VeterinariaActual)
            {
                Debug.Log("¡Has entrado al collider de la veterinaria!");
                gameManager.EntregarGato();
            }
        }
    }
}