using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public struct CutsceneSlide
{
    public Sprite sprite;
    [FormerlySerializedAs("speed")] public float duration;
    public float slideSpeed;
    public Vector3 initialPosition;
    public Vector3 targetPosition;
    [FormerlySerializedAs("vanishSpeed")] public float vanishTime;
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
            float timeElapsed = 0;
            while (timeElapsed < cutsceneSlide.duration)
            {
                image.transform.position = Vector3.Lerp(image.transform.position, cutsceneSlide.targetPosition, cutsceneSlide.slideSpeed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
                timeElapsed += Time.deltaTime;
            }

            StartCoroutine(VanishSlide(cutsceneSlide, image));
        }
    }

    IEnumerator VanishSlide(CutsceneSlide slide, GameObject image)
    {
        SpriteRenderer spriteRenderer = image.GetComponent<SpriteRenderer>();
        Color color = spriteRenderer.color;
        while (color.a > 0)
        {
            color.a -= Time.deltaTime/slide.vanishTime;
            spriteRenderer.color = color;
            yield return new WaitForEndOfFrame();
        }

        Destroy(image);
    }
}
