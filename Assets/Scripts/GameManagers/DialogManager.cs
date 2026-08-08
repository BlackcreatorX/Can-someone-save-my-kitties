using System.Collections;
using UnityEngine;
using TMPro;

public class DialogManager : MonoBehaviour
{
    [System.Serializable]
    public class Dialogo
    {
        public string personaje;
        public string texto;
    }

    [System.Serializable]
    public class DialogoJSON
    {
        public Dialogo[] dialogos;
    }

    [Header("JSON")]
    public TextAsset dialogJson;

    [Header("UI")]
    public GameObject dialogPanel;
    public TMP_Text nombreText;
    public TMP_Text dialogoText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip SonidoLlamada;
    


    // Ejecutar diálogo desde otro script
    public void EjecutarDialogo()
    {
        if (dialogJson == null)
        {
            Debug.LogError("No hay un Dialog.json asignado.");
            return;
        }

        DialogoJSON datos = JsonUtility.FromJson<DialogoJSON>(dialogJson.text);

        StartCoroutine(MostrarDialogoAleatorio(datos));
    }
     void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            EjecutarDialogo();
        }
    }

    public IEnumerator MostrarDialogoAleatorio(DialogoJSON datos)
    {
        if (datos == null || datos.dialogos.Length == 0)
            yield break;


        // Selecciona diálogo aleatorio
        Dialogo dialogo = datos.dialogos[Random.Range(0, datos.dialogos.Length)];


        // Sonido de llamada
        if (audioSource != null && SonidoLlamada != null)
            audioSource.PlayOneShot(SonidoLlamada);


        dialogPanel.SetActive(true);

        nombreText.text = dialogo.personaje;
        dialogoText.text = "";


        string texto = dialogo.texto;

        // Duración total de escritura
        float duracionEscritura = 2f;

        float tiempoPorLetra = texto.Length > 0
            ? duracionEscritura / texto.Length
            : 0f;


        foreach (char letra in texto)
        {
            dialogoText.text += letra;


        

            yield return new WaitForSeconds(tiempoPorLetra);
        }


        // Mantener diálogo 5 segundos
        yield return new WaitForSeconds(5f);


        dialogPanel.SetActive(false);
    }
}