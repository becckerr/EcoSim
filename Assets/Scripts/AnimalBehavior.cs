using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEngine;

public class AnimalBehavior : MonoBehaviour
{
    public float hunger, thirst, digestion, horny, age;

	public AnimalActions currentAction;

	float timeBetweenActionChoices = 1.5f;
	float timeToDeathByHunger = 100;
	float timeToDeathByThirst = 100;
	float criticalPercent = 0.75f;
	float drinkDuration = 5;
	float eatDuration = 6;
	float animalLifespan = 250;

    public GameManager gm;

    [SerializeField] float startSpeed, fertility;
	float speed;
    Vector3[] path;
    public Rigidbody rb;
    public GameObject hunter, pooPref, preyBaby, predBaby;
	public bool hunted, Dead;
	bool bred, digesting;

	[SerializeField] Transform foodTarget, waterTarget, mateTarget, nearestPred;
	float lastActionChooseTime;
	public Vector3 searchPoint, previousRPoint, randPoint;
	Vector3 _scale, lastPos;
	Vector3 zeroPoint = new Vector3(0,-0.55f,0);
	public int animalType, animalGender;
	int targetIndex, growthstage;
	Transform target;

	[SerializeField] GameObject gridObj, prContainer;
	Grid grid;
	public float scaleFactor;
	int stuck;
	//Node moveFromNode;
	//Node moveTargetNode;

	void Start()
    {
        gm = GameManager.instance;
		rb = gameObject.GetComponent<Rigidbody>();
		gridObj = GameObject.Find("A*");
		grid = gridObj.GetComponent<Grid>();

		init();
	}

    void init()
    {
		lastPos = new Vector3(0,-0.55f,0);
		searchPoint= new Vector3(0,0,0);
		randPoint= new Vector3(0,0,0);
		previousRPoint = new Vector3(0,0,0);
		lastActionChooseTime = 0;
		targetIndex= 0;
		hunger = 0;
		thirst = 0;
		digestion = 0;
		horny = 0;
		age = 0;
		stuck = 0;
		foodTarget = null;
		waterTarget = null; 
		mateTarget = null;
		nearestPred = null;
		currentAction = AnimalActions.None;
		hunted = false;
		bred = false;
		digesting = false;
		Dead = false;
		//moveFromNode = grid.NodeFromWorldPoint(transform.position);

      if(gameObject.tag == ("Prey"))
      {
			animalType = 1;
	  }
      if(gameObject.tag == ("Predator"))
      {
			animalType = 2;
	  }

		if(animalType == 1)
        {
			gm.Prey.Add(gameObject);
			prContainer = GameObject.Find("PreyContainer");
			transform.parent = prContainer.transform;
		}
		else if (animalType == 2)
        {
			gm.Predators.Add(gameObject);
			prContainer = GameObject.Find("PredatorContainer");
			transform.parent = prContainer.transform;
		}

		animalGender = Random.Range(1, 3);
		//_scale = transform.localScale;

		hunted = false;
	}

    // Update is called once per frame
    void Update()
    {
		if(!Dead)
		{
			//decreasing needs over time
			age += (Time.deltaTime * 1 / animalLifespan);
			//float clampValue = Mathf.Clamp01(growth);
			hunger += Time.deltaTime * 1 / timeToDeathByHunger;
			thirst += Time.deltaTime * 1 / timeToDeathByThirst;
			horny += Time.deltaTime * 1 / 125;

			//float ageFactor = (age * .5f * animalLifespan) / 200;
			//scaleFactor = Mathf.Clamp(ageFactor, .5f, 1f);
			//transform.localScale = _scale * scaleFactor;

			if (digesting)
        	{
				digestion += Time.deltaTime * 1 / 75;
			}
			if ((digestion >= 1) && (digesting))
        	{
				Poo();
				digestion = 0;
				digesting = false;
			}

			if (.25f > age)
			{
				growthstage = 0;
			}
			if ((.25f < age) && (age < .5f))
			{
				growthstage = 1;
			}
			if (age > .5)
			{
				growthstage = 2;
			}
			if ((age >= 1) || (hunger >= 1) || (thirst >= 1))
			{
				currentAction = AnimalActions.Die;
			}

			// Handle interactions with external things, like food, water, mates
			HandleInteractions();
			float timeSinceLastActionChoice = Time.time - lastActionChooseTime;
			if (timeSinceLastActionChoice > timeBetweenActionChoices)
			{
				hunted = false;

				if (animalType == 1)
				{
					CheckIfHunted();
				}

				speed = startSpeed;

				StuckCheck();
			}
		}
	}
	
