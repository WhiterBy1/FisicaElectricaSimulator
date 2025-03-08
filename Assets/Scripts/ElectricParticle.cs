using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectricParticle : MonoBehaviour
{
    public float charge = 1.0f; // Carga de la partícula (positiva o negativa)
    public float mass = 1.0f;   // Masa de la partícula
    public bool isStatic = false; // Si es true, la partícula no se moverá
    public float maxForce = 50.0f; // Control de fuerza máxima para evitar explosiones
    public float boundaryRadius = 50.0f; // Radio límite para contener las partículas
    public float minDistance = 0.5f; // Distancia mínima para evitar fuerzas extremas

    private Rigidbody rb;
    private static List<ElectricParticle> allParticles = new List<ElectricParticle>();

    // Constante de Coulomb (ajustada para simulación)
    public static float coulombConstant = 8.99f * Mathf.Pow(10, 9) * 0.000001f;

    // Visualización del campo eléctrico
    public bool showField = false;
    private LineRenderer fieldLine;
    private int fieldLinePoints = 10;
    private float fieldLineLength = 5.0f;

    // Debug
    public bool showForceGizmo = true;
    private Vector3 debugForce;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Configura el Rigidbody
        rb.useGravity = false;
        rb.drag = 0.5f; // Reducido para permitir más movimiento visible
        rb.angularDrag = 0.5f;
        rb.isKinematic = isStatic;
        rb.mass = mass;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.maxAngularVelocity = 7.0f;

        // Color basado en la carga
        UpdateParticleColor();

        // Registra esta partícula
        allParticles.Add(this);

        // Inicializa el LineRenderer para el campo eléctrico
        if (showField)
        {
            SetupFieldLines();
        }
    }

    void UpdateParticleColor()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            if (charge > 0)
                renderer.material.color = new Color(1, 0, 0, 1); // Rojo para carga positiva
            else if (charge < 0)
                renderer.material.color = new Color(0, 0, 1, 1); // Azul para carga negativa
            else
                renderer.material.color = new Color(0.5f, 0.5f, 0.5f, 1); // Gris para neutro
        }
    }

    void SetupFieldLines()
    {
        fieldLine = gameObject.AddComponent<LineRenderer>();
        fieldLine.positionCount = fieldLinePoints;
        fieldLine.startWidth = 0.05f;
        fieldLine.endWidth = 0.01f;

        // Color según la carga
        if (charge > 0)
            fieldLine.startColor = fieldLine.endColor = new Color(1, 0.5f, 0.5f, 0.5f);
        else
            fieldLine.startColor = fieldLine.endColor = new Color(0.5f, 0.5f, 1, 0.5f);
    }

    void OnDestroy()
    {
        allParticles.Remove(this);
    }

    void FixedUpdate()
    {
        // Aplica las fuerzas eléctricas
        ApplyElectricForces();


        // Actualiza la visualización del campo eléctrico
        if (showField)
        {
            UpdateFieldLines();
        }
    }

    void ApplyElectricForces()
    {
        if (isStatic)
            return;

        Vector3 totalForce = Vector3.zero;

        foreach (ElectricParticle otherParticle in allParticles)
        {
            if (otherParticle == this)
                continue;

            // Vector desde nuestra posición hacia la otra partícula
            Vector3 direction = otherParticle.transform.position - transform.position;
            float distance = direction.magnitude;

            // Evitar divisiones por cero y fuerzas extremas
            if (distance < minDistance)
                distance = minDistance;

            // Normalizar dirección
            Vector3 normalizedDirection = direction.normalized;

            // Calcular fuerza de Coulomb: F = k * (q1 * q2) / r^2
            // Producto de cargas determina atracción o repulsión
            float chargeProduct = charge * otherParticle.charge;
            float forceMagnitude = coulombConstant * Mathf.Abs(chargeProduct) / (distance * distance);

            // PUNTO CLAVE: La dirección de la fuerza depende del signo de las cargas
            Vector3 force;

            if (chargeProduct < 0) // Cargas opuestas: ATRACCIÓN
            {
                // Fuerza en dirección HACIA la otra partícula
                force = normalizedDirection * forceMagnitude;
            }
            else // Cargas iguales: REPULSIÓN
            {
                // Fuerza en dirección ALEJÁNDOSE de la otra partícula
                force = -normalizedDirection * forceMagnitude;
            }

            // Sumar al vector de fuerza total
            totalForce += force;
        }

        // Limitar la fuerza máxima
        if (totalForce.magnitude > maxForce)
        {
            totalForce = totalForce.normalized * maxForce;
        }

        // Para debug
        debugForce = totalForce;

        // Aplicar la fuerza calculada
        rb.AddForce(totalForce);
    }

    // Método para calcular el campo eléctrico en un punto del espacio
    public static Vector3 CalculateElectricField(Vector3 point)
    {
        Vector3 fieldVector = Vector3.zero;

        foreach (ElectricParticle particle in allParticles)
        {
            // Vector desde la partícula hacia el punto
            Vector3 direction = point - particle.transform.position;
            float distance = direction.magnitude;

            if (distance < 0.1f)
                distance = 0.1f;

            // Campo eléctrico: E = k * q / r^2
            float fieldMagnitude = coulombConstant * Mathf.Abs(particle.charge) / (distance * distance);
            Vector3 fieldContribution = direction.normalized * fieldMagnitude;

            // El signo de la carga determina la dirección
            if (particle.charge > 0) // Las líneas salen de cargas positivas
                fieldVector += fieldContribution;
            else // Las líneas entran en cargas negativas
                fieldVector -= fieldContribution;
        }

        return fieldVector;
    }

    void UpdateFieldLines()
    {
        if (fieldLine == null)
            return;

        Vector3[] positions = new Vector3[fieldLinePoints];
        Vector3 currentPos = transform.position;

        // La dirección inicial depende del signo de la carga
        Vector3 fieldDirection = (charge > 0) ? Vector3.up : -Vector3.up;

        for (int i = 0; i < fieldLinePoints; i++)
        {
            positions[i] = currentPos;

            // Calcular campo en el punto actual
            Vector3 electricField = CalculateElectricField(currentPos);

            // Si el campo es muy débil, usamos la última dirección conocida
            if (electricField.magnitude < 0.01f)
                electricField = fieldDirection * 0.01f;

            // Actualizamos la dirección y posición para el siguiente punto
            fieldDirection = electricField.normalized;

            // Si es carga negativa, invertimos para que las líneas entren
            if (charge < 0)
                fieldDirection = -fieldDirection;

            currentPos += fieldDirection * (fieldLineLength / fieldLinePoints);
        }

        fieldLine.SetPositions(positions);
    }

    void KeepInBounds(float boundaryRadius)
    {
        float distanceFromCenter = Vector3.Distance(transform.position, Vector3.zero);

        if (distanceFromCenter > boundaryRadius)
        {
            // Dirección hacia el centro
            Vector3 directionToCenter = -transform.position.normalized;

            // Fuerza que aumenta con la distancia
            float overshootDistance = distanceFromCenter - boundaryRadius;
            float boundaryForce = overshootDistance * 10.0f;

            rb.AddForce(directionToCenter * boundaryForce);
        }
    }

    public void SetCharge(float newCharge)
    {
        charge = newCharge;
        UpdateParticleColor();

        if (showField && fieldLine != null)
        {
            if (charge > 0)
                fieldLine.startColor = fieldLine.endColor = new Color(1, 0.5f, 0.5f, 0.5f);
            else
                fieldLine.startColor = fieldLine.endColor = new Color(0.5f, 0.5f, 1, 0.5f);
        }
    }

    // Dibuja el vector de fuerza para debug
    private void OnDrawGizmos()
    {
        if (showForceGizmo && Application.isPlaying)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, debugForce.normalized * 2);

            // Muestra la magnitud de la fuerza como texto
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + debugForce.normalized * 2.2f,
                debugForce.magnitude.ToString("F2"));
#endif
        }
    }
}