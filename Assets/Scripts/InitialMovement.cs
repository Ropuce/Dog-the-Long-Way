using System;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class Carriage : MonoBehaviour
{
    public Vector3 velocity;
    
    private Rigidbody _rb;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _rb = GetComponentInChildren<Rigidbody>();
        _rb.linearVelocity = transform.TransformVector(velocity);
    }
}
