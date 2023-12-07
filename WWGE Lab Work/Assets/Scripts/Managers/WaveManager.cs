using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }


    [SerializeField] private Transform _player;

    [SerializeField] private SpawnPosition[] _spawnPoints;
    [SerializeField] private Wave[] _waves;
    [ReadOnly] private int _currentWave = 0;


    [Header("UI")]

    [SerializeField] private WaveCounterUI _waveUI;


    // (Temp)
    private void Start()
    {
        // Start the waves.
        _currentWave = 0;
        StartCoroutine(WaveCountdown(_currentWave));
    }

    private IEnumerator WaveCountdown(int wave)
    {
        _waveUI.SetWaveText(_currentWave + 1, _waves.Length);
        
        float waveTimeRemaining = _waves[wave].WaveTimer;
        while (waveTimeRemaining > 0)
        {
            _waveUI.SetTimer(waveTimeRemaining);
            
            waveTimeRemaining -= Time.deltaTime;
            yield return null;
        }

        _waveUI.HideTimer();
        StartCoroutine(HandleWave());
        //_currentWave++;
        //_waveUI.SetWaveText(_currentWave + 1, _waves.Length);
    }


    private IEnumerator HandleWave()
    {
        Wave wave = _waves[_currentWave];
        
        List<GameObject> currentEnemies = new List<GameObject>();
        int spawns = 0;

        // Loop until all enemies have been spawned.
        do
        {
            // Loop for each enemy we should spawn.
            for (int i = 0; i < wave.SpawnCount; i++)
            {
                if (spawns >= wave.TotalSpawns)
                    break;
                
                // Get a random enemy from the current wave's available enemies.
                int enemyIndex = Random.Range(0, wave.WaveContents.Length);

                // Spawn the chosen enemy & add it to the currentEnemies list.
                currentEnemies.Add(SpawnEnemy(prefab: wave.WaveContents[enemyIndex].EnemyPrefab));

                // Decrement the spawns remaining for that spawn & increment the total spawns made.
                wave.WaveContents[enemyIndex].Count--;
                spawns++;
            }

            
            // Wait until time elapsed OR enemies are all dead.
            float pauseTimeRemaining = wave.TimeBetweenSpawns;
            yield return new WaitUntil(() =>
            {
                pauseTimeRemaining -= Time.deltaTime;
                currentEnemies.RemoveAll(enemy => enemy == null);
                Debug.Log(string.Format("Time Remaining: {0}. Enemies Remaining: {1}", pauseTimeRemaining, currentEnemies.Count));
                return currentEnemies.Count <= 0 || pauseTimeRemaining <= 0;
            });
        }
        while (spawns < wave.TotalSpawns);

        // Wait until all enemies are dead before ending the wave.
        yield return new WaitUntil(() =>
        {
            currentEnemies.RemoveAll(enemy => enemy == null);
            return currentEnemies.Count <= 0;
        });



        // The wave has ended.
        Debug.Log("Wave " + _currentWave + " has ended");

        // Start the next wave.
        _currentWave++;
        if (_currentWave < _waves.Length)
        {
            StartCoroutine(WaveCountdown(_currentWave));
        }
    }

    // Instantiate an enemy at a random spawn position & set their first target.
    private GameObject SpawnEnemy(GameObject prefab)
    {
        // Select a spawn position.
        SpawnPosition selectedSpawn = _spawnPoints[Random.Range(0, _spawnPoints.Length)];
        
        // Instantiate the enemy.
        Enemy enemy = Instantiate(prefab, selectedSpawn.SpawnPos, Quaternion.identity).GetComponent<Enemy>();

        // Set the Target & InitialTarget of the instantiated enemy.
        enemy.SetTarget(_player);
        enemy.SetInitialTarget(selectedSpawn.InitialTarget);

        return enemy.gameObject;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        for (int i = 0; i < _spawnPoints.Length; i++)
        {
            Gizmos.DrawWireSphere(_spawnPoints[i].SpawnPos, 1.5f);
        }
    }


    [System.Serializable]
    struct SpawnPosition
    {
        public Vector3 SpawnPos;
        public Transform InitialTarget;
    }
    [System.Serializable]
    struct Wave
    {
        public float WaveTimer;

        [Space(5)]

        public WaveContent[] WaveContents;
        public int TotalSpawns
        {
            get
            {
                // If we haven't yet cached the total spawns, then cache it.
                if (_cachedTotalSpawns == 0)
                {
                    _cachedTotalSpawns = 0;
                    for (int i = 0; i < WaveContents.Length; i++)
                        _cachedTotalSpawns += WaveContents[i].Count;
                }

                return _cachedTotalSpawns;
            }
        }
        private int _cachedTotalSpawns;

        public int SpawnCount;
        public float TimeBetweenSpawns;


        [System.Serializable]
        public struct WaveContent
        {
            public string name;
            public GameObject EnemyPrefab;
            [Min(1)] public int Count;
        }
    }
}