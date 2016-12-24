using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACOSolver : Singleton<ACOSolver>{
	//--------------------------------------
	// Setting Attributes
	//--------------------------------------
	[SerializeField]
	[Tooltip("Ant Prefab")]
	private VRPAntGameObject pbAnt;

	[SerializeField]
	private float antSpeed = 2;

	[SerializeField]
	private bool improvedACO = false;

	[SerializeField]
	private int iterations = 10;

	[SerializeField]
	private float pheromoneInfluence = 0.2f;

	[SerializeField]
	private float visibilityInfluence = 10;

	[SerializeField]
	[Range(0,1)]
	private float q0 = 0.1f;

	//--------------------------------------
	// Private Attributes
	//--------------------------------------
	private VRP vrp;
	private List<VRPAnt> ants;

	//--------------------------------------
	// Getters & Setters
	//--------------------------------------
	public float PheromoneInfluence {
		get {
			return this.pheromoneInfluence;
		}
	}

	public float VisibilityInfluence {
		get {
			return this.visibilityInfluence;
		}
	}

	//--------------------------------------
	// Unity Methods
	//--------------------------------------
	protected virtual void Start () {
		
	}
	
	// Update is called once per frame
	protected virtual void Update () {
		
	}

	protected virtual void OnEnable (){
		VRPGraphLoader.OnVRPLoaded += OnVRPLoaded;
	}

	protected virtual void OnDisable (){
		VRPGraphLoader.OnVRPLoaded -= OnVRPLoaded;
	}

	protected virtual void OnDestroy (){
		VRPGraphLoader.OnVRPLoaded -= OnVRPLoaded;
	}

	//--------------------------------------
	// Private Methods
	//--------------------------------------
	private void ACO(){
		List<VRPAnt> bestAnts = new List<VRPAnt>();

		for (int i = 0; i < iterations; i++) {
			VRPAnt bestAnt = ants[0];


			//for each ant
			foreach(VRPAnt ant in ants) {
				VRPNode depot = vrp.Graph.Nodes.Find(n => n.IsDepot); //first, the depot
				VRPNode currentNode = depot;
				vrp.Graph.resetNodesVisited ();
				currentNode.Visited = true;

//				while(!allNodesVisited(vrp.Graph.Nodes)){
					int quantity = ant.TheObject.Quantity;

					while(quantity > 0 && !allNodesVisited(vrp.Graph.Nodes)){
						VRPNode nextBestNode = selectNextNodeForVisiting(currentNode, quantity);

						if (nextBestNode.IsDepot || currentNode.Demand > quantity) {
							quantity = 0;
						}
						else {
							ant.createRoute (vrp.Graph, currentNode, nextBestNode);
							quantity -= nextBestNode.Demand;
							currentNode = nextBestNode;
							currentNode.Visited = true;
						}
					}

					//comming back to the depot
					ant.createRoute (vrp.Graph, currentNode, depot);
					quantity -= depot.Demand;
//				}

				//get the best ant with the minimum route distance cost
				if(ant != bestAnt && ant.RouteDistanceCost < bestAnt.RouteDistanceCost){
					bestAnt = ant;
				}
			}

			//pheromone global update
			foreach (ACOEdge e in bestAnt.Routes)
				e.Pheromone = pheromoneGlobalUpdate (e.Pheromone, bestAnt.RouteDistanceCost);

			bestAnts.Add (bestAnt);
		}

		//results
		foreach (VRPAnt a in bestAnts) {
			string tour = "A"+a.TheObject.Id+">>";
			foreach (ACOEdge e in a.Routes) {
				tour += e.Id + ".";
			}
			Debug.Log (tour);
		}
	}

	private bool allNodesVisited(List<VRPNode> nodes){
		bool all = true;

		foreach (VRPNode n in nodes) {
			all = n.Visited;

			if (!all)
				break;
		}

				return all;
	}

	private VRPNode selectNextNodeForVisiting(VRPNode current, int antQuantity){
		VRPNode bestNode = null;
		float bestScore = float.MinValue;
		bool firstCriteriaOfSelection = false;

		if (!improvedACO) {
			float q = Random.Range (0f, 1f);
			firstCriteriaOfSelection = q <= q0;
		}

		foreach (VRPNode n in vrp.Graph.Nodes) {
			if (!n.Id.Equals(current.Id) && !n.Visited && n.Demand <= antQuantity) {
				ACOEdge e = vrp.Graph.Edges.Find(x => x.NodeA.Id.Equals(current.Id) && x.NodeB.Id.Equals(n.Id));
				float prob = 0;

				if (!improvedACO && firstCriteriaOfSelection) {
					prob = calculateFirstOperandProbToVisitNextNode (e.Pheromone, e.Weight);
				} else {
					prob = calculateProbabilityToVisitNextNode (current, e.Pheromone, e.Weight);
				}

				if (prob > bestScore) {
					bestScore = prob;
					bestNode = n;
				}
			}
		}

		return bestNode;
	}

	

	private float calculateProbabilityToVisitNextNode(VRPNode current, float edgePheromone, int edgeCost){
		float prob = 0;
		float total = 0;

		foreach (VRPNode h in vrp.Graph.Nodes) {
			if (!h.Visited && !h.Id.Equals (current.Id)){
				ACOEdge e = vrp.Graph.Edges.Find(x => x.NodeA.Id.Equals(current.Id) && x.NodeB.Id.Equals(h.Id));
				total += calculateFirstOperandProbToVisitNextNode (e.Pheromone, e.Weight);
			}
		}

		prob = calculateFirstOperandProbToVisitNextNode (edgePheromone, edgeCost) / total;

		return prob;
	}

	private float calculateFirstOperandProbToVisitNextNode(float edgePheromone, int edgeCost){
		float visibility = edgeCost != 0 ? 1.0f / (edgeCost*1.0f) : float.MaxValue;

		return improvedACO ? Mathf.Pow (edgePheromone, pheromoneInfluence) * Mathf.Pow (visibility, visibilityInfluence) 
				: edgePheromone * Mathf.Pow (visibility, visibilityInfluence);
	}

	//--------------------------------------
	// Public Methods
	//--------------------------------------
	public float pheromoneLocalUpdate(float oldPheromone){
		return ((1.0f - ACOSolver.Instance.PheromoneInfluence) * oldPheromone) 
			+ ACOSolver.Instance.PheromoneInfluence * VRPGraphLoader.Instance.InitialPheromone;
	}

	public float pheromoneGlobalUpdate(float oldPheromone, float routeDistanceCost=0){
		return ((1.0f - ACOSolver.Instance.PheromoneInfluence) * oldPheromone)
			+ ACOSolver.Instance.PheromoneInfluence / routeDistanceCost;
	}

	//--------------------------------------
	// Virtual Methods
	//--------------------------------------
	protected virtual void spawnAnts(){
		foreach (VRPVehicle v in vrp.Vehicles) {
			VRPAntGameObject antGO = Instantiate (pbAnt, vrp.Depot.transform.position, Quaternion.identity) as VRPAntGameObject; //the spawn
			VRPAnt ant = new VRPAnt(v);

			//TODO for testing
			int nodeIndex = Random.Range (0, vrp.NodeGOs.Count - 1);
			antGO.loadAnt(ant, vrp.NodeGOs[nodeIndex].transform, antSpeed, vrp.Depot.transform);
		}
	}


	//--------------------------------------
	// Events
	//--------------------------------------
	public virtual void OnVRPLoaded(VRP pVRP){
		if (pVRP != null) {
			vrp = pVRP;

			//load ants
			ants = new List<VRPAnt>();
			foreach (VRPVehicle v in vrp.Vehicles)
				ants.Add (new VRPAnt(v));

			if (!improvedACO)
				ACO ();
//				spawnAnts();
		}
	}

}
