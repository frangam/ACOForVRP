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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class VRPGraphLoader : MonoBehaviour {
	[SerializeField]
	private Transform depot;

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

	void Awake () {
		DirectoryInfo di = new DirectoryInfo ("Assets/Resources");
		FileInfo[] files = di.GetFiles (graphFileNamePrefx+"*.txt"); //avoiding .meta files
		int graphIndex = Random.Range (0,files.Length-1); //get randomly an index for selecting a graph file
		FileInfo selectedFile = files [graphIndex];
		TextAsset ta = Resources.Load (selectedFile.Name.Split(new string[] {selectedFile.Extension}, System.StringSplitOptions.None)[0]) as TextAsset;
		string fileContent = ta.text; //the graph file content




		//TODO for testing
		spawnNodes (new List<VRPNode>(){new VRPNode(), new VRPNode(), new VRPNode(), new VRPNode(), new VRPNode()
			,new VRPNode(),new VRPNode(),new VRPNode(),new VRPNode(),new VRPNode()}, depot.position);
	}


	private void spawnNodes(List<VRPNode> nodes, Vector3 centerPos=default(Vector3)){
		int totalNodes = nodes.Count;

		for (int i = 0; i < totalNodes; i++){
			float progress = (i * 1.0f) / totalNodes; //progress 0-1
			float angle = progress * Mathf.PI * 2; //in radians
			float radius = randomRadius ? Random.Range(minSpawnRadius, maxSpawnRadius) : maxSpawnRadius;
			float x = Mathf.Sin(angle) * radius;
			float y = Mathf.Cos(angle) * radius;
			Vector3 pos = new Vector3(x, y, 0) + centerPos;
			VRPNodeGameObject nodeGO = Instantiate (pbNode, pos, Quaternion.identity) as VRPNodeGameObject; //the spawn
			nodeGO.loadNode(nodes[i]); //load node info

		}   
	}
}
