using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.UI;
using Random = UnityEngine.Random;

public class AmbientController : MonoBehaviour
{

    public static int maxLayers = 4;
    public float bpm = 120.0f;
    public int numBeatsPerSegment = 31;
    public AudioClip[] clips;
    public GameObject looperPrefab;
    
    // I have tried a couple of ways doing this. This one is not the best for code legibility, but it is the one that...
    // is serializable. This way they can be changed in the inspector.
    public int[] beatsLookup; //Contains the beats length of each layer
    // public Tuple<int,string>[] clipLookup; //Contains [beats,name] of each loop layer
    
    

    private double[] endTimes;
    private double nextMinLoop;
    private int sourceIndex = 0;
    // private int clipIndex = 0;
    private Looper[] loopers = new Looper[maxLayers];
    private int[] layerLookup  = new int[maxLayers]; // the index matching the looper index contains the currently playing track
    private bool running = false;

    void Start()
    {
        int layerCount = clips.Length;
        loopers = new Looper[layerCount];
        // for (int i = 0; i < layerCount; i++)
        // {
        //     GameObject child = new GameObject("MusicPlayer");
        //     child.transform.parent = gameObject.transform;
        //     loopers[i] = child.AddComponent<AudioSource>();
        //     loopers[i].loop = false;
        // }
        endTimes = new double[clips.Length];
        double startTime = AudioSettings.dspTime + 5.0f;
        for (int i = 0; i < layerCount; i++)
        {
            endTimes[i] = startTime;
        }

        for (int i = 0; i < maxLayers; i++)
        {
            // GameObject looperObject = new GameObject("looper");
            // AudioSource audioObject = looperObject.AddComponent<AudioSource>();
            // loopers[i] = audioObject.AddComponent<Looper>();
            GameObject prefabInstance = Instantiate(looperPrefab);
            loopers[i] = prefabInstance.GetComponent<Looper>();
            layerLookup[i] = -1;
        }

        nextMinLoop = startTime;
        
        running = true;
        Debug.Log("AmbientStarted");
    }

    void Update()
    {
        if (!running)
        {
            return;
        }

        double time = AudioSettings.dspTime;

        // Wait until next loop event is less than a second away
        if (time + 1.0f > nextMinLoop)
        {
            // Pick a random clip and play it with bass
            int[] nextLayers = new int[2];
            nextLayers[0] = 0;
            nextLayers[1] = Random.Range(1, clips.Length);
            QueueNext(nextLayers,nextMinLoop);

            // Debug.Log("Scheduled source " + " to start at time " + nextMinLoop);

            nextMinLoop += (60.0f / bpm) * numBeatsPerSegment;
            
            // running = false;
        }
    }

    public void QueueNext(int[] nextSet, double nextEventTime)
    {
        //nextSet must not be longer than maxlayers
        for (int i = 0; i < maxLayers; i++)
        {
            // If the layerLookup entry is not zero and is not in the nextset
            if (layerLookup[i] >= 0 && !(Array.IndexOf(nextSet,layerLookup[i])>-1))
            {
                
                if (endTimes[layerLookup[i]]-1.0f > nextEventTime) 
                {
                    Debug.Log("stopping early");
                    loopers[i].StopAt(nextEventTime);
                }
                
                layerLookup[i] = -1;
            }
        }
        foreach (int index in nextSet)
        {
            if (beatsLookup[index]>numBeatsPerSegment)
            {
                //check how much time left, and see if requeue is necessary
                if (endTimes[index]-1.0f> nextEventTime) 
                {
                    // It is already playing. leave it be.
                    // Debug.Log("No need to queue");
                    continue;
                }
            }
            // find the first free looper and play it
            int looperIndex = Array.IndexOf(layerLookup,index);
            if (looperIndex == -1)
            {
                looperIndex = Array.IndexOf(layerLookup, -1);
            } 
            if (looperIndex > -1)
            {
                endTimes[index] = nextEventTime + ((60.0f / bpm) * beatsLookup[index]);
                loopers[looperIndex].Enqueue(clips[index], nextEventTime);
                layerLookup[looperIndex] = index;
            }
        }
    }
    
    public void SetVolume(float newVolume)
    {
        foreach (Looper looper in loopers)
        {
            //looper.SetVolume(newVolume);
        }
    }
}
