using System.Collections;
using UnityEngine;

public class CameraFade : MonoBehaviour
{
    public CanvasGroup fadeCanvasGroup; // Reference to the UI panel with CanvasGroup
    public float defaultFadeDuration = 1.5f; // Default fade time (modifiable)
    public bool startFadedOut = true;

    private void Start()
    {
        if (fadeCanvasGroup == null)
        {
            Debug.LogError("CameraFade: No CanvasGroup assigned for fading!");
            return;
        }

        if(startFadedOut)
        {
            fadeCanvasGroup.alpha = 1; // Start fully faded out
            StartCoroutine(FadeIn(defaultFadeDuration)); // Automatically fade in
        }
        else
        {
            fadeCanvasGroup.alpha = 0;
        }
        
    }

    public IEnumerator FadeIn(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
            elapsedTime += Time.unscaledDeltaTime; // Use unscaled time to work during pause
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
    }

    public IEnumerator FadeOut(float duration)
    {
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / duration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;
    }

    public void SetAlpha(float alpha)
    {
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = Mathf.Clamp01(alpha); // Ensure value is between 0-1
        }
    }
}

