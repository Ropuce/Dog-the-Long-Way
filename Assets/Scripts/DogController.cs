using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody))]
public class DogController : MonoBehaviour
{
    private static readonly int b_MoveForward = Animator.StringToHash("MoveForward");
    private static readonly int b_TooStretched = Animator.StringToHash("TooStretched");
    private static readonly int b_Defeat = Animator.StringToHash("Defeat");
    private static readonly int b_Grab = Animator.StringToHash("Grab");
    private static readonly int t_AteApple = Animator.StringToHash("Ate Apple");


    private Rigidbody _rb;
    [SerializeField] private RopeVerlet _rope;
    private Transform _visual;

    [Header("Configuration")]
    [SerializeField] private bool _overrideStartingRotation;
    [SerializeField] private Vector3 _startingRotation = Vector3.zero;
    [SerializeField] private bool _overrideStartingPosition;
    [SerializeField] private Vector3 _initialPositionOffset = Vector3.zero;
    [SerializeField] public string _nextLevel = "SampleScene";
    [SerializeField] private Animator  _animator;
    
    [Header("Controls")]
    [SerializeField] private float rotationSpeed = 0.3f;
    [SerializeField] private float maxRotationSpeed = 0.5f; // Degrees per second
    [SerializeField] private float stableAngle = 10f; // TODO make the head not turn too far
    [SerializeField] public float acceleration = 10f;
    [SerializeField] public float maxSpeed = 3f;

    private Vector2 _movement =  Vector2.zero;
    private Vector3 _lastDirection;

    // Grab onto objects
    private List<Grabbable> _possibleAttachments = new List<Grabbable>();
    public Joint _attachmentPoint;
    public RigidbodyConstraints _attachmentConstraints;
    
    // Victory and defeat conditions
    public bool alive = true;
    public bool goalReached = false;
    

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
        if (_overrideStartingPosition) transform.position += _initialPositionOffset;
        
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
    private void Update()
    {
        _animator.SetBool(b_MoveForward, (_attachmentPoint) ? _movement.x != 0 :  _movement.y > 0);
        _animator.SetBool(b_TooStretched, _rope.tooStretchedOut);
        _animator.SetBool(b_Grab, _attachmentPoint);
    }

    void FixedUpdate()
    {
        if (!alive) return;
        
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
                var grabbable = _possibleAttachments[0];
                _rb.rotation.SetLookRotation(_rb.position-grabbable.rb.position);

                _attachmentConstraints = grabbable.rb.constraints;
                
                if ((grabbable.rb.constraints & RigidbodyConstraints.FreezePosition) == 0)
                {
                    grabbable.rb.constraints = RigidbodyConstraints.None;
                    _attachmentPoint = grabbable.gameObject.AddComponent<FixedJoint>();
                }
                else
                {
                    SpringJoint spJoint;
                    _attachmentPoint = spJoint = grabbable.gameObject.AddComponent<SpringJoint>();
                    spJoint.minDistance = 0;
                    spJoint.maxDistance = 0;
                    spJoint.damper = 5f;
                    spJoint.spring = 500f;
                    spJoint.enableCollision = true;
                }
                //_attachmentPoint.limits = new JointLimits()
                //{
                //    min = -30,
                //    max = 30,
                //    bounciness = 0.5f,
                //    contactDistance = 0.2f
                //};
                //_attachmentPoint.useLimits = true;
                //_attachmentPoint.spring = new JointSpring()
                //{
                //    targetPosition = 0,
                //    spring = 0.5f,
                //    damper = 0.9f
                //};
                //_attachmentPoint.useSpring = true;
                _attachmentPoint.anchor = new Vector3(0, 0, 1);
                _attachmentPoint.axis = new Vector3(0, 1, 0);
                _attachmentPoint.connectedBody = _rb;
            }
        }
        else if (_attachmentPoint)
        {
            _attachmentPoint.gameObject.GetComponent<Rigidbody>().constraints = _attachmentConstraints;
             Destroy(_attachmentPoint);
             _attachmentPoint = null;
             _attachmentConstraints = RigidbodyConstraints.None;
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

    void OnCollisionEnter(Collision collision)
    {
        Apple apple = collision.gameObject.GetComponent<Apple>();
        if (apple && !apple._eaten)
        {
            _animator.SetTrigger(t_AteApple);
            _rope.RopeLength += apple._lengthIncrease;
            apple._eaten = true;
            Destroy(apple.gameObject);
        }
        else if (alive && collision.gameObject.CompareTag("Finish"))
        {
            OnVictory();
        }
        else if (alive && collision.gameObject.CompareTag("Death"))
        {
            OnDefeat();
        }
    }

    Coroutine _levelEndRoutine = null;
    public void OnDefeat()
    {
        alive = false;
        if (_levelEndRoutine == null)
            _levelEndRoutine = StartCoroutine(DefeatCoroutine());
    }

    public void OnVictory()
    {
        goalReached = true;
        if (_levelEndRoutine == null)
        {
            _levelEndRoutine = StartCoroutine(VictoryRoutine());
        }
    }

    IEnumerator DefeatCoroutine()
    {
        // Animation and stuff goes here
        _animator.SetBool(b_Defeat, true);
        
        yield return null;
        
        // Reload
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    IEnumerator VictoryRoutine()
    {
        // Animation and stuff goes here

        yield return null;
        
        // Load next
        SceneManager.LoadScene(_nextLevel);
    }
}
