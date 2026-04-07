using UnityEngine;
using System.Linq;

public class MovementSystem : BaseSystem
{
    public GameSettings Settings;

    public override void Execute()
    {
        if (Settings == null) return;

        var gameState = World.Query<GameStateComponent>().FirstOrDefault();
        if (gameState != null && gameState.Get<GameStateComponent>().IsGameOver)
            return;

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

        // Выносим запросы за пределы цикла для оптимизации
        var allEnemies = World.Query<EnemyComponent, PositionComponent, MovementComponent, PathComponent>().ToList();
        var towers = World.Query<TowerComponent, PositionComponent>().ToList();

        foreach (var entity in allEnemies)
        {
            var pos = entity.Get<PositionComponent>();
            var move = entity.Get<MovementComponent>();
            var path = entity.Get<PathComponent>();

            if (path.Waypoints == null || path.Waypoints.Count == 0)
            {
                var player = World.Query<PlayerComponent>().FirstOrDefault();
                if (player != null) player.Get<PlayerComponent>().Lives--;
                World.DestroyEntity(entity);
                continue;
            }

            Vector3 targetPos = path.Waypoints.Peek();
            targetPos.y = pos.Value.y;
            Vector3 dir = (targetPos - pos.Value).normalized;

            float sepRadius = 1.2f;
            float sepStrength = 1.5f;
            foreach (var other in allEnemies)
            {
                if (entity.Id == other.Id) continue;
                float dist = Vector3.Distance(pos.Value, other.Get<PositionComponent>().Value);
                if (dist < sepRadius && dist > 0.01f)
                {
                    Vector3 away = (pos.Value - other.Get<PositionComponent>().Value).normalized;
                    dir += away * (1f - dist / sepRadius) * sepStrength;
                }
            }

            foreach (var tower in towers)
            {
                float dist = Vector3.Distance(pos.Value, tower.Get<PositionComponent>().Value);
                if (dist < Settings.AvoidanceRadius)
                {
                    Vector3 away = (pos.Value - tower.Get<PositionComponent>().Value).normalized;
                    away.y = 0;
                    dir += away * (1f - dist / Settings.AvoidanceRadius) * Settings.AvoidanceStrength;
                }
            }

            if (Settings.EnemySwayStrength > 0.01f)
            {
                dir.y = 0;
                Vector3 right = new Vector3(-dir.z, 0, dir.x).normalized;
                float sway = Mathf.Sin(Time.time * Settings.EnemySwaySpeed + entity.Id * 0.7f) * Settings.EnemySwayStrength;
                dir += right * sway;
            }

            dir.y = 0;
            dir.Normalize();
            pos.Value += dir * move.Speed * Time.deltaTime;

            if (entity.Has<RotationComponent>())
            {
                var rot = entity.Get<RotationComponent>();
                if (dir.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRot = Quaternion.LookRotation(dir);
                    float maxDeg = move.RotationSpeed * Time.deltaTime;
                    rot.Value = Quaternion.RotateTowards(rot.Value, targetRot, maxDeg);
                }
            }

            if (Vector3.Distance(new Vector3(pos.Value.x, 0, pos.Value.z), new Vector3(targetPos.x, 0, targetPos.z)) < Settings.PathCheckDistance)
                path.Waypoints.Dequeue();
        }
    }
}
