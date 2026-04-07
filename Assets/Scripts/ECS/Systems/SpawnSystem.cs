using UnityEngine;
using System.Collections.Generic;
using System.Linq;





public class SpawnSystem : BaseSystem
{
    public List<GameObject> PathPoints;
    public Dictionary<int, List<GameObject>> EnemyPrefabs;
    public GameSettings Settings; // ✅ Настройки


    private float _spawnTimer = 0f;
    private float _spawnDelay = 0.5f;
    

    private bool _isSpawning = false;
    private int _currentWaveEnemies = 0;
    private int _spawnedInWave = 0;
    private int _currentWaveNum = 1;

    public override void Execute()
    {
        var waveEntity = World.Query<WaveComponent>().FirstOrDefault();
        if (waveEntity == null) return;

        var wave = waveEntity.Get<WaveComponent>();


        if (!wave.WaveActive)
        {
            wave.WaveTimer -= Time.deltaTime;
            
            if (wave.WaveTimer <= 0 && wave.CurrentWave <= wave.TotalWaves)
            {
                wave.WaveActive = true;
                _currentWaveNum = wave.CurrentWave;
                _currentWaveEnemies = GetEnemiesForWave(wave.CurrentWave);
                _spawnedInWave = 0;
                _isSpawning = true;
                _spawnTimer = 0f;
                
                Debug.Log($"🌊 Волна {wave.CurrentWave} началась! Врагов: {_currentWaveEnemies}");
            }
            return;
        }


        if (_isSpawning && _spawnedInWave < _currentWaveEnemies)
        {
            _spawnTimer -= Time.deltaTime;
            
            if (_spawnTimer <= 0)
            {
                SpawnEnemy(_currentWaveNum);
                _spawnedInWave++;
                _spawnTimer = _spawnDelay;
                
                Debug.Log($"👾 Враг {_spawnedInWave}/{_currentWaveEnemies} заспавнен");
            }
        }
        else if (_isSpawning && _spawnedInWave >= _currentWaveEnemies)
        {
            _isSpawning = false;
            
            var enemies = World.Query<EnemyComponent>().Count();
            if (enemies == 0)
            {
                wave.WaveActive = false;
                wave.CurrentWave++;
                wave.WaveTimer = wave.WaveInterval;
                
                if (wave.CurrentWave > wave.TotalWaves)
                {
                    Debug.Log("🎉 Все волны пройдены!");
                }
                else
                {
                    Debug.Log($"⏳ Следующая волна через {wave.WaveInterval}с");
                }
            }
        }
        else if (!_isSpawning)
        {
            var enemies = World.Query<EnemyComponent>().Count();
            if (enemies == 0)
            {
                wave.WaveActive = false;
                wave.CurrentWave++;
                wave.WaveTimer = wave.WaveInterval;
                
                if (wave.CurrentWave > wave.TotalWaves)
                {
                    Debug.Log("🎉 Все волны пройдены!");
                }
                else
                {
                    Debug.Log($"⏳ Следующая волна через {wave.WaveInterval}с");
                }
            }
        }
    }


    int GetEnemiesForWave(int waveNum)
    {
        return waveNum switch
        {
            1 => Settings.Wave1Enemies,
            2 => Settings.Wave2Enemies,
            3 => Settings.Wave3Enemies,
            _ => 5
        };
    }

    void SpawnEnemy(int wave)
    {
        var entity = World.CreateEntity();

        var waypoints = new Queue<Vector3>(PathPoints.Select(p => p.transform.position));
        var startPos = waypoints.Peek();
        var scatteredPos = GetScatteredSpawnPosition(startPos);

        waypoints.Dequeue();


        var enemyConfig = GetEnemyConfigForWave(wave);

        entity.Add(new PositionComponent { Value = scatteredPos });
        entity.Add(new RotationComponent { Value = Quaternion.Euler(0, 90, 0) });
        entity.Add(new HealthComponent { Current = enemyConfig.Health, Max = enemyConfig.Health });
        entity.Add(new MovementComponent 
        { 
            Speed = enemyConfig.Speed, 
            RotationSpeed = enemyConfig.RotationSpeed,
            IsMoving = true,
            Direction = Vector3.forward
        });
        entity.Add(new EnemyComponent { Reward = enemyConfig.Reward, Wave = wave });
        entity.Add(new PathComponent { Waypoints = waypoints, CurrentIndex = 0 });
        

        entity.Add(new AvoidanceComponent 
        { 
            AvoidRadius = Settings.AvoidanceRadius,
            AvoidStrength = Settings.AvoidanceStrength,
            AvoidDirection = Vector3.zero
        });


        if (EnemyPrefabs.TryGetValue(wave, out var prefabs) && prefabs.Count > 0)
        {
            var obj = Object.Instantiate(prefabs[0]);
            obj.transform.position = scatteredPos;
            obj.transform.rotation = Quaternion.Euler(0, 90, 0);
            entity.Add(new UnityObjectComponent { Obj = obj });
        }
    }


    EnemyWaveConfig GetEnemyConfigForWave(int wave)
    {
        foreach (var config in Settings.EnemyConfigs)
        {
            if (config.WaveNumber == wave)
                return config;
        }
        

        Debug.LogWarning($"⚠️ EnemyConfig для волны {wave} не найден! Используем значения по умолчанию.");
        return new EnemyWaveConfig 
        { 
            WaveNumber = wave, 
            Health = 50f, 
            Speed = 3f, 
            Reward = 10,
            RotationSpeed = 10f
        };
    }


    Vector3 GetScatteredSpawnPosition(Vector3 center)
    {
        float scatterRadius = 5f;
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(0.5f, scatterRadius);
        float offsetX = Mathf.Cos(angle) * distance;
        float offsetZ = Mathf.Sin(angle) * distance;
        return new Vector3(center.x + offsetX, center.y, center.z + offsetZ);
    }

    public override void Cleanup()
    {
        _isSpawning = false;
        _spawnedInWave = 0;
        _currentWaveEnemies = 0;
    }
}