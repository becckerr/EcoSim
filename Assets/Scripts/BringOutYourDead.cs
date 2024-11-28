using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class BringOutYourDead : MonoBehaviour
{
    float timeBetweenActionChoices = 60;
    float lastActionChooseTime;
    Transform[] deadToClear;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float timeSinceLastActionChoice = Time.time - lastActionChooseTime;
		if (timeSinceLastActionChoice > timeBetweenActionChoices)
		{
            for(int i = 0; i < deadToClear.Length; i++)
            {Destroy(deadToClear[i]);}
			
            AddDead();
		}
    }

    void AddDead()
    {
        foreach(Transform child in gameObject.transform)
            {
                //deadToClear.Add(child);
            }
    }
}
