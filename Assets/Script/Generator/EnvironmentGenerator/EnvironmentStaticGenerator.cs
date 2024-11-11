using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class EnvironmentStaticGenerator : MonoBehaviour
{
    public static EnvironmentStaticGenerator instance = null;
    
    [System.Serializable]
    public class Level
    {
        [Header("������ ������")]
        public GameObject prefab;
        [Header("����������������� ������")]
        public float duration;
        [Header("������ ��������������� ��������")]
        public GameObject connectorPrefab;
        [Header("������� ����� �� ������")]
        public bool hasBoss;
        [Header("������ �����")]
        public GameObject bossPrefab;
    }

    [Header("��������� �������")]
    public List<Level> levels;

    [Header("����� ���������")]
    [Header("�������� �������� ����")]
    public float moveSpeed = 75f;
    [Header("������� ������� ������ �� ��������� �����")]
    public float bossAppearancePercentage = 0.75f;
    [Header("����� �� ��������� ������ ����� ���������� �����")]
    public float enemyStopBeforeBossTime = 15f;
    [Header("��������� ���������� ���������")]
    public int initialSegments = 3;
    [Header("���������� ����������� �������� �� ������")]
    public float offScreenOffset = 180f;
    [Header("�������� ���������� ����")]
    public float worldSpeedReductionRate = 0f;
    private float initialMoveSpeed;
    private int currentLevelIndex = -1;
    private float elapsedTime = 0f;
    private List<GameObject> spawnedSegments = new List<GameObject>();
    public GameObject lastSegment;
    private bool isMoving = true;
    private Camera mainCamera;
    private EnemyWaveGenerator enemyWaveGenerator;
    private MusicManager musicManager;
    private bool bossStarted = false;
    private bool isLevelChanging = false;
    private Coroutine levelTimerCoroutine;
    [SerializeField] private MovementSettings _settings;
    private float _currentEnvironmentSpeed;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        moveSpeed = _settings.flyingEnvironmentSpeed;
        initialMoveSpeed = moveSpeed;
        InitializeComponents();
        StartNextLevel();
    }

    void InitializeComponents()
    {
        musicManager = FindObjectOfType<MusicManager>();
        mainCamera = Camera.main;
        enemyWaveGenerator = FindObjectOfType<EnemyWaveGenerator>();
        lastSegment = GameObject.FindGameObjectWithTag("FirstSegment");

        if (lastSegment == null)
        {
            Debug.LogError("FirstSegment not found!");
            return;
        }

        spawnedSegments.Add(lastSegment);
    }

    void Update()
    {
        if (isMoving)
        {
            MoveEnvironment();
        }

        CheckAndDestroySegments();
    }
    public int GetCurrentLevel()
    {
        return currentLevelIndex;
    }
    void StartNextLevel()
    {
        if (isLevelChanging)
        {
            Debug.LogWarning("Attempting to start next level while level is already changing.");
            return;
        }

        StopAllCoroutines();

        currentLevelIndex++;
        if (currentLevelIndex >= levels.Count)
        {
            currentLevelIndex = levels.Count - 1; // ���������� �� ��������� ������
            Debug.Log("Reached last level. Continuing indefinitely.");
        }

        isLevelChanging = true;
        StartCoroutine(LevelRoutine());
    }

    public void SetEnvironmentSpeed(bool isRunning)
    {
        moveSpeed = isRunning ? _settings.runningEnvironmentSpeed : _settings.flyingEnvironmentSpeed;
        initialMoveSpeed = moveSpeed;  // ��������� initialMoveSpeed
        Debug.Log($"Environment speed set to: {moveSpeed}");
    }
    IEnumerator LevelRoutine()
    {
        Level currentLevel = levels[currentLevelIndex];

        Debug.Log($"Starting level {currentLevelIndex}");
        musicManager.SetLocationMusic(currentLevelIndex);
        elapsedTime = 0f;
        bossStarted = false;

        if (currentLevelIndex > 0)
        {
            SpawnConnectorSegment(levels[currentLevelIndex - 1].connectorPrefab);
            Debug.Log($"Spawned connector segment for level {currentLevelIndex}");
        }

        ResumeMovement();
        enemyWaveGenerator.StartEnemyGeneration(currentLevelIndex);
        Debug.Log($"Started enemy generation for level {currentLevelIndex}");

        isLevelChanging = false;

        while (true) // ����������� ���� ��� ���������� ������
        {
            levelTimerCoroutine = StartCoroutine(LevelTimer(currentLevel.duration));

            if (currentLevel.hasBoss)
            {
                yield return new WaitUntil(() => bossStarted);
                yield return new WaitUntil(() => !bossStarted);
            }
            else
            {
                yield return new WaitForSeconds(currentLevel.duration);
            }

            if (currentLevelIndex < levels.Count - 1)
            {
                break; // ����� �� �����, ���� ��� �� ��������� �������
            }

            // ����� ��� ���������� ���������� ������
            elapsedTime = 0f;
            bossStarted = false;
            Debug.Log($"Repeating last level {currentLevelIndex}");
        }

        Debug.Log($"Level {currentLevelIndex} completed.");
        StartNextLevel();
    }

    IEnumerator LevelTimer(float duration)
    {
        float remainingTime = duration;
        while (remainingTime > 0 && !bossStarted)
        {
            yield return null;
            remainingTime -= Time.deltaTime;
            elapsedTime = duration - remainingTime;

            if (levels[currentLevelIndex].hasBoss && !bossStarted && remainingTime <= duration * (1 - bossAppearancePercentage))
            {
                StartBossFight();
            }
        }

        Debug.Log($"Level {currentLevelIndex} timer completed. Remaining time: {remainingTime}");
    }

    void StartBossFight()
    {
        Debug.Log($"Starting boss fight on level {currentLevelIndex}");
        StartReducingSpeed();
        enemyWaveGenerator.StopEnemyGeneration();
        enemyWaveGenerator.StartBossFight(levels[currentLevelIndex].bossPrefab);
        bossStarted = true;
        musicManager.SetBossMusic(currentLevelIndex);
    }

   void SpawnNextSegment()
    {
        if (currentLevelIndex < 0 || currentLevelIndex >= levels.Count)
        {
            Debug.LogWarning($"Cannot spawn segment: Invalid currentLevelIndex: {currentLevelIndex}");
            return;
        }

        Level currentLevel = levels[currentLevelIndex];
        GameObject newSegment = Instantiate(currentLevel.prefab);

        if (lastSegment == null)
        {
            Debug.LogError("lastSegment is null!");
            return;
        }

        Transform endZone = lastSegment.transform.Find("EndZone");
        Transform startZone = newSegment.transform.Find("StartZone");

        if (endZone == null || startZone == null)
        {
            Debug.LogError("EndZone or StartZone not found!");
            Destroy(newSegment);
            return;
        }

        newSegment.transform.position = endZone.position - startZone.localPosition;
        newSegment.transform.rotation = lastSegment.transform.rotation;

        // ��������� ������� NatureGenerator �� ����� ��������
        NatureGenerator segmentNatureGenerator = newSegment.GetComponent<NatureGenerator>();
        if (segmentNatureGenerator != null)
        {
            segmentNatureGenerator.GenerateNatureForSegment(newSegment);
        }
        else
        {
            Debug.Log($"NatureGenerator not found on segment for level {currentLevelIndex}");
        }
        // �������� ������������� ����
        ArcGeneration[] arcGenerators = newSegment.GetComponents<ArcGeneration>();
        foreach (var arcGenerator in arcGenerators)
        {
            arcGenerator.TryGenerateArcs();
        }
        spawnedSegments.Add(newSegment);
        lastSegment = newSegment;

        Debug.Log($"Spawned new segment for level {currentLevelIndex}");
    }
    void SpawnConnectorSegment(GameObject connectorPrefab)
    {
        if (connectorPrefab == null)
        {
            Debug.LogWarning("Connector prefab is null. Skipping connector segment.");
            return;
        }

        GameObject connector = Instantiate(connectorPrefab);

        Transform endZone = lastSegment.transform.Find("EndZone");
        Transform startZone = connector.transform.Find("StartZone");

        if (endZone == null || startZone == null)
        {
            Debug.LogError("EndZone or StartZone not found in connector segment!");
            Destroy(connector);
            return;
        }

        connector.transform.position = endZone.position - startZone.localPosition;
        connector.transform.rotation = lastSegment.transform.rotation;

        spawnedSegments.Add(connector);
        lastSegment = connector;
    }

    void MoveEnvironment()
    {
        if (!isMoving) return; // ��������� ��������, ����� ������������� ��������, ���� isMoving == false

        foreach (GameObject segment in spawnedSegments)
        {
            segment.transform.Translate(Vector3.back * moveSpeed * Time.deltaTime);
        }
    }

    void CheckAndDestroySegments()
    {
        for (int i = spawnedSegments.Count - 1; i >= 0; i--)
        {
            GameObject segment = spawnedSegments[i];
            if (IsOutOfCameraView(segment))
            {
                spawnedSegments.RemoveAt(i);
                Destroy(segment);
                Debug.Log($"Destroyed out-of-view segment. Remaining segments: {spawnedSegments.Count}");
            }
        }

        if (spawnedSegments.Count < initialSegments)
        {
            SpawnNextSegment();
        }
    }

    private bool IsOutOfCameraView(GameObject obj)
    {
        return CameraUtility.IsOutOfCameraView(obj, mainCamera, offScreenOffset);
    }

    public void OnTriggerEntered()
    {
        SpawnNextSegment();
    }

  

    public void OnBossDefeated()
    {
        Debug.Log($"Boss defeated on level {currentLevelIndex}");
        bossStarted = false;
        if (levelTimerCoroutine != null)
        {
            StopCoroutine(levelTimerCoroutine);
        }
        StartCoroutine(DelayedNextLevel());
    }

    private IEnumerator DelayedNextLevel()
    {
        yield return new WaitForSeconds(2f); // ��������� �������� ����� ��������� � ���������� ������
        StartNextLevel();
    }

    public void ResumeMovement()
    {
        isMoving = true;
        initialMoveSpeed = moveSpeed;
        Debug.Log($"Resuming movement. Speed set to {moveSpeed}");
        if (!bossStarted)
        {
            enemyWaveGenerator.ResumeEnemyGeneration();
            Debug.Log("Resumed enemy generation");
        }
    }
    public void StopWorldOnPlayerDeath()
    {
        StopMovement();
        StopAllCoroutines();
        if (enemyWaveGenerator != null)
        {
            enemyWaveGenerator.StopEnemyGeneration();
        }
        if (musicManager != null)
        {
         //   musicManager.StopMusic(); // ��������������, ��� � ��� ���� ����� StopMusic � MusicManager
        }
        Debug.Log("World stopped due to player death");
    }

    public void StopMovement()
    {
        isMoving = false;
        moveSpeed = 0f; // ������������� �������� � 0
        Debug.Log("Environment movement stopped");
    }
    void StartReducingSpeed()
    {
        StartCoroutine(ReduceSpeedOverTime());
    }

    IEnumerator ReduceSpeedOverTime()
    {
        while (moveSpeed > 0)
        {
            moveSpeed = Mathf.Max(0, moveSpeed - worldSpeedReductionRate * Time.deltaTime);
            yield return null;
        }
        StopMovement();
    }
}