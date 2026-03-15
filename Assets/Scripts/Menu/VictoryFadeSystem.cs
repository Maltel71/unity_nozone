using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VictoryFadeSystem : MonoBehaviour
{
    [Header("Images")]
    [SerializeField] private Image imageOne;
    [SerializeField] private Image imageTwo;

    [Header("Image One")]
    [SerializeField] private float imageOneFadeDelay = 0f;
    [SerializeField] private float imageOneFadeDuration = 1f;

    [Header("Image Two")]
    [SerializeField] private float imageTwoFadeDelay = 1f;
    [SerializeField] private float imageTwoFadeDuration = 1f;

    private void Start()
    {
        if (imageOne != null) SetAlpha(imageOne, 0f);
        if (imageTwo != null) SetAlpha(imageTwo, 0f);

        if (imageOne != null) StartCoroutine(FadeIn(imageOne, imageOneFadeDelay, imageOneFadeDuration));
        if (imageTwo != null) StartCoroutine(FadeIn(imageTwo, imageTwoFadeDelay, imageTwoFadeDuration));
    }

    private IEnumerator FadeIn(Image image, float delay, float duration)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(image, Mathf.SmoothStep(0f, 1f, elapsed / duration));
            yield return null;
        }

        SetAlpha(image, 1f);
    }

    private static void SetAlpha(Image image, float alpha)
    {
        Color c = image.color;
        c.a = alpha;
        image.color = c;
    }
}