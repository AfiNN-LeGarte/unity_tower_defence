using UnityEngine;
using System.Linq;

public class MovementSystem : BaseSystem
{
    public GameSettings Settings;

    public override void Execute()
    {
        if (Settings == null) return;

        foreach (var proj in World.Query<ProjectileComponent, PositionComponent, MovementComponent>().ToList())
        {
            var pComp = proj.Get<ProjectileComponent>();
            var pPos = proj.Get<PositionComponent>();
            var pMove = proj.Get<MovementComponent>();

            var target = World.GetAll().FirstOrDefault(e => e.Id == pComp.TargetId && e.IsActive && e.Has<PositionComponent>());
            if (target != null)
                pMove.Direction = (target.Get<PositionComponent>().Value - pPos.Value).normalized;

            pPos.Value += pMove.Direction * pMove.Speed * Time.deltaTime;
        }

        var towers = World.Query<TowerComponent, PositionComponent>().ToList();

        foreach (var entity in World.Query<EnemyComponent, PositionComponent, MovementComponent, PathComponent, AvoidanceComponent>().ToList())
        {
            var pos = entity.Get<PositionComponent>();
            var move = entity.Get<MovementComponent>();
            var path = entity.Get<PathComponent>();
            var avoid = entity.Get<AvoidanceComponent>();

            if (path.Waypoints == null || path.Waypoints.Count == 0)
            {
                var player = World.Query<PlayerComponent>().FirstOrDefault();
                if (player != null) player.Get<PlayerComponent>().Lives--;
                World.DestroyEntity(entity);
                continue;
            }

            Vector3 targetPos = path.Waypoints.Peek();
            Vector3 dir = (targetPos - pos.Value).normalized;

            foreach (var tower in towers)
            {
                float dist = Vector3.Distance(pos.Value, tower.Get<PositionComponent>().Value);
                if (dist < avoid.AvoidRadius)
                {
                    Vector3 away = (pos.Value - tower.Get<PositionComponent>().Value).normalized;
                    dir += away * (1f - dist / avoid.AvoidRadius) * avoid.AvoidStrength;
                }
            }

            dir.Normalize();
            pos.Value += dir * move.Speed * Time.deltaTime;

            if (entity.Has<RotationComponent>())
            {
                var rot = entity.Get<RotationComponent>();
                rot.Value = Quaternion.Slerp(rot.Value, Quaternion.LookRotation(dir), move.RotationSpeed * Time.deltaTime);
            }

            if (Vector3.Distance(pos.Value, targetPos) < Settings.PathCheckDistance)
                path.Waypoints.Dequeue();
        }
    }
}
