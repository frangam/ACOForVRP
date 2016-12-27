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
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class ACOVRPGraphLoader : Singleton<ACOVRPGraphLoader> {
	//--------------------------------------
	// Setting Attributes
	//--------------------------------------
	[SerializeField]
	private VRPNodeGameObject depot;

	[SerializeField]
	private bool symetricDistance = true;

	[SerializeField]
	private bool avoidCycleSameNode = true;

	[SerializeField]
	private string graphFileNamePrefx="graph";

	[SerializeField]
	private float minSpawnRadius = 1f;

	[SerializeField]
	private float maxSpawnRadius = 4f;

	[SerializeField]
	private bool randomRadius = false;

	[SerializeField]
	private VRPNodeGameObject pbNode;

	[SerializeField]
	private bool shuffleNodes = true;

	[SerializeField]
	private bool fixEdgeWeightWithRealDist = true;

	[SerializeField]
	private ACOVRP vrp;

	//--------------------------------------
	// Events
	//--------------------------------------
	public delegate void VRPLoaded (ACOVRP vrp);
	public static event VRPLoaded OnVRPLoaded;

	//--------------------------------------
	// Getters & Setters
	//--------------------------------------

	//--------------------------------------
	// Unity Methods
	//--------------------------------------
	void Awake () {
		DirectoryInfo di = new DirectoryInfo ("Assets/Resources");
		FileInfo[] files = di.GetFiles (graphFileNamePrefx+"*.txt"); //avoiding .meta files
		int graphIndex = UnityEngine.Random.Range (0,files.Length-1); //get randomly an index for selecting a graph file
		FileInfo selectedFile = files [graphIndex];
		TextAsset ta = Resources.Load (selectedFile.Name.Split(new string[] {selectedFile.Extension}, System.StringSplitOptions.None)[0]) as TextAsset;
		string fileContent = ta.text; //the graph file content
		vrp = processVRPProblemFileContent (fileContent); //create the vrp graph
		List<VRPNodeGameObject> nodeGOs = spawnNodes (vrp.Graph.Nodes.Where( x => !x.IsDepot).ToList<VRPNode>(), depot.transform.position);
		vrp.NodeGOs = nodeGOs;
		OnVRPLoaded (vrp); //dispatch event
	}

	//--------------------------------------
	// Private Methods
	//--------------------------------------
	private ACOVRP processVRPProblemFileContent(string content){
		string[] data = content.Split(new string[] {"CUSTOMERS_NODES:"}, System.StringSplitOptions.None);
		string[] data2 = data[1].Split(new string[] {"\nVEHICLES:"}, System.StringSplitOptions.None);
		string[] data3 = data2[1].Split(new string[] {"\nCOSTS:\ttravel time"}, System.StringSplitOptions.None);
		string[] depots = data[0].Split(new string[] {"\nDEPOTS_NODES:"}, System.StringSplitOptions.None)[1].Split(new string[] {"\n"}, System.StringSplitOptions.None);
		string[] customers = data2 [0].Split(new string[] {"\n"}, System.StringSplitOptions.None);
		string[] edgesCosts = data3 [1].Split(new string[] {"\n"}, System.StringSplitOptions.None);
		string[] vehiclesSettings = data3 [0].Split(new string[] {"\n"}, System.StringSplitOptions.None);
		List<VRPNode> nodes = new List<VRPNode> ();
		List<ACOVRPEdge> edges = new List<ACOVRPEdge>();
		List<VRPVehicle> vehicles = new List<VRPVehicle>();
		ACOVRPGraph graph = null;
		//depot
		string[] depotValues = depots[2].Split(new string[] {"\t"}, System.StringSplitOptions.None);
		VRPNode depotNode = new VRPNode (depotValues [0], true, Int32.Parse(depotValues [2])*(-1));
		this.depot.loadNode (depotNode); //load depot node
		nodes.Add(depotNode);

		//vehicles
		for (int i = 2; i < vehiclesSettings.Length; i++) {
			string c = vehiclesSettings [i];
			if (c != "" && c !="\n" && c!="\r" && c!="\t") {
				string[] values = c.Split (new string[] { "\t" }, System.StringSplitOptions.None);
				vehicles.Add (new VRPVehicle (values[0], Int32.Parse(values [3])));
			}
		}

		//nodes (customers)
		for (int i=2; i<customers.Length; i++) {
			string c = customers [i];
			if (c != "" && c !="\n" && c!="\r" && c!="\t") {
				string[] values = c.Split (new string[] { "\t" }, System.StringSplitOptions.None);
				string nodeId = values [0];
				VRPNode n = new VRPNode (nodeId, Int32.Parse (values [2]), Int32.Parse (values [3]));
				nodes.Add (n);
			}
		}

		//edges
		for (int i=2; i<edgesCosts.Length; i++) {
			string c = edgesCosts [i];
			if (c != "" && c !="\n" && c!="\r" && c!="\t" && c!="END\r") {
				string[] values = c.Split (new string[] { "\t" }, System.StringSplitOptions.None);
				string nodeA = values [0];
				string nodeB = values [1];

				if (!nodeA.Equals(nodeB) || (nodeA.Equals(nodeB) && !avoidCycleSameNode)) {
					int cost = Int32.Parse (values [2]);
					ACOVRPEdge symetricEdge = edges.Find (e => e.NodeA.Id.Equals (nodeB) && e.NodeB.Id.Equals (nodeA));

					if (symetricEdge != null) {
						if (symetricDistance)
							edges.Add (new ACOVRPEdge (symetricEdge.NodeB, symetricEdge.NodeA, symetricEdge.Weight, ACOSolver.Instance.InitialPheromone));
						else
							edges.Add (new ACOVRPEdge (symetricEdge.NodeB, symetricEdge.NodeA, cost, ACOSolver.Instance.InitialPheromone));
					} else {
						edges.Add (new ACOVRPEdge (nodes.Find (n => n.Id.Equals (nodeA)), nodes.Find (n => n.Id.Equals (nodeB)), cost, ACOSolver.Instance.InitialPheromone));
					}
				}
			}
		}

		//update polar coordinates nodes (customers)
		foreach (VRPNode n in nodes) {
			ACOVRPEdge ed = null;
			float dist = 0;

			if (!n.IsDepot) {
				ed = edges.Find (e => e.NodeA.Id.Equals (n.Id) && e.NodeB.IsDepot);
				dist = ed.Weight * 1.0f ;
			}

			n.Distance = dist;
		}


		if (shuffleNodes)
			nodes.Shuffle ();

		graph = new ACOVRPGraph(nodes, edges);

		return new ACOVRP(graph, vehicles, this.depot);
	}

	private List<VRPNodeGameObject> spawnNodes(List<VRPNode> nodesOfGraph, Vector3 centerPos=default(Vector3)){
		List<VRPNodeGameObject> nodeGOs = new List<VRPNodeGameObject> ();
		List<VRPNode> nodes = new List<VRPNode>(nodesOfGraph);
		int totalNodes = nodes.Count;
		nodes.Sort ();

		//spawn around the depot
		for (int i = 0; i < totalNodes; i++){
			float progress = (i * 1.0f) / totalNodes; //progress 0-1
			float angle = progress * Mathf.PI * 2; //in radians
//			float radius = randomRadius ? UnityEngine.Random.Range(minSpawnRadius, maxSpawnRadius) : maxSpawnRadius;
			float x = Mathf.Sin(angle) * minSpawnRadius;
			float y = Mathf.Cos(angle) * minSpawnRadius;
			Vector3 pos = new Vector3(x, y, 0) + centerPos;
			VRPNodeGameObject nodeGO = null; 
			VRPNode node = nodes[i];

			x = x > 0 ? x + progress + 0.5f : x - progress - 0.5f;
			y = y > 0 ? y + progress + 0.5f : y - progress - 0.5f;

			node.updatePolarCoords (x, y);
			pos = new Vector3(x, y, 0);

			nodeGO = Instantiate (pbNode, pos, Quaternion.identity) as VRPNodeGameObject; //the spawn
			nodeGO.loadNode(node); //load node info
			nodeGOs.Add(nodeGO);
		}

		//fix distance
		if (fixEdgeWeightWithRealDist) {
			foreach (ACOVRPEdge e in vrp.Graph.Edges) {
				VRPNodeGameObject a = nodeGOs.Find (x => x.Node.Id.Equals (e.NodeA.Id));
				VRPNodeGameObject b = nodeGOs.Find (x => x.Node.Id.Equals (e.NodeB.Id));

				if (a != null && b != null) {
					e.Weight = (int)Vector3.Distance (b.transform.position, a.transform.position);
				}
			}
		}

		return nodeGOs;
	}

	private float meanDistanceOfEdgeToDepot(){
		List<ACOVRPEdge> edgesWithToDepot = vrp.Graph.Edges.Where (e => e.NodeB.IsDepot).ToList<ACOVRPEdge>();
		return edgesWithToDepot.Sum (e => e.Weight) / edgesWithToDepot.Count;
	}
	private int maxDistanceOfEdgeTomDepot(int order=0){
		List<ACOVRPEdge> edgesWithToDepot = vrp.Graph.Edges.Where (e => e.NodeB.IsDepot).ToList<ACOVRPEdge>();
		edgesWithToDepot.Sort ();
		edgesWithToDepot.Reverse ();

		return edgesWithToDepot [order].Weight;
	}
}
