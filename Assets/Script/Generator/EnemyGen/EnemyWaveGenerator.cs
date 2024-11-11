using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyWaveGenerator : MonoBehaviour
{
    [SerializeField] private EnemyWaveConfig config;

    private bool isGeneratingEnemies = false;
    private bool isBossFight = false;
    private GameObject currentBoss;
    private List<Coroutine> spawnCoroutines = new List<Coroutine>();
    private int currentLevel = 0;
    private Coroutine cleanupCoroutine;
    private float pauseAfterRunningDuration = 5f;

    public void StartEnemyGeneration(int level)
    {
        StopAllCoroutines();
        spawnCoroutines.Clear();
        isGeneratingEnemies = true;
        isBossFight = false;
        currentBoss = null;
        currentLevel = level;

        foreach (var enemyType in config.enemyTypes.Where(et => et.level == currentLevel))
        {
            spawnCoroutines.Add(StartCoroutine(SpawnEnemyTypeRoutine(enemyType)));
        }
    }

    public void StopEnemyGeneration()
    {
        isGeneratingEnemies = false;
        StopAllCoroutines();
        spawnCoroutines.Clear();
        DestroyAllEnemies();
    }

    public void ResumeEnemyGeneration()
    {
        if (!isBossFight)
        {
            StartCoroutine(ResumeEnemyGenerationWithDelay());
        }
    }

    private IEnumerator ResumeEnemyGenerationWithDelay()
    {
        yield return new WaitForSeconds(pauseAfterRunningDuration);
        StartEnemyGeneration(currentLevel);
    }

    public void StartBossFight(GameObject bossPrefab)
    {
        if (currentBoss == null)
        {
            StopEnemyGeneration();
            isBossFight = true;
            currentBoss = Instantiate(bossPrefab, GetBossSpawnPosition(), Quaternion.identity);
            Debug.Log("Boss fight started.");
        }
    }

    public void OnBossDefeated()
    {
        isBossFight = false;
        currentBoss = null;
        Debug.Log("Boss defeated.");
    }

    private IEnumerator SpawnEnemyTypeRoutine(EnemyWaveConfig.EnemyTypeConfig enemyType)
    {
        yield return new WaitForSeconds(enemyType.spawnStartTime);

        while (isGeneratingEnemies && !isBossFight)
        {
            foreach (var wave in enemyType.waveConfigs)
            {
                float waveEndTime = Time.time + wave.duration;
                while (Time.time < waveEndTime && isGeneratingEnemies && !isBossFight)
                {
                    if (CountActiveEnemiesOfType(enemyType) < enemyType.maxEnemies)
                    {
                        SpawnEnemiesForWave(enemyType, wave);
                    }
                    yield return new WaitForSeconds(Random.Range(wave.minSpawnInterval, wave.maxSpawnInterval));
                }
            }
        }
    }

    private void SpawnEnemiesForWave(EnemyWaveConfig.EnemyTypeConfig enemyType, EnemyWaveConfig.WaveConfig wave)
    {
        int enemiesToSpawn = Random.Range(wave.minEnemies, wave.maxEnemies + 1);
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            Vector3 spawnPosition = GetRandomSpawnPosition(enemyType.isFlying);
            if (spawnPosition != Vector3.zero)
            {
                Instantiate(enemyType.prefab, spawnPosition, Quaternion.identity);
            }
        }
    }

    private Vector3 GetRandomSpawnPosition(bool isFlying)
    {
        EnemyWaveConfig.SpawnArea area = isFlying ? config.flyingSpawnArea : config.groundSpawnArea;
        int maxAttempts = 20;
        float minDistanceBetweenEnemies = 15f;

        for (int i = 0; i < maxAttempts; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(area.min.x, area.max.x),
                Random.Range(area.min.y, area.max.y),
                Random.Range(area.min.z, area.max.z)
            );

            if (!Physics.CheckSphere(randomPosition, 1f, config.obstacleLayer) &&
                !Physics.CheckSphere(randomPosition, minDistanceBetweenEnemies, LayerMask.GetMask("Enemy")))
            {
                return randomPosition;
            }
        }

        Debug.LogWarning("Failed to find a suitable spawn position after " + maxAttempts + " attempts.");
        return Vector3.zero;
    }

    private int CountActiveEnemiesOfType(EnemyWaveConfig.EnemyTypeConfig enemyType)
    {
        return FindObjectsOfType<MonoBehaviour>().Count(e => e.gameObject.activeSelf && e.gameObject.name.StartsWith(enemyType.prefab.name));
    }

    public void DestroyAllEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
        Debug.Log($"Destroyed {enemies.Length} enemies.");
        StartCoroutine(DoubleCheckEnemyCleanup());
    }

    private IEnumerator DoubleCheckEnemyCleanup()
    {
        yield return new WaitForSeconds(5f);
        GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in remainingEnemies)
        {
            Destroy(enemy);
        }
        if (remainingEnemies.Length > 0)
        {
            Debug.Log($"Cleaned up {remainingEnemies.Length} remaining enemies in double-check.");
        }
    }

    private Vector3 GetBossSpawnPosition()
    {
        return new Vector3(0, 5, 100); // ѕример позиции, настройте под свои нужды
    }
}