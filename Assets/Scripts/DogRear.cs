using UnityEngine;
using UnityEngine.InputSystem;

// This class is in charge of adding more Dog segments if needed
[RequireComponent(typeof(Rigidbody))]
public class DogRear : MonoBehaviour
{
    private Rigidbody _rb;
    [SerializeField] private RopeVerlet _rope;

    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float maxSpeed = 3f;
    
    private float _movement;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
       _rb = GetComponent<Rigidbody>(); 
       _rb.maxLinearVelocity = maxSpeed;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!_rope.tooStretchedOut && _movement < 0)
        {
            _rope.followFront = false;
            _rb.AddForce(transform.forward * (acceleration * _movement));

        }
    }

    void OnJoystick(InputValue value)
    {
        _movement = value.Get<Vector2>().y;
    }
}
