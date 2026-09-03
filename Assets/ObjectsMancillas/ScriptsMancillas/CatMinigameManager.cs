using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CatVariant
{
    public Button[] buttons;
}

public class CatMinigameManager : MonoBehaviour
{
    [Header("Modo de Prueba")]
    public bool isTestMode = false;
    

    [Header("Transiciones (Blackout)")]
    public CanvasGroup blackoutPanel;
    public GameObject introTreeImage; 
    public float fadeSpeed = 2f;
    public float introTreeDuration = 1.5f;

    [Header("UI del Minijuego")]
    public GameObject minigamePanel;
    
    [Header("Grupos de Gatos")]
    public CatVariant[] catVariants;
    
    [Header("Ritmo de Aparición")]
    public float showDuration = 1.2f;
    public float hideDuration = 0.8f;

    [Header("GameManager")]
    [SerializeField] private GameManager gameManager;

    
    
    [Header("Feedback de Intentos")]
    public Image[] attemptIcons; 
    public Sprite unusedAttemptSprite; 
    public Sprite usedAttemptSprite; 

    [Header("Cursor UI Personalizado")]
    public RectTransform uiCursor; 
    public Image uiCursorImage;
    public Sprite openHandsSprite;
    public Sprite closedHandsSprite;

    [Header("Efectos de Sonido (SFX)")]
    public AudioSource sfxSource;
    [Tooltip("Sonido que se reproducirá en la pantalla del árbol antes de jugar")]
    public AudioClip catAppearSFX;
    public AudioClip clickSFX;
    public AudioClip winSFX;
    public AudioClip loseSFX;
    
    private bool isMinigameActive = false; 
    private int missClicksLeft; 
    private Coroutine minigameRoutine;
    private Button[] activeCatButtons; 

    void Awake()
    {
       gameManager = GetComponent<GameManager>();
    }

    void Start()
    {
        if (uiCursor != null) uiCursor.gameObject.SetActive(false);

        if (isTestMode)
        {
            if (blackoutPanel != null) blackoutPanel.alpha = 0f;
            StartMinigame();
        }
    }

    void Update()
    {
        if (isMinigameActive && uiCursor != null)
        {
            uiCursor.position = Input.mousePosition;

            if (Input.GetMouseButtonDown(0))
            {
                if (uiCursorImage != null) uiCursorImage.sprite = closedHandsSprite;
                PlaySound(clickSFX);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (uiCursorImage != null) uiCursorImage.sprite = openHandsSprite;
            }
        }
    }

  

    public void StartMinigame()
    {
        if (attemptIcons != null)
        {
            foreach (Image icon in attemptIcons)
            {
                if (icon != null) icon.sprite = unusedAttemptSprite;
            }
        }

        foreach (var variant in catVariants)
        {
            foreach (Button btn in variant.buttons)
            {
                btn.gameObject.SetActive(false);
                btn.onClick.RemoveAllListeners();
            }
        }

        if (catVariants != null && catVariants.Length > 0)
        {
            int randomVariantIndex = Random.Range(0, catVariants.Length);
            activeCatButtons = catVariants[randomVariantIndex].buttons;
        }

        if (activeCatButtons != null)
        {
            foreach (Button btn in activeCatButtons)
            {
                btn.onClick.AddListener(CatchCat);
            }
        }

        StartCoroutine(IntroSequence());
    }

    private IEnumerator FadeBlackout(float targetAlpha)
    {
        if (blackoutPanel == null) yield break;
        
        blackoutPanel.blocksRaycasts = true; 
        
        while (Mathf.Abs(blackoutPanel.alpha - targetAlpha) > 0.01f)
        {
            blackoutPanel.alpha = Mathf.MoveTowards(blackoutPanel.alpha, targetAlpha, Time.unscaledDeltaTime * fadeSpeed);
            yield return null;
        }
        
        blackoutPanel.alpha = targetAlpha;
        if (targetAlpha == 0f) blackoutPanel.blocksRaycasts = false; 
    }

