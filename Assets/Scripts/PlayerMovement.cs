using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    public GameObject player;
    [SerializeField] float hunger, thirst, digestion, horny;
    public int animalType, animalGender;
    bool digesting;
    float timeToDeathByHunger = 100;
	float timeToDeathByThirst = 100;
	float criticalPercent = 0.75f;
    public GameObject pooPref;
    public GameManager gm;
    public LayerMask groundMask;
    public Vector3 hitPos;
    [SerializeField] float speed, fertility;
    Vector3[] path;
    public Rigidbody rb;
    int targetIndex;
    public Transform pTarget;
    string tName;

    private void Start()
    {
        animalType = 2;
        animalGender = 1;
        hunger = 0;
		thirst = 0;
		digestion = 0;
		horny = 0;
        gm = GameManager.instance;
        digesting = false;
        //cc = GetComponent<CharacterController>();
        //cc.enabled = true;
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    void Update()
    {
        //if(pTarget != null)
        //{MoveTo(pTarget.position);}

		hunger += Time.deltaTime * 1 / timeToDeathByHunger;
		thirst += Time.deltaTime * 1 / timeToDeathByThirst;
		horny += Time.deltaTime * 1 / 125;

        if (Input.GetMouseButtonDown(0))
        {
            pTarget = null;
            MouseDown();
        }

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
    
        if(AreNeighbours(transform.position, pTarget.position))
        {
            if ((hunger > .33) && (pTarget.gameObject.GetComponent<AnimalBehavior>().animalType == 1))
            {
                float eatAmount;
			    eatAmount = pTarget.gameObject.GetComponent<AnimalBehavior>().age * 2f;

                hunger -= eatAmount;
				hunger = Mathf.Clamp01(hunger);

				digestion += eatAmount;
				digesting = true;

                eatTarget(pTarget);
            }

            if ((thirst > .33) && pTarget.gameObject.CompareTag("Water"))
            {
                thirst = 0;
            }

            if ((horny > .45) && (pTarget.gameObject.GetComponent<AnimalBehavior>().animalType == 2) && (pTarget.gameObject.GetComponent<AnimalBehavior>().age > .25f))
            {
                if (pTarget.gameObject.GetComponent<AnimalBehavior>().animalGender == 2)
                {
                    pTarget.gameObject.GetComponent<AnimalBehavior>().currentAction = AnimalActions.Breeding;
                    Debug.Log("Bred at " + gm.seconds);
					horny = 0;
                }
                else
                {Debug.Log("Keep trying buddy!");}
            }
        }

        if ((hunger >= 1) || (thirst >= 1))
			{
                Debug.Log("You died at " + gm.seconds);
				dest();
			}
    }

    void Poo()
    {
		Instantiate(pooPref, transform.position, Quaternion.identity);
	}

    void eatTarget(Transform t)
    {
        t.gameObject.GetComponent<AnimalBehavior>().currentAction = AnimalActions.Die;
        t.gameObject.GetComponent<AnimalBehavior>().Die();
        pTarget = null;
        Debug.Log("Ate at " + gm.seconds);
    }
    
    public bool AreNeighbours(Vector3 a, Vector3 b)
	{
		return System.Math.Abs(a.x - b.x) <= 4 && System.Math.Abs(a.z - b.z) <= 4;
	}

    void MouseDown()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.gameObject.CompareTag("Ground"))
            {
                hitPos = hit.point;
                MoveTo(hit.point);
                /*
                else
                {
                    if (hit.point.x < -100)
                    {
                        hitPos.x = -100;
                    }
                    if (hit.point.z < -100)
                    {
                        hitPos.z = -100;
                    }
                    if (hit.point.x > 0)
                    {
                        hitPos.x = 0;
                    }
                    if (hit.point.z > 0)
                    {
                        hitPos.z = 0;
                    }
                    MoveTo(hitPos);
                }
                */
            }
            if (!hit.collider.gameObject.CompareTag("Ground"))
            {
                hitPos = hit.collider.gameObject.transform.position;
                pTarget = hit.collider.gameObject.transform;
                MoveTo(pTarget.position);
            }
        }
    }

    void dest()
    {
        Destroy(gameObject);
    }

    protected void LookAt(Vector3 _target)
	{
		Vector3 offset = _target - transform.position;
		transform.eulerAngles = Vector3.up * Mathf.Atan2(offset.x, offset.y) * Mathf.Rad2Deg;
	}

    //pathfinding
    void MoveTo(Vector3 _target)
	{
		LookAt(_target);
		
		PathRequestManager.RequestPath(transform.position, _target, OnPathFound);
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

