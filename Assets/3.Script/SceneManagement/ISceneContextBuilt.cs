using UnityEngine;

public interface ISceneContextBuilt
{
    public int Priority { get; set; }

    public void OnSceneContextBuilt();
}
