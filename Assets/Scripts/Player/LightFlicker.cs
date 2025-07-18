using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    Light spotLight;

    [SerializeField]
    float maxOnDuration;
    [SerializeField]
    float minOnDuration;

    [SerializeField]
    float maxOffDuration;
    [SerializeField]
    float minOffDuration;

    [SerializeField]
    float defaultIntensity = 2;

    [SerializeField]
    float maxIntensity = 1;

    [SerializeField]
    float minIntensity = 0;

    private float timer;

    bool isActive;



    void Start()
    {
        timer = Random.Range(minOnDuration, maxOnDuration);
        isActive = true;
    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {

            isActive=!isActive;

            if (isActive)
            {
                spotLight.intensity = defaultIntensity;
                timer = Random.Range(minOnDuration, maxOnDuration);
            }
            else
            {
                spotLight.intensity = Random.Range(minIntensity, maxIntensity);
                timer = Random.Range(-minOffDuration, maxOffDuration);
            }
        }
    }
}
