using UnityEngine;

public class TriggerRun : MonoBehaviour
{
    private PlayerMove _playerMove;
    private PlayerShoot _playerShoot;
    private EnemyWaveGenerator _enemyWaveGenerator;
    private EnvironmentStaticGenerator _environmentGenerator;
    private FieldSpawn _fieldSpawn;
    private FlashlightController _flashlightController;
    private CameraModeController _cameraModeController;

    private void Start()
    {
        _playerMove = FindObjectOfType<PlayerMove>();
        _playerShoot = FindObjectOfType<PlayerShoot>();
        _enemyWaveGenerator = FindObjectOfType<EnemyWaveGenerator>();
        _environmentGenerator = FindObjectOfType<EnvironmentStaticGenerator>();
        _fieldSpawn = FindObjectOfType<FieldSpawn>();
        _flashlightController = FindObjectOfType<FlashlightController>();
        _cameraModeController = FindObjectOfType<CameraModeController>();
        LogMissingComponents();
    }

    private void LogMissingComponents()
    {
        if (_fieldSpawn == null) Debug.LogWarning("FieldSpawn not found!");
        if (_playerMove == null) Debug.LogError("PlayerMove not found!");
        if (_playerShoot == null) Debug.LogError("PlayerShoot not found!");
        if (_enemyWaveGenerator == null) Debug.LogError("EnemyWaveGenerator not found!");
        if (_environmentGenerator == null) Debug.LogError("EnvironmentStaticGenerator not found!");
        if (_cameraModeController == null) Debug.LogError("CameraModeController not found!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetRunningMode();
        }
    }

    private void SetRunningMode()
    {
        if (_playerMove != null) _playerMove.SetRunningMode(true);
        if (_playerShoot != null) _playerShoot.SetShootingEnabled(false);
        if (_enemyWaveGenerator != null)
        {
            _enemyWaveGenerator.StopEnemyGeneration();
            _enemyWaveGenerator.DestroyAllEnemies();
        }
        if (_environmentGenerator != null) _environmentGenerator.SetEnvironmentSpeed(true);
        if (_fieldSpawn != null) _fieldSpawn.SetSpawningEnabled(false);
        if (_flashlightController != null) _flashlightController.TurnOn();
        if (_cameraModeController != null) _cameraModeController.SetRunningMode(true);
        Debug.Log("Running mode activated. Shooting disabled. Enemy generation stopped. Camera fixed. Flashlight turned on.");
    }
}