using UnityEngine;
#if UNITY_6000
using Unity.Cinemachine;
#else
using Cinemachine;
#endif
public class VCamTargetAssigner : MonoBehaviour
{
    [SerializeField] private Transform target = null;
#if UNITY_6000
    private CinemachineCamera virtualCamera;

    private void Start()
    {
        virtualCamera = GetComponent<CinemachineCamera>();

        if (target != null)
        {
            virtualCamera.Follow = target;
            virtualCamera.LookAt = target;
        }
    }
#else
    private CinemachineVirtualCamera virtualCamera;

    private void Start() 
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        
        if (target != null)
        {
            virtualCamera.Follow = target;
            virtualCamera.LookAt = target;
        }
    }
#endif
}