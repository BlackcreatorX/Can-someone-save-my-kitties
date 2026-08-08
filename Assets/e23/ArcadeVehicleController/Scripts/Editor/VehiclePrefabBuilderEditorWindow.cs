using System;
using System.Collections.Generic;
using e23.Editor;
using e23.VehicleController.Audio;
using e23.VehicleController.Examples;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;

namespace e23.VehicleController.Editor
{
    public class VehiclePrefabBuilderEditorWindow : EditorWindow
    {
        [HideInInspector] [SerializeField] private string vehicleName = "newVehicle";
        [HideInInspector] [SerializeField] private GameObject vehicleModel;

        [HideInInspector] [SerializeField] private VehicleType vehicleType;
#if UNITY_6000
        [HideInInspector] [SerializeField] private PhysicsMaterial physicsMaterial;
#else
        [HideInInspector] [SerializeField] private PhysicMaterial physicsMaterial;
#endif
        [HideInInspector] [SerializeField] private bool useBoxCollider = true;

        [HideInInspector] [SerializeField] private string vehicleBodyName = "body";
        [HideInInspector] [SerializeField] private string frontLeftWheelName = "frontWheelLeft";
        [HideInInspector] [SerializeField] private string frontRightWheelName = "frontWheelRight";
        [HideInInspector] [SerializeField] private string backLeftWheelName = "backWheelLeft";
        [HideInInspector] [SerializeField] private string backRightWheelName = "backWheelRight";

        [HideInInspector] [SerializeField] private VehicleBehaviourSettings vehicleSettings;

        [HideInInspector] [SerializeField] private bool addAudioComponent;
        [HideInInspector] [SerializeField] private DriftSettings driftSettings;
        [HideInInspector] [SerializeField] private bool addEffectsComponent;
        [HideInInspector] [SerializeField] private GameObject smokeParticleSystemPrefab;
        [HideInInspector] [SerializeField] private int smokeCount = 1;
        [HideInInspector] [SerializeField] private GameObject trailRendererPrefab;
        [HideInInspector] [SerializeField] private int trailCount = 0;
        [HideInInspector] [SerializeField] private bool addCollisionEffectsComponent;
        [HideInInspector] [SerializeField] private bool addExampleInput;

        [SerializeField] private VehicleBuilderSettings vehicleBuilderSettings;

        private readonly Vector3 _parentOffset = new Vector3(0, 0.75f, 0);                // offset used to position the Parent of the whole vehicle above the ground
        private readonly Quaternion _defaultRotation = Quaternion.Euler(0, 0, 0);         // cache the default rotation 
        private readonly Vector3 _behaviourOffset = new Vector3(0, 0.35f, 0);             // offset of the vehicle behaviour game object
        private readonly Vector3 _physicsSphereCenter = new Vector3(0,0,0);               // var to easily change the physics sphere collider center
        private readonly float _physicsSphereRadius = 0.75f;                                    // size of the physics sphere collider
        private readonly float _physicsSphereRadiusWithBox = 0.24f;                             // size of the physics sphere collider, when using a box collider
        private readonly Vector3 _modelOffset = new Vector3(0, -1.1f, 0);                 // offset to place the model at ground level
        private readonly Vector3 _modelOffsetWithBox = new Vector3(0, -0.61f, 0);         // offset to place the mode at ground level, when using a box collider
        private List<Transform> _wheelTransforms;                                               // list of the wheels

        private BuilderLists _dataObject;
        private SerializedObject _serializedObject;
        private ReorderableList _reorderableAudioDataList;
        private ReorderableList _reorderableCollisionEffectsDataList;
        private Vector2 _scrollPos;

        [MenuItem("Tools/e23/AVC/Prefab Builder", false, 0)]
        private static void Init()
        {
#pragma warning disable 0219
            VehiclePrefabBuilderEditorWindow window = (VehiclePrefabBuilderEditorWindow)GetWindow(typeof(VehiclePrefabBuilderEditorWindow));
#pragma warning restore 0219
        }

