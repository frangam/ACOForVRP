/* 
 * ACOForVRP is an open-source useful graphical desktop application for the application
 * of Improved Ant Colony Optimization technique for Vehicle Routing Problem.
 * More details on <https://github.com/garmo/ACOForVRP/blob/master/README.md>
 * Copyright (C) 2016 Fran García Moreno
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
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
	[Tooltip("Pheromone Prefab")]
	private GameObject pbPheromone;

	[SerializeField]
	private float antSpeed = 2;

	[SerializeField]
	private bool improvedACO = false;

	[SerializeField]
	private bool TwoOPTForward = true;

	[SerializeField]
	private bool smallestAngleClustering = true;

	[SerializeField]
	private int iterations = 10;

	[SerializeField]
	private float pheromoneInfluence = 0.2f;

	[SerializeField]
	private float visibilityInfluence = 10;

	[SerializeField]
	[Range(0,1)]
	private float q0 = 0.1f;

	[SerializeField]
	private float startDelay = 0.5f;

	//--------------------------------------
	// Private Attributes
	//--------------------------------------
	private VRP vrp;
	private List<VRPAnt> ants;
	private Dictionary<int,List<GameObject>> pheromoneTrails = new Dictionary<int,List<GameObject>>();
	private int currentIteration = 0;

	/// <summary>
	/// The Ant GameObjects
	/// </summary>
	private List<VRPAntGameObject> antGOs;

	//--------------------------------------
	// Getters & Setters
	//--------------------------------------
	public GameObject PbPheromone {
		get {
			return this.pbPheromone;
		}
	}

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

	public List<GameObject> CurrentPheromoneTrails {
		get {
			return this.pheromoneTrails[currentIteration];
		}
		set {
			pheromoneTrails[currentIteration] = value;
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
		ACOVRPGraphLoader.OnVRPLoaded += OnVRPLoaded;
	}

	protected virtual void OnDisable (){
		ACOVRPGraphLoader.OnVRPLoaded -= OnVRPLoaded;
	}

	protected virtual void OnDestroy (){
		ACOVRPGraphLoader.OnVRPLoaded -= OnVRPLoaded;
	}

	//--------------------------------------
	// Private Methods
	//--------------------------------------
	private IEnumerator ACO(){
		antGOs = new List<VRPAntGameObject>();

		yield return new WaitForSeconds (startDelay);

		for (int i = 0; i < iterations; i++) {
			currentIteration = i;
			VRPAnt bestAnt = ants[0];
			VRPNode depot = vrp.Graph.Nodes.Find(n => n.IsDepot); //first, the depot
			vrp.Graph.resetNodesVisited ();
			pheromoneTrails.Add (currentIteration, new List<GameObject> ());

			//delete pheromone trails of previous iteration
			if (i > 0) {
				//TODO improve
//				foreach (GameObject p in pheromoneTrails[i-1]) {
//					Destroy (p);
//				}
//				pheromoneTrails.Remove (i - 1);
			}

			//--------------------------------------
			//ROUTE CONSTRUCTION
			//--------------------------------------
			//customers
			while(!allNodesVisited(vrp.Graph.Nodes)){

				//for each ant
				foreach(VRPAnt ant in ants) {
					ant.Routes = new List<ACOVRPEdge> ();
					VRPNode currentNode = depot;
					currentNode.Visited = true;
					int quantity = ant.TheObject.Quantity;

					//spawn ant GameObject
					VRPAntGameObject aGO = antGOs.Find(a=>a.Ant.TheObject.Id.Equals(ant.TheObject.Id));
					if (i == 0 || (i > 0 && aGO != null && aGO.DestroyWhenDepotReached)) {
						if(i>0)
							antGOs = new List<VRPAntGameObject>();

						aGO = spawnAnts (ant, depot);

						if (!antGOs.Contains (aGO))
							antGOs.Add (aGO);
					}
					else if(i > 0 && aGO != null && !aGO.DestroyWhenDepotReached){
						aGO.gameObject.SetActive (true);
					}
						

					while(quantity > 0 && !allNodesVisited(vrp.Graph.Nodes)){
						VRPNode nextBestNode = selectNextNodeForVisiting(currentNode, quantity);

						if (nextBestNode == null || nextBestNode.IsDepot || nextBestNode.Demand > quantity) {
							quantity = 0;

							//comming back to the depot
//							ant.createRoute (vrp.Graph, currentNode, depot);
							aGO.createRoute (vrp.Graph, currentNode, depot);
							currentNode.Visited = true;

							//tell to ant go to the depot
							aGO.comeBackToDepot();
							while (aGO.isActiveAndEnabled && !aGO.DestinationReached)
								yield return null;
						}
						else {
//							ant.createRoute (vrp.Graph, currentNode, nextBestNode);
							aGO.createRoute (vrp.Graph, currentNode, nextBestNode);
							quantity -= nextBestNode.Demand;
							currentNode = nextBestNode;
							currentNode.Visited = true;

							//tell to ant go to the next node
							aGO.goToNextNode (vrp.NodeGOs.Find (n => n.Node.Id.Equals (nextBestNode.Id)).transform);
							while (aGO.isActiveAndEnabled && !aGO.DestinationReached)
								yield return null;

							if (quantity == 0 || allNodesVisited(vrp.Graph.Nodes)) {
								//comming back to the depot
//								ant.createRoute (vrp.Graph, currentNode, depot);
								aGO.createRoute (vrp.Graph, currentNode, depot);
								currentNode.Visited = true;

								//tell to ant go to the depot
								aGO.comeBackToDepot();
								while (aGO.isActiveAndEnabled && !aGO.DestinationReached)
									yield return null;
							}
						}
					}

//					//get the best ant with the minimum route distance cost
//					if(ant != bestAnt && ant.RouteDistanceCost < bestAnt.RouteDistanceCost){
//						bestAnt = ant;
//					}
				}//_end_for_each_ant
			}//_end_while_customers

			//--------------------------------------
			//UPDATING PHASE
			//--------------------------------------

			//previous results
			foreach (VRPAnt a in ants) {
				string tour = "Prev: A"+a.TheObject.Id+">>";
				foreach (ACOVRPEdge e in a.Routes) {
					tour += e.Id + ".";
				}
				Debug.Log (tour);
			}

			//2-optimal heuristic
			ants = improveSolutionWithTwoOPT(ants, depot);

			//pheromone global update
			foreach(VRPAnt a in ants){
				pheromoneGlobalUpdate (a);
			}
		}

		//results
		foreach (VRPAnt a in ants) {
			string tour = "A"+a.TheObject.Id+">>";
			foreach (ACOVRPEdge e in a.Routes) {
				tour += e.Id + ".";
			}
			Debug.Log (tour);
		}
	}

	private List<VRPAnt> improveSolutionWithTwoOPT(List<VRPAnt> currentSolution, VRPNode depot){
		Dictionary<VRPNode,float> angles = new Dictionary<VRPNode, float> (); //node angles
		Dictionary<VRPAnt, Dictionary<int, List<VRPNode>>> nodesInQuadrants = new Dictionary<VRPAnt, Dictionary<int, List<VRPNode>>>();
		Dictionary<VRPAnt, List<VRPNode>> nodesInClusters = new Dictionary<VRPAnt,List<VRPNode>>();
		Dictionary<VRPAnt, int> currentQuantities = new Dictionary<VRPAnt, int>();

		//pass to a dictionary ant capacity
		foreach (VRPAnt a in currentSolution)
			currentQuantities.Add (a, a.TheObject.Quantity);

		//initialization
		foreach (VRPAnt a in currentSolution) {
			//each route of current ant
			foreach (VRPNode n in a.CompleteTour) {
				if (!n.IsDepot) {
					//locate in quadrant
					int quadrant = 1;

					if(n.X >= 0 && n.Y >= 0)
						quadrant = 1;
					else if (n.X <= 0 && n.Y >= 0)
						quadrant = 2;
					else if (n.X <= 0 && n.Y <= 0)
						quadrant = 3;
					else if (n.X >= 0 && n.Y <= 0)
						quadrant = 4;

					//register node and quadrant in a dictionary
					if (!nodesInQuadrants.ContainsKey (a))
						nodesInQuadrants.Add (a, new Dictionary<int, List<VRPNode>> (){ { quadrant, new List<VRPNode> (){ n } } });
					else if (!nodesInQuadrants [a].ContainsKey (quadrant))
						nodesInQuadrants [a].Add(quadrant, new List<VRPNode> (){ n });
					else if(!nodesInQuadrants[a][quadrant].Contains(n))
						nodesInQuadrants[a][quadrant].Add (n);						

					//calculate angles
					if (!angles.ContainsKey (n)) {
						float angle = Mathf.Asin ((n.Y - depot.Y) / (Mathf.Sqrt (Mathf.Pow (n.X - depot.X, 2) + Mathf.Pow (n.Y - depot.Y, 2))));
						angles.Add (n, angle);
					}
				}
			}
		}

		//Phase I: clustering creation
		if (TwoOPTForward) {
			foreach (VRPAnt a in currentSolution) {
				for (int i = 4; i > 0; i--) { //quadrant 4 to 1
					if(nodesInQuadrants[a].ContainsKey(i))
						createClusters (currentSolution, angles, nodesInQuadrants[a][i], currentQuantities, nodesInClusters, i, a);
				}
			}
		} else {
			foreach (VRPAnt a in currentSolution) {
				for (int i = 1; i < 5; i++) { //quadrant 1 to 4
					if(nodesInQuadrants[a].ContainsKey(i))
						createClusters (currentSolution, angles, nodesInQuadrants[a][i], currentQuantities, nodesInClusters, i, a);
				}
			}
		}

		//Phase II: route creation
		//create Distance Matrix
		foreach(VRPAnt a in nodesInClusters.Keys){
			Dictionary<VRPNode,float> distMatrix = new Dictionary<VRPNode,float> ();

			//create Distance Matrix
			foreach (VRPNode n in nodesInClusters[a]) {
				distMatrix.Add (n, Mathf.Sqrt(Mathf.Pow(n.X-depot.X,2)+Mathf.Pow(n.Y-depot.Y,2)));
			}

			//local search with min distance
			List<VRPNode> shortestPath = new List<VRPNode>();
			foreach (KeyValuePair<VRPNode,float> pair in distMatrix.OrderByDescending(p=>p.Value).ToList())
				shortestPath.Add (pair.Key);
			
			List<ACOVRPEdge> improvedRoutes = new List<ACOVRPEdge> (); //routes for ant asociated to the cluster

			VRPNode nFrom = depot, nTo = shortestPath [0];
			KeyValuePair<int, float> edgeWP = getWeightAndPheromoneOfEdge (nFrom, nTo);
			improvedRoutes.Add(new ACOVRPEdge(nFrom, nTo, edgeWP.Key, edgeWP.Value));

			for(int i=0; i<shortestPath.Count-1; i++){
				nFrom = shortestPath [i];
				nTo = shortestPath[i+1];
				edgeWP = getWeightAndPheromoneOfEdge (nFrom, nTo);
				improvedRoutes.Add(new ACOVRPEdge(nFrom, nTo, edgeWP.Key, edgeWP.Value));
			}
				
			nFrom = shortestPath[shortestPath.Count-1];
			nTo = depot;
			edgeWP = getWeightAndPheromoneOfEdge (nFrom, nTo);
			improvedRoutes.Add(new ACOVRPEdge(nFrom, nTo, edgeWP.Key, edgeWP.Value));

			//update routes of the ant of this cluster
			currentSolution.Find(c=>c.TheObject.Id.Equals(a.TheObject.Id)).Routes = new List<ACOVRPEdge>(improvedRoutes);
		}

		return currentSolution;
	}

	private KeyValuePair<int, float> getWeightAndPheromoneOfEdge(VRPNode a, VRPNode b){
		KeyValuePair<int, float> res = new KeyValuePair<int, float>();

		ACOVRPEdge edge = vrp.Graph.Edges.Find (e => e.NodeA.Id.Equals (a.Id) && e.NodeB.Id.Equals (b.Id));

		if (edge != null)
			res = new KeyValuePair<int, float> (edge.Weight, edge.Pheromone);

		return res;
	}

	private void createClusters(List<VRPAnt> currentSolution,Dictionary<VRPNode,float> angles,List<VRPNode> nodesOfQuadrant
		,Dictionary<VRPAnt, int> currentQuantities, Dictionary<VRPAnt, List<VRPNode>> nodesInClusters, int quadrant, VRPAnt curAnt){ 
		List<VRPNode> selectedNodes = new List<VRPNode> ();

		//select node for linking it
		VRPNode nodeSelected = getSmallestOrGreaterNodeBasedOnItsAngle (angles, nodesOfQuadrant, selectedNodes);

		if (nodeSelected != null) {
			nodesOfQuadrant.Remove (nodeSelected);

			//get the ant has that node in its complete tour
//			VRPAnt ant = currentSolution.Find (x => x.CompleteTour.Find (n => n.Id.Equals (nodeSelected.Id)) != null);
			int quantity = currentQuantities [curAnt]; //get ant quantity

			if (nodeSelected.Demand <= quantity) {
				selectedNodes.Add (nodeSelected);
				quantity -= nodeSelected.Demand;
				currentQuantities [curAnt] = quantity; //update quantity counter

				//while exists a node to link in this current quadrant
				while (nodeSelected != null) {
					nodeSelected = getSmallestOrGreaterNodeBasedOnItsAngle (angles, nodesOfQuadrant, selectedNodes);

					if (nodeSelected != null) {
						nodesOfQuadrant.Remove (nodeSelected);

						//get the ant has that node in its complete tour
//						ant = currentSolution.Find (x => x.CompleteTour.Find (n => n.Id.Equals (nodeSelected.Id)) != null);
						quantity = currentQuantities [curAnt]; //get ant quantity

						//cluster change when capacity constraint is not met
						if (nodeSelected.Demand > quantity) {
							if (nodesInClusters.ContainsKey (curAnt))
								nodesInClusters [curAnt].AddRange (selectedNodes);
							else
								nodesInClusters.Add (curAnt, selectedNodes);
//							cluster++; 
						} else {
							selectedNodes.Add (nodeSelected);
							quantity -= nodeSelected.Demand;
							currentQuantities [curAnt] = quantity; //update quantity counter
						}
					} else {
						if (nodesInClusters.ContainsKey (curAnt))
							nodesInClusters [curAnt].AddRange (selectedNodes);
						else
							nodesInClusters.Add (curAnt, selectedNodes);
					}
				}
			} 
			//cluster change when capacity constraint is not met
			else {
//				if(cluster > 0)
//					cluster++; 
			}
		}
	}

	private VRPNode getSmallestOrGreaterNodeBasedOnItsAngle(Dictionary<VRPNode,float> angles, List<VRPNode> nodesOfQuadrant, List<VRPNode> selectedNodes){
		VRPNode res = null;
		float best = smallestAngleClustering ? float.MaxValue : float.MinValue;

		foreach (VRPNode n in nodesOfQuadrant) {
			if (!selectedNodes.Contains (n)) {
				float angle = angles [n];

				if ((smallestAngleClustering && angle < best) || (!smallestAngleClustering && angle > best)) {
					best = angle;
					res = n;
				}
			}
		}

		return res;
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
				ACOVRPEdge e = vrp.Graph.Edges.Find(x => x.NodeA.Id.Equals(current.Id) && x.NodeB.Id.Equals(n.Id));
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
				ACOVRPEdge e = vrp.Graph.Edges.Find(x => x.NodeA.Id.Equals(current.Id) && x.NodeB.Id.Equals(h.Id));
				total += calculateFirstOperandProbToVisitNextNode (e.Pheromone, e.Weight);
			}
		}

		prob = calculateFirstOperandProbToVisitNextNode (edgePheromone, edgeCost) / total;

		return prob;
	}

	private float calculateFirstOperandProbToVisitNextNode(float edgePheromone, int edgeCost){
		float visibility = edgeCost != 0 ? 1.0f / (edgeCost*1.0f) : 1.0f;

		return improvedACO ? Mathf.Pow (edgePheromone, pheromoneInfluence) * Mathf.Pow (visibility, visibilityInfluence) 
				: edgePheromone * Mathf.Pow (visibility, visibilityInfluence);
	}
		

	//--------------------------------------
	// Public Methods
	//--------------------------------------
	public float pheromoneLocalUpdate(ACOVRPEdge edge){
		float newPhe = !improvedACO ? ((1.0f - PheromoneInfluence) * edge.Pheromone) + PheromoneInfluence : edge.Pheromone;
		vrp.Graph.Edges.Find (ed => ed.Id.Equals (edge.Id)).Pheromone = newPhe;

		return newPhe;
	}

	public void pheromoneGlobalUpdate(VRPAnt ant){		
		foreach (ACOVRPEdge e in ant.Routes) {
			float oldPheromone = e.Pheromone;
			float newPhe = !improvedACO ? ((1.0f - PheromoneInfluence) * oldPheromone) + PheromoneInfluence / ant.RouteDistanceCost
				: ((1.0f - PheromoneInfluence) * oldPheromone) * (currentIteration - 1) + pheromoneIncrement (ant);
			e.Pheromone = newPhe;
			vrp.Graph.Edges.Find (ed => ed.Id.Equals (e.Id)).Pheromone = newPhe;
		}
	}

	public float pheromoneIncrement(VRPAnt ant){
		float res = 0;
//		int Q = ant.TheObject.Quantity;
		int L = ant.Routes.Sum (r => r.Weight);
		res = 1 / L;


		return res;
	}

	//--------------------------------------
	// Virtual Methods
	//--------------------------------------
	protected virtual VRPAntGameObject spawnAnts(VRPAnt ant, VRPNode node){
		VRPAntGameObject antGO = Instantiate (pbAnt, vrp.Depot.transform.position, Quaternion.identity) as VRPAntGameObject; //the spawn
		VRPNodeGameObject nodeGO = node.IsDepot ? vrp.Depot : vrp.NodeGOs.Find(n=>n.Node.Id.Equals(node.Id));
		//TODO for testing
//		int nodeIndex = Random.Range (0, vrp.NodeGOs.Count - 1);
//		antGO.loadAnt(ant, vrp.NodeGOs[nodeIndex].transform, antSpeed, vrp.Depot.transform);
		antGO.loadAnt(ant, nodeGO.transform, antSpeed, vrp.Depot.transform);

		return antGO;
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

			//start ACO algorithm
			StartCoroutine(ACO ());
		}
	}

}
