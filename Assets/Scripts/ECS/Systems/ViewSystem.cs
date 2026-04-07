using UnityEngine;
using System.Linq;

public class ViewSystem : BaseSystem
{
    public override void Execute()
    {
        foreach (var entity in World.Query<UnityObjectComponent>().ToList())
        {
            if (!entity.Has<PositionComponent>()) continue;

            var view = entity.Get<UnityObjectComponent>();
            var pos = entity.Get<PositionComponent>();

            if (view.Obj == null) continue;

            view.Obj.transform.position = pos.Value;

            if (entity.Has<RotationComponent>())
            {
                var rot = entity.Get<RotationComponent>();
                view.Obj.transform.rotation = rot.Value;
            }
        }
    }
}