using System;
using UnityEngine;

public class CollisionSensor : MonoBehaviour
{
    public Action<Collision> OnCollisionEntered;
    private void OnCollisionEnter(Collision collision)
    {
        OnCollisionEntered?.Invoke(collision);
    }
}
