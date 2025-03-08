using System.Collections.Generic;
using UnityEngine;

public class VisualizadorCampoElectrico : MonoBehaviour
{
    [Header("Configuraci�n de Visualizaci�n")]
    [Tooltip("Part�culas a considerar para el c�lculo del campo")]
    public List<ParticulaElectrica> particulas = new List<ParticulaElectrica>();

    [Tooltip("N�mero de l�neas de campo por part�cula")]
    public int lineasPorParticula = 8;

    [Tooltip("Longitud m�xima de cada l�nea")]
    public float longitudLinea = 5f;

    [Tooltip("Puntos a calcular por l�nea")]
    public int puntosPorLinea = 30;

    [Tooltip("Distancia entre cada punto")]
    public float distanciaPuntos = 0.2f;

    [Tooltip("Material para las l�neas de campo")]
    public Material materialLinea;

    [Tooltip("Ancho de las l�neas")]
    public float anchoLinea = 0.05f;

    [Header("Actualizaci�n en Tiempo Real")]
    [Tooltip("Activar actualizaci�n en cada frame")]
    public bool actualizacionTiempoReal = true;

    [Tooltip("Intervalo de actualizaci�n en segundos (0 para cada frame)")]
    public float intervaloActualizacion = 0.1f;

    private float tiempoUltimaActualizacion;

    // Lista para almacenar todas las l�neas creadas
    private List<LineRenderer> lineasGeneradas = new List<LineRenderer>();

    private void Start()
    {
        if (particulas.Count == 0)
        {
            // Buscar todas las part�culas en la escena si no se asignaron manualmente
            particulas.AddRange(FindObjectsByType<ParticulaElectrica>(FindObjectsSortMode.None));
        }

        GenerarLineasDeCampo();
        tiempoUltimaActualizacion = Time.time;
    }

    private void Update()
    {
        if (actualizacionTiempoReal)
        {
            // Actualizar seg�n el intervalo especificado
            if (Time.time >= tiempoUltimaActualizacion + intervaloActualizacion)
            {
                ActualizarLineasDeCampo();
                tiempoUltimaActualizacion = Time.time;
            }
        }
    }

    // M�todo que puedes llamar para actualizar las l�neas (por ejemplo, desde un bot�n)
    public void ActualizarLineasDeCampo()
    {
        LimpiarLineasExistentes();
        GenerarLineasDeCampo();
    }

    private void GenerarLineasDeCampo()
    {
        if (materialLinea == null)
        {
            Debug.LogError("No se ha asignado un material para las l�neas");
            return;
        }

        foreach (ParticulaElectrica particula in particulas)
        {
            // Solo crear l�neas salientes desde part�culas positivas
            // y l�neas entrantes hacia part�culas negativas
            GenerarLineasParaParticula(particula);
        }
    }

    // El resto del c�digo permanece igual...
    private void GenerarLineasParaParticula(ParticulaElectrica particula)
    {
        // Determinar si las l�neas salen o entran a la part�cula
        bool lineasSalientes = particula.esPositiva;

        for (int i = 0; i < lineasPorParticula; i++)
        {
            // Crear objeto para la l�nea
            GameObject lineaObj = new GameObject($"LineaCampo_{particula.name}_{i}");
            lineaObj.transform.SetParent(transform);

            LineRenderer lineRenderer = lineaObj.AddComponent<LineRenderer>();
            lineRenderer.material = materialLinea;
            lineRenderer.startWidth = anchoLinea;
            lineRenderer.endWidth = anchoLinea * 0.5f; // M�s delgado al final
            lineRenderer.positionCount = puntosPorLinea;

            // Color seg�n el tipo de part�cula
            Color colorLinea = particula.esPositiva ? Color.red : Color.blue;
            lineRenderer.startColor = colorLinea;
            lineRenderer.endColor = new Color(colorLinea.r, colorLinea.g, colorLinea.b, 0.2f); // Transparente al final

            // Calcular puntos alrededor de la part�cula equidistantes
            float angulo = (2 * Mathf.PI * i) / lineasPorParticula;
            Vector3 direccionInicial = new Vector3(
                Mathf.Cos(angulo),
                0,
                Mathf.Sin(angulo)
            );

            // Punto inicial ligeramente separado de la part�cula
            Vector3 puntoInicial = particula.transform.position + direccionInicial * 0.3f;

            // Calcular y establecer puntos para la l�nea
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

        // Seguir la l�nea de campo punto por punto
        for (int i = 1; i < puntosPorLinea; i++)
        {
            // Calcular el campo el�ctrico total en el punto actual
            Vector3 campoTotal = CalcularCampoTotalEn(posicionActual);

            // Si el campo es demasiado d�bil, detener la l�nea
            if (campoTotal.magnitude < 0.001f)
                break;

            // Normalizar y aplicar la direcci�n correcta
            campoTotal = campoTotal.normalized * distanciaPuntos;
            if (!direccionSaliente)
                campoTotal = -campoTotal;

            // Calcular el siguiente punto
            posicionActual += campoTotal;
            puntos.Add(posicionActual);

            // Verificar si hemos alcanzado la longitud m�xima
            if (Vector3.Distance(puntoInicial, posicionActual) > longitudLinea)
                break;

            // Verificar si hemos llegado cerca de otra part�cula
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
            // Si estamos trazando l�neas salientes, nos interesa llegar a part�culas negativas
            // Si estamos trazando l�neas entrantes, nos interesa llegar a part�culas positivas
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