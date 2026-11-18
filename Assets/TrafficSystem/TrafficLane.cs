using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficLane : MonoBehaviour
{
    [Header("Puntos de la Calle")]
    [Tooltip("Punto 1: Donde aparecen los autos")]
    public Transform startPoint;
    [Tooltip("Punto 2: Donde desaparecen y se reinician")]
    public Transform endPoint;

    [Header("Configuración del Tráfico")]
    [Tooltip("Arrastra aquí tus prefabs de autos. Puedes poner varios para que haya variedad.")]
    public GameObject[] carPrefabs;
    [Tooltip("Cuántos autos habrá circulando al mismo tiempo en esta calle")]
    public int numberOfCars = 5;
    [Tooltip("Velocidad de movimiento de los autos")]
    public float speed = 10f;

    [Header("Destrucción")]
    [Tooltip("Si un prefab de auto no tiene Destructible, se le agrega uno con valores por defecto (modo Launch).")]
    public bool makeCarsDestructible = true;
    [Tooltip("Si es true, un auto que sale volando vuelve a su ruta cuando se detiene. Si es false, desaparece y se repone uno nuevo en el Punto 1.")]
    public bool rejoinRoute = true;
    [Tooltip("Solo con Rejoin Route desactivado: segundos antes de reponer un auto destruido. -1 para no reponer.")]
    public float respawnDelay = 3f;

    [Header("Golpe leve")]
    [Tooltip("Segundos que el auto se queda frenado tras un golpe leve del jugador.")]
    public float hitStopDuration = 1.5f;
    [Tooltip("Segundos que tarda en frenar.")]
    public float brakeTime = 0.2f;
    [Tooltip("Segundos que tarda en volver a la velocidad normal.")]
    public float accelerateTime = 0.8f;

    [Header("Reincorporación tras salir volando")]
    [Tooltip("Velocidad (lineal y angular) por debajo de la cual se considera que el auto se detuvo.")]
    public float settleSpeed = 0.3f;
    [Tooltip("Segundos que debe estar quieto antes de volver a la ruta.")]
    public float settleDuration = 1f;
    [Tooltip("Tiempo máximo que puede pasar volando antes de forzar su regreso a la ruta.")]
    public float maxLaunchedTime = 12f;
    [Tooltip("No vuelve a la ruta si el jugador está más cerca que esta distancia del punto de regreso.")]
    public float rejoinClearance = 8f;
    [Tooltip("Si cae más de esta distancia por debajo de la calle, regresa a la ruta de inmediato.")]
    public float fallLimit = 30f;
    [Tooltip("Segundos que tarda en enderezarse y deslizarse hasta la ruta, mirando en la dirección del tráfico.")]
    public float rejoinDuration = 0.6f;

    private class TrafficCar
    {
        public Rigidbody rb;
        public Destructible destructible;
        public bool canRejoin;
        public float speedFactor = 1f;
        public float pausedUntil;
        public float launchedTime;
        public float settledTime;

        // Transición de regreso a la ruta
        public Vector3 rejoinFromPos, rejoinToPos;
        public Quaternion rejoinFromRot, rejoinToRot;
        public float rejoinProgress;
    }

    private readonly List<TrafficCar> activeCars = new List<TrafficCar>();
    private readonly List<TrafficCar> launchedCars = new List<TrafficCar>();
    private readonly List<TrafficCar> rejoiningCars = new List<TrafficCar>();
    private Vector3 moveDirection;
    private Quaternion laneRotation;
    private float laneDistance;
    private Transform player;

    void Start()
    {
        if (startPoint == null || endPoint == null || carPrefabs.Length == 0 || numberOfCars <= 0)
        {
            Debug.LogWarning("Faltan referencias en el TrafficLane o el número de autos es 0.");
            return;
        }

        // El jugador se identifica por su VehicleVelocityTracker
        VehicleVelocityTracker tracker = FindFirstObjectByType<VehicleVelocityTracker>();
        if (tracker != null) { player = tracker.transform; }

        // 1. Calcular la dirección y la distancia total de la calle
        moveDirection = (endPoint.position - startPoint.position).normalized;
        laneRotation = Quaternion.LookRotation(moveDirection);
        laneDistance = Vector3.Distance(startPoint.position, endPoint.position);

        // 2. Calcular la separación exacta entre cada auto
        float spacing = laneDistance / numberOfCars;

        // 3. Generar los autos y distribuirlos a lo largo de la calle
        for (int i = 0; i < numberOfCars; i++)
        {
            // Calcular su posición inicial para que la calle empiece ya llena de tráfico
            Vector3 spawnPos = startPoint.position + (moveDirection * (spacing * i));
            SpawnCar(spawnPos);
        }
    }

    /// <summary>
    /// Crea un auto en la posición dada, lo prepara para moverse por física (kinematic)
    /// y lo conecta con su Destructible.
    /// </summary>
    private void SpawnCar(Vector3 position)
    {
        // Elegir un modelo de auto al azar de tu lista
        GameObject prefab = carPrefabs[Random.Range(0, carPrefabs.Length)];

        // Instanciar el auto mirando hacia el Punto 2, agrupado dentro de este objeto
        GameObject newCar = Instantiate(prefab, position, laneRotation, transform);

        // Rigidbody kinematic: así Unity calcula bien la velocidad del auto en los choques
        Rigidbody rb = newCar.GetComponent<Rigidbody>();
        if (rb == null) { rb = newCar.AddComponent<Rigidbody>(); }
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        Destructible destructible = newCar.GetComponent<Destructible>();
        if (destructible == null && makeCarsDestructible)
        {
            destructible = newCar.AddComponent<Destructible>();
        }

        TrafficCar car = new TrafficCar { rb = rb, destructible = destructible };

        if (destructible != null)
        {
            // Solo un auto en modo Launch puede volver a la ruta; en Replace se destruye
            car.canRejoin = rejoinRoute && destructible.Mode == Destructible.BreakMode.Launch;
            destructible.DestroyAfterBreak = !car.canRejoin;

            if (destructible.onBroken != null) { destructible.onBroken.AddListener(() => OnCarBroken(car)); }
            if (destructible.onSoftHit != null) { destructible.onSoftHit.AddListener(() => OnCarSoftHit(car)); }
        }

        activeCars.Add(car);
    }

    // ---------- Golpe leve: frena un momento y continúa ----------

    private void OnCarSoftHit(TrafficCar car)
    {
        car.pausedUntil = Time.time + hitStopDuration;
    }

    // ---------- Golpe fuerte: sale volando ----------

    private void OnCarBroken(TrafficCar car)
    {
        // Deja de moverlo por script: a partir de aquí lo controla la física
        activeCars.Remove(car);

        if (launchedCars.Contains(car)) { return; }

        if (car.canRejoin)
        {
            car.launchedTime = Time.time;
            car.settledTime = 0f;
            launchedCars.Add(car);
        }
        else if (respawnDelay >= 0f)
        {
            StartCoroutine(RespawnAfterDelay());
        }
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnDelay);

        SpawnCar(startPoint.position);
    }

    // Los autos se mueven en FixedUpdate porque son cuerpos kinematic
    void FixedUpdate()
    {
        float dt = Time.fixedDeltaTime;

        UpdateActiveCars(dt);
        UpdateLaunchedCars(dt);
        UpdateRejoiningCars(dt);
    }

    private void UpdateActiveCars(float dt)
    {
        for (int i = activeCars.Count - 1; i >= 0; i--)
        {
            TrafficCar car = activeCars[i];
            if (car.rb == null) { activeCars.RemoveAt(i); continue; }

            // Si la física ya tomó el control del auto (salió volando), no se mueve por script,
            // aunque no haya llegado el evento onBroken. Moverlo con MovePosition lo arrastraría
            // por la ruta mientras da vueltas.
            if (car.rb.isKinematic == false)
            {
                OnCarBroken(car);
                continue;
            }

            // Frena suave si está en pausa por un golpe leve, y acelera suave al reanudar
            bool paused = Time.time < car.pausedUntil;
            float target = paused ? 0f : 1f;
            float rate = 1f / Mathf.Max(0.01f, paused ? brakeTime : accelerateTime);
            car.speedFactor = Mathf.MoveTowards(car.speedFactor, target, rate * dt);

            if (car.speedFactor > 0f)
            {
                // Mover el auto en línea recta
                car.rb.MovePosition(car.rb.position + moveDirection * speed * car.speedFactor * dt);
            }

            // Qué tan lejos del inicio va, medido a lo largo de la calle
            float currentDist = Vector3.Dot(car.rb.position - startPoint.position, moveDirection);

            // Si el auto ya recorrió toda la distancia (llegó al Punto 2)
            if (currentDist >= laneDistance)
            {
                // Lo teletransportamos de vuelta al Punto 1
                car.rb.position = startPoint.position;
                car.rb.transform.position = startPoint.position;
            }
        }
    }

    // ---------- Reincorporación de autos lanzados ----------

    private void UpdateLaunchedCars(float dt)
    {
        for (int i = launchedCars.Count - 1; i >= 0; i--)
        {
            TrafficCar car = launchedCars[i];
            if (car.rb == null) { launchedCars.RemoveAt(i); continue; }

            bool still = car.rb.linearVelocity.magnitude < settleSpeed
                      && car.rb.angularVelocity.magnitude < settleSpeed;
            car.settledTime = still ? car.settledTime + dt : 0f;

            bool settled = car.settledTime >= settleDuration;
            bool timedOut = Time.time - car.launchedTime > maxLaunchedTime;
            bool fellOff = car.rb.position.y < startPoint.position.y - fallLimit;

            if (settled == false && timedOut == false && fellOff == false) { continue; }

            Vector3 rejoinPoint = GetRejoinPoint(car.rb.position);
            if (PlayerIsClear(rejoinPoint) == false) { continue; }

            BeginRejoin(car, rejoinPoint);
            launchedCars.RemoveAt(i);
        }
    }

    /// <summary>
    /// Punto de la ruta más cercano a la posición dada (proyección sobre la línea de la calle).
    /// </summary>
    private Vector3 GetRejoinPoint(Vector3 position)
    {
        float t = Vector3.Dot(position - startPoint.position, moveDirection);
        t = Mathf.Clamp(t, 0f, laneDistance * 0.95f);
        return startPoint.position + moveDirection * t;
    }

    private bool PlayerIsClear(Vector3 point)
    {
        return player == null || Vector3.Distance(player.position, point) > rejoinClearance;
    }

    /// <summary>
    /// Congela el auto y empieza a moverlo suavemente hasta la ruta, enderezándolo
    /// y orientándolo en la dirección del tráfico.
    /// </summary>
    private void BeginRejoin(TrafficCar car, Vector3 point)
    {
        Rigidbody rb = car.rb;

        // Primero se frena y luego vuelve a kinematic (asignar velocidad a un kinematic da aviso)
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;

        car.rejoinFromPos = rb.position;
        car.rejoinFromRot = rb.rotation;
        car.rejoinToPos = point;
        car.rejoinToRot = laneRotation;
        car.rejoinProgress = 0f;

        rejoiningCars.Add(car);
    }

    private void UpdateRejoiningCars(float dt)
    {
        for (int i = rejoiningCars.Count - 1; i >= 0; i--)
        {
            TrafficCar car = rejoiningCars[i];
            if (car.rb == null) { rejoiningCars.RemoveAt(i); continue; }

            car.rejoinProgress += dt / Mathf.Max(0.01f, rejoinDuration);
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(car.rejoinProgress));

            car.rb.MovePosition(Vector3.Lerp(car.rejoinFromPos, car.rejoinToPos, t));
            car.rb.MoveRotation(Quaternion.Slerp(car.rejoinFromRot, car.rejoinToRot, t));

            if (car.rejoinProgress < 1f) { continue; }

            // Ya está en la ruta, derecho y mirando hacia el Punto 2: vuelve a ser tráfico normal
            if (car.destructible != null) { car.destructible.ResetState(); }

            car.speedFactor = 0f;
            car.pausedUntil = 0f;
            activeCars.Add(car);
            rejoiningCars.RemoveAt(i);
        }
    }

    // Esta función dibuja líneas visuales en el Editor de Unity (No se ven en el juego final)
    private void OnDrawGizmos()
    {
        if (startPoint != null && endPoint != null)
        {
            // Línea cyan para la ruta
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(startPoint.position, endPoint.position);

            // Esfera verde para el inicio (Punto 1)
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(startPoint.position, 1f);

            // Esfera roja para el final (Punto 2)
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(endPoint.position, 1f);
        }
    }
}