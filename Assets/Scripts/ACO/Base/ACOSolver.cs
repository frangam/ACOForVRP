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
using System.Diagnostics;
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
	[Tooltip("If you want to see how ants are spawned and visualize the ACO processs, check this")]
	private bool showVisualProcess = true;

	[SerializeField]
	private float antSpeed = 2;

	[SerializeField]
	private bool improvedACO = true;

	[SerializeField]
	private bool TwoOPTForward = true;

	[SerializeField]
	private bool smallestAngleClustering = true;

	[SerializeField]
	private int iterations = 10;

	[SerializeField]
	private float initialPheromone = 0.001f;

	[SerializeField]
	private float pheromoneInfluence = 2f;

	[SerializeField]
	private float visibilityInfluence = 1;

	[SerializeField]
	[Range(0,1)]
	private float ro = 0.88f;

	[SerializeField]
	[Range(0,1)]
	private float q0 = 0.1f;

	[SerializeField]
	private float impIACO_Q = 1000f;

	[SerializeField]
	private float startDelay = 0.5f;

	[SerializeField]
	private bool visualInitPheromone = false;

	[SerializeField]
	private float antsInitMovSpeed = 10;

	[SerializeField]
	private float antsSolMovSpeed = 5;

	[SerializeField]
	private float waitTimeAmongAnts = 0.25f;

	[SerializeField]
	private Color[] antColors;

	[SerializeField]
	private Color[] solColors;

	[SerializeField]
	private bool doMutation = true;

	//--------------------------------------
	// Dispatch Events
	//--------------------------------------
	public static event System.Action OnSolved = delegate{};

	//--------------------------------------
	// Private Attributes
	//--------------------------------------
	private ACOVRP vrp;
	private List<VRPAnt> ants;
	private Dictionary<ACOVRPEdge,List<VRPPheromoneGO>> pheromoneTrails = new Dictionary<ACOVRPEdge,List<VRPPheromoneGO>>();
	private int currentIteration = 0;
	private bool vrpLoaded = false;
	private VRPAntGameObject curAntGO;
	private Stopwatch stopWatch = new Stopwatch();
	private bool solved = false;
	private List<VRPAnt> bestSolution;

	/// <summary>
	/// The Ant GameObjects
	/// </summary>
	private List<VRPAntGameObject> antGOs;

	//--------------------------------------
	// Getters & Setters
	//--------------------------------------
	public bool ImprovedACO {
		get {
			return this.improvedACO;
		}
		set {
			improvedACO = value;
		}
	}
	public bool Solved{
		get{
			return this.solved;
		}
	}
	public int CurrentIteration {
		get {
			return this.currentIteration;
		}
		set {
			currentIteration = value;
		}
	}
	public float ImpIACO_Q {
		get {
			return this.impIACO_Q;
		}
		set {
			impIACO_Q = value;
		}
	}
	public float InitialPheromone {
		get {
			return this.initialPheromone;
		}
		set{
			this.initialPheromone = value;
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
		set{
			this.pheromoneInfluence = value;
		}
	}

	public float VisibilityInfluence {
		get {
			return this.visibilityInfluence;
		}
		set{
			this.visibilityInfluence = value;
		}
	}

	public float AntSpeed {
		get {
			return this.antSpeed;
		}
		set {
			antSpeed = value;
		}
	}

	public int Iterations {
		get {
			return this.iterations;
		}
		set {
			iterations = value;
		}
	}

	public float Ro {
		get {
			return this.ro;
		}
		set {
			ro = value;
		}
	}

	public VRPAntGameObject CurAntGO {
		get {
			return this.curAntGO;
		}
		set {
			curAntGO = value;
		}
	}

	public bool VisualInitPheromone {
		get {
			return this.visualInitPheromone;
		}
		set {
			visualInitPheromone = value;
		}
	}
	public bool ShowVisualProcess {
		get {
			return this.showVisualProcess;
		}
		set {
			showVisualProcess = value;
		}
	}

	public Stopwatch StopWatch {
		get {
			return this.stopWatch;
		}
	}

	public bool DoMutation {
		get {
			return this.doMutation;
		}
		set {
			doMutation = value;
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
	/// <summary>
	/// ACO Algorithm.
	/// 
	/// For IACO: 
	/// B. Yu, Z. Z. Yang, and B. Yao, “An improved ant colony optimization for vehicle routing problem,” Eur. J. Oper. Res., 2009.
	/// </summary>
	private IEnumerator ACO(){
		solved = false;
		antGOs = new List<VRPAntGameObject>();
		int totalEdges = vrp.Graph.Edges.Count;
		float bestWeight = float.MaxValue;

		yield return new WaitForSeconds (startDelay*2);

		stopWatch.Start (); //start measuring

		for (int i = 0; i < iterations; i++) {
			currentIteration = i;
			UIManager.Instance.updateIteration (currentIteration);
			VRPAnt bestAnt = ants[0];
			VRPNode depot = vrp.Graph.Nodes.Find(n => n.IsDepot); //first, the depot
			vrp.Graph.resetNodesVisited ();


			//delete pheromone trails GameObjects of previous iteration
			if (showVisualProcess && i > 0) {
				updatePheromoneGameObjects ();
			}

			//--------------------------------------
			//ROUTE CONSTRUCTION
			//--------------------------------------
			//customers
			while(!allNodesVisited(vrp.Graph.Nodes)){

				//for each ant
				for(int antIndex=0; antIndex<ants.Count; antIndex++) {
					VRPAnt ant = ants[antIndex];
					ant.resetRoute();
					VRPNode currentNode = depot;
					currentNode.Visited = true;
					int quantity = ant.TheObject.Quantity;
					UIManager.Instance.updateAntQuantity (quantity, ant.TheObject.InitialQuantity);
					VRPAntGameObject aGO = null;

					//spawn ant GameObject
					if(showVisualProcess)
						aGO = spawnAntProcess(i, antIndex, ant, depot);
						

					while(quantity > 0 && !allNodesVisited(vrp.Graph.Nodes)){
						VRPNode nextBestNode = selectNextNodeForVisiting(currentNode, quantity);

						if (nextBestNode == null || nextBestNode.IsDepot || nextBestNode.Demand > quantity) {
							quantity = 0;
							UIManager.Instance.updateAntQuantity (quantity, ant.TheObject.InitialQuantity);

							//comming back to the depot
							ACOVRPEdge edgeThrough = showVisualProcess ? aGO.createRoute (vrp.Graph, currentNode, depot) : ant.createRoute(vrp.Graph, currentNode, depot);
							currentNode.Visited = true;

							//tell to ant gameobject go to the depot
							if (showVisualProcess) {
								aGO.comeBackToDepot (edgeThrough);
								while (aGO.isActiveAndEnabled && !aGO.DestinationReached)
									yield return null;
							}
						}
						else {
							ACOVRPEdge edgeThrough = showVisualProcess ? aGO.createRoute (vrp.Graph, currentNode, nextBestNode) : ant.createRoute(vrp.Graph, currentNode, nextBestNode);
							quantity -= nextBestNode.Demand;
							currentNode = nextBestNode;
							currentNode.Visited = true;

							//tell to ant gameobject go to the next node
							if (showVisualProcess) {
								aGO.goToNextNode (edgeThrough, vrp.NodeGOs.Find (n => n.Node.Id.Equals (nextBestNode.Id)).transform);
								while (aGO.isActiveAndEnabled && !aGO.DestinationReached)
									yield return null;
								UIManager.Instance.updateAntQuantity (quantity, ant.TheObject.InitialQuantity);
							}

							if (quantity == 0 || allNodesVisited(vrp.Graph.Nodes)) {
								//comming back to the depot
								edgeThrough = showVisualProcess ? aGO.createRoute (vrp.Graph, currentNode, depot) : ant.createRoute(vrp.Graph, currentNode, depot);
								currentNode.Visited = true;

								//tell to ant gameobject go to the depot
								if (showVisualProcess) {
									aGO.comeBackToDepot (edgeThrough);
									while (aGO.isActiveAndEnabled && !aGO.DestinationReached)
										yield return null;
								}
							}
						}
					}

					if (showVisualProcess)
						yield return new WaitForSeconds (waitTimeAmongAnts);
					else
						yield return null;
				}//_end_for_each_ant
			}//_end_while_customers


			//previous results
			string solution = "";
//			if (totalEdges <= 100 || i == iterations-1) {
				for (int aind = 0; aind < ants.Count; aind++) {
					string tour = ants [aind].Paths [0].NodeA.Id + "-";
					for (int ind = 0; ind < ants [aind].Paths.Count; ind++) {
						tour += ind < ants [aind].Paths.Count - 1 ? ants [aind].Paths [ind].NodeB.Id + "-" : ants [aind].Paths [ind].NodeB.Id;
					}
					//				UnityEngine.Debug.Log (tour + ". Cost: "+a.TotalRouteWeight.ToString());
					solution += aind < ants.Count - 1 ? tour + ". " : tour;
				}
				UIManager.Instance.showTotalRoutesCost (true, solution, ants.Sum (a => a.TotalRouteWeight)); 
//			}
				

			//--------------------------------------
			//UPDATING PHASE
			//--------------------------------------
			//mutation
			if (doMutation) {
				List<VRPAnt> antsCopy = new List<VRPAnt> (ants);
				List<VRPAnt> mutated = mutation (ants, i);
				if (mutated != null) {
					ants = mutated;
				} else {
					ants = new List<VRPAnt> (antsCopy);
					UnityEngine.Debug.Log ("restoring ants from copy. no mutation");
				}
			}



			//2-optimal heuristic
			ants = improveSolutionWithTwoOPT (ants, depot);

			//pheromone global update
			pheromoneGlobalUpdate ();


			//register the best solution found
			float curSolWeight = ants.Sum(a=>a.TotalRouteWeight);
			if (curSolWeight < bestWeight) {
				bestWeight = curSolWeight;

				bestSolution = new List<VRPAnt>();
				foreach(VRPAnt a in ants){
					bestSolution.Add (new VRPAnt(a));
				}

//				UnityEngine.Debug.Log ("current best sol found: " + bestSolution.Sum (b => b.TotalRouteWeight).ToString ());
			}

			//we draw the best solution  found
			if (i == iterations - 1) {
				stopWatch.Stop ();

				UnityEngine.Debug.Log ("best cost: "+bestSolution.Sum(b=>b.TotalRouteWeight).ToString());

				ants = new List<VRPAnt> (bestSolution);

//				UnityEngine.Debug.Log ("ants: " + ants.Sum (b => b.TotalRouteWeight).ToString ());

//				if(!showVisualProcess)
					StartCoroutine (drawFinalOptimization ());
			}

				//			//update graph nodes with the best solution
				//			List<VRPNode> bestSol = new List<VRPNode>(){depot};
				//			foreach (VRPAnt a in ants) {
				//				foreach (VRPNode n in a.CompleteTourWithOutDepot) {
				//					bestSol.Add (n);
				//					vrp.Graph.Nodes.Remove (n);
				//				}
				//			}
				//			//sort nodes based on best solution
				//			List<VRPNode> currentNodesInGraph = vrp.Graph.Nodes;
				//			vrp.Graph.Nodes.RemoveAll (n => true);
				//			vrp.Graph.Nodes.AddRange (bestSol);
				//			vrp.Graph.Nodes.AddRange (currentNodesInGraph);
				//
			//optimal results
//			if (totalEdges <= 100 || i == iterations-1) {
				solution = "";
				
				for (int aind = 0; aind < ants.Count; aind++) {
					string tour = ants [aind].Paths [0].NodeA.Id + "-";
					for (int ind = 0; ind < ants [aind].Paths.Count; ind++) {
						tour += ind < ants [aind].Paths.Count - 1 ? ants [aind].Paths [ind].NodeB.Id + "-" : ants [aind].Paths [ind].NodeB.Id;
					}
					//				UnityEngine.Debug.Log (tour + ". Cost: "+a.TotalRouteWeight.ToString());
					solution += aind < ants.Count - 1 ? tour + ". " : tour;
				}

				UIManager.Instance.showTotalRoutesCost (false, solution, ants.Sum (x => x.TotalRouteWeight)); 
			UnityEngine.Debug.Log ("(It: "+(i+1).ToString()+")Cost: "+ants.Sum (x => x.TotalRouteWeight).ToString());

//			}


		}




		solved = true;
		OnSolved ();
	}

	private IEnumerator initEdges(){
		yield return new WaitForSeconds (startDelay);

		foreach (ACOVRPEdge e in vrp.Graph.Edges) {
			if (!e.NodeA.Id.Equals (e.NodeB.Id)) {
//				Debug.Log (e.NodeA.Id+"-"+e.NodeB.Id);
				VRPNodeGameObject find = vrp.NodeGOs.Find (n => n.Node.Id.Equals (e.NodeB.Id));

				if(find != null){
					VRPAntGameObject aGO = spawnAnts (ants[0], e.NodeA, true);
					aGO.MovSpeed = antsInitMovSpeed;
					aGO.goToNextNode (e, find.transform);
					while (!aGO.DestinationReached)
						yield return null;
					Destroy (aGO.gameObject);
				}
			}
		}

		StartCoroutine (ACO());
	}

	private IEnumerator drawFinalOptimization(){
//		updatePheromoneGameObjects ();

		yield return new WaitForSeconds (startDelay);

		int antIndex = 0; //color at index 0 is for initialization pheromone
		foreach (VRPAnt ant in ants) {
			VRPAntGameObject aGO = antGOs.Find(a=>a.Ant.TheObject.Id.Equals(ant.TheObject.Id));
			if (aGO == null) {
				aGO = spawnAnts (ant, vrp.Depot.Node);
			} else {
				aGO.gameObject.SetActive (true);
			}

			if (antColors.Length > antIndex) {
				aGO.PheromoneColor = solColors [antIndex];
				antIndex++;
			}

			foreach (ACOVRPEdge e in ant.Paths) {
				if (!e.NodeA.Id.Equals (e.NodeB.Id)) {
					//				Debug.Log (e.NodeA.Id+"-"+e.NodeB.Id);
					VRPNodeGameObject find = vrp.NodeGOs.Find (n => n.Node.Id.Equals (e.NodeB.Id));

					if (find != null || e.NodeB.IsDepot) {
						aGO.MovSpeed = antsSolMovSpeed;

						if (!e.NodeB.IsDepot)
							aGO.goToNextNode (e, find.transform);
						else
							aGO.comeBackToDepot (e);
						
						while (aGO.isActiveAndEnabled && !aGO.DestinationReached)
							yield return null;
//						Destroy (aGO.gameObject);
					}
				}
			}
		}
	}

	private void updatePheromoneGameObjects(bool deleteAll = false){
		Dictionary<ACOVRPEdge,List<VRPPheromoneGO>> phForDeleting = new Dictionary<ACOVRPEdge,List<VRPPheromoneGO>> ();
		foreach (ACOVRPEdge pEd in pheromoneTrails.Keys) {
			List<VRPPheromoneGO> li = new List<VRPPheromoneGO> ();
			int totalSpawedAtEdge = pheromoneTrails [pEd].Count;
			double res = ((pEd.Pheromone * totalSpawedAtEdge) / pEd.PreviousPheromone)*0.5f;
			int rest = !deleteAll ? (int)(res) : 0;

			pheromoneTrails [pEd].Shuffle ();

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

	private List<VRPAnt> improveSolutionWithTwoOPT(List<VRPAnt> currentSolution, VRPNode depot){
		List<ACOVRPEdge> improvedRoutes = new List<ACOVRPEdge> (); //result

		foreach (VRPAnt a in currentSolution) { 
			List<VRPNode> completeTour = a.improveCurrentRouteWithTwoOPT (vrp.Graph);
			improvedRoutes = new List<ACOVRPEdge> ();

			for(int i=0; i<completeTour.Count-1; i++){ //completeTour.Count-1 because last node is depot
				VRPNode nFrom = completeTour [i];
				VRPNode nTo = completeTour[i+1];
				KeyValuePair<float, double> edgeWP = getWeightAndPheromoneOfEdge (nFrom, nTo);
				improvedRoutes.Add(new ACOVRPEdge(nFrom, nTo, edgeWP.Key, edgeWP.Value));
			}

			a.Paths = new List<ACOVRPEdge> (improvedRoutes);
		}

		return currentSolution;
	}

	private KeyValuePair<float, double> getWeightAndPheromoneOfEdge(VRPNode a, VRPNode b){
		KeyValuePair<float, double> res = new KeyValuePair<float, double>();

		ACOVRPEdge edge = vrp.Graph.Edges.Find (e => e.NodeA.Id.Equals (a.Id) && e.NodeB.Id.Equals (b.Id));

		if (edge != null)
			res = new KeyValuePair<float, double> (edge.Weight, edge.Pheromone);

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

	private double calculateFirstOperandProbToVisitNextNode(double edgePheromone, float edgeCost){
		double visibility = edgeCost != 0 ? 1.0 / (edgeCost*1.0) : 1.0;

		return improvedACO ? System.Math.Pow (edgePheromone, pheromoneInfluence) * System.Math.Pow (visibility, visibilityInfluence) 
				: edgePheromone * System.Math.Pow (visibility, visibilityInfluence);
	}

	/// <summary>
	/// Ant Spawn process 
	/// </summary>
	/// <returns>The ant game object.</returns>
	/// <param name="curIt">Current iteration index.</param>
	/// <param name="antIndex">Ant index.</param>
	/// param name="ant">The Ant.</param>
	/// /// param name="depot">The Depot.</param>
	private VRPAntGameObject spawnAntProcess(int curIt, int antIndex, VRPAnt ant, VRPNode depot){
		VRPAntGameObject aGO = antGOs.Find(a=>a.Ant.TheObject.Id.Equals(ant.TheObject.Id));
		if (curIt == 0 || (curIt > 0 && aGO != null && aGO.DestroyWhenDepotReached)) {
			if(curIt>0)
				antGOs = new List<VRPAntGameObject>();

			aGO = spawnAnts (ant, depot);

			if (!antGOs.Contains (aGO))
				antGOs.Add (aGO);
		}
		else if(curIt > 0 && aGO != null && !aGO.DestroyWhenDepotReached){
			aGO.gameObject.SetActive (true);
		}
		curAntGO = aGO;

		if (antColors.Length > antIndex+1) {
			aGO.PheromoneColor = antColors [antIndex+1];
		}

		return aGO;
	}

	private List<VRPAnt> mutation(List<VRPAnt> ants, int curIt){
		List<VRPAnt> res = new List<VRPAnt> ();
		List<VRPAnt> antsForMutating = getAntsForMutating (ants, curIt); //get ants for mutating
		bool violatedCapacity = false;
		List<List<VRPAnt>> childSolutions = new List<List<VRPAnt>> ();

		if (curIt == iterations-1) {
			return null;
		}

		//generate child solutions for each mutating ant
		foreach (VRPAnt curMutAnt in antsForMutating) {
			int curAntIndex = ants.IndexOf (curMutAnt);

			//the other ants distinct of the current mutating ant
			List<VRPAnt> otherAnts = ants.Where (a => !a.TheObject.Id.Equals (curMutAnt.TheObject.Id)).ToList(); 


//			int indexRandom = Random.Range(0, otherAnts.Count);
//			int selectdAntIndex = ants.IndexOf (otherAnts [indexRandom]);
//			VRPAnt selectedAnt = ants[selectdAntIndex]; //get randomly another ant distinct to the current one
			List<VRPNode> customersFirst = curMutAnt.Customers; 
//			List<VRPNode> customersSecond = selectedAnt.Customers;
			List<VRPAnt> childSol = new List<VRPAnt> (ants); //copy parent solution

			//first shuffle customers for random search in every iteration
			customersFirst.Shuffle ();
//			customersSecond.Shuffle ();
			otherAnts.Shuffle (); //shuffle the other ants for exchange the nodes randomly

			//select two nodes without violating capacity constraint
			//make the nodes exchange
			//(1) - (2) - (3) (Mutating Ant)
			//which node can exhange of other ant (iterate over all nodes)
			//(1*) - (2*)
			for(int o=0; o<otherAnts.Count; o++){ //search a valid other ant candidaty with does not violate capacity constraint
				int selectdAntIndex = ants.IndexOf (otherAnts [o]);
				VRPAnt selectedAnt = new VRPAnt(ants[selectdAntIndex]); //copy of this ant from parent solution
				List<VRPNode> customersSecond = selectedAnt.Customers; //get the customers of this other ant
				customersSecond.Shuffle (); //for randomly

				for (int i = 0; i < customersFirst.Count; i++) {
					VRPNode nExOne = customersFirst [i]; //node of current mutating ant route

					//search node candidate of other ant for exchanging
					for (int j = 0; j < customersSecond.Count; j++) {
						VRPNode nExTwo = customersSecond [j]; //node of second route

						//exchange at current ant
						if (!nExOne.IsDepot && !nExTwo.IsDepot) {
							//exchange node two in first route
							int edgesUpdatedWithNode = 0;
							Dictionary<int, ACOVRPEdge> edgesForUpdateFirst = new Dictionary<int, ACOVRPEdge> ();
							for (int k = 0; k < curMutAnt.Paths.Count && edgesUpdatedWithNode < 2; k++) {
								ACOVRPEdge e = curMutAnt.Paths [k]; //edge of current mutating ant
								if (e.NodeA.Id.Equals (nExOne.Id)) {
									edgesForUpdateFirst.Add (k, new ACOVRPEdge (nExTwo, e.NodeB, vrp.Graph.Edges.Find (x => x.NodeA.Id.Equals (nExTwo.Id) && x.NodeB.Id.Equals (e.NodeB.Id)).Weight, vrp.Graph.Edges.Find (x => x.NodeA.Id.Equals (nExTwo.Id) && x.NodeB.Id.Equals (e.NodeB.Id)).Pheromone));
									edgesUpdatedWithNode++;
								} else if (e.NodeB.Id.Equals (nExOne.Id)) {
									edgesForUpdateFirst.Add (k, new ACOVRPEdge (e.NodeA, nExTwo, vrp.Graph.Edges.Find (x => x.NodeA.Id.Equals (e.NodeA.Id) && x.NodeB.Id.Equals (nExTwo.Id)).Weight, vrp.Graph.Edges.Find (x => x.NodeA.Id.Equals (nExTwo.Id) && x.NodeB.Id.Equals (e.NodeB.Id)).Pheromone));
									edgesUpdatedWithNode++;
								}
							}

							violatedCapacity = curMutAnt.TotalDemandSatisfied > curMutAnt.TheObject.Quantity;

							//exchange at selected randomly ant
							if (!violatedCapacity) {
								//exchange node two in first route
								edgesUpdatedWithNode = 0;
								Dictionary<int, ACOVRPEdge> edgesForUpdateSecond = new Dictionary<int, ACOVRPEdge> ();
								for (int k = 0; k < selectedAnt.Paths.Count && edgesUpdatedWithNode < 2; k++) {
									ACOVRPEdge e = selectedAnt.Paths [k];
									if (e.NodeA.Id.Equals (nExTwo.Id)) {
										edgesForUpdateSecond.Add (k, new ACOVRPEdge (nExOne, e.NodeB, vrp.Graph.Edges.Find (x => x.NodeA.Id.Equals (nExOne.Id) && x.NodeB.Id.Equals (e.NodeB.Id)).Weight, vrp.Graph.Edges.Find (x => x.NodeA.Id.Equals (nExOne.Id) && x.NodeB.Id.Equals (e.NodeB.Id)).Pheromone));
										edgesUpdatedWithNode++;
									} else if (e.NodeB.Id.Equals (nExTwo.Id)) {
										edgesForUpdateSecond.Add (k, new ACOVRPEdge (e.NodeA, nExOne, vrp.Graph.Edges.Find (x => x.NodeA.Id.Equals (e.NodeA.Id) && x.NodeB.Id.Equals (nExOne.Id)).Weight, vrp.Graph.Edges.Find (x => x.NodeA.Id.Equals (e.NodeA.Id) && x.NodeB.Id.Equals (nExOne.Id)).Pheromone));
										edgesUpdatedWithNode++;
									}
								}

								violatedCapacity = selectedAnt.TotalDemandSatisfied > selectedAnt.TheObject.Quantity;

								if (!violatedCapacity) {
									//now update definitely the edges on the ants
									foreach (int k in edgesForUpdateFirst.Keys)
										childSol[curAntIndex].Paths [k] = edgesForUpdateFirst [k];
									foreach (int k in edgesForUpdateSecond.Keys)
										childSol[selectdAntIndex].Paths [k] = edgesForUpdateSecond [k];

									//add current childs sol to the child solution collections
									childSolutions.Add (childSol);

									//							//add the ants for the current solution mutated
									//							res.Add (firstAnt);
									//							res.Add (secondAnt);
									break; //we have a valid two mutated tours
								}
							}//end_if (!violatedCapacity) 
						}//end_if (!nExOne.IsDepot && !nExTwo.IsDepot)
					}//end_for (int j = 0; j < customersSecond.Count; j++) 
					if (!violatedCapacity)
						break;
				}//end_for (int i = 0; i < customersFirst.Count; i++) 

				if (!violatedCapacity)
					break;
			}//end_for(int o=0; o<otherAnts.Count; o++){
		}//end_foreach (VRPAnt curMutAnt in antsForMutating) 
	

		if (!violatedCapacity) {
			//2-opt for each child solution
			for (int i=0; i<childSolutions.Count; i++) {
				childSolutions[i] = improveSolutionWithTwoOPT (childSolutions[i], vrp.Depot.Node);
			}

			//get the best child solution
			List<List<VRPAnt>> bestChildSolution = childSolutions.OrderBy(child=>child.Sum(route=>route.TotalRouteWeight)).Take(1).ToList();
			res = bestChildSolution[0];
		} else {
			res = null;
		}

		return res;
	}
		

	private List<VRPAnt> getAntsForMutating(List<VRPAnt> ants, int curIt){
		Dictionary<VRPAnt, float> probs = new Dictionary<VRPAnt, float> (); //all mutation probabilitis for each ant
		List<VRPAnt> res = new List<VRPAnt>();

		foreach (VRPAnt a in ants) {
			float p = mutationProbability (a, curIt);

			float dice = Random.Range (0, 1); // Throw the dice

			if(p >= dice)
				probs.Add (a, p);
		}

		//return the mutating ants with the greatest mutation probability values
		List<KeyValuePair<VRPAnt, float>> orderedByDescending = probs.OrderByDescending(pair => pair.Value).ToList<KeyValuePair<VRPAnt, float>>();
		foreach (KeyValuePair<VRPAnt, float> p in orderedByDescending) {
			res.Add (p.Key);
		}

		return res;
	}

	private float mutationProbability(VRPAnt ant, int curIt){
		float prob = 0f;
		float pMin = 1.0f / ant.TotalCustomers;
		float pMax = 1.0f / vrp.Vehicles.Count;
		prob = pMin + Mathf.Pow((pMax-pMin), (1-((curIt+1)*1.0f))/(Iterations*1.0f));

		return prob;
	}
		

	//--------------------------------------
	// Public Methods
	//--------------------------------------
	public void startRun(){
		if (vrpLoaded && vrp != null) {
			//start ACO algorithm
			if (visualInitPheromone)
				StartCoroutine (initEdges ());
			else
				StartCoroutine (ACO ());
		}
	}

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
				if (improvedACO) {
					double impInc = pheromoneImprovedIncrement (edgeIJ, ant);
					newPhe += impInc;
				}
				else {
					edgeIJ.Pheromone = ((1.0 - ro) * edgeIJ.Pheromone) + ro / ant.TotalRouteWeight;
				}
			}

			if(improvedACO){
				double value = (ro * edgeIJ.Pheromone) + newPhe;
				double tMin = impIACO_Q / (getSumDistanceFromDepotToCustomers (true));
				double tMax = impIACO_Q / (getSumDistanceFromDepotToCustomers (false));
				edgeIJ.Pheromone = value.Clamp(tMin, tMax); //clamp the value with these two bounds

//				edgeIJ.Pheromone = value;
			}
		}
	}

	public float getSumDistanceFromDepotToCustomers(bool mulByTwo=true){
		return vrp.Graph.Edges.Where (e => e.NodeA.IsDepot && !e.NodeB.IsDepot).Sum (e=>mulByTwo ? 2f*e.Weight : e.Weight);
	}

	public double globalUpdateResult(ACOVRPEdge edge, VRPAnt ant){
		double oldPheromone = edge.Pheromone;
		double newPhe = !improvedACO ? ((1.0 - ro) * oldPheromone) + ro / ant.TotalRouteWeight
//			: ((1.0f - ro) * oldPheromone) * (currentIteration - 1) + pheromoneIncrement (ant);
			: (ro * oldPheromone) + pheromoneImprovedIncrement(edge, ant);
		return newPhe;
	}

	public double pheromoneImprovedIncrement(ACOVRPEdge edgeIJ, VRPAnt ant){
		double inc = 0.0;
		float Q = impIACO_Q;//ants[0].TheObject.Quantity;
		float L = ants.Sum(a=>a.TotalRouteWeight);
		int K = Mathf.Clamp(ants.Count, 1, int.MaxValue);
		float dij = edgeIJ.Weight;
		bool routeIJInAntPaths = ant.Paths.Find (edgeK => edgeK.NodeA.Id.Equals (edgeIJ.NodeA.Id) && edgeK.NodeB.Id.Equals (edgeIJ.NodeB.Id)) != null;

		if (routeIJInAntPaths) {
			int mk = Mathf.Clamp (ant.CompleteTourWithOutDepot.Count, 1, int.MaxValue); //m>0
			float Dk = Mathf.Clamp (ant.TotalRouteWeight, 1, int.MaxValue);
			inc = (Q *1.0 / (L * K)) * ((Dk - dij)*1.0 / (mk * Dk));
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

			vrpLoaded = true;

			UnityEngine.Debug.Log ("VRP loaded");
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
