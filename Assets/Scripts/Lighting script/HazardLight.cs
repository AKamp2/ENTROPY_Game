using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.UIElements;

public class HazardLight : MonoBehaviour
{
    [SerializeField]
    private bool isHazard;
    [SerializeField]
    private Light light;
    [SerializeField] 
    private Light lightBase;
    [SerializeField]
    private float rotateParam;

    public bool IsHazard
    {
        get { return isHazard; }
        set { isHazard = value; }
    }


    // Update is called once per frame
    void Update()
    {
        //if the hazrd is set
        if(isHazard)
        {
            if(!light.enabled)
            {
                light.enabled = true;
                lightBase.enabled = true;
            }
            light.transform.Rotate(Vector3.up * rotateParam * Time.deltaTime);
        }
        else if (!isHazard)
        {
            if (light.enabled)
            {
                light.enabled = false;
                lightBase.enabled = false;
            }
        }
    }
}
