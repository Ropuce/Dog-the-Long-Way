using UnityEngine;

public class DebugConfig : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DogController _dogController;
    [SerializeField] private RopeVerlet _rope;
    [SerializeField] private DogRear _dogRear;
    
    [Header("Dog")]
    [SerializeField] private bool _overrideRopeLength;
    [SerializeField] private float _ropeLength;
    [SerializeField] private bool _overrideMovementSpeed;
    [SerializeField] private float _movementSpeed;
    
    
    void FixedUpdate()
    {
        if (_overrideRopeLength) _rope.RopeLength = _ropeLength;   
        if (_overrideMovementSpeed) _dogController.acceleration = _dogController.maxSpeed = _movementSpeed;
    }
}
