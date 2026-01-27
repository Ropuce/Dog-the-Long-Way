using UnityEngine;

public class ThisGameConfiguration : MonoBehaviour
{
    
    #if UNITY_EDITOR
    public int targetFPS = 30;
    #endif
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        #if UNITY_EDITOR
        if (targetFPS != 0)
        {
            // Limit FPS
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = Mathf.Abs(targetFPS);
        }
        #endif
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
