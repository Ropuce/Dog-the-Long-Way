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

    [Header("Controls")]
    [SerializeField] private float rotationSpeed = 0.3f;
    [SerializeField] private float maxRotationSpeed = 0.5f; // Degrees per second
    [SerializeField] private float stableAngle = 10f; // TODO make the head not turn too far
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float maxSpeed = 3f;

    private Vector2 _movement =  Vector2.zero;
    private Vector3 _lastDirection;

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
        _rb.AddTorque(Vector3.up * (rotationSpeed * _movement.x), ForceMode.Acceleration);
        if (!_rope.tooStretchedOut && _movement.y > 0)
        {
            _rope.followFront = true;
            _rb.AddForce(transform.forward * (acceleration * _movement.y));
        }
    }
    

    void OnJoystick(InputValue value)
    {
        _movement = value.Get<Vector2>();
    }
    
}
