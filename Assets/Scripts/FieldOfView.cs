using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FieldOfView : MonoBehaviour
{

	public float viewRadius;
	[Range(0, 360)]
	public float viewAngle;

	public LayerMask predatorMask;
	public LayerMask preyMask;
	public LayerMask plantMask;
	public LayerMask waterMask;
	public LayerMask obstacleMask;

	//[HideInInspector]
	public List<Transform> visiblePredators = new List<Transform>();
	public List<Transform> visiblePrey = new List<Transform>();
	public List<Transform> visiblePlants = new List<Transform>();
	public List<Transform> visibleWater = new List<Transform>();

	public bool plantNotSpotted, preyNotSpotted, predatorNotSpotted, noTarget;

	public Transform closestPlant, closestPrey, closestPredator, closestWater;

	public void findClosestPlant()
	{
		visiblePlants.Clear();
		Collider[] plantsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, plantMask);
		for (int r = 0; r < plantsInViewRadius.Length; r++)
		{
			Transform plant = plantsInViewRadius[r].transform;
			Vector3 dirToPlant = (plant.position - transform.position).normalized;
			if (Vector3.Angle(transform.forward, dirToPlant) < viewAngle / 2)
			{
				float dstToPlant = Vector3.Distance(transform.position, plant.position);

				if ((!Physics.Raycast(transform.position, dirToPlant, dstToPlant, obstacleMask)) && (plant != gameObject.transform) && (!visiblePlants.Contains(plant)))
				{
					visiblePlants.Add(plant);
				}
			}
		}

		float closestDistanceSqr1 = Mathf.Infinity;
		foreach (Transform _plant in visiblePlants)
		{
			Vector3 directionToTarget1 = _plant.transform.position - transform.position;

			float dSqrToTarget1 = directionToTarget1.sqrMagnitude;

			if ((dSqrToTarget1 < closestDistanceSqr1) & _plant.gameObject.activeSelf)
			{
				closestDistanceSqr1 = dSqrToTarget1;
				closestPlant = _plant;
			}
		}
	}

	public void findClosestPredator()
	{
		visiblePredators.Clear();
		Collider[] predatorsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, predatorMask);
		for (int i = 0; i < predatorsInViewRadius.Length; i++)
		{
			//gets the direction to these objects
			Transform predator = predatorsInViewRadius[i].transform;
			Vector3 dirToPredator = (predator.position - transform.position).normalized;
			if (Vector3.Angle(transform.forward, dirToPredator) < viewAngle / 2)
			{
				float dstToPredator = Vector3.Distance(transform.position, predator.position);

				if ((!Physics.Raycast(transform.position, dirToPredator, dstToPredator, obstacleMask)) && (predator != gameObject.transform) && (!visiblePredators.Contains(predator)))
				{
					//add the target to the list
					visiblePredators.Add(predator);
				}
			}
		}

		float closestDistanceSqr2 = Mathf.Infinity;
		foreach (Transform _predators in visiblePredators)
		{
			Vector3 directionToTarget2 = _predators.transform.position - transform.position;

			float dSqrToTarget2 = directionToTarget2.sqrMagnitude;

			if ((dSqrToTarget2 < closestDistanceSqr2) && _predators.gameObject.activeSelf)
			{
				closestDistanceSqr2 = dSqrToTarget2;
				closestPredator = _predators;
			}
		}
	}

	public void findClosestPrey()
	{
		visiblePrey.Clear();
		Collider[] preyInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, preyMask);
		for (int t = 0; t < preyInViewRadius.Length; t++)
		{
			Transform prey = preyInViewRadius[t].transform;
			Vector3 dirToPrey = (prey.position - transform.position).normalized;
			if (Vector3.Angle(transform.forward, dirToPrey) < viewAngle / 2)
			{
				float dstToPrey = Vector3.Distance(transform.position, prey.position);

				if ((!Physics.Raycast(transform.position, dirToPrey, dstToPrey, obstacleMask)) && (prey != gameObject.transform) && (!visiblePrey.Contains(prey)))
				{
					visiblePrey.Add(prey);
				}
			}
		}

		float closestDistanceSqr2 = Mathf.Infinity;
		foreach (Transform _prey in visiblePrey)
		{
			Vector3 directionToTarget2 = _prey.transform.position - transform.position;

			float dSqrToTarget2 = directionToTarget2.sqrMagnitude;

			if ((dSqrToTarget2 < closestDistanceSqr2) && _prey.gameObject.activeSelf)
			{
				closestDistanceSqr2 = dSqrToTarget2;
				closestPrey = _prey;
			}
		}
	}

	public void findClosestWater()
    {
		Collider[] waterInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, waterMask);
		for (int q= 0; q < waterInViewRadius.Length; q++)
		{
			Transform water = waterInViewRadius[q].transform;
			Vector3 dirToWater = (water.position - transform.position).normalized;
			if (Vector3.Angle(transform.forward, dirToWater) < viewAngle / 2)
			{
				float dstToWater = Vector3.Distance(transform.position, water.position);

				if ((!Physics.Raycast(transform.position, dirToWater, dstToWater, obstacleMask)) && (!visibleWater.Contains(water)))
				{
					visibleWater.Add(water);
				}
			}
		}

		float closestDistanceSqr3 = Mathf.Infinity;
		foreach (Transform _water in visibleWater)
		{
			Vector3 directionToTarget3 = _water.transform.position - transform.position;

			float dSqrToTarget3 = directionToTarget3.sqrMagnitude;

			if (dSqrToTarget3 < closestDistanceSqr3)
			{
				closestDistanceSqr3 = dSqrToTarget3;
				closestWater = _water;
			}
		}
	}

	public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
	{
		if (!angleIsGlobal)
		{
			angleInDegrees += transform.eulerAngles.y;
		}
		return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
	}
}