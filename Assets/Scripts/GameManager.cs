using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;

    int coordX, coordY;
    string animaltype;
    public List<string> Data = new List<string>();
    float timeBetweenOutputs = 5;
    float lastOutputTime;
    bool complete;
    string result;
    [SerializeField] int TotalTimeInSeconds = 600;

    public List<GameObject> Predators = new List<GameObject>();
    public List<GameObject> Prey = new List<GameObject>();
    public List<GameObject> Plants = new List<GameObject>();

    public float seconds;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(this);

        //Init();
    }

    void Init()
    {}

    void Start()
    {}

    public void timeOutput()
    {
        Debug.Log(seconds);
    }

    void Update()
    {
        seconds += 1 * Time.deltaTime;

        float timeSinceLastOutput = Time.time - lastOutputTime;
        if (timeSinceLastOutput > timeBetweenOutputs)
        {
            lastOutputTime = Time.time;
  
            if(!complete)
            {
                Output();
            }
        }

        if (seconds > TotalTimeInSeconds & (!complete))
        {
            FinalOutput();
        }
    }

    void Output()
    {
        string predCount = (Predators.Count + " predators at " + seconds + " seconds");
        string preyCount = (Prey.Count + " prey at" + seconds + " seconds");
        string plantCount = (Plants.Count + " plants at " + seconds + " seconds");

        Debug.Log(predCount);
        Debug.Log(preyCount);
        Debug.Log(plantCount);

        Data.Add(predCount);
        Data.Add(preyCount);
        Data.Add(plantCount);
    }

    void FinalOutput()
    {
        foreach (string _data in Data)
        {
            result += _data.ToString() + ", ";
        }
        Debug.Log(result);

        Complete();
    }

    void Complete()
    {
        complete = true;
    }
}
