using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using e23.VehicleController;

/// <summary>
/// Objeto del entorno que se rompe o sale volando cuando el jugador lo golpea
/// a suficiente velocidad. Si el golpe es más lento, el objeto se comporta
/// como un obstáculo normal y avisa con onSoftHit (útil para que un auto de
/// tráfico frene un momento).
///
/// Requisitos:
///  - Un Collider (no trigger) en este objeto o en sus hijos.
///  - El objeto NO debe estar marcado como Static si usa el modo Launch.
///  - El jugador debe tener VehicleVelocityTracker en el GameObject de su Rigidbody
///    (el que está asignado en VehicleBehaviour.VehicleRigidbody). Identifica al
///    jugador y permite devolverle el impulso al romper algo.
///  - Pon los objetos destruibles en una capa que NO esté en el groundMask del
///    VehicleBehaviourSettings, o el auto los tomará como suelo y se inclinará sobre ellos.
/// </summary>
public class Destructible : MonoBehaviour
{
    public enum BreakMode
    {
        Launch,   // El mismo objeto sale volando
        Replace   // Se reemplaza por un prefab roto con fragmentos (Rigidbodies)
    }

    [Header("Umbral de impacto")]
    [Tooltip("Velocidad relativa mínima para que el objeto se rompa.")]
    [SerializeField] private float minImpactSpeed = 12f;
    [Tooltip("Por debajo de esta velocidad el contacto se ignora (roces, objetos en reposo). Entre este valor y Min Impact Speed se dispara onSoftHit.")]
    [SerializeField] private float minSoftHitSpeed = 1.5f;

    [Header("Modo de destrucción")]
    [SerializeField] private BreakMode mode = BreakMode.Launch;
    [Tooltip("Solo en modo Replace: prefab con los fragmentos (cada uno con Rigidbody).")]
    [SerializeField] private GameObject brokenPrefab;
    [Tooltip("Solo en modo Launch: si es false, el objeto lanzado no se destruye (lo maneja otro script, como TrafficLane, que puede reiniciarlo con ResetState).")]
    [SerializeField] private bool destroyAfterBreak = true;

    [Header("Fuerza")]
    [Tooltip("Multiplicador de la velocidad de impacto que se aplica al objeto.")]
    [SerializeField] private float launchForce = 0.6f;
    [Tooltip("Qué tanto sale hacia arriba (0 = recto, 1 = 45 grados).")]
    [SerializeField] private float upwardBias = 0.5f;
    [SerializeField] private float spin = 6f;
    [Tooltip("Masa del objeto en modo Launch (solo si no tiene Rigidbody propio).")]
    [SerializeField] private float objectMass = 50f;

    [Header("Efectos")]
    [Tooltip("Partículas al romperse. Puede llevar AutoDestroyEffect.")]
    [SerializeField] private GameObject breakEffect;
    [SerializeField] private AudioClip breakSound;
    [Tooltip("Segundos que duran los restos antes de desaparecer.")]
    [SerializeField] private float debrisLifetime = 4f;

    [Header("Jugador")]
    [Tooltip("Qué porcentaje de su velocidad conserva el auto al romper el objeto.")]
    [Range(0f, 1f)]
    [SerializeField] private float velocityRetention = 0.85f;

    [Header("Eventos")]
    [Tooltip("Se dispara al romperse. Útil para puntos, o para sacar un auto de tráfico de su ruta.")]
    public UnityEvent onBroken;
    [Tooltip("Se dispara cuando el jugador golpea el objeto sin romperlo.")]
    public UnityEvent onSoftHit;

    private const float SoftHitCooldown = 0.5f;

    private bool _broken;
    private float _lastSoftHitTime = -10f;
    private Collider _ignoredPlayerCollider;

    public BreakMode Mode => mode;

    public bool DestroyAfterBreak
    {
        get => destroyAfterBreak;
        set => destroyAfterBreak = value;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (_broken) { return; }
        if (IsPlayer(collision) == false) { return; }

        float impactSpeed = collision.relativeVelocity.magnitude;

        if (impactSpeed < minImpactSpeed)
        {
            if (impactSpeed >= minSoftHitSpeed && Time.time - _lastSoftHitTime > SoftHitCooldown)
            {
                _lastSoftHitTime = Time.time;
                onSoftHit?.Invoke();
            }
            return;
        }

        Break(collision, impactSpeed);
    }

    private static bool IsPlayer(Collision collision)
    {
        Collider other = collision.collider;

        if (other.GetComponentInParent<VehicleBehaviour>() != null) { return true; }
        if (other.GetComponentInParent<VehicleBehaviourLinker>() != null) { return true; }

        // Con la física de esfera, el Rigidbody está en un objeto aparte del VehicleBehaviour,
        // así que los dos chequeos de arriba fallan. El tracker identifica al jugador en ese caso.
        return collision.rigidbody != null
            && collision.rigidbody.GetComponent<VehicleVelocityTracker>() != null;
    }