	void StuckCheck()
	{
		if(lastPos == transform.position)
		{
			stuck += 1;

			if(stuck < 3)
			{
				ChooseNextAction();
			}
			else
			{
				currentAction = AnimalActions.None;
				stuck = 0;
			}
		}
		else
		{
			stuck = 0;
			ChooseNextAction();
		}
	}
	
	void CheckIfHunted()
	{
		gameObject.GetComponent<FieldOfView>().findClosestPredator();
		if (gameObject.GetComponent<FieldOfView>().closestPredator != null)
		{
			nearestPred = gameObject.GetComponent<FieldOfView>().closestPredator;
			float distance = Vector2.Distance(transform.position, nearestPred.position);
			if ((distance < 10) && (thirst < criticalPercent) && (hunger < criticalPercent))
			{
				hunted = true;
			}
			//else
			//{hunted = false;}
		}
		//else if (gameObject.GetComponent<FieldOfView>().closestPredator == null)
		//{hunted = false;}
	}

	protected virtual void ChooseNextAction()
	{
		lastActionChooseTime = Time.time;
		lastPos = transform.position;

		if(hunted)
        {
			currentAction = AnimalActions.Escape;
		}
		if(!hunted)
        {
				bool currentlyEating = currentAction == AnimalActions.Eating && foodTarget && hunger > 0;
			if (((hunger >= thirst) && (hunger > .33f)) || currentlyEating && thirst < criticalPercent)
			{
				getFood();
			}
			else if ((hunger < thirst) && (thirst >= 0.33f))
			{
				getWater();
			}
			else if ((hunger < 0.33f) && (thirst < 0.33f) && (horny > 0.45f))
			{
				getMate();
			}
			else if ((hunger < 0.33f) && (thirst < 0.33f) && (horny < 0.45f))
			{
				currentAction = AnimalActions.Exploring;
			}
		}
		
		Act();
	}

	void HandleInteractions()
	{
		switch (currentAction)
		{
			case AnimalActions.Eating:
				if ((foodTarget != null) && (hunger > 0))
				{
					if (animalType ==1)
                	{
						float eatAmount = Mathf.Min(hunger, Time.deltaTime * 2 / eatDuration);
						foodTarget.gameObject.GetComponent<PlantBehavior>().growth -= eatAmount;
						hunger -= eatAmount;
						hunger = Mathf.Clamp01(hunger);

						digestion += eatAmount;
						digesting = true;
					}
					else if (animalType == 2)
                	{
						float eatAmount;
						if(foodTarget.gameObject.GetComponent<AnimalBehavior>().animalType == 1)
						{eatAmount = foodTarget.gameObject.GetComponent<AnimalBehavior>().age * 2f;}
						else
						{eatAmount = foodTarget.gameObject.GetComponent<AnimalBehavior>().age * .5f;}
						
						hunger -= eatAmount;
						hunger = Mathf.Clamp01(hunger);
						foodTarget.gameObject.GetComponent<AnimalBehavior>().currentAction = AnimalActions.Die;
						foodTarget.gameObject.GetComponent<AnimalBehavior>().Die();

						digestion += eatAmount;
						digesting = true;
					}
				}
			break;
			case AnimalActions.Drinking:
				if (thirst > 0)
				{
					thirst -= Time.deltaTime * 2 / drinkDuration;
					thirst = Mathf.Clamp01(thirst);
				}
			break;
			case AnimalActions.Breeding:
				if (horny > 0)
				{
					mateTarget.gameObject.GetComponent<AnimalBehavior>().currentAction = AnimalActions.Breeding;
					horny = 0;
				}
				if (!bred)
				{
					bred = true;
					Propagate();
				}
			break;
			case AnimalActions.Die:
				Die();
			break;
		}
	}

