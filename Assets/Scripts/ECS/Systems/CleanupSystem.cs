using UnityEngine;
using System.Linq;

public class CleanupSystem : BaseSystem
{
    public override void Execute()
    {
        var deadEntities = World.GetAll().Where(e => !e.IsActive).ToList();
        foreach (var e in deadEntities)
        {
            if (e.Has<UnityObjectComponent>())
            {
                var view = e.Get<UnityObjectComponent>();
                if (view.Obj != null)
                    Object.Destroy(view.Obj);
            }
        }
    }
}
