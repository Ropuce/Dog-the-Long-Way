using System.Collections;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera _camera;
    public static Billboard executor;
    public static Coroutine billboardCoroutine;
    public static Vector3 billboardOrientation;
    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Set a SINGLE coroutine to pull the camera's current orientation
        _camera = Camera.main;
        executor ??= this; // If null set to this
        
        if (executor)
            billboardCoroutine ??= executor.StartCoroutine(BillboardCoroutine());
    }

    IEnumerator BillboardCoroutine()
    {
        while (true)
        {
            billboardOrientation = _camera ? _camera.transform.forward: Vector3.forward;
            yield return null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(billboardOrientation);
    }
}
