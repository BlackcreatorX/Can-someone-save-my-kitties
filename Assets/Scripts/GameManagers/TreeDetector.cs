using UnityEngine;

public class TreeDetector : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;

    private void Start()
    {
        if (gameManager == null)
        {
            gameManager = FindObjectOfType<GameManager>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (gameManager == null)
        {
            Debug.LogWarning("No se encontró el GameManager.");
            return;
        }

        if (other.CompareTag("Tree"))
        {
            other.gameObject.SetActive(false);

            gameManager.LlegadaAlArbol();
            gameManager.Startminigame();
        }
        else if (other.CompareTag("PetCare") && gameManager.LlendoAlVeterinario)
        {
            ScoreManager.Instance.AgregarDinero(100);
            gameManager.EntregarGato();
        }
    }
}