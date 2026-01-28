using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

// Taken from https://www.youtube.com/watch?v=bxG3XP4MVzk
// With a couple upgrades, like 3D compatibility and generating the rope in any direction
public class RopeVerlet : MonoBehaviour
{
    [Header("Rope")]
    [SerializeField] private int _numOfRopeSegments = 50;
    [SerializeField] private float _ropeSegmentLength = 0.225f;
    [SerializeField] private Vector3 _generationAxis = Vector3.back;
    [SerializeField] private Rigidbody _attachPointFront;
    [SerializeField] private Rigidbody _attachPointBack;
    
    [Header("Physics")]
    [SerializeField] private Vector3 _gravity = new Vector3(0f,0f,0f);
    [SerializeField] private float _dampingFactor = 0.8f;
    [SerializeField] private LayerMask _collisionMask;
    [SerializeField] private float _collisionRadius = 0.2f;
    [SerializeField] private float _bounceFactor = 0.1f; // Higher = slippery, Lower = sticky
    [SerializeField] public bool followFront = true;
    [SerializeField] private float followStrength = 35f;
    [SerializeField] private float _distanceBreakingPoint = 0.5f;

    [Header("Constraints")]
    [SerializeField] private int _numOfContraintRuns = 50;

    [Header("Optimizations")]
    [SerializeField] private int _collisionSegmentInterval = 2;
    
    private LineRenderer _lineRenderer;
    private List<RopeSegment> _ropeSegments = new List<RopeSegment>();

    [HideInInspector] public bool tooStretchedOut = false;
    

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = _numOfRopeSegments;
        
        // Line renderer must be at (0,0,0)
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;

        if (!_attachPointFront)
        {
            _attachPointFront = new GameObject("RopeAnchorFront").AddComponent<Rigidbody>();
            _attachPointBack = new GameObject("RopeAnchorBack").AddComponent<Rigidbody>();
            _attachPointFront.isKinematic = _attachPointBack.isKinematic = true;
            _attachPointFront.transform.parent = _attachPointBack.transform.parent = null;
        }
        
        Vector3 startPoint = _attachPointFront.position - _attachPointFront.transform.forward * 0.1f;
        _generationAxis = _attachPointFront.transform.TransformDirection(_generationAxis).normalized;

        for (int i = 0; i < _numOfRopeSegments; i++)
        {
            _ropeSegments.Add(new RopeSegment(startPoint));
            startPoint += _generationAxis * _ropeSegmentLength;
        }

