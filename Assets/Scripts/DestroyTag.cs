using Unity.VisualScripting;
using UnityEngine;

public class DestroyTag : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] public string _targetTag;
    [SerializeField] public float _delay = 0;
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(_targetTag))
        {
            Destroy(collision.gameObject, _delay);
        }
    }
}
