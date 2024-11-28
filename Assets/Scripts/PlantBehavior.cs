using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantBehavior : MonoBehaviour
{
    public GameManager gm;
    [SerializeField] int fertility;
    int growthstage, propCheck;
    public float growth;
    float plantLifespan = 100;
    [SerializeField] GameObject Plant, newPlant;
    [SerializeField] GameObject gridObj, pContainer;
    Grid grid;

    Vector3 _scale, _posi;

    void Start()
    {
        gm = GameManager.instance;
        init();
    }
    // Start is called before the first frame update
    void init()
    {
        Plant = transform.GetChild(0).gameObject;
        pContainer = GameObject.Find("PlantContainer");
        gridObj = GameObject.Find("A*");
        grid = gridObj.GetComponent<Grid>();
        growth = 0f;

        transform.parent = pContainer.transform;

        _scale = Plant.transform.localScale;
        _posi = Plant.transform.localPosition;

        propCheck = 0;

        gm.Plants.Add(gameObject);
        //position = gm.Plants.IndexOf(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        growth += Time.deltaTime * 2f / plantLifespan;

        //Plant.transform.localPosition = _posi * ((growth * plantLifespan) / 100);
        //Plant.transform.localScale = _scale * ((growth * plantLifespan) / 100);

        if(growth < 0)
        {
            Die();
        }
        if (.25f > growth)
        {
            growthstage = 0;
        }
        if ((.25f < growth) && (growth < .5f))
        {
            growthstage = 1;
        }
        if(growth > .5)
        {
            growthstage = 2;
        }
        if (growth > 1)
        {
            Die();
        }

        if (propCheck != growthstage)
        {
            propCheck = growthstage;

            Propagate();
        }
    }

    void Die()
    {
        //int p = Plants.IndexOf(this);
        gm.Plants.Remove(this.gameObject);
        Destroy(gameObject);
    }

    void Propagate()
    {
        Node cNode = grid.NodeFromWorldPoint(transform.position);
        List<Node> neighbours = grid.GetNeighbours(cNode);

        foreach (Node neighbour in neighbours)
        {
            int probability = Random.Range(-100, 101);

            if ((neighbour.walkable) && (neighbour.notOccupied) && (probability > fertility))
            {
                Vector3 spawnLocation = neighbour.worldPosition;
                Instantiate(newPlant, spawnLocation, Quaternion.identity);
                //newp.transform.localScale = new Vector3(.75f, .75f, .75f);
            }
        }
    }
}
