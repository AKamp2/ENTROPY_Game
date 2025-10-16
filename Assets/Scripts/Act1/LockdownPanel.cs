using UnityEngine;
using TMPro;
using System.Collections;

public class LockdownPanel : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject reactivatePanel;
    [SerializeField] private GameObject deactivatePanel;

    [Header("Texts")]
    [SerializeField] private TextMeshProUGUI reactivateText;
    [SerializeField] private TextMeshProUGUI deactivateText;

    [Header("Screen Components")]
    [SerializeField] private GameObject screenBacking;
    [SerializeField] private Material offMaterial;
    [SerializeField] private Material onMaterial;
    [SerializeField] private GameObject offScreen;

    [Header("Lockdown Status")]
    [SerializeField] private GameObject awaitingLockdown;
    [SerializeField] private GameObject completedLockdown;

    [Header("Blink Settings")]
    [SerializeField] private float blinkInterval = 0.5f;

    private Coroutine currentBlinkCoroutine;

    private void Start()
    {
        // Initialize panels
        reactivatePanel.SetActive(true);
        deactivatePanel.SetActive(false);

        // Initialize lockdown status
        if (awaitingLockdown != null) awaitingLockdown.SetActive(true);
        if (completedLockdown != null) completedLockdown.SetActive(false);

        // Start with blinking reactivate text
        StartBlinking(reactivateText);

        // Start with screen turned on
        TurnOn();
    }

    private void StartBlinking(TextMeshProUGUI tmp)
    {
        if (currentBlinkCoroutine != null)
            StopCoroutine(currentBlinkCoroutine);

        currentBlinkCoroutine = StartCoroutine(BlinkText(tmp));
    }

    private IEnumerator BlinkText(TextMeshProUGUI tmp)
    {
        while (true)
        {
            tmp.enabled = !tmp.enabled;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    public void SwitchToDeactivate()
    {
        if (currentBlinkCoroutine != null)
            StopCoroutine(currentBlinkCoroutine);

        reactivatePanel.SetActive(false);
        deactivatePanel.SetActive(true);

        StartBlinking(deactivateText);
    }

    public void TurnOn()
    {
        if (screenBacking != null && onMaterial != null)
        {
            var renderer = screenBacking.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material = onMaterial;
        }

        if (offScreen != null)
            offScreen.SetActive(false);
    }

    public void TurnOff()
    {
        if (screenBacking != null && offMaterial != null)
        {
            var renderer = screenBacking.GetComponent<Renderer>();
            if (renderer != null)
                renderer.material = offMaterial;
        }

        if (offScreen != null)
            offScreen.SetActive(true);
    }

    /// <summary>
    /// Marks the lockdown as complete: disables awaitingLockdown and enables completedLockdown.
    /// </summary>
    public void CompleteLockdown()
    {
        if (awaitingLockdown != null)
            awaitingLockdown.SetActive(false);

        if (completedLockdown != null)
            completedLockdown.SetActive(true);
    }
}