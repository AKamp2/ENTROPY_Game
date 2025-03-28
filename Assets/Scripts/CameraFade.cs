using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFade : MonoBehaviour
{
    public float speedScale = 1f;
    public Color fadeColor = Color.black;
    public AnimationCurve Curve = new AnimationCurve(new Keyframe(0, 1),
        new Keyframe(0.5f, 0.5f, -1.5f, -1.5f), new Keyframe(1, 0));
    public bool startFadedOut = false;

    public float alpha = 0f;
    private Texture2D texture;
    private int direction = 0;
    private float time = 0f;

    private void Start()
    {
        if (startFadedOut)
        {
            alpha = 1f;
            StartCoroutine(FadeIn()); // Automatically start fading in if scene starts faded out
        }
        else
        {
            alpha = 0f;
        }

        texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha));
        texture.Apply();
    }

    private void Update()
    {
        // Removed key input logic
    }

    public void OnGUI()
    {
        if (alpha > 0f)
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture);

        if (direction != 0)
        {
            time += direction * Time.deltaTime * speedScale;
            alpha = Curve.Evaluate(time);
            texture.SetPixel(0, 0, new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha));
            texture.Apply();

            if (alpha <= 0f || alpha >= 1f) direction = 0; // Stop the fading once it reaches the limit
        }
    }

    // Method to trigger Fade In
    public IEnumerator FadeIn()
    {
        time = 1f;
        direction = -1; // Start fading in
        yield return null;
    }

    // Method to trigger Fade Out
    public IEnumerator FadeOut()
    {
        time = 0f;
        direction = 1; // Start fading out
        yield return null;
    }
}

