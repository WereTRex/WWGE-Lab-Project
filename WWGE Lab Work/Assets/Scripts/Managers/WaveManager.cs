using System.Collections;
using System.Collections.Generic;
using System.Linq;
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


    // (Temp)
    private void Start()
    {
        StartCoroutine(HandleWave());
    }


    private IEnumerator HandleWave()
    {
        Wave wave = _waves[_currentWave];
        
        List<GameObject> currentEnemies = new List<GameObject>();
        int spawns = 0;
        // Loop until the wave has completed (All enemies have been spawned and killed).
        do
        {
            // Loop for each enemy we should spawn.
            for (int i = 0; i < wave.SpawnCount; i++)
            {
                // Get a random enemy from the current wave's available enemies.
                int enemyIndex = Random.Range(0, wave.WaveContents.Length);

                // Spawn the chosen enemy & add it to the currentEnemies list.
                currentEnemies.Add(SpawnEnemy(prefab: wave.WaveContents[enemyIndex].EnemyPrefab));

                // Decrement the spawns remaining for that spawn & increment the total spawns made.
                wave.WaveContents[enemyIndex].Count--;
                spawns++;
            }

            // If we have spawned all enemies:
            if (spawns >= wave.TotalSpawns)
            {
                // Wait until time elapsed OR enemies are all dead.
                float pauseTimeRemaining = wave.TimeBetweenSpawns;
                yield return new WaitUntil(() =>
                {
                    pauseTimeRemaining -= Time.deltaTime;
                    currentEnemies.RemoveAll(enemy => enemy == null);
                    return currentEnemies.Count <= 0 || pauseTimeRemaining <= 0;
                });
            }
            // If we haven't spawned all enemies, instead wait until we should spawn more.
            else
                yield return new WaitForSeconds(wave.TimeBetweenSpawns);
        }
        while (currentEnemies.Count > 0 && spawns < wave.TotalSpawns);

        // The wave has ended.
        Debug.Log("Wave " + _currentWave + " has ended");
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