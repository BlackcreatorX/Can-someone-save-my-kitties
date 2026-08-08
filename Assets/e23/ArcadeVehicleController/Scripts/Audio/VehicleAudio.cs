using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace e23.VehicleController.Audio
{
    [RequireComponent(typeof(VehicleBehaviour))]
    public class VehicleAudio : MonoBehaviour
    {
        [SerializeField] private List<VehicleAudioData> audioData = null;
        [SerializeField] private DriftSettings driftSettings;
        
        private VehicleBehaviour _vehicleBehaviour;
        private Dictionary<AudioType, AudioSource> _audioSources;
        private Dictionary<string, AudioSource> _customAudioSources;
        
        private float _defaultPitch;
        private bool _shouldPlayDriftSfx;
        private WaitForSeconds _waitForSeconds;
        private CollisionManager _collisionManager;
        
        private void Awake()
        {
            GetRequiredComponents();
            AddAudioSources();
            RegisterActions(true);
        }

        private void GetRequiredComponents()
        {
            _vehicleBehaviour = GetComponent<VehicleBehaviour>();
            _collisionManager = GetComponentInParent<CollisionManager>();
        }

        private void RegisterActions(bool register)
        {
            _vehicleBehaviour.OnStartEngine -= PlayEngine;
            if (_collisionManager != null)
            {
                _collisionManager.OnVehicleCollisionEnter -= PlayClipOnCollision;
            }

            if (register == false) { return; }

            _vehicleBehaviour.OnStartEngine += PlayEngine;
            if (_collisionManager != null)
            {
                _collisionManager.OnVehicleCollisionEnter += PlayClipOnCollision;
            }
        }

        private void PlayEngine(bool enable)
        {
            if (enable)
            {
                if (_audioSources.ContainsKey(AudioType.EngineStart))
                {
                    _waitForSeconds = new WaitForSeconds(GetClip(AudioType.EngineStart).length);
                    PlayClip(AudioType.EngineStart);
                    StartCoroutine(PlayEngineClipAfterDelay(AudioType.EngineRunning));
                    return;
                }

                PlayClip(AudioType.EngineRunning);
                _vehicleBehaviour.InvokeEngineStarted(true);
            }
            else
            {
                _audioSources[AudioType.EngineRunning].Stop();
                _vehicleBehaviour.InvokeEngineStarted(false);
                if (_audioSources.ContainsKey(AudioType.EngineStart)) { PlayClip(AudioType.EngineOff); }
            }
        }

        private void AddAudioSources()
        {
            _audioSources ??= new Dictionary<AudioType, AudioSource>();

            audioData.ForEach(data =>
            {
                if (data.AudioType == AudioType.Custom)
                {
                    _customAudioSources ??= new Dictionary<string, AudioSource>();
                    _customAudioSources.Add(data.AudioID, gameObject.AddComponent<AudioSource>());
                    SetupAudioSource(_customAudioSources[data.AudioID], data);
                }
                else
                {
                    _audioSources.Add(data.AudioType, gameObject.AddComponent<AudioSource>()); 
                    SetupAudioSource(_audioSources[data.AudioType], data);
                }
                
                if (data.AudioType == AudioType.EngineRunning) { _defaultPitch = data.Pitch; }
            });
        }

        private void SetupAudioSource(AudioSource audioSource, VehicleAudioData data)
        {
            audioSource.clip = data.AudioClip;
            audioSource.outputAudioMixerGroup = data.AudioMixerGroup;
            audioSource.playOnAwake = data.PlayOnAwake;
            audioSource.loop = data.Loop;
            audioSource.priority = data.Priority;
            audioSource.volume = data.Volume;
            audioSource.pitch = data.Pitch;
            audioSource.panStereo = data.StereoPan;
            audioSource.spatialBlend = data.SpatialBlend;
            audioSource.reverbZoneMix = data.ReverbZoneMix;

            audioSource.dopplerLevel = data.DopplerLevel;
            audioSource.spread = data.Spread;
            audioSource.rolloffMode = data.AudioRollOff;
            audioSource.minDistance = data.MinDistance;
            audioSource.maxDistance = data.MaxDistance;
        }
        
        private void Update()
        {
            if (_vehicleBehaviour.EngineRunning == true)
            {
                float extra = 0f;
                if (_vehicleBehaviour.OnGround == false) { extra = Mathf.Lerp(extra, 3, Time.deltaTime * 50f); }
                else if (extra > 0f) { extra = Mathf.Lerp(extra, 0f, Time.deltaTime * 12f); }
                
                float normalisedSpeed = _vehicleBehaviour.CurrentSpeed / _vehicleBehaviour.MaxSpeed;
                _audioSources[AudioType.EngineRunning].pitch = _defaultPitch + normalisedSpeed + extra;
            }

            if (_audioSources.ContainsKey(AudioType.Drift) == false) { return; }
            
            DriftCheck();
            
            if (_shouldPlayDriftSfx && _audioSources[AudioType.Drift].isPlaying == false) 
            { PlayClip(AudioType.Drift); }
            else if (_shouldPlayDriftSfx == false && _audioSources[AudioType.Drift].isPlaying == true)
            { StopClip(AudioType.Drift); }
        }

        private void DriftCheck()
        {
            _shouldPlayDriftSfx = _vehicleBehaviour.OnGround && 
                                  _vehicleBehaviour.GetVehicleVelocitySqrMagnitude > (_vehicleBehaviour.MaxSpeed / driftSettings.SkidSpeed) && 
                                  (Vector3.Angle(_vehicleBehaviour.GetVehicleVelocity, _vehicleBehaviour.VehicleModel.forward) > driftSettings.SkidAngle);
        }

        private void PlayClip(AudioType audioType)
        {
            if (_audioSources.ContainsKey(audioType) == false)
            {
                Debug.LogWarning($"Attempted to play vehicle audio of type {audioType}, no clip found.", gameObject);
                return;
            }
            _audioSources[audioType].Play();
        }

        private void PlayClip(string audioID)
        {
            if (_customAudioSources.ContainsKey(audioID) == false)
            {
                Debug.LogWarning($"Attempted to play vehicle audio of type {audioID}, no clip found.", gameObject);
                return;
            }
            _customAudioSources[audioID].Play();
        }

        private IEnumerator PlayEngineClipAfterDelay(AudioType audioType)
        {
            yield return _waitForSeconds;
            _vehicleBehaviour.InvokeEngineStarted(true);
            PlayClip(audioType);
        }

        private void PlayClipOnCollision(Collision collision)
        {
            if (collision.relativeVelocity.magnitude < 15f)
            { return; }
            
            float velocityDot = Vector3.Dot(collision.GetContact(0).normal, collision.relativeVelocity);
            float upDot = Vector3.Dot(collision.GetContact(0).normal, Vector3.up);
            
            if ((collision.relativeVelocity.magnitude <= 0.1f && upDot == 1f) || (velocityDot >= -0.21f && velocityDot <= 0.21f && upDot != 0f))
            {
                return;
            }
            
            if (collision.gameObject.TryGetComponent(out AudioTag audioTag))
            {
                if (_customAudioSources == null || _customAudioSources.ContainsKey(audioTag.ID) == false)
                {
                    Debug.LogWarning($"Audio clip with ID {audioTag.ID} has not been added to VehicleAudio", gameObject);
                }
                else
                {
                    PlayClip(audioTag.ID);
                    return;
                }
            }

            PlayClip(AudioType.Collision);
        }
        
        private void StopClip(AudioType audioType) => _audioSources[audioType].Stop();

        private AudioClip GetClip(AudioType audioType)
        {
            foreach (VehicleAudioData data in audioData)
            {
                if (data.AudioType == audioType)
                {
                    return data.AudioClip;
                }
            }

            Debug.LogWarning($"Audio clip for {audioType} not found in list of Audio Data, please make sure it's assigned.", gameObject);
            return null;
        }
        
        private AudioSource GetAudioSource(string id)
        {
            foreach (VehicleAudioData data in audioData)
            {
                if (string.Compare("", id) == 0)
                {
                    return _audioSources[data.AudioType];
                }
            }

            Debug.LogWarning($"Audio ID {id} not found in list of Audio Data, please make sure it's assigned and named correctly", gameObject);
            return null;
        }

        public void AddAudioData(VehicleAudioData newData)
        {
            if (audioData == null) { audioData = new List<VehicleAudioData>(); }
            audioData.Add(newData);
        }

        public void AddDriftSettings(DriftSettings newSettings) => driftSettings = newSettings;
    }
}