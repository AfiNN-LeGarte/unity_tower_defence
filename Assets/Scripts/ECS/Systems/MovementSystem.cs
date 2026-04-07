using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class MovementSystem : BaseSystem
{
    public GameSettings Settings;
    

    private float _globalNoiseOffset = 0f;

    public override void Execute()
    {

        _globalNoiseOffset += Time.deltaTime * 0.5f;


        var towers = World.Query<TowerComponent, PositionComponent>().ToList();


        foreach (var entity in World.Query<PositionComponent, MovementComponent, ProjectileComponent>().ToList())
        {
            var pos = entity.Get<PositionComponent>();
            var move = entity.Get<MovementComponent>();
            var proj = entity.Get<ProjectileComponent>();

            var target = World.GetAll().FirstOrDefault(e => 
                e.IsActive && e.Id == proj.TargetId && e.Has<PositionComponent>());

            if (target != null)
            {
                var targetPos = target.Get<PositionComponent>();
                move.Direction = (targetPos.Value - pos.Value).normalized;
            }

            if (move.IsMoving && move.Direction.magnitude > 0.001f)
            {
                pos.Value += move.Direction * move.Speed * Time.deltaTime;
            }
        }


        foreach (var entity in World.Query<PositionComponent, MovementComponent, PathComponent, EnemyComponent, AvoidanceComponent>().ToList())
        {
            var pos = entity.Get<PositionComponent>();
            var move = entity.Get<MovementComponent>();
            var path = entity.Get<PathComponent>();
            var avoidance = entity.Get<AvoidanceComponent>();

            if (path.Waypoints == null || path.Waypoints.Count == 0)
            {
                var player = World.Query<PlayerComponent>().FirstOrDefault();
                if (player != null)
                {
                    player.Get<PlayerComponent>().Lives--;
                }
                World.DestroyEntity(entity);
                continue;
            }


            var target = path.Waypoints.Peek();
            var pathDirection = (target - pos.Value);
            pathDirection.y = 0;
            pathDirection.Normalize();


            Vector3 avoidanceForce = Vector3.zero;
            
            foreach (var tower in towers)
            {
                var towerPos = tower.Get<PositionComponent>();
                float distToTower = Vector3.Distance(
                    new Vector3(pos.Value.x, 0, pos.Value.z), 
                    new Vector3(towerPos.Value.x, 0, towerPos.Value.z)
                );
                
                if (distToTower < avoidance.AvoidRadius)
                {
                    Vector3 awayFromTower = (pos.Value - towerPos.Value);
                    awayFromTower.y = 0;
                    awayFromTower.Normalize();
                    
                    float avoidanceStrength = (1f - distToTower / avoidance.AvoidRadius) * avoidance.AvoidStrength;
                    avoidanceForce += awayFromTower * avoidanceStrength;
                }
            }


            Vector3 swayForce = GetNaturalSway(entity.Id, Time.time);
            

            avoidance.AvoidDirection = avoidanceForce + swayForce;


            Vector3 finalDirection = (pathDirection + avoidanceForce + swayForce).normalized;
            
            finalDirection.y = 0;
            finalDirection.Normalize();


            if (move.IsMoving && move.Direction.magnitude > 0.001f)
            {
                move.Direction = finalDirection;
                pos.Value += move.Direction * move.Speed * Time.deltaTime;


                if (entity.Has<RotationComponent>())
                {
                    var rot = entity.Get<RotationComponent>();
                    Quaternion targetRot = Quaternion.LookRotation(move.Direction);
                    rot.Value = Quaternion.Slerp(rot.Value, targetRot, move.RotationSpeed * Time.deltaTime);
                }
            }


            var distToTarget = Vector3.Distance(
                new Vector3(pos.Value.x, 0, pos.Value.z), 
                new Vector3(target.x, 0, target.z)
            );
            
            if (distToTarget < Settings.PathCheckDistance)
            {
                path.Waypoints.Dequeue();
                path.CurrentIndex++;
            }
        }
    }



    Vector3 GetNaturalSway(int entityId, float time)
    {

        float uniqueOffset = entityId * 100f;
        

        float noiseX = Mathf.PerlinNoise(time * Settings.EnemySwaySpeed + uniqueOffset, 0f) * 2f - 1f;
        float noiseZ = Mathf.PerlinNoise(0f, time * Settings.EnemySwaySpeed + uniqueOffset) * 2f - 1f;
        

        float swayStrength = Settings.EnemySwayStrength;
        
        Vector3 sway = new Vector3(noiseX * swayStrength, 0, noiseZ * swayStrength);
        
        return sway;
    }
}