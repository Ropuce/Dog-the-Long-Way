using System;
using UnityEngine;
using System.Collections.Generic;

// Taken from https://www.youtube.com/watch?v=bxG3XP4MVzk
// With a couple upgrades, like 3D compatibility and generating the rope in any direction
public class RopeVerlet : MonoBehaviour
{
    [Header("Rope")]
    [SerializeField] private int _numOfRopeSegments = 50;
    [SerializeField] private float _ropeSegmentLength = 0.225f;
    [SerializeField] private Vector3 _generationAxis = Vector3.back;
    [SerializeField] private Transform _attachPoint;
    
    [Header("Physics")]
    [SerializeField] private Vector3 _gravity = new Vector3(0f,0f,0f);
    [SerializeField] private float _dampingFactor = 0.98f;
    [SerializeField] private LayerMask _collisionMask;
    [SerializeField] private float _collisionRadius = 0.1f;
    [SerializeField] private float _bounceFactor = 0.1f; // Higher = slippery, Lower = sticky

    [Header("Constraints")]
    [SerializeField] private int _numOfContraintRuns = 50;

    [Header("Optimizations")]
    [SerializeField] private int _collisionSegmentInterval = 2;
    
    private LineRenderer _lineRenderer;
    private List<RopeSegment> _ropeSegments = new List<RopeSegment>();
    
    

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.positionCount = _numOfRopeSegments;

        if (!_attachPoint) _attachPoint = transform;
        
        Vector3 startPoint = _attachPoint.position;
        _generationAxis.Normalize();

        for (int i = 0; i < _numOfRopeSegments; i++)
        {
            _ropeSegments.Add(new RopeSegment(startPoint));
            startPoint += _generationAxis * _ropeSegmentLength;
        }
    }

    // Update is called once per frame
    void Update()
    {
        DrawRope();
    }

    void FixedUpdate()
    {
        Simulate();
        
        for (int i = 0; i < _numOfContraintRuns; i++)
        {
            ApplyConstraints();
            
            if (i %  _collisionSegmentInterval == 0)
                HandleCollisions();
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
        ropeSegment.CurrentPosition = _attachPoint.position;
        _ropeSegments[0] = ropeSegment;
        for (int i = 0; i < _ropeSegments.Count - 1; i++)
        {
            RopeSegment currentSegment = _ropeSegments[i];
            RopeSegment nextSegment = _ropeSegments[i + 1];
            
            float dist = (currentSegment.CurrentPosition - nextSegment.CurrentPosition).magnitude;
            float difference = (dist - _ropeSegmentLength);
            
            Vector3 changeDir = (currentSegment.CurrentPosition - nextSegment.CurrentPosition).normalized;
            Vector3 changeVector = changeDir * difference;

            if (i != 0)
            {
                currentSegment.CurrentPosition -= (changeVector * 0.5f);
                nextSegment.CurrentPosition += (changeVector * 0.5f);
            }
            else
            {
                nextSegment.CurrentPosition += changeVector;
            }

            _ropeSegments[i] = currentSegment;
            _ropeSegments[i + 1] = nextSegment;
        }
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
