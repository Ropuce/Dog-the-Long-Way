using UnityEngine;
using UnityEngine.SceneManagement;

public class TimedSceneChanger : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string _targetScene;
    [SerializeField] private float _delay;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke(nameof(SwitchToScene), _delay);
    }

    void SwitchToScene()
    {
        SceneManager.LoadScene(_targetScene);
    }
    
}
