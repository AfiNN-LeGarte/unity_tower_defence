using UnityEngine;
using System.Linq;

public class ProjectileSystem : BaseSystem
{
    public GameSettings Settings; // ✅ Настройки

    public override void Execute()
    {
        foreach (var proj in World.Query<ProjectileComponent, PositionComponent, UnityObjectComponent>().ToList())
        {
            var pComp = proj.Get<ProjectileComponent>();
            var pPos = proj.Get<PositionComponent>();
            var pView = proj.Get<UnityObjectComponent>();

            if (pView.Obj == null)
            {
                World.DestroyEntity(proj);
                continue;
            }

            var target = World.GetAll().FirstOrDefault(e => 
                e.Id == pComp.TargetId && 
                e.IsActive && 
                e.Has<PositionComponent>() && 
                e.Has<HealthComponent>());

            if (target == null)
            {
                Object.Destroy(pView.Obj);
                World.DestroyEntity(proj);
                continue;
            }

            var targetHealth = target.Get<HealthComponent>();
            if (targetHealth.Current <= 0)
            {
                Object.Destroy(pView.Obj);
                World.DestroyEntity(proj);
                continue;
            }

            var targetPos = target.Get<PositionComponent>();
            var distance = Vector3.Distance(pPos.Value, targetPos.Value);
            
            if (distance < (Settings?.HitDistance ?? 0.5f))
            {
                targetHealth.Current -= pComp.Damage;

                if (targetHealth.Current <= 0)
                {
                    var enemy = target.Get<EnemyComponent>();
                    var player = World.Query<PlayerComponent>().FirstOrDefault();
                    
                    if (player != null && enemy != null)
                    {
                        player.Get<PlayerComponent>().Gold += enemy.Reward;
                    }
                }

                Object.Destroy(pView.Obj);
                World.DestroyEntity(proj);
                continue;
            }

            if (pPos.Value.y < (Settings?.DestroyY ?? -10f))
            {
                Object.Destroy(pView.Obj);
                World.DestroyEntity(proj);
            }
        }
    }
}