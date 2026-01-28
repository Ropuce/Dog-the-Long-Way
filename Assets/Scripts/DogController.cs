using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class DogController : MonoBehaviour
{
    private Rigidbody _rb;
    [SerializeField] private RopeVerlet _rope;
    private Transform _visual;

    [Header("Configuration")]
    [SerializeField] private bool _overrideStartingRotation;
    [SerializeField] private Vector3 _startingRotation = Vector3.zero;
    
    [Header("Controls")]
    [SerializeField] private float rotationSpeed = 0.3f;
    [SerializeField] private float maxRotationSpeed = 0.5f; // Degrees per second
    [SerializeField] private float stableAngle = 10f; // TODO make the head not turn too far
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float maxSpeed = 3f;

    private Vector2 _movement =  Vector2.zero;
    private Vector3 _lastDirection;

    // Grab onto objects
    private List<Grabbable> _possibleAttachments = new List<Grabbable>();
    public HingeJoint _attachmentPoint;
    

    private float MaxSpeed
    {
        get => maxSpeed;
        set
        {
            maxSpeed = value;
            if (_rb) _rb.maxLinearVelocity = value;
        }
    }
    // Local variable stores in degrees, rigidbody takes radians
    private float MaxRotationSpeed
    {
        get => maxRotationSpeed;
        set
        {
            var rads = Mathf.Deg2Rad * value;
            maxRotationSpeed = value;
            if (_rb) _rb.maxAngularVelocity = rads;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_overrideStartingRotation) transform.rotation = Quaternion.Euler(_startingRotation);
        
        _rb = GetComponent<Rigidbody>();
        _visual = transform.GetChild(0);
        if (!_visual)
        {
            _visual = new GameObject("NoVisual").transform;
            _visual.parent = transform;
            _visual.localPosition = Vector3.zero;
        }

        // Apply max speed to rigidbody
        if (_rb)
        {
            var rads = Mathf.Deg2Rad * maxRotationSpeed;
            _rb.maxAngularVelocity = rads;
            _rb.maxLinearVelocity = maxSpeed;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (_attachmentPoint)
        {
            _rope._followFront = true;
            _rb.AddTorque(Vector3.up * (_movement.x * acceleration),ForceMode.Acceleration);
        }
        else
        {
            _rb.AddTorque(Vector3.up * (rotationSpeed * _movement.x), ForceMode.Acceleration);
        }
        
        if (!_rope.tooStretchedOut && _movement.y > 0)
        {
            _rope._followFront = true;
            _rb.AddForce(transform.forward * (acceleration * _movement.y), ForceMode.Acceleration);
        }
    }
    

    void OnJoystick(InputValue value)
    {
        _movement = value.Get<Vector2>();
    }

    void OnGrab(InputValue value)
    {
        if (value.isPressed)
        {
            if (_possibleAttachments.Count > 0)
            {
                _attachmentPoint = gameObject.AddComponent<HingeJoint>();
                _attachmentPoint.limits = new JointLimits()
                {
                    min = -30,
                    max = 30,
                    bounciness = 0.5f,
                    contactDistance = 0.2f
                };
                _attachmentPoint.useLimits = true;
                _attachmentPoint.spring = new JointSpring()
                {
                    targetPosition = 0,
                    spring = 0.5f,
                    damper = 0.9f
                };
                _attachmentPoint.useSpring = true;
                _attachmentPoint.anchor = new Vector3(0, 0, 1);
                _attachmentPoint.axis = new Vector3(0, 1, 0);
                _attachmentPoint.connectedBody = _possibleAttachments[0].rb;
            }
        }
        else if (_attachmentPoint)
        {
             Destroy(_attachmentPoint);
             _attachmentPoint = null;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var grabbable = other.gameObject.GetComponent<Grabbable>();
        if (grabbable != null && !_possibleAttachments.Contains(grabbable))
            _possibleAttachments.Add(grabbable);
    }

    void OnTriggerExit(Collider other)
    {
        var grabbable = other.gameObject.GetComponent<Grabbable>();
        _possibleAttachments.Remove(grabbable);
    }
}
