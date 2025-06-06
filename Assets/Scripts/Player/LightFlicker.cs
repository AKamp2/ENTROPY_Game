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
    float timer;

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
                spotLight.intensity = 30;
                timer = Random.Range(minOnDuration, maxOnDuration);
            }
            else
            {
                spotLight.intensity = Random.Range(25, 29);
                timer = Random.Range(-minOffDuration, maxOffDuration);
            }
        }
    }
}
