using UnityEngine;

public class DelayDelete : MonoBehaviour
{
    
    public float delay;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, delay);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
