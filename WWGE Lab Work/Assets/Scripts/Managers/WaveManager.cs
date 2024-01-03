using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary> A Singleton Manager script that handles waves and the spawning of enemies.</summary>
public class WaveManager : MonoBehaviour
{
    // Singleton.
    public static WaveManager Instance;
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }


    [Header("Spawn Points")]
    [SerializeField] private EnemySpawn[] _spawnPoints;


    [Header("Waves")]
    [SerializeField] private SpawnWave[] _waves;
    [SerializeField] private float _timeBetweenWaves = 10f;
    [ReadOnly, SerializeField] private int _currentWave; // Note: Current waves starts at 1, so subtract 1 when accessing arrays.

    public static event System.Action OnFinalWaveCompleted;

    // Accessors
    public int CurrentWave { get => _currentWave; }
    public int MaxWaves { get => _waves.Length; }


    [Header("UI")]
    [SerializeField] private WaveCounterUI _waveUI;


    private void Start()
    {
        // Start the first wave.
        _currentWave = 1;
        StartCoroutine(WaveCountdown());
    }

    private IEnumerator WaveCountdown()
    {
        // Notify the UI that the wave countdown has started.
        _waveUI.SetWaveText(_currentWave, _waves.Length);

        // Wait until the wave delay has elapsed.
        for (float timeRemaining = _timeBetweenWaves; timeRemaining > 0; timeRemaining -= Time.deltaTime)
        {
            // Update the UI.
            _waveUI.SetTimer(timeRemaining);

            // Wait for the next frame.
            yield return null;
        }

        // Notify the wave UI to hide itself.
        _waveUI.HideTimer();

        // Start the wave.
        StartCoroutine(HandleWave());
    }


    private IEnumerator HandleWave()
    {
        // Cache the current wave.
        SpawnWave wave = _waves[_currentWave - 1];

        int spawns = 0;
        List<GameObject> aliveEnemies = new List<GameObject>();

        // Loop until all enemies have been spawned.
        while (spawns < wave.TotalSpawns)
        {
            // Loop for each enemy we should spawn.
            for (int i = 0; i < wave.SpawnsPerTrigger; i++)
            {
                // If we have spawned enough enemies, then break out of the loop.
                if (spawns > wave.TotalSpawns)
                    break;

                // Get a random enemy from the current wave's available enemies.
                int enemyIndex = Random.Range(0, wave.WaveContents.Where(content => content.ValidSpawn).Count());

                // Spawn the chosen enemy & add it to the aliveEnemies list.
                aliveEnemies.Add(SpawnEnemy(wave.WaveContents[enemyIndex].EnemyPrefab));
                wave.WaveContents[enemyIndex].EnemySpawned();

                // Increment the total spawns made.
                spawns++;
            }

            // Wait until the time between spawns has elapsed OR all enemies are dead.
            float pauseTimeRemaining = wave.TimeBetweenSpawns;
            yield return new WaitUntil(() =>
            {
                pauseTimeRemaining -= Time.deltaTime;
                aliveEnemies.RemoveAll(enemy => enemy == null);

                return aliveEnemies.Count < 0 || pauseTimeRemaining <= 0;
            });
        }


        // Wait until all enemies are dead before ending the wave.
        yield return new WaitUntil(() =>
        {
            aliveEnemies.RemoveAll(enemy => enemy == null);
            return aliveEnemies.Count <= 0;
        });

        // The wave has ended.
        // Start the countdown for the next wave.
        _currentWave++;
        if (_currentWave <= _waves.Length)
            StartCoroutine(WaveCountdown());
        else
        {
            Debug.Log("Defeated All Waves");
            OnFinalWaveCompleted?.Invoke();
        }
    }

    /// <summary> Instantiate and Setup an enemy from a prefab.</summary>
    private GameObject SpawnEnemy(GameObject prefab)
    {
        // Select a spawn position.
        EnemySpawn selectedSpawn = _spawnPoints[Random.Range(0, _spawnPoints.Length)];

        // Instantiate the enemy.
        GameObject enemyGO = Instantiate(prefab, selectedSpawn.SpawnPosition, Quaternion.identity);
        enemyGO.SetActive(true);

        // Set the InitialTarget of the instantiated enemy.
        enemyGO.GetComponent<SpawnableEntity>().SetInitialTarget(selectedSpawn.AssociatedBarrier.transform);

        // Return the setup enemy.
        return enemyGO;
    }



    #region Structs
    [System.Serializable]
    private struct SpawnWave
    {
        public string name;

        // Wave Contents.
        [System.Serializable]
        public struct WaveContent
        {
            public GameObject EnemyPrefab;
            [Min(1)] public int Count;
            private int _spawns;

            public bool ValidSpawn => _spawns < Count;
            public void EnemySpawned() => _spawns++;
        }

        public WaveContent[] WaveContents;
        private int _cachedTotalSpawns;
        public int TotalSpawns
        {
            get
            {
                // If we have not yet cached the total spawns, calculate and cache it.
                if (_cachedTotalSpawns == 0)
                {
                    _cachedTotalSpawns = 0;
                    for (int i = 0; i < WaveContents.Length; i++)
                        _cachedTotalSpawns += WaveContents[i].Count;
                }

                // Return the cached value.
                return _cachedTotalSpawns;
            }
        }


        // Timing Parameters.
        public float TimeBetweenSpawns;
        public int SpawnsPerTrigger;
    }
    #endregion
}