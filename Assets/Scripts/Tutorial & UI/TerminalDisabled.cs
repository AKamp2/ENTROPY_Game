using System.Collections;
using UnityEngine;

public class TerminalDisabled : MonoBehaviour
{
    [SerializeField] private GameObject disabledScreen;
    [SerializeField] private Light screenLight;
    private float lightIntensity;
    private bool isDisabled = false;
    private Coroutine disabledRoutine;

    public bool IsDisabled
    {
        get { return isDisabled; }
        set { isDisabled = value; }
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lightIntensity = screenLight.intensity;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartFlashing()
    {
        disabledRoutine = StartCoroutine(ShowDisabledScreen());
    }

    public void StopFlashing()
    {
        StopCoroutine(disabledRoutine);
        disabledScreen.SetActive(false);
        screenLight.intensity = lightIntensity;
    }

    /// <summary>
    /// flash the screen between on and off until we stop the coroutine with the above methods
    /// </summary>
    /// <returns></returns>
    private IEnumerator ShowDisabledScreen()
    {
        isDisabled = true;

        while(isDisabled)
        {
            yield return new WaitForSeconds(1f);

            //Debug.LogError("Screen Enabled");
            disabledScreen.SetActive(true);
            screenLight.intensity = lightIntensity;

            yield return new WaitForSeconds(1f);

            //Debug.LogError("Disabled");
            disabledScreen.SetActive(false);
            screenLight.intensity = 0;

            

        }
    }
}
