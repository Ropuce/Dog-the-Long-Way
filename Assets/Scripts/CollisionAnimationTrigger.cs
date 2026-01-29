using System;
using UnityEngine;

// Triggers the animation on this gameobject via the specified trigger once a rigidbody enters in contact
[RequireComponent(typeof(Animator))]
public class CollisionAnimTrigger : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string _triggerName;

    private void Start()
    {
        _animator ??= GetComponent<Animator>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Rigidbody>())
            _animator?.SetTrigger(_triggerName);
    }
}
