using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class DogController : MonoBehaviour
{
    private Rigidbody _rb;
    private Transform _visual;

    [SerializeField] private float rotationSpeed = 20f;
    [SerializeField] private float maxRotationSpeed = 5f; // Degrees per second
    [SerializeField] private float stableAngle = 10f;
    [SerializeField] private float acceleration = 5f;
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
            _visual = new GameObject("visual").transform;
            _visual.parent = transform;
            _visual.localPosition = Vector3.zero;
        }

        // Apply max speed to rigidbody
        MaxSpeed = MaxSpeed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 v3Direction = new Vector3(_movement.x, 0, _movement.y);
        _rb.AddForce(v3Direction * acceleration, ForceMode.Force);
        // _rb.linearVelocity = Vector3.Lerp(_rb.linearVelocity, new Vector3(_direction.x, _rb.linearVelocity.y, _direction.y), acceleration * Time.fixedDeltaTime);
        if (_movement != Vector2.zero) 
            _lastDirection = v3Direction.normalized;
        //_rb.AddTorque(Vector3.up * (rotationSpeed * Time.fixedDeltaTime), ForceMode.Acceleration);
        //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(v3Direction), rotationSpeed * Time.fixedDeltaTime);
            
            
        float angle = Vector3.SignedAngle(transform.forward, _lastDirection, Vector3.up);
        if (Mathf.Abs(angle) > stableAngle)
        {
            _rb.angularDamping = 0.05f;
            _rb.AddTorque(Vector3.up * (Mathf.Deg2Rad * angle * rotationSpeed), ForceMode.Force);
            Debug.Log("Rotating!");
        }
        else
        {
            _rb.angularDamping = 1;
        }
        Debug.Log($"CurrentForward:{transform.forward}\nTargetForward:{_lastDirection}\nAngle:{angle}");
    }
    

    void OnJoystick(InputValue value)
    {
        _movement = value.Get<Vector2>() * maxSpeed;
    }
    
}
