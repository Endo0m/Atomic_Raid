
using UnityEngine;

public class SpawnFieldBuff : MonoBehaviour
{
    [SerializeField] private GameObject[] _fieldsPrefabs;

    [SerializeField] private int _timeSpawnMin;
    [SerializeField] private int _timeSpawnMax;
    [SerializeField] private float _positionXRandomMin;
    [SerializeField] private float _positionXRandomMax;
    [SerializeField] private float _positionYRandomMin;
    [SerializeField] private float _positionYRandomMax;

    private Vector3 _positionSpawn;

    private float _spawnTime;
    private float _time;
    private float _positionX;
    private float _positionY;
    private int _spawnRandomIndex;

    private void Start()
    {
        RandomTimeSpawn();
    }

    private void Update()
    {
        _time += Time.deltaTime;
        if(_time >= _spawnTime)
        {
            RandomTimeSpawn();
            RandomPositionSpawn();

            _time = 0;
            _spawnRandomIndex = Random.Range(0, _fieldsPrefabs.Length);

            Instantiate(_fieldsPrefabs[_spawnRandomIndex], _positionSpawn, Quaternion.identity);
        }
    }

    /// <summary>
    /// Устанавливаем рандомное время следующего появления поля
    /// </summary>
    private void RandomTimeSpawn()
    {
        _spawnTime = Random.Range(_timeSpawnMin, _timeSpawnMax);
    } 

    /// <summary>
    /// Устанавливаем рандомную позицию появления следующего поля
    /// </summary>
    private void RandomPositionSpawn()
    {
        _positionX = Random.Range(_positionXRandomMin, _positionXRandomMax);
        _positionY = Random.Range(_positionYRandomMin, _positionYRandomMax);

        _positionSpawn = new Vector3(_positionX, _positionY, transform.position.z);
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawCube(transform.position, new Vector3(_positionX, _positionY, 1));
    }
}
