using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour
{
    
    [SerializeField]
    public Transform[] diningLightGroup;

    [SerializeField]
    private Dictionary<Light, float> initLightIntensity = new Dictionary<Light, float>();

    private Color lightColor = new Color(0.75f, 0.75f, 0.75f, 0.0f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Shuffle(diningLightGroup);
        SaveInitLightIntensities(diningLightGroup);

    }

 

    public void FlickerLights(Transform[] lightGroup)
    {
        StartCoroutine(EnableLights(lightGroup));
    }

    private IEnumerator EnableLights(Transform[] lightGroup)
    {
        // each light group
        foreach (Transform t in lightGroup)
        {
            Light[] lights = t.GetComponentsInChildren<Light>();
            MeshRenderer[] meshes = t.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer mesh in meshes)
            {
                StartCoroutine(FlickerIntensity(null, mesh, 2.0f));
            }

            // flicker lights on with coroutine
            foreach (Light light in lights)
            {
                StartCoroutine(FlickerIntensity(light, null, initLightIntensity[light]));
            }

            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator FlickerIntensity(Light light, MeshRenderer mesh, float maxIntensity)
    {
        // do flicker logic here
        float timer = 0f;
        float flickerTimer = 0f;
        float flickerDelay = 0f;

        float lerpIntensity;
        bool isOn = false;

        // loops duration of flicker. 2.0f duration
        while (timer < 2.0f)
        {
            // lerp the brightness of max intensity so it gradually fades brighter
            lerpIntensity = Mathf.Lerp(0, maxIntensity, Mathf.Clamp01(timer / 2.0f));

            // swaps to lowlight after flickerdelay
            if (flickerTimer > flickerDelay)
            {
                isOn = !isOn;
                flickerTimer = 0.0f;
            }

            if (isOn)
            {
                // longer delay if on
                flickerDelay = Random.Range(0.5f, 2);
            }
            else
            {
                // randomize the intensity if flickering
                lerpIntensity = Random.Range(0, lerpIntensity);
                flickerDelay = Random.Range(0, 0.5f);
            }

            // applies intensity to respective item
            if (mesh != null)
            {
                mesh.material.SetColor("_EmissionColor", lightColor * lerpIntensity);
            }
            else if (light != null)
            {
                light.intensity = lerpIntensity;
            }

            timer += Time.deltaTime;
            flickerTimer += Time.deltaTime;

            yield return null;
        }

        // ensure on when flicker over
        if (mesh != null)
        {
            mesh.material.SetColor("_EmissionColor", lightColor * maxIntensity);
        }
        else if (light != null)
        {
            light.intensity = maxIntensity;
        }


        yield return null;
    }

    private void SaveInitLightIntensities(Transform[] array)
    {
        foreach (Transform transform in array)
        {
            // saves the initial light values so in editor can display the intended lights
            foreach (Light l in transform.GetComponentsInChildren<Light>())
            {
                initLightIntensity[l] = l.intensity;
                l.intensity = 0.0f;
            }

            // sets lights to off
            foreach (MeshRenderer m in transform.GetComponentsInChildren<MeshRenderer>())
            {
                m.material.SetColor("_EmissionColor", lightColor * 0f);
            }
        }
    }

    // randomizes array
    private void Shuffle(Transform[] array)
    {
        System.Random rng = new System.Random();
        int n = array.Length;
        while (n > 1)
        {
            // Get a random index from 0 to n-1
            int k = rng.Next(n--);
            // Swap the element at the current end with the random element
            Transform temp = array[n];     
            array[n] = array[k];
            array[k] = temp;
        }
    }
}
