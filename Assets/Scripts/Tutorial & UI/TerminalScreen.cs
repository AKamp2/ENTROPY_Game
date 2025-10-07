using TMPro;
using UnityEngine;
using System.Collections;

public class TerminalScreen : MonoBehaviour
{
    [SerializeField] TMP_Text terminalText;
    [SerializeField] string fullText = "> LOADING DATA\n- searching for Connection\n-- ALAN network established\n\n> Subnetwork Detected\n- Upload Beginning";
    [SerializeField] float typeDelay = 0.025f;

    private string currentText = "";
    private bool isTyping = true;

    [SerializeField] private AudioSource bootupSource;
    [SerializeField] private TerminalAudioManager terminalAudio;

    [SerializeField] TerminalPopup popup;

    private void Start()
    {
        terminalAudio = FindFirstObjectByType<TerminalAudioManager>();
    }

    //do a typewriter effect for the upload text in the terminal
    public IEnumerator TypeText()
    {
        terminalText.gameObject.SetActive(true);

        // Start bootup audio
        terminalAudio?.PlayBootupSound(bootupSource);

        foreach (char c in fullText)
        {
            currentText += c;
            terminalText.text = currentText + "<color=#00C3EB>|</color>"; // blinking green cursor
            yield return new WaitForSeconds(typeDelay);
        }
        
        isTyping = false;
        terminalAudio.FadeOutBootup(bootupSource);

        popup.StartUpload();

    }
}
