using UnityEngine;

public class ParticulaElectrica : MonoBehaviour
{
    [Header("Propiedades de la Partícula")]
    [Tooltip("True para carga positiva, False para negativa")]
    public bool esPositiva = true;

    [Tooltip("Magnitud de la carga eléctrica")]
    public float magnitudCarga = 10f;

    [Tooltip("Color para representar visualmente la partícula")]
    public Color colorParticula;

    private Renderer rend;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            // Asignar color según la carga
            colorParticula = esPositiva ? Color.red : Color.blue;
            rend.material.color = colorParticula;
        }
    }

    // Método público para obtener la carga con signo
    public float ObtenerCargaConSigno()
    {
        return esPositiva ? magnitudCarga : -magnitudCarga;
    }

    // Método público para calcular el campo eléctrico en una posición
    public Vector3 CalcularCampoElectrico(Vector3 posicion)
    {
        Vector3 direccion = posicion - transform.position;
        float distancia = direccion.magnitude;

        // Evitar divisiones por cero o valores muy pequeños
        if (distancia < 0.001f)
            return Vector3.zero;

        // Aplicar la ley de Coulomb (E = k*q/r²)
        // El valor 8.99e9 es la constante electrostática, pero usamos 1 para simplificar
        float intensidad = ObtenerCargaConSigno() / (distancia * distancia);

        return direccion.normalized * intensidad;
    }

    private void OnDrawGizmos()
    {
        // Dibujar un gizmo para ver la partícula en el editor
        Gizmos.color = esPositiva ? Color.red : Color.blue;
        Gizmos.DrawSphere(transform.position, 0.2f);
    }
}