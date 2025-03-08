using System.Collections.Generic;
using UnityEngine;

public class VisualizadorCampoElectrico : MonoBehaviour
{
    [Header("Configuración de Visualización")]
    [Tooltip("Partículas a considerar para el cálculo del campo")]
    public List<ParticulaElectrica> particulas = new List<ParticulaElectrica>();

    [Tooltip("Número de líneas de campo por partícula")]
    public int lineasPorParticula = 8;

    [Tooltip("Longitud máxima de cada línea")]
    public float longitudLinea = 5f;

    [Tooltip("Puntos a calcular por línea")]
    public int puntosPorLinea = 30;

    [Tooltip("Distancia entre cada punto")]
    public float distanciaPuntos = 0.2f;

    [Tooltip("Material para las líneas de campo")]
    public Material materialLinea;

    [Tooltip("Ancho de las líneas")]
    public float anchoLinea = 0.05f;

    [Header("Actualización en Tiempo Real")]
    [Tooltip("Activar actualización en cada frame")]
    public bool actualizacionTiempoReal = true;

    [Tooltip("Intervalo de actualización en segundos (0 para cada frame)")]
    public float intervaloActualizacion = 0.1f;

    private float tiempoUltimaActualizacion;

    // Lista para almacenar todas las líneas creadas
    private List<LineRenderer> lineasGeneradas = new List<LineRenderer>();

    private void Start()
    {
        if (particulas.Count == 0)
        {
            // Buscar todas las partículas en la escena si no se asignaron manualmente
            particulas.AddRange(FindObjectsByType<ParticulaElectrica>(FindObjectsSortMode.None));
        }

        GenerarLineasDeCampo();
        tiempoUltimaActualizacion = Time.time;
    }

    private void Update()
    {
        if (actualizacionTiempoReal)
        {
            // Actualizar según el intervalo especificado
            if (Time.time >= tiempoUltimaActualizacion + intervaloActualizacion)
            {
                ActualizarLineasDeCampo();
                tiempoUltimaActualizacion = Time.time;
            }
        }
    }

    // Método que puedes llamar para actualizar las líneas (por ejemplo, desde un botón)
    public void ActualizarLineasDeCampo()
    {
        LimpiarLineasExistentes();
        GenerarLineasDeCampo();
    }

    private void GenerarLineasDeCampo()
    {
        if (materialLinea == null)
        {
            Debug.LogError("No se ha asignado un material para las líneas");
            return;
        }

        foreach (ParticulaElectrica particula in particulas)
        {
            // Solo crear líneas salientes desde partículas positivas
            // y líneas entrantes hacia partículas negativas
            GenerarLineasParaParticula(particula);
        }
    }

    // El resto del código permanece igual...
    private void GenerarLineasParaParticula(ParticulaElectrica particula)
    {
        // Determinar si las líneas salen o entran a la partícula
        bool lineasSalientes = particula.esPositiva;

        for (int i = 0; i < lineasPorParticula; i++)
        {
            // Crear objeto para la línea
            GameObject lineaObj = new GameObject($"LineaCampo_{particula.name}_{i}");
            lineaObj.transform.SetParent(transform);

            LineRenderer lineRenderer = lineaObj.AddComponent<LineRenderer>();
            lineRenderer.material = materialLinea;
            lineRenderer.startWidth = anchoLinea;
            lineRenderer.endWidth = anchoLinea * 0.5f; // Más delgado al final
            lineRenderer.positionCount = puntosPorLinea;

            // Color según el tipo de partícula
            Color colorLinea = particula.esPositiva ? Color.red : Color.blue;
            lineRenderer.startColor = colorLinea;
            lineRenderer.endColor = new Color(colorLinea.r, colorLinea.g, colorLinea.b, 0.2f); // Transparente al final

            // Calcular puntos alrededor de la partícula equidistantes
            float angulo = (2 * Mathf.PI * i) / lineasPorParticula;
            Vector3 direccionInicial = new Vector3(
                Mathf.Cos(angulo),
                0,
                Mathf.Sin(angulo)
            );

            // Punto inicial ligeramente separado de la partícula
            Vector3 puntoInicial = particula.transform.position + direccionInicial * 0.3f;

            // Calcular y establecer puntos para la línea
            List<Vector3> puntosLinea = SeguirLineaDeCampo(puntoInicial, lineasSalientes);
            lineRenderer.SetPositions(puntosLinea.ToArray());

            lineasGeneradas.Add(lineRenderer);
        }
    }

    private List<Vector3> SeguirLineaDeCampo(Vector3 puntoInicial, bool direccionSaliente)
    {
        List<Vector3> puntos = new List<Vector3>();
        Vector3 posicionActual = puntoInicial;

        puntos.Add(posicionActual);

        // Seguir la línea de campo punto por punto
        for (int i = 1; i < puntosPorLinea; i++)
        {
            // Calcular el campo eléctrico total en el punto actual
            Vector3 campoTotal = CalcularCampoTotalEn(posicionActual);

            // Si el campo es demasiado débil, detener la línea
            if (campoTotal.magnitude < 0.001f)
                break;

            // Normalizar y aplicar la dirección correcta
            campoTotal = campoTotal.normalized * distanciaPuntos;
            if (!direccionSaliente)
                campoTotal = -campoTotal;

            // Calcular el siguiente punto
            posicionActual += campoTotal;
            puntos.Add(posicionActual);

            // Verificar si hemos alcanzado la longitud máxima
            if (Vector3.Distance(puntoInicial, posicionActual) > longitudLinea)
                break;

            // Verificar si hemos llegado cerca de otra partícula
            if (HemosLlegadoAParticula(posicionActual, direccionSaliente))
                break;
        }

        return puntos;
    }

    private Vector3 CalcularCampoTotalEn(Vector3 posicion)
    {
        Vector3 campoTotal = Vector3.zero;

        foreach (ParticulaElectrica particula in particulas)
        {
            campoTotal += particula.CalcularCampoElectrico(posicion);
        }

        return campoTotal;
    }

    private bool HemosLlegadoAParticula(Vector3 posicion, bool lineasSalientes)
    {
        foreach (ParticulaElectrica particula in particulas)
        {
            // Si estamos trazando líneas salientes, nos interesa llegar a partículas negativas
            // Si estamos trazando líneas entrantes, nos interesa llegar a partículas positivas
            if (particula.esPositiva == lineasSalientes)
                continue;

            float distancia = Vector3.Distance(posicion, particula.transform.position);
            if (distancia < 0.3f)
                return true;
        }
        return false;
    }

    private void LimpiarLineasExistentes()
    {
        foreach (LineRenderer linea in lineasGeneradas)
        {
            if (linea != null)
                Destroy(linea.gameObject);
        }

        lineasGeneradas.Clear();
    }

    private void OnValidate()
    {
        // Para actualizar en el editor
        if (Application.isPlaying)
        {
            ActualizarLineasDeCampo();
        }
        else
        {
            // Asegurarse de que no se llame a SendMessage durante OnValidate
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null)
                {
                    ActualizarLineasDeCampo();
                }
            };
        }
    }
}