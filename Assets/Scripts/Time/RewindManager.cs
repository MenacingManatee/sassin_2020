using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewindManager : MonoBehaviour
{
    public static bool rewinding;
    public static bool playing;
    public static float maxRewindTime = 30;

    public static RewindManager instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartRewind()
    {
        rewinding = true;
    }

    public void StopRewind()
    {
        rewinding = false;
        playing = true;
    }

    public void StopPlaying()
    {
        playing = false;
        
    }
}

public class RewindPoint
{
    public Vector3 position;
    public Quaternion rotation;

    public RewindPoint ( Vector3 pos , Quaternion rot)
    {
        position = pos;
        rotation = rot;
    }
}