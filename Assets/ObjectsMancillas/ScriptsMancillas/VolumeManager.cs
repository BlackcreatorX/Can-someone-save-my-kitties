using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeManager : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra tu AudioMixer principal aquí")]
    public AudioMixer audioMixer;
    
    [Header("Sliders")]
    public Slider musicSlider;
    public Slider sfxSlider;

    // Claves para guardar los datos
    private const string MUSIC_PREF = "MusicVolume";
    private const string SFX_PREF = "SFXVolume";

    void Start()
    {
        // Cargar valores guardados en sesiones anteriores (1f es el máximo por defecto)
        float musicVolume = PlayerPrefs.GetFloat(MUSIC_PREF, 1f);
        float sfxVolume = PlayerPrefs.GetFloat(SFX_PREF, 1f);

        // Asignar visualmente los valores a los sliders si existen en esta escena
        if(musicSlider != null) musicSlider.value = musicVolume;
        if(sfxSlider != null) sfxSlider.value = sfxVolume;

        // Aplicar el volumen al mixer
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
    }

    // Conecta esta función al evento "On Value Changed (Single)" del Slider de Música
    public void SetMusicVolume(float volume)
    {
        // Convertimos el valor lineal (slider) a logarítmico (decibelios que usa Unity)
        float dbVolume = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("MusicVolume", dbVolume);
        PlayerPrefs.SetFloat(MUSIC_PREF, volume);
    }

    // Conecta esta función al evento "On Value Changed (Single)" del Slider de SFX
    public void SetSFXVolume(float volume)
    {
        float dbVolume = Mathf.Log10(Mathf.Clamp(volume, 0.0001f, 1f)) * 20f;
        audioMixer.SetFloat("SFXVolume", dbVolume);
        PlayerPrefs.SetFloat(SFX_PREF, volume);
    }
}
