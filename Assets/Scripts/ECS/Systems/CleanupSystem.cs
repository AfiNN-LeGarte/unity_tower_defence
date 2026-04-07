using UnityEngine;
using System.Linq;





public class CleanupSystem : BaseSystem
{
    public override void Execute()
    {
        foreach (var entity in World.GetAll())
        {
            if (!entity.IsActive) continue;

            if (entity.Has<HealthComponent>())
            {
                var health = entity.Get<HealthComponent>();
                if (health.Current <= 0)
                {
                    if (entity.Has<UnityObjectComponent>())
                    {
                        var view = entity.Get<UnityObjectComponent>();
                        if (view.Obj != null)
                            Object.Destroy(view.Obj);
                    }
                    World.DestroyEntity(entity);
                }
            }
        }
    }
}