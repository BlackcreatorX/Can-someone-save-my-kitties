using System.Collections.Generic;
using e23.VehicleController.Audio;

using UnityEngine;

namespace e23.VehicleController.Editor
{
    public class VehicleBuilderSettings : ScriptableObject
    {
        [HideInInspector] [SerializeField] private string vehicleName;
        [HideInInspector] [SerializeField] private GameObject vehicleModel;
        [HideInInspector] [SerializeField] private VehicleType vehicleType;
#if UNITY_6000
        [HideInInspector] [SerializeField] private PhysicsMaterial physicsMaterial;
#else
        [HideInInspector] [SerializeField] private PhysicMaterial physicsMaterial;
#endif
        [HideInInspector] [SerializeField] private bool useBoxCollider;
        [HideInInspector] [SerializeField] private string bodyName;
        [HideInInspector] [SerializeField] private string frontLeftWheelName;
        [HideInInspector] [SerializeField] private string frontRightWheelName;
        [HideInInspector] [SerializeField] private string backLeftWheelName;
        [HideInInspector] [SerializeField] private string backRightWheelName;
        [HideInInspector] [SerializeField] private VehicleBehaviourSettings vehicleSettings;
        [HideInInspector] [SerializeField] private bool addAudioComponent;
        [HideInInspector] [SerializeField] private List<VehicleAudioData> audioDatas;
        [HideInInspector] [SerializeField] private DriftSettings driftSettings;
        [HideInInspector] [SerializeField] private bool addEffectsComponent;
        [HideInInspector] [SerializeField] private GameObject smokeParticleSystemPrefab;
        [HideInInspector] [SerializeField] private int smokeCount;
        [HideInInspector] [SerializeField] private GameObject trailRendererPrefab;
        [HideInInspector] [SerializeField] private int trailCount;
        [HideInInspector] [SerializeField] private bool addCollisionEffectsComponent;
        [HideInInspector] [SerializeField] private List<CollisionEffectsData> collisionEffectsDatas;
        [HideInInspector] [SerializeField] private bool addExampleInput;

        public string VehicleName
        { get => vehicleName; set => vehicleName = value; }
        
        public GameObject VehicleModel 
        { get => vehicleModel; set => vehicleModel = value; }
        
        public VehicleType VehicleType 
        {  get => vehicleType; set => vehicleType = value; }

#if UNITY_6000
        public PhysicsMaterial PhysicsMaterial 
        {  get => physicsMaterial; set => physicsMaterial = value; }
#else
        public PhysicMaterial PhysicsMaterial 
        {  get => physicsMaterial; set => physicsMaterial = value; }
#endif
        public bool UseBoxCollider 
        { get => useBoxCollider; set => useBoxCollider = value; }
        
        public string BodyName 
        { get => bodyName; set => bodyName = value; }
        
        public string FrontLeftWheelName 
        { get => frontLeftWheelName; set => frontLeftWheelName = value; }
        
        public string FrontRightWheelName 
        { get => frontRightWheelName; set => frontRightWheelName = value; }
        
        public string BackLeftWheelName 
        { get => backLeftWheelName; set => backLeftWheelName = value; }
        
        public string BackRightWheelName 
        { get => backRightWheelName; set => backRightWheelName = value; }
        
        public VehicleBehaviourSettings VehicleSettings 
        { get => vehicleSettings; set => vehicleSettings = value; }
        
        public bool AddAudioComponent 
        { get => addAudioComponent; set => addAudioComponent = value; }
        
        public List<VehicleAudioData> AudioDatas
        { get => audioDatas; set => audioDatas = value; }
        
        public DriftSettings DriftSettings 
        { get => driftSettings; set => driftSettings = value; }
        
        public bool AddEffectsComponent 
        { get => addEffectsComponent; set => addEffectsComponent = value; }
        
        public GameObject SmokeParticleSystemPrefab 
        { get => smokeParticleSystemPrefab; set => smokeParticleSystemPrefab = value; }
        
        public int SmokeCount 
        { get => smokeCount; set => smokeCount = value; }
        
        public GameObject TrailRendererPrefab 
        { get => trailRendererPrefab; set => trailRendererPrefab = value; }
        
        public int TrailCount 
        { get => trailCount; set => trailCount = value; }
        
        public bool AddCollisionEffectsComponent 
        { get => addCollisionEffectsComponent; set => addCollisionEffectsComponent = value; }
        
        public List<CollisionEffectsData> CollisionEffectsDatas
        { get => collisionEffectsDatas; set => collisionEffectsDatas = value; }
        
        public bool AddExampleInput 
        { get => addExampleInput; set => addExampleInput = value; }
    }
}