    private IEnumerator IntroSequence()
    {
        yield return new WaitForSecondsRealtime(0.2f);
        yield return StartCoroutine(FadeBlackout(1f));

        if (introTreeImage != null) introTreeImage.SetActive(true);
        minigamePanel.SetActive(false);

        // Quita la pantalla negra para ver el árbol...
        yield return StartCoroutine(FadeBlackout(0f));
        
        // --- AQUÍ SE REPRODUCE EL SONIDO UNA SOLA VEZ ---
        PlaySound(catAppearSFX);
        
        // Pausa dramática mostrando el árbol...
        yield return new WaitForSecondsRealtime(introTreeDuration);
        
        yield return StartCoroutine(FadeBlackout(1f));

        if (introTreeImage != null) introTreeImage.SetActive(false);
        minigamePanel.SetActive(true);
        missClicksLeft = 3; 

        yield return StartCoroutine(FadeBlackout(0f));

        isMinigameActive = true;
        if (uiCursor != null)
        {
            uiCursor.gameObject.SetActive(true);
            if (uiCursorImage != null) uiCursorImage.sprite = openHandsSprite;
        }
        Cursor.visible = false; 

        minigameRoutine = StartCoroutine(MinigameLoop());
    }

    private IEnumerator MinigameLoop()
    {
        yield return new WaitForSecondsRealtime(1f);
        
        int lastCatIndex = -1;

        while (true)
        {
            if (activeCatButtons == null || activeCatButtons.Length == 0) yield break;

            int randomIndex = Random.Range(0, activeCatButtons.Length);
            
            if (activeCatButtons.Length > 1)
            {
                while (randomIndex == lastCatIndex)
                {
                    randomIndex = Random.Range(0, activeCatButtons.Length);
                }
            }
            
            lastCatIndex = randomIndex;

            Button activeCat = activeCatButtons[randomIndex];
            activeCat.gameObject.SetActive(true);
            
            // (El sonido de aparición ha sido eliminado de aquí)
            
            yield return new WaitForSecondsRealtime(showDuration);
            
            if (activeCat.gameObject.activeSelf)
            {
                activeCat.gameObject.SetActive(false);
            }

            yield return new WaitForSecondsRealtime(hideDuration);
        }
    }

    public void CatchCat()
    {
        if (minigameRoutine != null) StopCoroutine(minigameRoutine);
        
        if (activeCatButtons != null)
        {
            foreach (Button btn in activeCatButtons) btn.gameObject.SetActive(false);
        }

        StartCoroutine(EndMinigame(true));
    }

    public void MissClickBackground()
    {
        missClicksLeft--; 
        
        if (attemptIcons != null && missClicksLeft >= 0 && missClicksLeft < attemptIcons.Length)
        {
            if (attemptIcons[missClicksLeft] != null)
            {
                attemptIcons[missClicksLeft].sprite = usedAttemptSprite;
            }
        }
        
        if (missClicksLeft <= 0)
        {
            if (minigameRoutine != null) StopCoroutine(minigameRoutine);
            
            if (activeCatButtons != null)
            {
                foreach (Button btn in activeCatButtons) btn.gameObject.SetActive(false);
            }
            
            StartCoroutine(EndMinigame(false));
        }
    }

    private IEnumerator EndMinigame(bool won)
    {
        isMinigameActive = false;
        if (uiCursor != null) uiCursor.gameObject.SetActive(false);
        Cursor.visible = true; 

        PlaySound(won ? winSFX : loseSFX);

        if (gameManager != null)
        {
            if (won)
            {
                gameManager.IrAVeterinaria();
            }
            else
            {
                gameManager.FalloMinijuego();
            }
        }

        yield return new WaitForSecondsRealtime(0.5f);
        yield return StartCoroutine(FadeBlackout(1f));
        
        minigamePanel.SetActive(false);
        
        if (isTestMode)
        {
            Debug.Log(won ? "¡Prueba superada! Atrapaste al gato." : "¡Prueba fallida! Hiciste 3 clics fuera.");
            yield return new WaitForSecondsRealtime(1f);
            StartMinigame(); 
        }
        else
        {
         
            
            yield return StartCoroutine(FadeBlackout(0f));
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip); 
        }
    }
}