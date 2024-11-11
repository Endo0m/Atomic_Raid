using UnityEngine;
public class TriggerFly : MonoBehaviour
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
        if (_flashlightController == null) Debug.LogError("FlashlightController not found!");
        if (_cameraModeController == null) Debug.LogError("CameraModeController not found!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SetFlyingMode();
        }
    }

    private void SetFlyingMode()
    {
        if (_playerMove != null) _playerMove.SetRunningMode(false);
        if (_playerShoot != null) _playerShoot.SetShootingEnabled(true);
        if (_enemyWaveGenerator != null && _environmentGenerator != null)
        {
            _enemyWaveGenerator.StartEnemyGeneration(_environmentGenerator.GetCurrentLevel());
        }
        if (_environmentGenerator != null) _environmentGenerator.SetEnvironmentSpeed(false);
        if (_fieldSpawn != null) _fieldSpawn.SetSpawningEnabled(true);
        if (_flashlightController != null) _flashlightController.TurnOff();
        // Камера и объекты теперь контролируются автоматически через CameraModeController
        Debug.Log("Flying mode activated. Shooting enabled. Enemy generation resumed. Flashlight turned off.");
    }
}