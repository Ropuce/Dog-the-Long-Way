using UnityEngine;
using UnityEngine.InputSystem;

// This class is in charge of adding more Dog segments if needed
[RequireComponent(typeof(Rigidbody))]
public class DogRear : MonoBehaviour
{
    [SerializeField] DogController _dogController;
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
        if (_dogController.alive && !_rope.tooStretchedOut && _movement < 0)
        {
            _rope._followFront = false;
            _rb.AddForce(transform.forward * (acceleration * _movement));
        }
    }

    void OnJoystick(InputValue value)
    {
        _movement = value.Get<Vector2>().y;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (_dogController.alive && collision.gameObject.CompareTag("Death"))
        {
            _dogController.OnDefeat();
        }
    }
}
