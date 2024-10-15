using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public List<GameObject> gameObjects = new List<GameObject>();

    void Start()
    {
         
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
}

public class MusicData
{
    public SequenceData sequenceData;
    public string title;
    public int level;
    public Sprite sprite;
}
