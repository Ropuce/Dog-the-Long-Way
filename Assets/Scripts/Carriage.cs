using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct CarriageConfig
{
    public Vector3 velocity;
    public float duration;
    
}

[RequireComponent(typeof(Rigidbody))]
public class Carriage : MonoBehaviour
{
    public CarriageConfig carriageConfig;
    
    private Rigidbody _rb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponentInChildren<Rigidbody>();
        _rb.linearVelocity = transform.TransformVector(carriageConfig.velocity);
        Destroy(gameObject,carriageConfig.duration);
    }
}
