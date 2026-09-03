using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Paneles de UI")]
    public GameObject pauseMenuPanel;
    public GameObject settingsPanel;

    [Header("Configuración de Escenas")]
    public string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    void Start()
    {
        // Asegurarnos de que el menú de pausa esté oculto al iniciar el nivel
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    void Update()
    {
        // Alternar pausa con la tecla Escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                // Si el jugador está viendo los settings, Escape lo devuelve al menú de pausa
                if (settingsPanel.activeSelf)
                {
                    ShowPauseMenu(); 
                }
                else
                {
                    ResumeGame(); 
                }
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Esto congela el tiempo del juego y físicas
        pauseMenuPanel.SetActive(true);
    }

    // Llama a esto desde el botón "Resume"
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Descongela el tiempo
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(false);
    }

    // Llama a esto desde el botón "Settings" del menú de pausa
    public void ShowSettings()
    {
        pauseMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    // Llama a esto desde el botón "Back" del panel de settings
    public void ShowPauseMenu()
    {
        settingsPanel.SetActive(false);
        pauseMenuPanel.SetActive(true);
    }

    // Llama a esto desde el botón "Main Menu"
    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f; // IMPORTANTÍSIMO: Restaurar el tiempo antes de cambiar de escena
        SceneManager.LoadScene(mainMenuSceneName);
    }
}