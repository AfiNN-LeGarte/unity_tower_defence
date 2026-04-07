using UnityEngine;
using System.Linq;

public class TowerSystem : BaseSystem
{
    public GameSettings Settings;

    public override void Execute()
    {
        if (Settings == null) return;

        var gameState = World.Query<GameStateComponent>().FirstOrDefault();
        if (gameState?.Get<GameStateComponent>().IsGameOver == true) return;

        foreach (var tower in World.Query<TowerComponent, PositionComponent>().ToList())
        {
            var tComp = tower.Get<TowerComponent>();
            var tPos = tower.Get<PositionComponent>();

            tComp.CooldownTimer -= Time.deltaTime;
            if (tComp.CooldownTimer > 0) continue;

            Entity target = null;
            float minDist = tComp.Range;

            foreach (var enemy in World.Query<EnemyComponent, PositionComponent, HealthComponent>().ToList())
            {
                if (enemy.Get<HealthComponent>().Current <= 0) continue;
                float dist = Vector3.Distance(tPos.Value, enemy.Get<PositionComponent>().Value);
                if (dist < minDist)
                {
                    minDist = dist;
                    target = enemy;
                }
            }

            if (target != null)
            {
                tComp.TargetId = target.Id;
                tComp.CooldownTimer = tComp.Cooldown;

                var proj = World.CreateEntity();
                proj.Add(new PositionComponent { Value = tPos.Value });
                proj.Add(new ProjectileComponent { TargetId = target.Id, Damage = tComp.Damage });
                proj.Add(new MovementComponent { Speed = 20f, Direction = Vector3.forward, IsMoving = true });

                if (PrefabDatabase.ProjectilePrefab != null)
                    proj.Add(new UnityObjectComponent { Obj = Object.Instantiate(PrefabDatabase.ProjectilePrefab, tPos.Value, Quaternion.identity) });
            }
        }
    }
}
