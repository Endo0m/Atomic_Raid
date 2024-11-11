using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldSpawn : MonoBehaviour
{
    private ConfigData configData;
    private int currentStageIndex = 0;
    private float stageTimer = 0f;
    private EnvironmentStaticGenerator environmentGenerator;
    private Camera mainCamera;
    private List<GameObject> spawnedFields = new List<GameObject>();
    private bool isSpawningEnabled = true;
    private Coroutine spawnRoutine;

    [Header("Не трогать")]
    [Header("слои проверяют чтобы не создаваться внутри них")]
    [SerializeField] private LayerMask fieldLayer;
    [Header("луч чтобы крепить к окруженнию")]
    [SerializeField] private float raycastDistance = 10f;

    private void Start()
    {
        configData = ConfigData.Instance;
        environmentGenerator = FindObjectOfType<EnvironmentStaticGenerator>();
        mainCamera = Camera.main;
        StartSpawning();
    }

    private void Update()
    {
        CheckAndDestroyFields();
    }

    public void SetSpawningEnabled(bool enabled)
    {
        if (isSpawningEnabled != enabled)
        {
            isSpawningEnabled = enabled;
            if (enabled)
            {
                StartSpawning();
            }
            else
            {
                StopSpawning();
            }
            Debug.Log($"Field spawning {(enabled ? "enabled" : "disabled")}");
        }
    }

    private void StartSpawning()
    {
        if (spawnRoutine == null)
        {
            spawnRoutine = StartCoroutine(SpawnFieldsRoutine());
        }
    }

    private void StopSpawning()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private IEnumerator SpawnFieldsRoutine()
    {
        yield return new WaitForSeconds(configData.initialSpawnDelay);

        while (true)
        {
            if (!isSpawningEnabled)
            {
                yield return new WaitUntil(() => isSpawningEnabled);
            }

            ConfigData.SpawnStage currentStage = configData.spawnStages[currentStageIndex];

            for (int i = 0; i < currentStage.fieldsPerSpawn; i++)
            {
                SpawnField(currentStage.buffProbability, currentStage.debuffProbability);
            }

            yield return new WaitForSeconds(currentStage.spawnInterval);

            stageTimer += currentStage.spawnInterval;
            if (stageTimer >= currentStage.stageDuration)
            {
                stageTimer = 0f;
                if (currentStageIndex < configData.spawnStages.Length - 1)
                {
                    currentStageIndex++;
                }
            }
        }
    }

    private void SpawnField(float buffProb, float debuffProb)
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        if (spawnPosition == Vector3.zero) return;

        float totalProb = buffProb + debuffProb;
        float randomValue = Random.value * totalProb;

        GameObject prefabToSpawn;
        if (randomValue < buffProb)
        {
            prefabToSpawn = configData.buffPrefabs[Random.Range(0, configData.buffPrefabs.Length)];
        }
        else
        {
            prefabToSpawn = configData.debuffPrefabs[Random.Range(0, configData.debuffPrefabs.Length)];
        }

        GameObject spawnedField = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        spawnedField.transform.localScale = prefabToSpawn.transform.localScale;
        spawnedField.layer = LayerMask.NameToLayer("Field");

        // Прикрепляем поле к окружению
        RaycastHit hit;
        if (Physics.Raycast(spawnPosition, Vector3.down, out hit, raycastDistance, ~(fieldLayer | LayerMask.GetMask("Enemy", "Objects"))))
        {
            spawnedField.transform.SetParent(hit.transform);
        }
        else
        {
            spawnedField.transform.SetParent(environmentGenerator.lastSegment.transform);
        }

        spawnedFields.Add(spawnedField);
    }
    private Vector3 GetRandomSpawnPosition()
    {
        ConfigData.FieldSpawnConfig spawnConfig = configData.fieldSpawnConfig;
        GameObject lastSegment = environmentGenerator.lastSegment;

        if (lastSegment == null) return Vector3.zero;

        for (int i = 0; i < spawnConfig.maxSpawnAttempts; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(spawnConfig.areaMin.x, spawnConfig.areaMax.x),
                Random.Range(spawnConfig.areaMin.y, spawnConfig.areaMax.y),
                Random.Range(spawnConfig.areaMin.z, spawnConfig.areaMax.z)
            );

            // Проверяем, нет ли коллизий с объектами на слоях Enemy, Objects и Field
            Collider[] colliders = Physics.OverlapSphere(randomPosition, 0.5f, LayerMask.GetMask("Enemy", "Objects", "Field"));

            if (colliders.Length == 0 && IsPositionFarEnoughFromOtherPortals(randomPosition, spawnConfig.minDistanceBetweenPortals))
            {
                return randomPosition;
            }
        }

        Debug.LogWarning("Failed to find a suitable spawn position after " + spawnConfig.maxSpawnAttempts + " attempts.");
        return Vector3.zero;
    }
    private bool IsPositionFarEnoughFromOtherPortals(Vector3 position, float minDistance)
    {
        foreach (GameObject field in spawnedFields)
        {
            if (field != null && Vector3.Distance(position, field.transform.position) < minDistance)
            {
                return false;
            }
        }
        return true;
    }

    private void CheckAndDestroyFields()
    {
        for (int i = spawnedFields.Count - 1; i >= 0; i--)
        {
            GameObject field = spawnedFields[i];
            if (field == null || IsOutOfCameraView(field))
            {
                if (field != null)
                {
                    Destroy(field);
                }
                spawnedFields.RemoveAt(i);
            }
        }
    }

    private bool IsOutOfCameraView(GameObject obj)
    {
        return CameraUtility.IsOutOfCameraView(obj, mainCamera, environmentGenerator.offScreenOffset);
    }

    private void OnDrawGizmos()
    {
        if (configData == null) configData = ConfigData.Instance;
        if (configData == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(
            (configData.fieldSpawnConfig.areaMin + configData.fieldSpawnConfig.areaMax) / 2,
            configData.fieldSpawnConfig.areaMax - configData.fieldSpawnConfig.areaMin
        );
    }
}