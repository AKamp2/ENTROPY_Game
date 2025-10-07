using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TerminalPopup : MonoBehaviour
{
    public TMP_Text uploadText;
    [SerializeField] GameObject uploadCompleteText;
    [SerializeField] GameObject screenBlur;
    public float uploadDuration = 3f;
    [SerializeField] Slider progressFill;
    [SerializeField] GameObject terminalText;
    private float currentTime = 0f;
    private bool isUploading = false;
    public bool isUploaded = false;
    public void StartUpload()
    {
        screenBlur.SetActive(true);
        gameObject.SetActive(true);
        
        currentTime = 0f;
        isUploading = true;
        progressFill.value = 0f;
        uploadText.text = "UPLOADING... 0%";
        StartCoroutine(UploadProgress());
    }

    //void Update()
    //{
    //    if (isUploading)
    //    {
    //        currentTime += Time.deltaTime;
    //        float progress = Mathf.Clamp01(currentTime / uploadDuration);
    //        progressFill.value = progress;
    //        uploadText.text = $"UPLOADING... {(int)(progress * 100)}%";

    //        if (progress >= 1f)
    //        {
    //            isUploading = false;
    //            uploadCompleteText.SetActive(true);
    //        }
    //    }
    //}
    private IEnumerator UploadProgress()
    {
        float currentTime = 0f;

        while (currentTime < uploadDuration)
        {
            currentTime += Time.deltaTime;
            float progress = Mathf.Clamp01(currentTime / uploadDuration);
            progressFill.value = progress;
            uploadText.text = $"UPLOADING... {(int)(progress * 100)}%";

            yield return null;
        }

        // Upload complete
        progressFill.value = 1f;
        uploadText.text = "UPLOADING... 100%";
        uploadCompleteText.SetActive(true);

        yield return new WaitForSeconds(1f);
        isUploaded = true;

        terminalText.SetActive(false);
        gameObject.SetActive(false);
        
    }

   
}
