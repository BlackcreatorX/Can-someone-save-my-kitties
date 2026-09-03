using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Paneles de UI")]
    public GameObject mainMenuPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;
    public GameObject tutorialPanel;

    [Header("Configuración de Escenas")]
    public string gameSceneName = "GameScene";

    [Header("Tutorial: Libro Animado")]
    [Tooltip("El RectTransform de la imagen del libro")]
    public RectTransform bookRect;
    [Tooltip("El componente Animator de la imagen del libro")]
    public Animator bookAnimator;
    [Tooltip("Todos los CanvasGroup que contienen el texto/imágenes de cada página (en orden)")]
    public CanvasGroup[] bookPages;
    
    [Header("Tutorial: Tiempos y Ajustes")]
    public float dropHeight = 800f; // Qué tan alto inicia la caída
    public float dropDuration = 0.5f; // Cuánto tarda en caer a la mesa
    public float textFadeSpeed = 10f; // Velocidad del difuminado del texto
    public float pageTurnAnimDuration = 0.5f; // Cuánto dura tu animación de sprite de pasar página
    public float closeBookAnimDuration = 0.5f; // Cuánto dura tu animación de sprite de cerrar el libro

    private int currentTutorialPage = 0;
    private bool isBookAnimating = false;
    private Vector2 originalBookPos;
    private Coroutine tutorialRoutine;

    void Start()
    {
        // Guardar la posición original (en la mesa) donde debe aterrizar el libro
        if (bookRect != null)
        {
            originalBookPos = bookRect.anchoredPosition;
        }

        ShowMainMenu();
    }

    // --- FUNCIONES DEL MENÚ GENERAL ---
    public void PlayGame()
    {
        SceneManager.LoadScene(gameSceneName);
    }

    public void ShowSettings()
    {
        mainMenuPanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void ShowCredits()
    {
        mainMenuPanel.SetActive(false);
        creditsPanel.SetActive(true);
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);
        
        if (tutorialPanel != null && tutorialPanel.activeSelf) 
        {
            tutorialPanel.SetActive(false); 
            // Si salimos del tutorial a la mitad, detenemos cualquier animación pendiente
            if (tutorialRoutine != null) StopCoroutine(tutorialRoutine);
        }
    }

    public void QuitGame()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // --- FUNCIONES DEL TUTORIAL (LIBRO) ---

    public void ShowTutorial()
    {
        mainMenuPanel.SetActive(false);
        tutorialPanel.SetActive(true);
        
        // Iniciamos la secuencia del libro cayendo
        if (tutorialRoutine != null) StopCoroutine(tutorialRoutine);
        tutorialRoutine = StartCoroutine(OpenTutorialSequence());
    }

    // Se llama desde el Botón "Pasar Página" o "Siguiente"
    public void NextTutorialPage()
{
    Debug.Log("1. Clic detectado en el botón");
    
    if (isBookAnimating) 
    {
        Debug.Log("2. BLOQUEO: isBookAnimating sigue siendo TRUE. La animación anterior no terminó.");
        return; 
    }

    Debug.Log("3. EXITO: El botón pasó el bloqueo. Iniciando Corrutina de pasar página...");
    tutorialRoutine = StartCoroutine(NextPageSequence());
}

    private IEnumerator OpenTutorialSequence()
    {
        isBookAnimating = true;
        currentTutorialPage = 0;

        // 1. Apagar todos los textos de las páginas
        foreach (var page in bookPages)
        {
            page.alpha = 0f;
            page.gameObject.SetActive(false);
        }

        // 2. Mover el libro hacia arriba
        bookRect.anchoredPosition = new Vector2(originalBookPos.x, originalBookPos.y + dropHeight);
        
        // Asegurarnos de que el Animator empiece en la animación base (Libro Cerrado)
        if (bookAnimator != null) bookAnimator.Play("IdleClosed"); // <-- Reemplaza "IdleClosed" con el nombre de tu estado inicial

        // 3. Caída suave hacia la mesa
        float t = 0;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dropDuration;
            bookRect.anchoredPosition = Vector2.Lerp(new Vector2(originalBookPos.x, originalBookPos.y + dropHeight), originalBookPos, t);
            yield return null;
        }
        bookRect.anchoredPosition = originalBookPos; // Ajuste perfecto al final

        // 4. Aparecer el texto de la portada (Página 0)
        bookPages[0].gameObject.SetActive(true);
        yield return StartCoroutine(FadeText(bookPages[0], 1f));

        isBookAnimating = false; // Liberar el botón
    }

    private IEnumerator NextPageSequence()
    {
        isBookAnimating = true; // Bloquear botón

        // 1. Difuminar el texto actual rápido
        yield return StartCoroutine(FadeText(bookPages[currentTutorialPage], 0f));
        bookPages[currentTutorialPage].gameObject.SetActive(false);

        // 2. ¿Llegamos al final del libro?
        if (currentTutorialPage >= bookPages.Length - 1)
        {
            // A. Animación de CERRAR el libro
            if (bookAnimator != null) bookAnimator.SetTrigger("CloseBook");
            yield return new WaitForSecondsRealtime(closeBookAnimDuration);

            // B. Reiniciar a la portada
            currentTutorialPage = 0;
        }
        else
        {
            // A. Animación de PASAR página
            if (bookAnimator != null) bookAnimator.SetTrigger("TurnPage");
            yield return new WaitForSecondsRealtime(pageTurnAnimDuration);
            
            // B. Sumar página
            currentTutorialPage++;
        }

        // 3. Aparecer el nuevo texto
        bookPages[currentTutorialPage].gameObject.SetActive(true);
        yield return StartCoroutine(FadeText(bookPages[currentTutorialPage], 1f));

        isBookAnimating = false; // Desbloquear botón
    }

    // Función auxiliar para difuminar CanvasGroups
    private IEnumerator FadeText(CanvasGroup cg, float targetAlpha)
    {
        while (Mathf.Abs(cg.alpha - targetAlpha) > 0.01f)
        {
            cg.alpha = Mathf.MoveTowards(cg.alpha, targetAlpha, Time.unscaledDeltaTime * textFadeSpeed);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }
}