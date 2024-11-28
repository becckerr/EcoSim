using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
	public bool displayGridGizmos;
	public LayerMask unwalkableMask;
	public LayerMask[] objMasks;
	public LayerMask[] occupiableObjMasks;
	public Vector2 gridWorldSize;
	public float nodeRadius;
	public TerrainType[] walkableRegions;
	public int obstacleProximityPenalty = 10;
	Dictionary<int, int> walkableRegionsDictionary = new Dictionary<int, int>();
	LayerMask walkableMask;

	Node[,] grid;

	float nodeDiameter;
	int gridSizeX, gridSizeY;

	int penaltyMin = int.MaxValue;
	int penaltyMax = int.MinValue;

	float timeBetweenUpdate = 1;
	float lastUpdateTime;

	static System.Random prng;

	void Awake()
	{
		nodeDiameter = nodeRadius * 2;
		gridSizeX = Mathf.RoundToInt(gridWorldSize.x / nodeDiameter);
		gridSizeY = Mathf.RoundToInt(gridWorldSize.y / nodeDiameter);

		foreach (TerrainType region in walkableRegions)
		{
			walkableMask.value |= region.terrainMask.value;
			walkableRegionsDictionary.Add((int)Mathf.Log(region.terrainMask.value, 2), region.terrainPenalty);
		}

		CreateGrid();
	}

	void Start()
	{
		prng = new System.Random();

		CreateGrid();
	}

	void Update()
    {
		float timeSinceLastUpdate = Time.time - lastUpdateTime;
		if (timeSinceLastUpdate > timeBetweenUpdate)
		{
			call();
		}
	}

	void call()
    {
		lastUpdateTime = Time.time;
		checkGrid();
	}

	void checkGrid()
	{
		foreach (Node _node in grid)
		{
			_node.notOccupied = true;
			foreach (LayerMask _mask in occupiableObjMasks)
            {
				_node.notOccupied = !(Physics.CheckSphere(_node.worldPosition, nodeRadius, _mask));

				if (!_node.notOccupied)
					break;
			}

			if (!_node.walkable)
			{
				_node.movementPenalty += obstacleProximityPenalty;
			}
		}
	}

	public int MaxSize
	{
		get
		{
			return gridSizeX * gridSizeY;
		}
	}

	void CreateGrid()
	{
		grid = new Node[gridSizeX, gridSizeY];
		Vector3 worldBottomLeft = transform.position - Vector3.right * gridWorldSize.x / 2 - Vector3.forward * gridWorldSize.y / 2;

		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = 0; y < gridSizeY; y++)
			{
				Vector3 worldPoint = worldBottomLeft + Vector3.right * (x * nodeDiameter + nodeRadius) + Vector3.forward * (y * nodeDiameter + nodeRadius);
				bool walkable = !(Physics.CheckSphere(worldPoint, nodeRadius, unwalkableMask));
				//use dictionary?
				bool notOccupied = true;

				foreach (LayerMask _mask in occupiableObjMasks)
                {
					notOccupied = !(Physics.CheckSphere(worldPoint, nodeRadius, _mask));

					if (!notOccupied)
						break;
				}
				/*
				for (int m = 0; m < objMasks.Length; m++)
				{
					notOccupied = (Physics.CheckSphere(worldPoint, nodeRadius, objMasks[m]));

					if (!notOccupied)
						break;
				}
				*/
				int movementPenalty = 0;

				Ray ray = new Ray(worldPoint + Vector3.up * 50, Vector3.down);
				RaycastHit hit;
				if (Physics.Raycast(ray, out hit, 100, walkableMask))
				{
					walkableRegionsDictionary.TryGetValue(hit.collider.gameObject.layer, out movementPenalty);
				}

				if (!walkable)
				{
					movementPenalty += obstacleProximityPenalty;
				}


				grid[x, y] = new Node(walkable, notOccupied, worldPoint, x, y, movementPenalty);
			}
		}

		BlurPenaltyMap(3);

	}

	void BlurPenaltyMap(int blurSize)
	{
		int kernelSize = blurSize * 2 + 1;
		int kernelExtents = (kernelSize - 1) / 2;

		int[,] penaltiesHorizontalPass = new int[gridSizeX, gridSizeY];
		int[,] penaltiesVerticalPass = new int[gridSizeX, gridSizeY];

		for (int y = 0; y < gridSizeY; y++)
		{
			for (int x = -kernelExtents; x <= kernelExtents; x++)
			{
				int sampleX = Mathf.Clamp(x, 0, kernelExtents);
				penaltiesHorizontalPass[0, y] += grid[sampleX, y].movementPenalty;
			}

			for (int x = 1; x < gridSizeX; x++)
			{
				int removeIndex = Mathf.Clamp(x - kernelExtents - 1, 0, gridSizeX);
				int addIndex = Mathf.Clamp(x + kernelExtents, 0, gridSizeX - 1);

				penaltiesHorizontalPass[x, y] = penaltiesHorizontalPass[x - 1, y] - grid[removeIndex, y].movementPenalty + grid[addIndex, y].movementPenalty;
			}
		}

		for (int x = 0; x < gridSizeX; x++)
		{
			for (int y = -kernelExtents; y <= kernelExtents; y++)
			{
				int sampleY = Mathf.Clamp(y, 0, kernelExtents);
				penaltiesVerticalPass[x, 0] += penaltiesHorizontalPass[x, sampleY];
			}

			int blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, 0] / (kernelSize * kernelSize));
			grid[x, 0].movementPenalty = blurredPenalty;

			for (int y = 1; y < gridSizeY; y++)
			{
				int removeIndex = Mathf.Clamp(y - kernelExtents - 1, 0, gridSizeY);
				int addIndex = Mathf.Clamp(y + kernelExtents, 0, gridSizeY - 1);

				penaltiesVerticalPass[x, y] = penaltiesVerticalPass[x, y - 1] - penaltiesHorizontalPass[x, removeIndex] + penaltiesHorizontalPass[x, addIndex];
				blurredPenalty = Mathf.RoundToInt((float)penaltiesVerticalPass[x, y] / (kernelSize * kernelSize));
				grid[x, y].movementPenalty = blurredPenalty;

				if (blurredPenalty > penaltyMax)
				{
					penaltyMax = blurredPenalty;
				}
				if (blurredPenalty < penaltyMin)
				{
					penaltyMin = blurredPenalty;
				}
			}
		}

	}

	public List<Node> GetNeighbours(Node node)
	{
		List<Node> neighbours = new List<Node>();

		for (int x = -1; x <= 1; x++)
		{
			for (int y = -1; y <= 1; y++)
			{
				if (x == 0 && y == 0)
					continue;

				int checkX = node.gridX + x;
				int checkY = node.gridY + y;

				if (checkX >= 0 && checkX < gridSizeX && checkY >= 0 && checkY < gridSizeY)
				{
					neighbours.Add(grid[checkX, checkY]);
				}
			}
		}

		return neighbours;
	}

	public Node NodeFromWorldPoint(Vector3 worldPosition)
	{
		float percentX = (worldPosition.x + gridWorldSize.x / 2) / gridWorldSize.x;
		float percentY = (worldPosition.z + gridWorldSize.y / 2) / gridWorldSize.y;
		percentX = Mathf.Clamp01(percentX);
		percentY = Mathf.Clamp01(percentY);

		int x = Mathf.RoundToInt((gridSizeX - 1) * percentX);
		int y = Mathf.RoundToInt((gridSizeY - 1) * percentY);
		return grid[x, y];
	}
	
	public Node GetNextTileRandom(Node current)
	{
		List<Node> neighbours = GetNeighbours(current);
		if (neighbours.Count == 0)
		{
			return current;
		}
		return neighbours[prng.Next(neighbours.Count)];
	}
	/*
	public Node GetNextTileWeighted(Node current, Node previous, double forwardProbability = 0.2, int weightingIterations = 3)
	{

		if (current == previous)
		{

			return GetNextTileRandom(current);
		}

		int forwardOffsetX = current.gridX - previous.gridX;
		int forwardOffsetY = current.gridY - previous.gridY;
		Vector3 offset = new Vector3(forwardOffsetX, forwardOffsetY, 0);
		// Random chance of returning foward tile (if walkable)
		if (prng.NextDouble() < forwardProbability)
		{
			Vector3 newPos = current.worldPosition + offset;
			Node forwardNode = NodeFromWorldPoint(newPos);

			if (forwardNode.gridX >= 0 && forwardNode.gridX < nodeDiameter && forwardNode.gridY >= 0 && forwardNode.gridY < nodeDiameter)
			{
				if (forwardNode.walkable)
				{
					return forwardNode;
				}
			}
		}

		// Get walkable neighbours
		var neighbours = GetNeighbours(current);
		if (neighbours.Count == 0)
		{
			return current;
		}

		// From n random tiles, pick the one that is most aligned with the forward direction:
		Vector2 forwardDir = new Vector2(forwardOffsetX, forwardOffsetY).normalized;
		float bestScore = float.MinValue;
		Node bestNeighbour = current;

		for (int i = 0; i < weightingIterations; i++)
		{
			Node neighbour = neighbours[prng.Next(neighbours.Count)];
			Vector2 _offset = new Vector2(neighbour.gridX - current.gridX, neighbour.gridY - current.gridY); //im sleepy double check this later
			float score = Vector2.Dot(_offset.normalized, forwardDir);
			if (score > bestScore)
			{
				bestScore = score;
				bestNeighbour = neighbour;
			}
		}

		return bestNeighbour;
	}
	*/
	void OnDrawGizmos()
	{
		Gizmos.DrawWireCube(transform.position, new Vector3(gridWorldSize.x, 1, gridWorldSize.y));
		if (grid != null && displayGridGizmos)
		{
			foreach (Node n in grid)
			{
				Gizmos.color = Color.Lerp(Color.white, Color.black, Mathf.InverseLerp(penaltyMin, penaltyMax, n.movementPenalty));
				Gizmos.color = (n.walkable) ? Gizmos.color : Color.red;
				Gizmos.color = (n.notOccupied) ? Gizmos.color : Color.yellow;
				Gizmos.DrawCube(n.worldPosition, Vector3.one * (nodeDiameter));
			}
		}
	}

	[System.Serializable]
	public class TerrainType
	{
		public LayerMask terrainMask;
		public int terrainPenalty;
	}
}
