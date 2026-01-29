using System.Collections;
using UnityEngine;

public class CarriageSpawner : MonoBehaviour
{
    [SerializeField] private GameObject _carriagePrefab;
    [SerializeField] private float delay;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            var gObject = Instantiate(_carriagePrefab, transform.position, transform.rotation);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
