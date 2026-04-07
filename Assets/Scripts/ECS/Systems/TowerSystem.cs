using UnityEngine;
using System.Linq;

public class TowerSystem : BaseSystem
{
    public GameObject ProjectilePrefab;
    public GameSettings Settings;

    
    public override void Execute()
    {

        var gameState = World.Query<GameStateComponent>().FirstOrDefault();
        if (gameState != null && gameState.Get<GameStateComponent>().IsGameOver)
            return;

        foreach (var tower in World.Query<TowerComponent, PositionComponent>().ToList())
        {
            var tComp = tower.Get<TowerComponent>();
            var tPos = tower.Get<PositionComponent>();

            tComp.CooldownTimer -= Time.deltaTime;
            if (tComp.CooldownTimer > 0) continue;

            int targetId = -1;
            float minDist = tComp.Range;


            var enemies = World.Query<EnemyComponent, PositionComponent, HealthComponent>().ToList();
            
            foreach (var enemy in enemies)
            {
                var ePos = enemy.Get<PositionComponent>();
                var eHealth = enemy.Get<HealthComponent>();
                
                if (eHealth.Current <= 0) continue;

                var dist = Vector3.Distance(tPos.Value, ePos.Value);
                
                if (dist <= minDist)
                {
                    minDist = dist;
                    targetId = enemy.Id;
                }
            }


            if (targetId != -1)
            {
                tComp.TargetId = targetId;
                tComp.CooldownTimer = tComp.Cooldown;

                var projectile = World.CreateEntity();
                projectile.Add(new PositionComponent { Value = tPos.Value });
                projectile.Add(new RotationComponent { Value = Quaternion.identity });
                projectile.Add(new MovementComponent 
                { 
                    Speed = 20f,
                    Direction = Vector3.forward,
                    IsMoving = true
                });
                projectile.Add(new ProjectileComponent 
                { 
                    TargetId = targetId,
                    Damage = tComp.Damage
                });

                if (ProjectilePrefab != null)
                {
                    var obj = Object.Instantiate(ProjectilePrefab, tPos.Value, Quaternion.identity);
                    projectile.Add(new UnityObjectComponent { Obj = obj });
                }
            }
        }
    }
}