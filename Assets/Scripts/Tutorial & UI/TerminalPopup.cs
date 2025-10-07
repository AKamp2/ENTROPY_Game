using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TerminalPopup : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Text uploadText;
    [SerializeField] GameObject popupObject;
    [SerializeField] GameObject uploadCompleteText;
    [SerializeField] GameObject screenBlur;
    public float uploadDuration = 3f;

    private bool isUploaded = false;
    public bool IsUploaded => isUploaded;

    /// <summary>
    /// Start the upload sequence with progress bar and audio
    /// </summary>
    public void StartUpload()
    {
        screenBlur.SetActive(true);
        popupObject.SetActive(true);
        
        currentTime = 0f;
        isUploading = true;
        progressFill.value = 0f;
        uploadText.text = "UPLOADING... 0%";
        uploadCompleteText.SetActive(false);
        isUploaded = false;

        // Start bootup audio
        terminalAudio?.PlayBootupSound();

        StartCoroutine(UploadProgress());
    }

    private IEnumerator UploadProgress()
    {
        float currentTime = 0f;

        while (currentTime < uploadDuration)
        {
            currentTime += Time.deltaTime;
            float progress = Mathf.Clamp01(currentTime / uploadDuration);

            // Update progress bar
            progressFill.value = progress;
            uploadText.text = $"UPLOADING... {(int)(progress * 100)}%";

            // Fade out bootup as progress reaches 100%
            if (terminalAudio != null && terminalAudio.bootupSource != null)
            {
                terminalAudio.bootupSource.volume = Mathf.Lerp(1f, 0f, progress);
            }

            yield return null;
        }

        // Ensure progress bar is full
        progressFill.value = 1f;
        uploadText.text = "UPLOADING... 100%";

        // Stop bootup completely
        terminalAudio?.bootupSource?.Stop();

        // Show upload complete UI
        uploadCompleteText.SetActive(true);

        // Play upload complete sound once
        terminalAudio?.PlayUploadCompleteSound();

        yield return new WaitForSeconds(1f);
        isUploaded = true;

        terminalText.SetActive(false);
        popupObject.SetActive(false);
        
    }
}





