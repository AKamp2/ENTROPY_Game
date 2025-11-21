using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LightLocation
{
    Dining,
    EscapePod
}

public class LightData
{
    public Transform[] lightGroup;
    public Dictionary<Light, float> initLightIntensity;

    public LightData(Transform[] lg, Dictionary<Light, float> li)
    {
        initLightIntensity = li;
        lightGroup = lg;
    }
}

public class LightManager : MonoBehaviour
{
    [SerializeField]
    public Dictionary<LightLocation, LightData> lightData;

    [SerializeField]
    private Transform[] diningLightGroup;
    [SerializeField]
    private Transform[] escapeLightGroup;


    private Color lightColor = new Color(0.75f, 0.75f, 0.75f, 0.0f);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lightData = new Dictionary<LightLocation, LightData>();

        SaveLightData(LightLocation.Dining, diningLightGroup);
        SaveLightData(LightLocation.EscapePod, escapeLightGroup);

        
    }

 

    //private Coroutine FickerLights(LightLocation lightEnum, float totalDuration, float singleLightDuration, bool randomSequence)
    //{

    //    if (randomSequence)
    //    {
    //        lightData[lightEnum].lightGroup = Shuffle(lightData[lightEnum].lightGroup);
    //    }

    //    return StartCoroutine(EnableLights(lightData[lightEnum].lightGroup, lightData[lightEnum].initLightIntensity, totalDuration, singleLightDuration));
    //}

    //public Coroutine FadeOutLights(LightLocation lightEnum, float totalDuration)
    //{
    //    return StartCoroutine(ControlAllLights(lightData[lightEnum].lightGroup, lightData[lightEnum].initLightIntensity, 0.0f, totalDuration));
    //}

    public Coroutine MultiplyLights(LightLocation lightEnum, float multiplier, float totalDuration)
    {
        return StartCoroutine(MultiplyLightsTask(lightData[lightEnum].lightGroup, lightData[lightEnum].initLightIntensity, multiplier, totalDuration));
    }

    public IEnumerator FlickerLights(LightLocation lightEnum, float totalDuration, float singleLightDuration, bool randomSequence)
    {
        Transform[] lightGroup = lightData[lightEnum].lightGroup;
        Dictionary<Light, float> initLightIntensity = lightData[lightEnum].initLightIntensity;

        if (randomSequence)
        {
            lightGroup = Shuffle(lightGroup);
        }

        // calculate delay time such that it is based on total duration and time of each light
        float delayBetweenLights;

        if (lightGroup.Length > 1)
            delayBetweenLights = (totalDuration - singleLightDuration) / (lightGroup.Length - 1);
        else
            delayBetweenLights = 0f;

        // each light group
        foreach (Transform t in lightGroup)
        {
            Light[] lights = t.GetComponentsInChildren<Light>();
            MeshRenderer[] meshes = t.GetComponentsInChildren<MeshRenderer>();

            foreach (MeshRenderer mesh in meshes)
            {
                StartCoroutine(FlickerIntensity(null, mesh, 2.0f, singleLightDuration));
            }

            // flicker lights on with coroutine
            foreach (Light light in lights)
            {
                StartCoroutine(FlickerIntensity(light, null, initLightIntensity[light], singleLightDuration));
            }

            // if there is no delay, dont wait
            if (delayBetweenLights <= 0.0f)
            {
                yield return null;
            }

            yield return new WaitForSeconds(delayBetweenLights);
        }
    }

    private IEnumerator FlickerIntensity(Light light, MeshRenderer mesh, float maxIntensity, float singleLightDuration)
    {
        // do flicker logic here
        float timer = 0f; // overall time of flickering
        float flickerTimer = 0f; // time on a current state
        float flickerDelay = 0f; // time till next flicker

        float lerpIntensity;
        bool isOn = false;

        // loops duration of flicker. 2.0f duration
        while (timer < singleLightDuration)
        {
            // lerp the brightness of max intensity so it gradually fades brighter
            lerpIntensity = Mathf.Lerp(0, maxIntensity, Mathf.Clamp01(timer / singleLightDuration));

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

    public IEnumerator FadeOutAllLights(LightLocation lightEnum, float endIntensity, float totalDuration)
    {
        Transform[] lightGroup = lightData[lightEnum].lightGroup;
        Dictionary<Light, float> initLightIntensity = lightData[lightEnum].initLightIntensity;

        // Gather all lights first
        List<Light> allLights = new List<Light>();

        foreach (Transform t in lightGroup)
        {
            allLights.AddRange(t.GetComponentsInChildren<Light>());
        }

        float time = 0.0f;

        while (time < totalDuration)
        {
            time += Time.deltaTime;
            float t = time / totalDuration;

            foreach (Light light in allLights)
            {
                light.intensity = Mathf.Lerp(initLightIntensity[light], endIntensity, t);
            }

            // yield once per frame for smooth fading
            yield return null;
        }

        // Ensure final intensity is exactly 0
        foreach (Light light in allLights)
        {
            light.intensity = endIntensity;
        }
    }

    
    private IEnumerator MultiplyLightsTask(Transform[] lightGroup, Dictionary<Light, float> initLightIntensity, float multiplier, float totalDuration)
    {
        // Gather all lights first
        List<Light> allLights = new List<Light>();

        Dictionary<Light, float> multipliedLightIntensity = new Dictionary<Light, float>(initLightIntensity);
        List<Light> multipliedKeys = new List<Light>(multipliedLightIntensity.Keys); 
        foreach (Light key in multipliedKeys)
        {
            multipliedLightIntensity[key] *= multiplier;
        }


        foreach (Transform t in lightGroup)
        {
            allLights.AddRange(t.GetComponentsInChildren<Light>());
        }

        float time = 0.0f;

        while (time < totalDuration)
        {
            time += Time.deltaTime;
            float t = time / totalDuration;

            foreach (Light light in allLights)
            {
                light.intensity = Mathf.Lerp(initLightIntensity[light], multipliedLightIntensity[light], t);
            }

            // yield once per frame for smooth fading
            yield return null;
        }

        // Ensure final intensity is exactly 0
        foreach (Light light in allLights)
        {
            light.intensity = multipliedLightIntensity[light];
        }
    }



   

    private void SaveLightData(LightLocation saveLocation, Transform[] array)
    {
        Dictionary<Light, float> initLightIntensity = new Dictionary<Light, float>();

        foreach (Transform transform in array)
        {
            // saves the initial light values so in editor can display the intended lights
            foreach (Light l in transform.GetComponentsInChildren<Light>())
            {
                initLightIntensity[l] = l.intensity;
            }

        }

        lightData.Add(saveLocation, new LightData(array, initLightIntensity));
        Debug.Log("Save Successful!");

        DisableLights(array);
    }

    // turn off lights
    private void DisableLights(Transform[] array)
    {
        foreach (Transform transform in array)
        {
            // saves the initial light values so in editor can display the intended lights
            foreach (Light l in transform.GetComponentsInChildren<Light>())
            {
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
    private Transform[] Shuffle(Transform[] array)
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

        return array;
    }
}