        _attachPointBack.position = startPoint; // Attach to the end of the rope
    }

    // Update is called once per frame
    void Update()
    {
        DrawRope();
    }

    void FixedUpdate()
    {
        Simulate();
        
        if (followFront)
        {
            for (int i = 0; i < _numOfContraintRuns; i++)
            {
                ApplyConstraints();

                if (i % _collisionSegmentInterval == 0)
                    HandleCollisions();
            }
            
            // Adjust back segment
            var changeVectorFinal = GetChangeVector(_ropeSegments[^1].CurrentPosition, _attachPointBack.position);
            var force = (changeVectorFinal / changeVectorFinal.magnitude) * (followStrength);
            _attachPointBack.AddForce(force, ForceMode.Acceleration);
            if (!tooStretchedOut && changeVectorFinal.magnitude > _distanceBreakingPoint)
            {
                followFront = false;
                tooStretchedOut = true;
            }
        }
        else
        {
            for (int i = 0; i < _numOfContraintRuns; i++)
            {
                ApplyConstraintsBackwards();

                if (i % _collisionSegmentInterval == 0)
                    HandleCollisions();
            }
            
            // Adjust  segment
            var changeVectorFinal = GetChangeVector(_ropeSegments[0].CurrentPosition, _attachPointFront.position);
            var force = (changeVectorFinal / changeVectorFinal.magnitude) * (followStrength);
            _attachPointFront.AddForce(force, ForceMode.Acceleration);
            if (tooStretchedOut && changeVectorFinal.magnitude < _distanceBreakingPoint)
            {
                followFront = true;
                tooStretchedOut = false;
            }
        }
    }

    private void DrawRope()
    {
        Vector3[] ropePositions = new Vector3[_numOfRopeSegments];
        for (int i = 0; i < _ropeSegments.Count; i++)
        {
            ropePositions[i] = _ropeSegments[i].CurrentPosition;
        }
        
        _lineRenderer.SetPositions(ropePositions);
    }

    private void Simulate()
    {
        for (int i = 0; i < _ropeSegments.Count; i++)
        {
            RopeSegment segment = _ropeSegments[i];
            Vector3 velocity = (segment.CurrentPosition - segment.OldPosition) * _dampingFactor;
            
            segment.OldPosition = segment.CurrentPosition;
            segment.CurrentPosition += velocity;
            segment.CurrentPosition += _gravity * Time.fixedDeltaTime;
            _ropeSegments[i] = segment;
        }
    }

    private void ApplyConstraints()
    {
        RopeSegment ropeSegment = _ropeSegments[0];
        ropeSegment.CurrentPosition = _attachPointFront.position - _attachPointFront.transform.forward * 0.1f;
        _ropeSegments[0] = ropeSegment;
        for (int i = 0; i < _ropeSegments.Count - 1; i++)
        {
            RopeSegment currentSegment = _ropeSegments[i];
            RopeSegment nextSegment = _ropeSegments[i + 1];
            
            Vector3 changeVector = GetChangeVector(currentSegment.CurrentPosition, nextSegment.CurrentPosition);
            
            if (i == 0)
            {
                // Attached to what drags the rope
                nextSegment.CurrentPosition += changeVector;
            }
            else
            {
                currentSegment.CurrentPosition -= (changeVector * 0.5f);
                nextSegment.CurrentPosition += (changeVector * 0.5f);
            }

            _ropeSegments[i] = currentSegment;
            _ropeSegments[i + 1] = nextSegment;
        }

    }
    
    private void ApplyConstraintsBackwards()
    {
        //Same as ApplyConstraints, but back to front instead
        RopeSegment anchoredSegment = _ropeSegments[^1];
        anchoredSegment.CurrentPosition = _attachPointBack.position;
        _ropeSegments[^1] = anchoredSegment;
        for (int i = _ropeSegments.Count - 1; i > 0; i--)
        {
            RopeSegment currentSegment = _ropeSegments[i];
            RopeSegment nextSegment = _ropeSegments[i - 1];
            
            Vector3 changeVector = GetChangeVector(currentSegment.CurrentPosition, nextSegment.CurrentPosition);

            if (i == (_ropeSegments.Count - 1))
            {
                // Attached to what drags the rope
                nextSegment.CurrentPosition += changeVector;
            }
            else
            {
                currentSegment.CurrentPosition -= (changeVector * 0.5f);
                nextSegment.CurrentPosition += (changeVector * 0.5f);
            }

            _ropeSegments[i] = currentSegment;
            _ropeSegments[i - 1] = nextSegment;
            
        }
        
        //_attachPointFront.LookAt(_ropeSegments[0].CurrentPosition);
        
    }

    private Vector3 GetChangeVector(Vector3 current, Vector3 next)
    {
        float dist = (current - next).magnitude;
        float difference = (dist - _ropeSegmentLength);
            
        Vector3 changeDir = (current - next).normalized;
        return (changeDir * difference);
    }

    private void HandleCollisions()
    {
        for (int i = 1; i < _ropeSegments.Count; i++)
        {
            RopeSegment segment = _ropeSegments[i];
            Vector3 velocity = segment.CurrentPosition - segment.OldPosition;
            Collider[] colliders = Physics.OverlapSphere(segment.CurrentPosition, _collisionRadius, _collisionMask);

            foreach (Collider col in colliders)
            {
                Vector3 closestPoint = col.ClosestPoint(segment.CurrentPosition);
                float distance = Vector3.Distance(segment.CurrentPosition, closestPoint);
                
                // If within the collision radius, resolve
                if (distance < _collisionRadius)
                {
                    Vector3 normal = (segment.CurrentPosition - closestPoint).normalized;
                    if (normal == Vector3.zero)
                    {
                        // Fallback method
                        normal = (segment.CurrentPosition - col.transform.position).normalized;
                    }

                    float depth = _collisionRadius - distance;
                    segment.CurrentPosition += normal * depth;
                    velocity = Vector3.Reflect(velocity, normal) * _bounceFactor;
                }
            }
            
            segment.OldPosition = segment.CurrentPosition - velocity;
            _ropeSegments[i] = segment;
        }
        
        
    }

    struct RopeSegment
    {
        public Vector3 CurrentPosition;
        public Vector3 OldPosition;

        public RopeSegment(Vector3 pos)
        {
            OldPosition = CurrentPosition = pos;
        }
    }
}