    private void Break(Collision collision, float impactSpeed)
    {
        _broken = true;

        ContactPoint contact = collision.GetContact(0);
        Rigidbody playerRb = collision.rigidbody;
        VehicleVelocityTracker tracker = playerRb != null ? playerRb.GetComponent<VehicleVelocityTracker>() : null;

        // Dirección del golpe: hacia donde iba el auto
        Vector3 playerVelocity = tracker != null ? tracker.LastVelocity : -collision.relativeVelocity;
        Vector3 hitDirection = playerVelocity.sqrMagnitude > 0.01f
            ? playerVelocity.normalized
            : (transform.position - collision.transform.position).normalized;

        Vector3 launchDirection = (hitDirection + Vector3.up * upwardBias).normalized;

        // Que los restos no frenen de nuevo al auto
        IgnorePlayerCollisions(collision.collider);

        // Que el auto no pierda el impulso
        if (tracker != null && playerRb != null)
        {
            playerRb.linearVelocity = tracker.LastVelocity * velocityRetention;
        }

        SpawnFeedback(contact);
        onBroken?.Invoke();

        if (mode == BreakMode.Replace && brokenPrefab != null)
        {
            BreakIntoFragments(launchDirection, impactSpeed);
        }
        else
        {
            LaunchSelf(launchDirection, impactSpeed);
        }
    }

    private void IgnorePlayerCollisions(Collider playerCollider)
    {
        _ignoredPlayerCollider = playerCollider;

        Collider[] myColliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < myColliders.Length; i++)
        {
            Physics.IgnoreCollision(myColliders[i], playerCollider, true);
        }
    }

    /// <summary>
    /// Deja el objeto listo para romperse de nuevo. Solo tiene sentido en modo Launch
    /// con Destroy After Break desactivado: quien lo reubique debe volver a poner su
    /// Rigidbody en kinematic si lo necesita.
    /// </summary>
    public void ResetState()
    {
        if (_ignoredPlayerCollider != null)
        {
            Collider[] myColliders = GetComponentsInChildren<Collider>();
            for (int i = 0; i < myColliders.Length; i++)
            {
                Physics.IgnoreCollision(myColliders[i], _ignoredPlayerCollider, false);
            }
        }

        _ignoredPlayerCollider = null;
        _broken = false;
    }

    private void SpawnFeedback(ContactPoint contact)
    {
        if (breakEffect != null)
        {
            Quaternion rotation = contact.normal.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(contact.normal)
                : Quaternion.identity;
            Instantiate(breakEffect, contact.point, rotation);
        }

        if (breakSound != null)
        {
            AudioSource.PlayClipAtPoint(breakSound, contact.point);
        }
    }

    private void LaunchSelf(Vector3 direction, float impactSpeed)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            rb.mass = objectMass;
        }

        rb.isKinematic = false;
        rb.AddForce(direction * impactSpeed * launchForce, ForceMode.VelocityChange);
        rb.AddTorque(Random.onUnitSphere * spin, ForceMode.VelocityChange);

        if (destroyAfterBreak)
        {
            StartCoroutine(ShrinkAndDestroy(gameObject, debrisLifetime));
        }
    }

    private void BreakIntoFragments(Vector3 direction, float impactSpeed)
    {
        GameObject broken = Instantiate(brokenPrefab, transform.position, transform.rotation);
        broken.transform.localScale = transform.localScale;

        Rigidbody[] fragments = broken.GetComponentsInChildren<Rigidbody>();
        for (int i = 0; i < fragments.Length; i++)
        {
            float variation = Random.Range(0.6f, 1.2f);
            fragments[i].AddForce(direction * impactSpeed * launchForce * variation, ForceMode.VelocityChange);
            fragments[i].AddTorque(Random.onUnitSphere * spin, ForceMode.VelocityChange);
        }

        StartCoroutine(ShrinkAndDestroy(broken, debrisLifetime));

        // El objeto original ya no hace falta
        foreach (Renderer r in GetComponentsInChildren<Renderer>()) { r.enabled = false; }
        foreach (Collider c in GetComponentsInChildren<Collider>()) { c.enabled = false; }
    }

    /// <summary>
    /// Espera, encoge el objeto en medio segundo y lo destruye para no acumular restos.
    /// Corre sobre este componente, así que se destruye el objeto original al final también.
    /// </summary>
    private IEnumerator ShrinkAndDestroy(GameObject target, float lifetime)
    {
        const float shrinkTime = 0.5f;

        yield return new WaitForSeconds(Mathf.Max(0f, lifetime - shrinkTime));

        if (target != null)
        {
            Vector3 startScale = target.transform.localScale;
            float t = 0f;

            while (t < shrinkTime && target != null)
            {
                t += Time.deltaTime;
                target.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t / shrinkTime);
                yield return null;
            }

            if (target != null) { Destroy(target); }
        }

        // En modo Replace el original sigue vivo (invisible): lo limpiamos
        if (gameObject != null) { Destroy(gameObject); }
    }
}