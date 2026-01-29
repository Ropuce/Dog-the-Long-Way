using UnityEngine;

public class Apple : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] public float _lengthIncrease; // Increase of rope length in meters once touched
    [SerializeField] public bool _eaten;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_eaten) Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
