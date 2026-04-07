using UnityEngine;
using System.Linq;

public class MovementSystem : BaseSystem
{
    public GameSettings Settings;

    public override void Execute()
    {
        if (Settings == null) return;

        // Снаряды
        foreach (var proj in World.Query<ProjectileComponent>().ToList())
        {
            if (!proj.Has<PositionComponent>() || !proj.Has<MovementComponent>()) continue;

            var pComp = proj.Get<ProjectileComponent>();
            var pPos = proj.Get<PositionComponent>();
            var pMove = proj.Get<MovementComponent>();

            var target = World.GetAll().FirstOrDefault(e => 
                e.Id == pComp.TargetId && 
                e.IsActive && 
                e.Has<PositionComponent>());

            if (target != null)
                pMove.Direction = (target.Get<PositionComponent>().Value - pPos.Value).normalized;

            pPos.Value += pMove.Direction * pMove.Speed * Time.deltaTime;
        }

        // Враги
        var towers = World.Query<TowerComponent>().ToList();

        foreach (var entity in World.Query<EnemyComponent>().ToList())
        {
            if (!entity.Has<PositionComponent>() || 
                !entity.Has<MovementComponent>() || 
                !entity.Has<PathComponent>() || 
                !entity.Has<AvoidanceComponent>()) continue;

            var pos = entity.Get<PositionComponent>();
            var move = entity.Get<MovementComponent>();
            var path = entity.Get<PathComponent>();
            var avoid = entity.Get<AvoidanceComponent>();

            if (path.Waypoints == null || path.Waypoints.Count == 0)
            {
                var player = World.Query<PlayerComponent>().FirstOrDefault();
                if (player != null) 
                    player.Get<PlayerComponent>().Lives--;
                World.DestroyEntity(entity);
                continue;
            }

            Vector3 targetPos = path.Waypoints.Peek();
            
            Vector3 flatPos = new Vector3(pos.Value.x, 0, pos.Value.z);
            Vector3 flatTarget = new Vector3(targetPos.x, 0, targetPos.z);
            Vector3 dir = (flatTarget - flatPos).normalized;

            // Избегание башен
            foreach (var tower in towers)
            {
                if (!tower.Has<PositionComponent>()) continue;

                float dist = Vector3.Distance(flatPos, new Vector3(
                    tower.Get<PositionComponent>().Value.x, 
                    0, 
                    tower.Get<PositionComponent>().Value.z));

                if (dist < avoid.AvoidRadius)
                {
                    Vector3 away = (flatPos - new Vector3(
                        tower.Get<PositionComponent>().Value.x, 
                        0, 
                        tower.Get<PositionComponent>().Value.z)).normalized;
                    dir += away * (1f - dist / avoid.AvoidRadius) * avoid.AvoidStrength;
                }
            }

            dir.Normalize();
            
            pos.Value += dir * move.Speed * Time.deltaTime;
            pos.Value = new Vector3(pos.Value.x, pos.Value.y, pos.Value.z);

            if (entity.Has<RotationComponent>())
            {
                var rot = entity.Get<RotationComponent>();
                rot.Value = Quaternion.Slerp(rot.Value, Quaternion.LookRotation(dir), move.RotationSpeed * Time.deltaTime);
            }

            float distToTarget = Vector3.Distance(flatPos, flatTarget);
            if (distToTarget < Settings.PathCheckDistance)
                path.Waypoints.Dequeue();
        }
    }
}