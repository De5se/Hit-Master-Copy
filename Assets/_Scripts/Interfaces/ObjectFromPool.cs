using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ObjectFromPool
{
    public void ReturnToPool();

    public void OnObjectSpawned();
}
