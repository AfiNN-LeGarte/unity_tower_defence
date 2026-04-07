using UnityEngine;
using System.Linq;

public class ProjectileSystem : BaseSystem
{
    public GameSettings Settings;

    public override void Execute()
    {
        if (Settings == null) return;

        foreach (var proj in World.Query<ProjectileComponent, PositionComponent, UnityObjectComponent>().ToList())
        {
            var pComp = proj.Get<ProjectileComponent>();
            var pPos = proj.Get<PositionComponent>();
            var pView = proj.Get<UnityObjectComponent>();

            if (pView.Obj == null || pPos.Value.y < Settings.DestroyY)
            {
                if (pView.Obj != null) Object.Destroy(pView.Obj);
                World.DestroyEntity(proj);
                continue;
            }

            var target = World.GetAll().FirstOrDefault(e => e.Id == pComp.TargetId && e.IsActive && e.Has<HealthComponent>());
            if (target == null || target.Get<HealthComponent>().Current <= 0)
            {
                if (pView.Obj != null) Object.Destroy(pView.Obj);
                World.DestroyEntity(proj);
                continue;
            }

            float dist = Vector3.Distance(pPos.Value, target.Get<PositionComponent>().Value);
            if (dist < Settings.HitDistance)
            {
                var health = target.Get<HealthComponent>();
                health.Current -= pComp.Damage;

                if (health.Current <= 0 && target.Has<EnemyComponent>())
                {
                    var player = World.Query<PlayerComponent>().FirstOrDefault();
                    if (player != null) player.Get<PlayerComponent>().Gold += target.Get<EnemyComponent>().Reward;
                }

                if (pView.Obj != null) Object.Destroy(pView.Obj);
                World.DestroyEntity(proj);
            }
        }
    }
}