	protected void Act()
	{
		switch (currentAction)
		{
			case AnimalActions.Exploring:
				Wander(1);
				//Node node = grid.NodeFromWorldPoint(transform.position);
				//StartMoveToCoord(grid.GetNextTileWeighted(node, moveFromNode), node);
				break;
			case AnimalActions.GoingToFood:
				if (AreNeighbours(transform.position, foodTarget.position))
				{
					LookAt(foodTarget.position);
					currentAction = AnimalActions.Eating;
				}
				else
				{
					if(animalType == 2)
					{speed = startSpeed * 2;}
					MoveTo(foodTarget.position);
				}
				break;
			case AnimalActions.GoingToWater:
				if (AreNeighbours(transform.position, waterTarget.position))
				{
					LookAt(waterTarget.position);
					currentAction = AnimalActions.Drinking;
				}
				else
				{
					MoveTo(waterTarget.position);
				}
				break;
			case AnimalActions.GoingToMate:
				if (AreNeighbours(transform.position, mateTarget.position))
				{
					LookAt(mateTarget.position);
					currentAction = AnimalActions.Breeding;
				}
				else
				{
					MoveTo(mateTarget.position);
				}
				break;
			case AnimalActions.Escape:
				Vector3 escapePoint = 2 * (transform.position - nearestPred.position);
				MoveTo(escapePoint);
			break;
		}
	}
	/*
	protected void StartMoveToCoord (Node target, Node current) {
        LookAt (moveTargetNode.worldPosition);

		float moveTime = Mathf.Min (1, Time.deltaTime * speed);
        transform.position = Vector3.Lerp (current.worldPosition, target.worldPosition, moveTime);
    }
	*/
	protected void LookAt(Vector3 target)
	{
		Vector3 offset = target - transform.position;
		transform.eulerAngles = Vector3.up * Mathf.Atan2(offset.x, offset.y) * Mathf.Rad2Deg;
	}
	
	void getFood()
	{
		switch (animalType)
		{
			case 1:
				gameObject.GetComponent<FieldOfView>().findClosestPlant();

				if (gameObject.GetComponent<FieldOfView>().closestPlant != null)
				{
					foodTarget = gameObject.GetComponent<FieldOfView>().closestPlant;
					currentAction = AnimalActions.GoingToFood;
				}
				else
				{
					currentAction = AnimalActions.Exploring;
				}
				break;
			case 2:
				gameObject.GetComponent<FieldOfView>().findClosestPrey();

				if (gameObject.GetComponent<FieldOfView>().closestPrey != null)
				{
					foodTarget = gameObject.GetComponent<FieldOfView>().closestPrey;
					currentAction = AnimalActions.GoingToFood;
				}
				else if (foodTarget == null)
				{
					if(hunger > .95)
					{
						gameObject.GetComponent<FieldOfView>().findClosestPredator();

						if (gameObject.GetComponent<FieldOfView>().closestPredator != null)
						{
							foodTarget = gameObject.GetComponent<FieldOfView>().closestPredator;
							currentAction = AnimalActions.GoingToFood;
							Debug.Log("Hannibal time");
						}
						else
						{
							currentAction = AnimalActions.Exploring;
						}
					}
					else
					{
						currentAction = AnimalActions.Exploring;
					}
				}
				break;
		}
	}

	void getWater()
	{
		gameObject.GetComponent<FieldOfView>().findClosestWater();

		if (gameObject.GetComponent<FieldOfView>().closestWater != null)
		{
			waterTarget = gameObject.GetComponent<FieldOfView>().closestWater;
			currentAction = AnimalActions.GoingToWater;
		}
		else
		{
			currentAction = AnimalActions.Exploring;
		}
	}

	void getMate()
    {
		switch (animalType)
		{
			case 1:
				gameObject.GetComponent<FieldOfView>().findClosestPrey();

				if (gameObject.GetComponent<FieldOfView>().closestPrey != null)
				{
					mateTarget = gameObject.GetComponent<FieldOfView>().closestPrey;
				
					if ((animalGender != mateTarget.gameObject.GetComponent<AnimalBehavior>().animalGender) && (mateTarget.gameObject.GetComponent<AnimalBehavior>().age > .25f))
					{
						currentAction = AnimalActions.GoingToMate;

						bred = false;
						mateTarget.gameObject.GetComponent<AnimalBehavior>().bred = false;
					}
					else
					{
						currentAction = AnimalActions.Exploring;
					}
				}
				else if (gameObject.GetComponent<FieldOfView>().closestPrey == null)
				{
					currentAction = AnimalActions.Exploring;
				}
				break;
			case 2:
				gameObject.GetComponent<FieldOfView>().findClosestPredator();

				if (gameObject.GetComponent<FieldOfView>().closestPredator != null)
				{
					mateTarget = gameObject.GetComponent<FieldOfView>().closestPredator;

					if ((animalGender != mateTarget.gameObject.GetComponent<AnimalBehavior>().animalGender) && (mateTarget.gameObject.GetComponent<AnimalBehavior>().age > .25f) && (mateTarget.gameObject.GetComponent<AnimalBehavior>().hunger < criticalPercent))
					{
						currentAction = AnimalActions.GoingToMate;

						bred = false;
						mateTarget.gameObject.GetComponent<AnimalBehavior>().bred = false;
						mateTarget.gameObject.GetComponent<AnimalBehavior>().mateTarget = this.gameObject.transform;
						mateTarget.gameObject.GetComponent<AnimalBehavior>().currentAction = AnimalActions.GoingToMate;
					}
					else
					{
						currentAction = AnimalActions.Exploring;
					}
				}
				else if (gameObject.GetComponent<FieldOfView>().closestPredator == null)
				{
					currentAction = AnimalActions.Exploring;
				}
				break;
		}
	}
	
