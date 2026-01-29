using UnityEngine;

public class GrabAnimationTrigger : MonoBehaviour
{
    [SerializeField] private Animator _animator;
    [SerializeField] private string _triggerName = "Trigger";
    [SerializeField] private bool _triggered = false;

    private void Start()
    {
        _animator ??= GetComponent<Animator>();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Rigidbody>())
            TriggerAnimation();
    }

    public void TriggerAnimation()
    {
        if (_triggered) return;
        
        _animator?.SetTrigger(_triggerName);
        _triggered = true;
    }
}
