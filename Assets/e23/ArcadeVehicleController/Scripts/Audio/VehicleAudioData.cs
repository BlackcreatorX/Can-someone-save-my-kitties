using UnityEngine;
using UnityEngine.Audio;

namespace e23.VehicleController.Audio
{
    [CreateAssetMenu(fileName = nameof(VehicleAudioData), menuName = "e23/AVC/Audio Data", order = 4)]
    public class VehicleAudioData : ScriptableObject
    {
#pragma warning disable 0649
        [SerializeField] private string audioID = "";
        [SerializeField] private AudioType audioType;
        [SerializeField] private AudioClip audioClip = null;
        [SerializeField] private bool playOnAwake = false;
        [SerializeField] private bool loop = true;
        [SerializeField] private AudioMixerGroup audioMixerGroup = null;
        [SerializeField] private LayerMask layerMask;
#pragma warning restore 0649
        [Header("AudioSource Settings")]
        [SerializeField] [Range(0, 256)] private int priority = 128;
        [SerializeField] [Range(0f, 1f)] private float volume = 1f;
        [SerializeField] [Range(-3f, 3f)] private float pitch = 1f;
        [SerializeField] [Range(-1f, 1f)] private float stereoPan = 0f;
        [SerializeField] [Range(0f, 1f)] private float spatialBlend = 0f;
        [SerializeField] [Range(0f, 1.1f)] private float reverbZoneMix = 1f;

        [Header("3D Sound Settings")]
        [SerializeField] [Range(0f, 5f)] private float dopplerLevel = 1f;
        [SerializeField] [Range(0f, 360f)] private float spread = 0f;
        [SerializeField] private AudioRolloffMode audioRolloffMode = AudioRolloffMode.Logarithmic;
        [SerializeField] private float minDistance = 1f;
        [SerializeField] private float maxDistance = 500f;

        public string AudioID => audioID;
        public AudioType AudioType => audioType;
        public AudioClip AudioClip => audioClip;
        public bool PlayOnAwake => playOnAwake;
        public bool Loop => loop;
        public AudioMixerGroup AudioMixerGroup => audioMixerGroup;
        public LayerMask LayerMask => layerMask;
        public int Priority => priority;
        public float Volume => volume;
        public float Pitch => pitch;
        public float StereoPan => stereoPan;
        public float SpatialBlend => spatialBlend;
        public float ReverbZoneMix => reverbZoneMix;
        
        public float DopplerLevel => dopplerLevel;
        public float Spread => spread;
        public AudioRolloffMode AudioRollOff => audioRolloffMode;
        public float MinDistance => minDistance;
        public float MaxDistance => maxDistance;
    }
}