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

public class VRPGraphLoader : MonoBehaviour {
	[SerializeField]
	private VRPNodeGameObject depot;

	[SerializeField]
	private string graphFileNamePrefx="graph";

	[SerializeField]
	private VRPNodeGameObject pbNode;

	[SerializeField]
	private float minSpawnRadius = 1f;

	[SerializeField]
	private float maxSpawnRadius = 4f;

	[SerializeField]
	private bool randomRadius = false;

	[SerializeField]
	private VRP vrp;

	void Awake () {
		DirectoryInfo di = new DirectoryInfo ("Assets/Resources");
		FileInfo[] files = di.GetFiles (graphFileNamePrefx+"*.txt"); //avoiding .meta files
		int graphIndex = UnityEngine.Random.Range (0,files.Length-1); //get randomly an index for selecting a graph file
		FileInfo selectedFile = files [graphIndex];
		TextAsset ta = Resources.Load (selectedFile.Name.Split(new string[] {selectedFile.Extension}, System.StringSplitOptions.None)[0]) as TextAsset;
		string fileContent = ta.text; //the graph file content
		vrp = processVRPProblemFileContent (fileContent); //create the vrp graph

		//TODO for testing
		spawnNodes (vrp.Graph.Nodes.Where( x => !x.IsDepot).ToList<VRPNode>(), depot.transform.position);
//		spawnNodes (new List<VRPNode>(){new VRPNode(), new VRPNode(), new VRPNode(), new VRPNode(), new VRPNode()
//			,new VRPNode(),new VRPNode(),new VRPNode(),new VRPNode(),new VRPNode()}, depot.position);
	}

	private VRP processVRPProblemFileContent(string content){
		string[] data = content.Split(new string[] {"CUSTOMERS_NODES:"}, System.StringSplitOptions.None);
		string[] data2 = data[1].Split(new string[] {"\nVEHICLES:"}, System.StringSplitOptions.None);
		string[] data3 = data2[1].Split(new string[] {"\nCOSTS:\ttravel time"}, System.StringSplitOptions.None);
		string[] depots = data[0].Split(new string[] {"\nDEPOTS_NODES:"}, System.StringSplitOptions.None)[1].Split(new string[] {"\n"}, System.StringSplitOptions.None);
		string[] customers = data2 [0].Split(new string[] {"\n"}, System.StringSplitOptions.None);
		string[] edgesCosts = data3 [1].Split(new string[] {"\n"}, System.StringSplitOptions.None);;
		string[] vehiclesSettings = data3 [0].Split(new string[] {"\n"}, System.StringSplitOptions.None);;
		List<VRPNode> nodes = new List<VRPNode> ();
		List<VRPEdge> edges = new List<VRPEdge>();
		List<VRPVehicle> vehicles = new List<VRPVehicle>();
		VRPGraph graph = null;
		//depot
		string[] depotValues = depots[2].Split(new string[] {"\t"}, System.StringSplitOptions.None);
		VRPNode depotNode = new VRPNode (depotValues [0], true, Int32.Parse(depotValues [2]));
		this.depot.loadNode (depotNode); //load depot node
		nodes.Add(depotNode);

		//nodes (customers)
		for (int i=2; i<customers.Length; i++) {
			string c = customers [i];
			if (c != "" && c !="\n" && c!="\r" && c!="\t") {
				string[] values = c.Split (new string[] { "\t" }, System.StringSplitOptions.None);
				nodes.Add (new VRPNode (values[0], Int32.Parse(values [2])));
			}
		}

		graph = new VRPGraph(nodes, edges);

		return new VRP(graph, vehicles);
	}

	private void spawnNodes(List<VRPNode> nodes, Vector3 centerPos=default(Vector3)){
		int totalNodes = nodes.Count;

		for (int i = 0; i < totalNodes; i++){
			float progress = (i * 1.0f) / totalNodes; //progress 0-1
			float angle = progress * Mathf.PI * 2; //in radians
			float radius = randomRadius ? UnityEngine.Random.Range(minSpawnRadius, maxSpawnRadius) : maxSpawnRadius;
			float x = Mathf.Sin(angle) * radius;
			float y = Mathf.Cos(angle) * radius;
			Vector3 pos = new Vector3(x, y, 0) + centerPos;
			VRPNodeGameObject nodeGO = Instantiate (pbNode, pos, Quaternion.identity) as VRPNodeGameObject; //the spawn
			nodeGO.loadNode(nodes[i]); //load node info

		}   
	}
}
