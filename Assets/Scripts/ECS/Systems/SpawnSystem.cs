using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class SpawnSystem : BaseSystem
{
    public List<GameObject> PathPoints;
    public GameSettings Settings;

    private float spawnTimer;
    private float spawnDelay = 0.5f;
    private bool isSpawning;
    private int spawnedInWave;
    private int currentWaveEnemies;

    public override void Execute()
    {
        if (Settings == null) return;

        var waveEntity = World.Query<WaveComponent>().FirstOrDefault();
        if (waveEntity == null) return;
        var wave = waveEntity.Get<WaveComponent>();

        if (!wave.WaveActive)
        {
            wave.WaveTimer -= Time.deltaTime;
            if (wave.WaveTimer <= 0 && wave.CurrentWave <= wave.TotalWaves)
            {
                wave.WaveActive = true;
                currentWaveEnemies = GetEnemiesForWave(wave.CurrentWave);
                spawnedInWave = 0;
                isSpawning = true;
                spawnTimer = 0f;
            }
            return;
        }

        if (isSpawning && spawnedInWave < currentWaveEnemies)
        {
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0)
            {
                SpawnEnemy(wave.CurrentWave);
                spawnedInWave++;
                spawnTimer = spawnDelay;
            }
        }
        else
        {
            if (World.Query<EnemyComponent>().Count() == 0)
            {
                wave.WaveActive = false;
                wave.CurrentWave++;
                wave.WaveTimer = wave.WaveInterval;
                isSpawning = false;
                spawnedInWave = 0;
            }
        }
    }

    int GetEnemiesForWave(int waveNum) => waveNum switch
    {
        1 => Settings.Wave1Enemies,
        2 => Settings.Wave2Enemies,
        3 => Settings.Wave3Enemies,
        _ => 5
    };

    void SpawnEnemy(int wave)
    {
        var entity = World.CreateEntity();
        var waypoints = new Queue<Vector3>(PathPoints.Select(p => p.transform.position));
        var config = GetEnemyConfigForWave(wave);

        Vector3 startPos = waypoints.Peek();
        
        entity.Add(new PositionComponent { Value = startPos });
        entity.Add(new RotationComponent { Value = Quaternion.Euler(0, 90, 0) });
        entity.Add(new HealthComponent { Current = config.Health, Max = config.Health });
        entity.Add(new MovementComponent { 
            Speed = config.Speed, 
            RotationSpeed = config.RotationSpeed, 
            IsMoving = true, 
            Direction = Vector3.forward 
        });
        entity.Add(new EnemyComponent { Reward = config.Reward, Wave = wave });
        entity.Add(new PathComponent { Waypoints = waypoints, CurrentIndex = 0 });
        entity.Add(new AvoidanceComponent { 
            AvoidRadius = Settings.AvoidanceRadius, 
            AvoidStrength = Settings.AvoidanceStrength, 
            AvoidDirection = Vector3.zero 
        });

        if (PrefabDatabase.EnemyPrefabs.TryGetValue(wave, out var prefabs) && prefabs.Count > 0)
            entity.Add(new UnityObjectComponent { 
                Obj = Object.Instantiate(prefabs[0], startPos, Quaternion.Euler(0, 90, 0)) 
            });

        waypoints.Dequeue();
    }

    EnemyWaveConfig GetEnemyConfigForWave(int wave)
    {
        foreach (var c in Settings.EnemyConfigs)
            if (c.WaveNumber == wave) return c;
        return new EnemyWaveConfig { WaveNumber = wave, Health = 50f, Speed = 3f, Reward = 10, RotationSpeed = 10f };
    }
}