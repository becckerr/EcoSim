using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dropping : MonoBehaviour
{
    bool notSpawnable;
    float timer;

    public GameObject newPlant;

    [SerializeField] GameObject gridObj;
    Grid grid;

    // Start is called before the first frame update
    void Start()
    {
        gridObj = GameObject.Find("A*");
        grid = gridObj.GetComponent<Grid>();
        notSpawnable = false;
    }

    void Update()
    {
        timer += Time.deltaTime * 1f;

        if ((timer > 5) & (!notSpawnable))
        {
            Call();

            notSpawnable = true;
        }
    }
    void Call()
    {
        SpawnPlant();
    }

    void SpawnPlant()
    {
        Node pNode = grid.NodeFromWorldPoint(transform.position);
        int probability = Random.Range(-100, 101);

        if ((pNode.walkable) && (probability > 50))
        {
            Vector3 spawnLocation = pNode.worldPosition;

            Instantiate(newPlant, spawnLocation, Quaternion.identity);

            Dest();
        }
        else
        {
            Dest();
        }
    }

    void Dest()
    {
        Destroy(gameObject);
    }
}
