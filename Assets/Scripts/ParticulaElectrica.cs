using UnityEngine;

public class ParticulaElectrica : MonoBehaviour
{
    [Header("Propiedades de la Part�cula")]
    [Tooltip("True para carga positiva, False para negativa")]
    public bool esPositiva = true;

    [Tooltip("Magnitud de la carga el�ctrica")]
    public float magnitudCarga = 10f;

    [Tooltip("Color para representar visualmente la part�cula")]
    public Color colorParticula;

    private Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // Asignar color seg�n la carga
            colorParticula = esPositiva ? Color.red : Color.blue;
            rend.material.color = colorParticula;
        }
    }

    // M�todo p�blico para obtener la carga con signo
    public float ObtenerCargaConSigno()
    {
        return esPositiva ? magnitudCarga : -magnitudCarga;
    }

    // M�todo p�blico para calcular el campo el�ctrico en una posici�n
    public Vector3 CalcularCampoElectrico(Vector3 posicion)
    {
        Vector3 direccion = posicion - transform.position;
        float distancia = direccion.magnitude;

        // Evitar divisiones por cero o valores muy peque�os
        if (distancia < 0.001f)
            return Vector3.zero;

        // Aplicar la ley de Coulomb (E = k*q/r�)
        // El valor 8.99e9 es la constante electrost�tica, pero usamos 1 para simplificar
        float intensidad = ObtenerCargaConSigno() / (distancia * distancia);

        return direccion.normalized * intensidad;
    }

    private void OnDrawGizmos()
    {
        // Dibujar un gizmo para ver la part�cula en el editor
        Gizmos.color = esPositiva ? Color.red : Color.blue;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}