        private void OnEnable()
        {
            _dataObject = CreateInstance<BuilderLists>();
            _serializedObject = new SerializedObject(_dataObject);
            FindVehicleBuilderSettings();
            SetupReorderable();
        }

        private void OnDisable() => SavePrefabSetup();

        private void FindVehicleBuilderSettings()
        {
            string settingsType = "t:" + nameof(VehicleBuilderSettings);
            string[] guids = AssetDatabase.FindAssets(settingsType);
            
            if (guids.Length == 0)
            {
                VehicleBuilderSettings newSettings = CreateInstance<VehicleBuilderSettings>();
                AssetDatabase.CreateAsset(newSettings, "Assets/e23/ArcadeVehicleController/Scripts/Editor/VehicleBuilderSettings.asset");
                vehicleBuilderSettings = newSettings;

                SavePrefabSetup();
            }

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                VehicleBuilderSettings vbs = (VehicleBuilderSettings)AssetDatabase.LoadAssetAtPath(path, typeof(VehicleBuilderSettings));

                vehicleBuilderSettings = vbs;
                LoadPrefabSetup();
            }
        }

        private void SetupReorderable()
        {
            _reorderableAudioDataList =
                new ReorderableList(_serializedObject, _serializedObject.FindProperty("audioData"), true, true, true, true)
                    {
                        drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Audio Data"); },
                        drawElementCallback = (rect, index, active, focused) =>
                        {
                            var element = _reorderableAudioDataList.serializedProperty.GetArrayElementAtIndex(index);
                            rect.y += 2;

                            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                        },
                        onAddCallback = addList =>
                        {
                            var index = addList.serializedProperty.arraySize;
                            addList.serializedProperty.arraySize++;
                            addList.index = index;
                            var element = addList.serializedProperty.GetArrayElementAtIndex(index);
                            element.objectReferenceValue = null;
                        }
                    };

            _reorderableCollisionEffectsDataList=
                new ReorderableList(_serializedObject, _serializedObject.FindProperty("collisionData"), true, true, true, true)
                    {
                        drawHeaderCallback = rect => { EditorGUI.LabelField(rect, "Collision Effects Data"); },
                        drawElementCallback = (rect, index, active, focused) =>
                        {
                            var element = _reorderableCollisionEffectsDataList.serializedProperty.GetArrayElementAtIndex(index);
                            rect.y += 2;

                            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), element, GUIContent.none);
                        },
                        onAddCallback = addList =>
                        {
                            var index = addList.serializedProperty.arraySize;
                            addList.serializedProperty.arraySize++;
                            addList.index = index;
                            var element = addList.serializedProperty.GetArrayElementAtIndex(index);
                            element.objectReferenceValue = null;
                        }
                    };
        }
        
        private void OnGUI()
        {
            if (_dataObject == null)
            {
                _dataObject = CreateInstance<BuilderLists>();
                _serializedObject = new SerializedObject(_dataObject); 
                LoadPrefabSetup(); 
                SetupReorderable(); 
            }
            
            if (GUILayout.Button("Build Vehicle", GUILayout.MinHeight(100), GUILayout.Height(50)))
            {
                CreateVehicle();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos, GUILayout.Width(position.width), GUILayout.Height(position.height - 75f));

            EditorBoilerPlate.CreateLabelField("Vehicle", true);
            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Vehicle Name");
            vehicleName = EditorGUILayout.TextField(vehicleName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Vehicle Model");
            vehicleModel = (GameObject)EditorGUILayout.ObjectField(vehicleModel, typeof(GameObject), false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Vehicle Type");
            vehicleType = (VehicleType)EditorGUILayout.EnumPopup(vehicleType);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Physics Material");
#if UNITY_6000
            physicsMaterial = (PhysicsMaterial)EditorGUILayout.ObjectField(physicsMaterial, typeof(PhysicsMaterial), false);
#else
            physicsMaterial = (PhysicMaterial)EditorGUILayout.ObjectField(physicsMaterial, typeof(PhysicMaterial), false);
#endif
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorGUILayout.BeginVertical();
            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Use Box Collider");
            //useBoxCollider = EditorGUILayout.Toggle("", useBoxCollider);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.HelpBox("Toggling use box collider has now been hidden.\nThe prefab will be created using a box collider. If you wish to still use non-box collider setup, please see line #192 and #607 in VehiclePrefabBuilderEditorWindow.cs", MessageType.Info, true);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            if (useBoxCollider == true) { DisplayBodyName(); }

            EditorBoilerPlate.CreateLabelField("Wheel Names", true);
            if (vehicleType == VehicleType.FourWheels)
            {
                DisplayTwoWheels(true);
                DisplayFourWheels();
            }
            else if (vehicleType == VehicleType.ThreeWheels)
            {
                DisplayTwoWheels(false);
                DisplayThreeWheels();

                if (trailCount > 3) { trailCount = 3; }
            }
            else if (vehicleType == VehicleType.TwoWheels)
            {
                DisplayTwoWheels(false);

                if (trailCount > 2) { trailCount = 2; }
            }

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Vehicle Settings");
            vehicleSettings = (VehicleBehaviourSettings)EditorGUILayout.ObjectField(vehicleSettings, typeof(VehicleBehaviourSettings), false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorBoilerPlate.CreateLabelField("Optional Components", true);
            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Add audio component");
            addAudioComponent = EditorGUILayout.Toggle("", addAudioComponent);
            EditorGUILayout.EndHorizontal();
            
            if (addAudioComponent) { DisplayAudioData(); }
            
            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Add drifting effects component");
            addEffectsComponent = EditorGUILayout.Toggle("", addEffectsComponent);
            EditorGUILayout.EndHorizontal();

            if (addEffectsComponent == true) { DisplayEffects(); }
            if (addAudioComponent == true || addEffectsComponent == true) { DisplayDriftSettings(); }

            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Add collision effects component");
            addCollisionEffectsComponent = EditorGUILayout.Toggle("", addCollisionEffectsComponent);
            EditorGUILayout.EndHorizontal();
            
            if (addCollisionEffectsComponent == true) { DisplayCollisionEffects(); }
            
            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Add example input");
            addExampleInput = EditorGUILayout.Toggle("", addExampleInput);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            EditorBoilerPlate.DrawSeparatorLine();
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            // only present to display vehicleBuilderSettings separately
            UnityEditor.Editor editor = UnityEditor.Editor.CreateEditor(this);
            editor.DrawDefaultInspector();

            EditorGUILayout.EndScrollView();
        }

        private void DisplayBodyName()
        {
            EditorBoilerPlate.CreateLabelField("Body Name", true);
            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Vehicle Body");
            vehicleBodyName = EditorGUILayout.TextField(vehicleBodyName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }

        private void DisplayTwoWheels(bool withFour)
        {
            string frontLabel = withFour == true ? "Front Left Wheel" : "Front Wheel";
            string backLabel = withFour == true ? "Back Left Wheel" : "Back Wheel";

            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField(frontLabel);
            frontLeftWheelName = EditorGUILayout.TextField(frontLeftWheelName);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField(backLabel);
            backLeftWheelName = EditorGUILayout.TextField(backLeftWheelName);
            EditorGUILayout.EndHorizontal();
        }

        private void DisplayThreeWheels()
        {
            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Third Wheel");
            backRightWheelName = EditorGUILayout.TextField(backRightWheelName);
            EditorGUILayout.EndHorizontal();
        }

        private void DisplayFourWheels()
        {
            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Front Right Wheel");
            frontRightWheelName = EditorGUILayout.TextField(frontRightWheelName);
            EditorGUILayout.EndHorizontal();        

            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Back Right Wheel");
            backRightWheelName = EditorGUILayout.TextField(backRightWheelName);
            EditorGUILayout.EndHorizontal();
        }

        private void DisplayAudioData()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            _serializedObject.Update();
            _reorderableAudioDataList.DoLayoutList();
            _serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndHorizontal();
        }

        private void DisplayEffects()
        {
            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Smoke Particles Prefab");
            smokeParticleSystemPrefab = (GameObject)EditorGUILayout.ObjectField(smokeParticleSystemPrefab, typeof(GameObject), false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Trail Renderer Prefab");
            trailRendererPrefab = (GameObject)EditorGUILayout.ObjectField(trailRendererPrefab, typeof(GameObject), false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Trail Count");
            trailCount = EditorGUILayout.IntSlider(trailCount, 0, 4);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space();
        }

        private void DisplayCollisionEffects()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            _serializedObject.Update();
            _reorderableCollisionEffectsDataList.DoLayoutList();
            _serializedObject.ApplyModifiedProperties();
            EditorGUILayout.EndHorizontal();
        }

        private void DisplayDriftSettings()
        {
            EditorBoilerPlate.DrawSeparatorLine();
            if (driftSettings == null)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.HelpBox("Drift settings required for Audio and Effects.", MessageType.Info, true);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorBoilerPlate.CreateLabelField("Drift Settings");
            driftSettings = (DriftSettings)EditorGUILayout.ObjectField(driftSettings, typeof(DriftSettings), false);
            EditorGUILayout.EndHorizontal();
            EditorBoilerPlate.DrawSeparatorLine();
            EditorGUILayout.Space();
        }
        
        private void CreateVehicle()
        {
            GameObject vehicleParent = new GameObject(vehicleName);
            UpdateTransform(vehicleParent.transform, null, _parentOffset, _defaultRotation);

            Transform parentTransform = useBoxCollider == true ? CreateRigidbodyGameObject() : vehicleParent.transform;

            GameObject vehicleBehaviour = new GameObject("Vehicle");
            SetupVehicleBehaviour(vehicleBehaviour, parentTransform);

            GameObject sphereRigidBody = new GameObject("SphereRigidBody");
            SetupSphere(sphereRigidBody, parentTransform);
            Rigidbody srb = useBoxCollider == true ? parentTransform.GetComponent<Rigidbody>() : sphereRigidBody.GetComponent<Rigidbody>();

            GameObject newVehicleModel = (GameObject) PrefabUtility.InstantiatePrefab(vehicleModel);
            SetupModel(newVehicleModel, vehicleBehaviour.transform);

            VehicleBehaviour vb = vehicleBehaviour.GetComponent<VehicleBehaviour>();
            TryFindVehicleParts(vb, newVehicleModel, srb);

            if (useBoxCollider == true) { CreateBoxCollider(newVehicleModel); }

            if (addEffectsComponent == true)
            {
                GetWheelTransforms(vb);
                CreateEffectsObjects(vehicleBehaviour.transform);
            }
            
            Selection.activeObject = vehicleBehaviour;
            Undo.RegisterCreatedObjectUndo(vehicleParent, "AVC Vehicle Created");

            Transform CreateRigidbodyGameObject()
            {
                GameObject rbObject = new GameObject("Rigidbody");
                rbObject.transform.SetParent(vehicleParent.transform);

                Rigidbody rb = rbObject.AddComponent<Rigidbody>();
                rb.mass = 100;
#if UNITY_6000
                rb.linearDamping = 1;
                rb.angularDamping = 0;
#else
                rb.drag = 1;
                rb.angularDrag = 0;
#endif
                rb.freezeRotation = true;

                rbObject.AddComponent<CollisionManager>();
                
                return rbObject.transform;
            }
        }

        private void SetupVehicleBehaviour(GameObject obj, Transform parent)
        {
            UpdateTransform(obj.transform, parent, _behaviourOffset, _defaultRotation);

            obj.AddComponent<VehicleBehaviour>();

            if (addAudioComponent == true)
            {
                VehicleAudio va = obj.gameObject.AddComponent<VehicleAudio>();
                if (_dataObject.audioData.Count > 0) { _dataObject.audioData.ForEach(ad => va.AddAudioData(ad)); }
                if (driftSettings != null) { va.AddDriftSettings(driftSettings); }
            }
            
            if (addEffectsComponent == true)
            {
                VehicleEffects ve = obj.AddComponent<VehicleEffects>();
                if (driftSettings != null) { ve.AddDriftSettings(driftSettings); }
            }

            if (addCollisionEffectsComponent == true)
            {
                CollisionEffects ce = obj.AddComponent<CollisionEffects>();
                if (_dataObject.collisionData.Count > 0) { _dataObject.collisionData.ForEach(cd => ce.AddEffectsData(cd)); }
            }
            
            if (addExampleInput == true)
            {
                ExampleInput exInput = obj.AddComponent<ExampleInput>();
                exInput.VehicleBehaviour = obj.GetComponent<VehicleBehaviour>();
            }
        }

        private void SetupSphere(GameObject obj, Transform parent)
        {
            UpdateTransform(obj.transform, parent, Vector3.zero, _defaultRotation);

            if (useBoxCollider == false)
            {
                Rigidbody rb = obj.AddComponent<Rigidbody>();
                rb.mass = 100;
#if UNITY_6000
                rb.linearDamping = 1;
                rb.angularDamping = 0;
#else
                rb.drag = 1;
                rb.angularDrag = 0;
#endif
            }

            SphereCollider sc = obj.AddComponent<SphereCollider>();

            float sphereRadius = useBoxCollider == true ? _physicsSphereRadiusWithBox : _physicsSphereRadius;
            SetupSphereCollider(sc, _physicsSphereCenter, sphereRadius, false, physicsMaterial);
        }

#if UNITY_6000
        private void SetupSphereCollider(SphereCollider sc, Vector3 center, float radius, bool isTrigger, PhysicsMaterial phxMaterial = null)
        {
            sc.center = center;
            sc.radius = radius;
            sc.isTrigger = isTrigger;
            sc.material = phxMaterial;
        }
#else
        private void SetupSphereCollider(SphereCollider sc, Vector3 center, float radius, bool isTrigger, PhysicMaterial phxMaterial = null)
        {
            sc.center = center;
            sc.radius = radius;
            sc.isTrigger = isTrigger;
            sc.material = phxMaterial;
        }
#endif

        private void SetupModel(GameObject obj, Transform parent)
        {
            if (obj == null)
            {
                Debug.Log("No vehicle model prefab has been assigned, only the skeleton will be created");
                return;
            }

            obj.name = vehicleModel.name;

            Vector3 posOffset = useBoxCollider == true ? _modelOffsetWithBox : _modelOffset;
            UpdateTransform(obj.transform, parent, posOffset, _defaultRotation);
        }

        private void TryFindVehicleParts(VehicleBehaviour vehicleBehaviour, GameObject obj, Rigidbody rigidBody)
        {
            if (obj == null) { return; }

            vehicleBehaviour.VehicleModel = obj.transform;
            vehicleBehaviour.VehicleRigidbody = rigidBody;
            vehicleBehaviour.VehicleBody = SearchForPart(obj.transform, vehicleBodyName);
            
            vehicleBehaviour.VehicleWheelCount = vehicleType;
            
            vehicleBehaviour.FrontLeftWheel = SearchForPart(vehicleBehaviour.transform, frontLeftWheelName);
            vehicleBehaviour.FrontRightWheel = SearchForPart(vehicleBehaviour.transform, frontRightWheelName);
            vehicleBehaviour.BackLeftWheel = SearchForPart(vehicleBehaviour.transform, backLeftWheelName);
            vehicleBehaviour.BackRightWheel = SearchForPart(vehicleBehaviour.transform, backRightWheelName);

            if (vehicleSettings != null) { vehicleBehaviour.VehicleSettings = vehicleSettings; }
            else
            {
                Debug.Log("No Vehicle Settings have been added, to create one: Right mouse click in the project window -> create -> e23 -> Vehicle Settings. Then assign the asset.");
            }
        }

        private Transform SearchForPart(Transform parent, string part)
        {
            foreach(Transform t in parent.GetComponentsInChildren<Transform>())
            {
                string name = t.name.ToLower();
                if (name.Contains(part.ToLower()))
                {
                    return t;
                }
            }
            
            return null;
        }

        private void CreateEffectsObjects(Transform parent)
        {
            if (smokeParticleSystemPrefab != null)
            {
                for (int i = 0; i < smokeCount; i++)
                {
                    GameObject newSmokeParticles = (GameObject) PrefabUtility.InstantiatePrefab(smokeParticleSystemPrefab);
                    newSmokeParticles.name = smokeParticleSystemPrefab.name;
                    UpdateTransform(newSmokeParticles.transform, parent, _behaviourOffset, Quaternion.Euler(0.0f, 180.0f, 0.0f));
                }
            }

            if (trailRendererPrefab != null && trailCount > 0)
            {
                GameObject trailParent = new GameObject("TrailsParent");
                UpdateTransform(trailParent.transform, parent, Vector3.zero, _defaultRotation);

                for (int i = trailCount - 1; i >=0; i--)
                {
                    if (_wheelTransforms.Count <= i || _wheelTransforms[i] == null) { Debug.LogWarning($"A wheel was not found, please check the spelling and try again. {i}"); continue; }

                    GameObject newtrailRenderer = (GameObject) PrefabUtility.InstantiatePrefab(trailRendererPrefab);
                    newtrailRenderer.name = trailRendererPrefab.name;
                    Vector3 trailPos = _wheelTransforms[i] != null ? _wheelTransforms[i].position : Vector3.zero;
                    trailPos.y = _modelOffset.y + 0.03f;
                    UpdateTransform(newtrailRenderer.transform, trailParent.transform, trailPos, Quaternion.Euler(90.0f, 0.0f, 0.0f));
                }
            }
        }

        private void UpdateTransform(Transform obj, Transform parent, Vector3 pos, Quaternion rot)
        {
            obj.SetParent(parent);
            obj.localPosition = pos;
            obj.localRotation = rot;
        }

        private void CreateBoxCollider(GameObject model)
        {
            Transform boxParent = SearchForPart(model.transform, vehicleBodyName);

            GameObject boxCollider = new GameObject("VehicleCollider");
            boxCollider.transform.SetParent(boxParent);
            boxCollider.AddComponent<BoxCollider>();

            Bounds bodyBounds = boxParent.GetComponentInChildren<Renderer>().bounds;
            var renderers = boxParent.GetComponentsInChildren<Renderer>();

            foreach (var renderer in renderers)
            {
                bodyBounds.Encapsulate(renderer.bounds);
            }

            boxCollider.transform.localScale = bodyBounds.size;
            
            float yRawPos = bodyBounds.center.y + bodyBounds.max.y;
            float yPos = bodyBounds.size.y < 1f ? yRawPos : yRawPos / Mathf.Floor(yRawPos);
            Vector3 boxPos = new Vector3(boxCollider.transform.localPosition.x, yPos, boxCollider.transform.localPosition.z);
            
            boxCollider.transform.localPosition = boxPos;
        }

        private void GetWheelTransforms(VehicleBehaviour vehicleBehaviour)
        {
            _wheelTransforms = new List<Transform>();

            if (vehicleBehaviour.BackLeftWheel != null) { _wheelTransforms.Add(vehicleBehaviour.BackLeftWheel); }
            if (vehicleBehaviour.BackRightWheel != null) { _wheelTransforms.Add(vehicleBehaviour.BackRightWheel); }
            if (vehicleBehaviour.FrontLeftWheel != null) { _wheelTransforms.Add(vehicleBehaviour.FrontLeftWheel); }
            if (vehicleBehaviour.FrontRightWheel != null) { _wheelTransforms.Add(vehicleBehaviour.FrontRightWheel); }
        }

        private void LoadPrefabSetup()
        {
            vehicleName = vehicleBuilderSettings.VehicleName;
            vehicleModel = vehicleBuilderSettings.VehicleModel;
            vehicleType = vehicleBuilderSettings.VehicleType;
            physicsMaterial = vehicleBuilderSettings.PhysicsMaterial;
            useBoxCollider = true; //vehicleBuilderSettings.UseBoxCollider;
            vehicleBodyName = vehicleBuilderSettings.BodyName;
            frontLeftWheelName = vehicleBuilderSettings.FrontLeftWheelName;
            frontRightWheelName = vehicleBuilderSettings.FrontRightWheelName;
            backLeftWheelName = vehicleBuilderSettings.BackLeftWheelName;
            backRightWheelName = vehicleBuilderSettings.BackRightWheelName;
            vehicleSettings = vehicleBuilderSettings.VehicleSettings;
            addAudioComponent = vehicleBuilderSettings.AddAudioComponent;
            _dataObject.audioData = vehicleBuilderSettings.AudioDatas;
            driftSettings = vehicleBuilderSettings.DriftSettings;
            addEffectsComponent = vehicleBuilderSettings.AddEffectsComponent;
            smokeParticleSystemPrefab = vehicleBuilderSettings.SmokeParticleSystemPrefab;
            smokeCount = vehicleBuilderSettings.SmokeCount;
            trailRendererPrefab = vehicleBuilderSettings.TrailRendererPrefab;
            trailCount = vehicleBuilderSettings.TrailCount;
            addCollisionEffectsComponent = vehicleBuilderSettings.AddCollisionEffectsComponent;
            _dataObject.collisionData = vehicleBuilderSettings.CollisionEffectsDatas;
            addExampleInput = vehicleBuilderSettings.AddExampleInput;
        }

        private void SavePrefabSetup()
        {
            vehicleBuilderSettings.VehicleName = vehicleName;
            vehicleBuilderSettings.VehicleModel = vehicleModel;
            vehicleBuilderSettings.VehicleType = vehicleType;
            vehicleBuilderSettings.PhysicsMaterial = physicsMaterial;
            vehicleBuilderSettings.UseBoxCollider = useBoxCollider;
            vehicleBuilderSettings.BodyName = vehicleBodyName;
            vehicleBuilderSettings.FrontLeftWheelName = frontLeftWheelName;
            vehicleBuilderSettings.FrontRightWheelName = frontRightWheelName;
            vehicleBuilderSettings.BackLeftWheelName = backLeftWheelName;
            vehicleBuilderSettings.BackRightWheelName = backRightWheelName;
            vehicleBuilderSettings.VehicleSettings = vehicleSettings;
            vehicleBuilderSettings.AddAudioComponent = addAudioComponent;
            vehicleBuilderSettings.AudioDatas = _dataObject.audioData;
            vehicleBuilderSettings.DriftSettings = driftSettings;
            vehicleBuilderSettings.AddEffectsComponent = addEffectsComponent;
            vehicleBuilderSettings.SmokeParticleSystemPrefab = smokeParticleSystemPrefab;
            vehicleBuilderSettings.SmokeCount = smokeCount;
            vehicleBuilderSettings.TrailRendererPrefab = trailRendererPrefab;
            vehicleBuilderSettings.TrailCount = trailCount;
            vehicleBuilderSettings.AddCollisionEffectsComponent = addCollisionEffectsComponent;
            vehicleBuilderSettings.CollisionEffectsDatas = _dataObject.collisionData;
            vehicleBuilderSettings.AddExampleInput = addExampleInput;

            if (vehicleBuilderSettings != null)
            {
                EditorUtility.SetDirty(vehicleBuilderSettings);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }
    }

    [Serializable]
    public class BuilderLists : ScriptableObject
    {
        public List<VehicleAudioData> audioData = new List<VehicleAudioData>();
        public List<CollisionEffectsData> collisionData = new List<CollisionEffectsData>();
    }
}