	void Wander(int wandType)
	{
		switch(wandType)
		{
			case 1:
				Vector3 course = new Vector3(Mathf.Round(previousRPoint.x), 0, Mathf.Round(previousRPoint.z)); 
				randPoint = new Vector3(Random.Range(-20, 21)+.5f, 0, Random.Range(-20, 21)+.5f);
				searchPoint = transform.position + randPoint + course;

				wanderMove(grid.NodeFromWorldPoint(searchPoint));
			break;

			case 2:
				Vector3 ncourse = new Vector3(Mathf.Round(-previousRPoint.x), 0, Mathf.Round(-previousRPoint.z)); 
				randPoint = new Vector3(Random.Range(-10, 11)+.5f, 0, Random.Range(-10, 11)+.5f);
				searchPoint = transform.position + randPoint + ncourse;

				wanderMove(grid.NodeFromWorldPoint(searchPoint));
			break;

		}
	}
	void wanderMove(Node endNode)
	{
		switch (endNode.walkable)
		{
			case true:
				previousRPoint = randPoint;
				MoveTo(endNode.worldPosition);
				break;
			case false:
				//issue when they have to turn, probably from divison?
				Wander(2);
				previousRPoint = randPoint;
				break;
		}
		//issue arises here
	}
	
	void Poo()
    {
		Instantiate(pooPref, transform.position, Quaternion.identity);
	}

	public void Die()
    {
		switch (animalType)
		{
			case 1:
				gm.Prey.Remove(this.gameObject);
				break;
			case 2:
				gm.Predators.Remove(this.gameObject);
				break;
		}
		Dead = true;
		GameObject dContainer = GameObject.Find("DeadContainer");
		transform.parent = dContainer.transform;
		gameObject.SetActive(false);
	}

	public void Destroy()
	{
		Destroy(gameObject);
	}

	void Propagate()
	{
		if(animalGender == 2)
        {
			Node cNode = grid.NodeFromWorldPoint(transform.position);
			List<Node> neighbours = grid.GetNeighbours(cNode);

			foreach (Node neighbour in neighbours)
			{
				int probability = Random.Range(0, 101);

				if ((neighbour.walkable) && (probability > fertility))
				{
					Vector3 spawnLocation = neighbour.worldPosition;
					switch (animalType)
					{
						case 1:
							Instantiate(preyBaby, spawnLocation, Quaternion.identity);
							break;
						case 2:
							Instantiate(predBaby, spawnLocation, Quaternion.identity);
							break;
					}
				}
			}
		}
	}

	public bool AreNeighbours(Vector3 a, Vector3 b)
	{
		return System.Math.Abs(a.x - b.x) <= 4 && System.Math.Abs(a.z - b.z) <= 4;
	}

	//move to target
	void MoveTo(Vector3 target)
	{
		LookAt(target);
		
		PathRequestManager.RequestPath(transform.position, target, OnPathFound);
	}

	public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
	{
		if (pathSuccessful)
		{
			path = newPath;
			targetIndex = 0;
			StopCoroutine("FollowPath");
			StartCoroutine("FollowPath");
		}
	}

	IEnumerator FollowPath()
	{
		Vector3 currentWaypoint = path[0];
		while (true)
		{
			if (transform.position == currentWaypoint)
			{
				targetIndex++;
				if (targetIndex >= path.Length)
				{
					yield break;
				}
				currentWaypoint = path[targetIndex];
			}

			transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);
			yield return null;

		}
	}
}

