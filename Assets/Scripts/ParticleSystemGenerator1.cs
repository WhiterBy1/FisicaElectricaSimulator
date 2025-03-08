using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemGenerator : MonoBehaviour
{
    public GameObject particlePrefab;
    public int numPositiveParticles = 5;
    public int numNegativeParticles = 5;
    public float spawnRadius = 10.0f;
    public float minInitialDistance = 3.0f; // Distancia mínima entre partículas

    void Start()
    {
        if (particlePrefab == null)
        {
            Debug.LogError("Necesitas asignar un prefab de partícula!");
            return;
        }

        StartCoroutine(SpawnParticlesWithDelay());
    }

    IEnumerator SpawnParticlesWithDelay()
    {
        List<Vector3> usedPositions = new List<Vector3>();

        // Crea partículas positivas
        for (int i = 0; i < numPositiveParticles; i++)
        {
            Vector3 randomPos = GetRandomPositionWithMinDistance(usedPositions);
            usedPositions.Add(randomPos);

            GameObject particle = Instantiate(particlePrefab, randomPos, Quaternion.identity);
            ElectricParticle ep = particle.GetComponent<ElectricParticle>();
            if (ep == null)
                ep = particle.AddComponent<ElectricParticle>();

            ep.charge = Random.Range(0.5f, 1.5f); // Carga positiva más controlada
            ep.mass = Random.Range(1.0f, 3.0f); // Varía la masa para mayor estabilidad
            particle.name = "Positive_" + i;

            // Escala la partícula según su masa
            float scale = 0.5f + (ep.mass / 3.0f);
            particle.transform.localScale = new Vector3(scale, scale, scale);

            yield return new WaitForSeconds(0.1f); // Pequeña pausa entre creaciones
        }

        // Crea partículas negativas
        for (int i = 0; i < numNegativeParticles; i++)
        {
            Vector3 randomPos = GetRandomPositionWithMinDistance(usedPositions);
            usedPositions.Add(randomPos);

            GameObject particle = Instantiate(particlePrefab, randomPos, Quaternion.identity);
            ElectricParticle ep = particle.GetComponent<ElectricParticle>();
            if (ep == null)
                ep = particle.AddComponent<ElectricParticle>();

            ep.charge = Random.Range(-1.5f, -0.5f); // Carga negativa más controlada
            ep.mass = Random.Range(1.0f, 3.0f);
            particle.name = "Negative_" + i;

            // Escala la partícula según su masa
            float scale = 0.5f + (ep.mass / 3.0f);
            particle.transform.localScale = new Vector3(scale, scale, scale);

            yield return new WaitForSeconds(0.1f);
        }
    }

    // Método para obtener una posición que esté a una distancia mínima de otras partículas
    Vector3 GetRandomPositionWithMinDistance(List<Vector3> existingPositions)
    {
        Vector3 randomPos;
        bool validPosition = false;
        int maxAttempts = 30;
        int attempts = 0;

        do
        {
            randomPos = Random.insideUnitSphere * spawnRadius;
            validPosition = true;

            foreach (Vector3 pos in existingPositions)
            {
                if (Vector3.Distance(randomPos, pos) < minInitialDistance)
                {
                    validPosition = false;
                    break;
                }
            }

            attempts++;

        } while (!validPosition && attempts < maxAttempts);

        return randomPos;
    }
}