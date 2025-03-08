using System.Collections.Generic;
using UnityEngine;

public class VisualizadorVectoresCampo : MonoBehaviour
{
    [Header("Configuraci�n de Vectores")]
    [Tooltip("Part�culas a considerar para el c�lculo del campo")]
    public List<ParticulaElectrica> particulas = new List<ParticulaElectrica>();

    [Tooltip("Separaci�n entre vectores en X y Z")]
    public float espaciadoVectores = 1f;

    [Tooltip("Dimensiones de la cuadr�cula (cu�ntos vectores en cada direcci�n)")]
    public Vector2Int dimensionesCuadricula = new Vector2Int(10, 10);

    [Tooltip("Escala de los vectores (qu� tan grandes se muestran)")]
    public float escalaVectores = 0.5f;

    [Tooltip("Color para los vectores")]
    public Color colorVectores = Color.yellow;

    [Tooltip("Intensidad m�nima para mostrar un vector")]
    public float umbralIntensidad = 0.1f;

    [Header("Actualizaci�n en Tiempo Real")]
    [Tooltip("Activar actualizaci�n en cada frame")]
    public bool actualizacionTiempoReal = true;

    [Tooltip("Intervalo de actualizaci�n en segundos (0 para cada frame)")]
    public float intervaloActualizacion = 0.1f;

    private float tiempoUltimaActualizacion;

    // Lista para almacenar todos los vectores creados
    private List<GameObject> vectoresGenerados = new List<GameObject>();

    // Referencias a las partes del vector prefab
    private GameObject vectorPrefab;

    private void Start()
    {
        if (particulas.Count == 0)
        {
            // Buscar todas las part�culas en la escena si no se asignaron manualmente
            particulas.AddRange(FindObjectsByType<ParticulaElectrica>(FindObjectsSortMode.None));
        }

        // Crear un prefab simple para los vectores
        CrearVectorPrefab();

        // Generar la cuadr�cula de vectores
        GenerarVectoresDeCampo();

        tiempoUltimaActualizacion = Time.time;
    }

    private void Update()
    {
        if (actualizacionTiempoReal)
        {
            // Actualizar seg�n el intervalo especificado
            if (Time.time >= tiempoUltimaActualizacion + intervaloActualizacion)
            {
                ActualizarVectoresDeCampo();
                tiempoUltimaActualizacion = Time.time;
            }
        }
    }

    private void CrearVectorPrefab()
    {
        vectorPrefab = new GameObject("VectorPrefab");
        vectorPrefab.SetActive(false); // Ocultarlo, es solo una plantilla

        // Crear cuerpo del vector (cilindro)
        GameObject cuerpo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cuerpo.transform.SetParent(vectorPrefab.transform);
        cuerpo.transform.localPosition = Vector3.zero;
        cuerpo.transform.localRotation = Quaternion.Euler(90, 0, 0);
        cuerpo.transform.localScale = new Vector3(0.05f, 0.25f, 0.05f);

        // Crear punta del vector (cono)
        GameObject punta = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        punta.transform.SetParent(vectorPrefab.transform);
        punta.transform.localPosition = new Vector3(0, 0, 0.5f);
        punta.transform.localRotation = Quaternion.Euler(90, 0, 0);
        punta.transform.localScale = new Vector3(0.1f, 0.2f, 0.1f);

        // Eliminar colliders (no son necesarios)
        Destroy(cuerpo.GetComponent<Collider>());
        Destroy(punta.GetComponent<Collider>());

        // Crear material
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = colorVectores;

        // Aplicar material
        cuerpo.GetComponent<Renderer>().material = mat;
        punta.GetComponent<Renderer>().material = mat;
    }

    // M�todo p�blico para actualizar los vectores (por ejemplo, desde un bot�n)
    public void ActualizarVectoresDeCampo()
    {
        LimpiarVectoresExistentes();
        GenerarVectoresDeCampo();
    }

    // El resto del c�digo permanece igual...
    private void GenerarVectoresDeCampo()
    {
        // Calcular el centro de la cuadr�cula
        Vector3 centro = transform.position;
        float mitadX = (dimensionesCuadricula.x - 1) * espaciadoVectores * 0.5f;
        float mitadZ = (dimensionesCuadricula.y - 1) * espaciadoVectores * 0.5f;

        // Generar cuadr�cula de vectores
        for (int x = 0; x < dimensionesCuadricula.x; x++)
        {
            for (int z = 0; z < dimensionesCuadricula.y; z++)
            {
                // Calcular posici�n del vector
                Vector3 posicion = centro + new Vector3(
                    x * espaciadoVectores - mitadX,
                    0,
                    z * espaciadoVectores - mitadZ
                );

                // Verificar si hay una part�cula muy cerca
                bool hayParticulaCerca = false;
                foreach (ParticulaElectrica particula in particulas)
                {
                    if (Vector3.Distance(posicion, particula.transform.position) < 0.5f)
                    {
                        hayParticulaCerca = true;
                        break;
                    }
                }

                // Si hay una part�cula cerca, no poner vector
                if (hayParticulaCerca) continue;

                // Calcular el campo el�ctrico en este punto
                Vector3 campoTotal = CalcularCampoTotalEn(posicion);

                // Si el campo es muy d�bil, no mostrar vector
                if (campoTotal.magnitude < umbralIntensidad) continue;

                // Crear vector
                CrearVectorEn(posicion, campoTotal);
            }
        }
    }

    private void CrearVectorEn(Vector3 posicion, Vector3 campo)
    {
        // Instanciar el prefab del vector
        GameObject vector = Instantiate(vectorPrefab, posicion, Quaternion.identity);
        vector.transform.SetParent(transform);
        vector.SetActive(true);

        // Escalar seg�n la intensidad del campo (opcional)
        float intensidad = Mathf.Clamp(campo.magnitude, 0.5f, 2f);
        vector.transform.localScale = Vector3.one * intensidad * escalaVectores;

        // Orientar en la direcci�n del campo
        vector.transform.rotation = Quaternion.LookRotation(campo.normalized);

        // A�adir a la lista para poder limpiarlos despu�s
        vectoresGenerados.Add(vector);
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

    private void LimpiarVectoresExistentes()
    {
        foreach (GameObject vector in vectoresGenerados)
        {
            if (vector != null)
                Destroy(vector);
        }

        vectoresGenerados.Clear();
    }

    private void OnValidate()
    {
        // Para actualizar en el editor
        if (Application.isPlaying && vectorPrefab != null)
        {
            ActualizarVectoresDeCampo();
        }
    }

    private void OnDestroy()
    {
        // Limpiar el prefab al destruir el componente
        if (vectorPrefab != null)
            Destroy(vectorPrefab);
    }
}