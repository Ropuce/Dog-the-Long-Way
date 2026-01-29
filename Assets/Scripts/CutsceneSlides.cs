using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

[Serializable]
public struct CutsceneSlide
{
    public Sprite sprite;
    public float speed;
    public Vector3 initialPosition;
    public Vector3 targetPosition;
    public float vanishSpeed;
}

public class CutsceneSlides : MonoBehaviour
{
    [SerializeField] private GameObject slidePrefab;
    [SerializeField] private List<CutsceneSlide> slides;
    [SerializeField] private bool autostart;
    private Camera _camera;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _camera = Camera.main;
        if (autostart)
            StartCoroutine(Cutscene());
    }

    IEnumerator Cutscene()
    {
        foreach (var cutsceneSlide in slides)
        {
            GameObject image = Instantiate(slidePrefab, cutsceneSlide.initialPosition,_camera?.transform.rotation??Quaternion.identity);
            image.GetComponent<SpriteRenderer>().sprite = cutsceneSlide.sprite;
            while (Vector3.Distance(image.transform.position, cutsceneSlide.targetPosition) > 0.01f)
            {
                image.transform.position = Vector3.Lerp(image.transform.position, cutsceneSlide.targetPosition, cutsceneSlide.speed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            StartCoroutine(VanishSlide(cutsceneSlide, image));
        }
    }

    IEnumerator VanishSlide(CutsceneSlide slide, GameObject image)
    {
        SpriteRenderer renderer = image.GetComponent<SpriteRenderer>();
        Color color = renderer.color;
        while (color.a > 0)
        {
            color.a -= slide.vanishSpeed;
            renderer.color = color;
            yield return new WaitForEndOfFrame();
        }

        Destroy(image);
    }
}
