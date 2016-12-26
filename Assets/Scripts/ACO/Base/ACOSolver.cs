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
	private VRPPheromoneGO pbPheromone;

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
	private double initialPheromone = 0.001;

	[SerializeField]
	private float pheromoneInfluence = 1f;

	[SerializeField]
	private float visibilityInfluence = 5;

	[SerializeField]
	[Range(0,1)]
	private float ro = 0.1f;

	[SerializeField]
	[Range(0,1)]
	private float q0 = 0.1f;

	[SerializeField]
	private float startDelay = 0.5f;

	//--------------------------------------
	// Private Attributes
	//--------------------------------------
	private ACOVRP vrp;
	private List<VRPAnt> ants;
	private Dictionary<ACOVRPEdge,List<VRPPheromoneGO>> pheromoneTrails = new Dictionary<ACOVRPEdge,List<VRPPheromoneGO>>();
	private int currentIteration = 0;

	/// <summary>
	/// The Ant GameObjects
	/// </summary>
	private List<VRPAntGameObject> antGOs;

	//--------------------------------------
	// Getters & Setters
	//--------------------------------------
	public double InitialPheromone {
		get {
			return this.initialPheromone;
		}
	}
	public VRPPheromoneGO PbPheromone {
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
		VRPAntGameObject.OnPheromoneSpawned += OnPheromoneSpawned;
	}

	protected virtual void OnDisable (){
		ACOVRPGraphLoader.OnVRPLoaded -= OnVRPLoaded;
		VRPAntGameObject.OnPheromoneSpawned -= OnPheromoneSpawned;
	}

	protected virtual void OnDestroy (){
		ACOVRPGraphLoader.OnVRPLoaded -= OnVRPLoaded;
		VRPAntGameObject.OnPheromoneSpawned -= OnPheromoneSpawned;
	}

	//--------------------------------------
	// Private Methods
	//--------------------------------------
	private IEnumerator ACO(){
		antGOs = new List<VRPAntGameObject>();

		yield return new WaitForSeconds (startDelay*2);

		for (int i = 0; i < iterations; i++) {
			currentIteration = i;
			VRPAnt bestAnt = ants[0];
			VRPNode depot = vrp.Graph.Nodes.Find(n => n.IsDepot); //first, the depot
			vrp.Graph.resetNodesVisited ();

			//spawn initial ant to init pheromone value 

			//delete pheromone trails GameObjects of previous iteration
			if (i > 0) {
				Dictionary<ACOVRPEdge,List<VRPPheromoneGO>> phForDeleting = new Dictionary<ACOVRPEdge,List<VRPPheromoneGO>> ();
				foreach (ACOVRPEdge pEd in pheromoneTrails.Keys) {
					List<VRPPheromoneGO> li = new List<VRPPheromoneGO> ();
					int totalSpawedAtEdge = pheromoneTrails [pEd].Count;
					double res = ((pEd.Pheromone * totalSpawedAtEdge) / pEd.PreviousPheromone)*8;
					int rest = (int)(res);

					if (rest <= 0) {
						foreach (VRPPheromoneGO p in pheromoneTrails[pEd])
							li.Add (p);
					} else {
						for (int j = 0; j < Mathf.Clamp(pheromoneTrails [pEd].Count - rest, 1, pheromoneTrails [pEd].Count); j++)
							li.Add (pheromoneTrails [pEd] [j]);
					}
						
					phForDeleting.Add (pEd, li);
				}

				//delete pheromone gameobjects
				if (phForDeleting.Count > 0 && phForDeleting.Count > 0) {
					foreach (ACOVRPEdge e in phForDeleting.Keys) {
						foreach (VRPPheromoneGO go in phForDeleting[e]) {
							pheromoneTrails [e].Remove (go);
							Destroy (go.gameObject);
						}
					}
				}
			}

			//--------------------------------------
			//ROUTE CONSTRUCTION
			//--------------------------------------
			//customers
			while(!allNodesVisited(vrp.Graph.Nodes)){

				//for each ant
				foreach(VRPAnt ant in ants) {
					ant.Paths = new List<ACOVRPEdge> ();
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
							ACOVRPEdge edgeThrough = aGO.createRoute (vrp.Graph, currentNode, depot);
							currentNode.Visited = true;

							//tell to ant go to the depot
							aGO.comeBackToDepot(edgeThrough);
							while (aGO.isActiveAndEnabled && !aGO.DestinationReached)
								yield return null;
						}
						else {
//							ant.createRoute (vrp.Graph, currentNode, nextBestNode);
							ACOVRPEdge edgeThrough = aGO.createRoute (vrp.Graph, currentNode, nextBestNode);
							quantity -= nextBestNode.Demand;
							currentNode = nextBestNode;
							currentNode.Visited = true;

							//tell to ant go to the next node
							aGO.goToNextNode (edgeThrough, vrp.NodeGOs.Find (n => n.Node.Id.Equals (nextBestNode.Id)).transform);
							while (aGO.isActiveAndEnabled && !aGO.DestinationReached)
								yield return null;

							if (quantity == 0 || allNodesVisited(vrp.Graph.Nodes)) {
								//comming back to the depot
//								ant.createRoute (vrp.Graph, currentNode, depot);
								edgeThrough = aGO.createRoute (vrp.Graph, currentNode, depot);
								currentNode.Visited = true;

								//tell to ant go to the depot
								aGO.comeBackToDepot(edgeThrough);
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
				foreach (ACOVRPEdge e in a.Paths) {
					tour += e.Id + ".";
				}
				Debug.Log (tour);
			}

			//2-optimal heuristic
			ants = improveSolutionWithTwoOPT(ants, depot);

			//update graph nodes with the best solution
			List<VRPNode> bestSol = new List<VRPNode>(){depot};
			foreach (VRPAnt a in ants) {
				foreach (VRPNode n in a.CompleteTourWithOutDepot) {
					bestSol.Add (n);
					vrp.Graph.Nodes.Remove (n);
				}
			}
			//sort nodes based on best solution
			List<VRPNode> currentNodesInGraph = vrp.Graph.Nodes;
			vrp.Graph.Nodes.RemoveAll (n => true);
			vrp.Graph.Nodes.AddRange (bestSol);
			vrp.Graph.Nodes.AddRange (currentNodesInGraph);

			//optimal results
			foreach (VRPAnt a in ants) {
				string tour = "Impv: A"+a.TheObject.Id+">>";
				foreach (ACOVRPEdge e in a.Paths) {
					tour += e.Id + ".";
				}
				Debug.Log (tour);
			}


			//pheromone global update
			pheromoneGlobalUpdate ();
		}

		//results
		foreach (VRPAnt a in ants) {
			string tour = "A"+a.TheObject.Id+">>";
			foreach (ACOVRPEdge e in a.Paths) {
				tour += e.Id + ".";
			}
			Debug.Log (tour);
		}
	}

	private IEnumerator initEdges(){
		yield return new WaitForSeconds (startDelay);

//		VRPNode init = vrp.Graph.Edges [0].NodeA;
//		VRPAntGameObject aGO = spawnAnts (ants[0], init, true);

		foreach (ACOVRPEdge e in vrp.Graph.Edges) {
			if (!e.NodeA.Id.Equals (e.NodeB.Id)) {
				Debug.Log (e.NodeA.Id+"-"+e.NodeB.Id);
				VRPNodeGameObject find = vrp.NodeGOs.Find (n => n.Node.Id.Equals (e.NodeB.Id));

				if(find != null){
					VRPAntGameObject aGO = spawnAnts (ants[0], e.NodeA, true);
					aGO.goToNextNode (e, find.transform);
					while (!aGO.DestinationReached)
						yield return null;
					Destroy (aGO.gameObject);
				}
			}
		}

		StartCoroutine (ACO());
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
			KeyValuePair<int, double> edgeWP = getWeightAndPheromoneOfEdge (nFrom, nTo);
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
			currentSolution.Find(c=>c.TheObject.Id.Equals(a.TheObject.Id)).Paths = new List<ACOVRPEdge>(improvedRoutes);
		}

		return currentSolution;
	}

	private KeyValuePair<int, double> getWeightAndPheromoneOfEdge(VRPNode a, VRPNode b){
		KeyValuePair<int, double> res = new KeyValuePair<int, double>();

		ACOVRPEdge edge = vrp.Graph.Edges.Find (e => e.NodeA.Id.Equals (a.Id) && e.NodeB.Id.Equals (b.Id));

		if (edge != null)
			res = new KeyValuePair<int, double> (edge.Weight, edge.Pheromone);

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
		bool firstCriteriaOfSelection = false;
		double bestScore = float.MinValue;
		double total = 1.0;

		if (!improvedACO) {
			float q = Random.Range (0f, 1f);
			firstCriteriaOfSelection = q <= q0;

			if (!firstCriteriaOfSelection) {
				total = totalTauNij (current);
			}
		} else {
			total = totalTauNij (current);
		}

		foreach (VRPNode nextNode in vrp.Graph.Nodes) {
			if (!nextNode.Id.Equals(current.Id) && !nextNode.Visited && nextNode.Demand <= antQuantity) {
				double prob = 0.0;
				ACOVRPEdge edgeFromCurNodeToNextNode = vrp.Graph.Edges.Find(x => x.NodeA.Id.Equals(current.Id) && x.NodeB.Id.Equals(nextNode.Id));

				if (!improvedACO && firstCriteriaOfSelection) {
					prob = calculateFirstOperandProbToVisitNextNode (edgeFromCurNodeToNextNode.Pheromone, edgeFromCurNodeToNextNode.Weight);
				} else {
					prob = calculateFirstOperandProbToVisitNextNode (edgeFromCurNodeToNextNode.Pheromone, edgeFromCurNodeToNextNode.Weight) / total;
				}

				if (prob > bestScore) {
					bestScore = prob;
					bestNode = nextNode;
				}
			}
		}

		return bestNode;
	}
		
	private double totalTauNij(VRPNode current){
		double total = 0.0;
		foreach (VRPNode h in vrp.Graph.Nodes) {
			if (!h.Visited && !h.Id.Equals (current.Id)){
				ACOVRPEdge edge = vrp.Graph.Edges.Find(x => x.NodeA.Id.Equals(current.Id) && x.NodeB.Id.Equals(h.Id));
				total += calculateFirstOperandProbToVisitNextNode (edge.Pheromone, edge.Weight);
			}
		}
		return total;
	}

	private double calculateFirstOperandProbToVisitNextNode(double edgePheromone, int edgeCost){
		double visibility = edgeCost != 0 ? 1.0 / (edgeCost*1.0) : 1.0;

		return improvedACO ? System.Math.Pow (edgePheromone, pheromoneInfluence) * System.Math.Pow (visibility, visibilityInfluence) 
				: edgePheromone * System.Math.Pow (visibility, visibilityInfluence);
	}
		

	//--------------------------------------
	// Public Methods
	//--------------------------------------
	public double pheromoneLocalUpdate(ACOVRPEdge edge){
		double newPhe = !improvedACO ? ((1.0 - PheromoneInfluence) * edge.Pheromone) + PheromoneInfluence : edge.Pheromone;
		vrp.Graph.Edges.Find (ed => ed.Id.Equals (edge.Id)).Pheromone = newPhe;

		return newPhe;
	}

	public void pheromoneGlobalUpdate(){		
		//update pheromone in all of edges of the Graph
		foreach (ACOVRPEdge edgeIJ in vrp.Graph.Edges) {
			double newPhe = 0.0;
			foreach (VRPAnt ant in ants) {
				newPhe = globalUpdateResult (edgeIJ, ant);
			}
			edgeIJ.Pheromone = newPhe;
		}
	}

	public double globalUpdateResult(ACOVRPEdge edge, VRPAnt ant){
		double oldPheromone = edge.Pheromone;
		double newPhe = !improvedACO ? ((1.0 - ro) * oldPheromone) + ro / ant.RouteDistanceCost
//			: ((1.0f - ro) * oldPheromone) * (currentIteration - 1) + pheromoneIncrement (ant);
			: (ro * oldPheromone) + pheromoneImprovedIncrement(edge, ant);
		return newPhe;
	}

	public double pheromoneImprovedIncrement(ACOVRPEdge edgeIJ, VRPAnt ant){
		double inc = 0.0;
		int Q = ants[0].TheObject.Quantity;
		int L = ants.Sum(a=>a.TotalRouteWeight);
		int K = Mathf.Clamp(ants.Count, 1, int.MaxValue);
		int dij = edgeIJ.Weight;
		bool routeIJInAntRoutes = ant.Paths.Find (edgeK => edgeK.NodeA.Id.Equals (edgeIJ.NodeA.Id) && edgeK.NodeB.Id.Equals (edgeIJ.NodeB.Id)) != null;

		if (routeIJInAntRoutes) {
			int mk = Mathf.Clamp (ant.CompleteTourWithOutDepot.Count, 1, int.MaxValue); //m>0
			int Dk = Mathf.Clamp (ant.TotalRouteWeight, 1, int.MaxValue);
			inc = (Q / (L * K)) * ((Dk - dij) / (mk * Dk));
		}

		return inc;
	}

	//--------------------------------------
	// Virtual Methods
	//--------------------------------------
	protected virtual VRPAntGameObject spawnAnts(VRPAnt ant, VRPNode node, bool pIsInitializationAnt=false){
		VRPAntGameObject antGO = null;

		if(!pIsInitializationAnt)
			antGO = Instantiate (pbAnt, vrp.Depot.transform.position, Quaternion.identity) as VRPAntGameObject; //the spawn
		else
			antGO = Instantiate (pbAnt, node.IsDepot ? vrp.Depot.transform.position : vrp.NodeGOs.Find(n=>n.Node.Id.Equals(node.Id)).transform.position, Quaternion.identity) as VRPAntGameObject; //the spawn

		VRPNodeGameObject nodeGO = node.IsDepot ? vrp.Depot : vrp.NodeGOs.Find(n=>n.Node.Id.Equals(node.Id));
		//for testing
//		int nodeIndex = Random.Range (0, vrp.NodeGOs.Count - 1);
//		antGO.loadAnt(ant, vrp.NodeGOs[nodeIndex].transform, antSpeed, vrp.Depot.transform);

		antGO.loadAnt(ant, nodeGO.transform, antSpeed, vrp.Depot.transform, pIsInitializationAnt);

		return antGO;
	}


	//--------------------------------------
	// Events
	//--------------------------------------
	public virtual void OnVRPLoaded(ACOVRP pVRP){
		if (pVRP != null) {
			vrp = pVRP;

			//load ants
			ants = new List<VRPAnt>();
			foreach (VRPVehicle v in vrp.Vehicles)
				ants.Add (new VRPAnt(v));

			//start ACO algorithm
//			StartCoroutine(ACO ());
			StartCoroutine(initEdges());
		}
	}

	public virtual void OnPheromoneSpawned(VRPPheromoneGO pheromoneGO){
		if (pheromoneTrails.ContainsKey (pheromoneGO.Edge)) {
			pheromoneTrails [pheromoneGO.Edge].Add (pheromoneGO);
		} else {
			pheromoneTrails.Add (pheromoneGO.Edge, new List<VRPPheromoneGO> (){ pheromoneGO });
		}

//		Debug.Log ("Spawning at Edge: "+pheromoneGO.Edge.NodeA.Id+"-"+pheromoneGO.Edge.NodeB.Id);
	}
}
