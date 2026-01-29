using Unity.VisualScripting;
using UnityEngine;

public class DestroyTag : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] public string _targetTag;
    
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(_targetTag))
        {
            Destroy(collision.gameObject);
        }
    }
}
