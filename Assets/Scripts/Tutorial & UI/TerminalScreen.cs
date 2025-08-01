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

    [SerializeField] TerminalPopup popup;
    void Start()
    {
        terminalText.text = "";
        StartCoroutine(TypeText());
    }

    IEnumerator TypeText()
    {
        foreach (char c in fullText)
        {
            currentText += c;
            terminalText.text = currentText + "<color=#00C3EB>|</color>"; // blinking green cursor
            yield return new WaitForSeconds(typeDelay);
        }

        isTyping = false;
        popup.StartUpload();

    }
}
