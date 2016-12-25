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
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class VRPGraph: WeightedGraph<VRPNode, ACOVRPEdge> {
	//--------------------------------------
	// Constructors
	//--------------------------------------
	public VRPGraph():base(){}
	public VRPGraph(List<VRPNode> nodes, List<ACOVRPEdge> edges): base(nodes, edges, true){} //is a complete graph

	//--------------------------------------
	// Public Methods
	//--------------------------------------
	public void resetNodesVisited(){
		foreach (VRPNode n in Nodes)
			n.Visited = false;
	}

	public void resetPheromone(){
		foreach (ACOVRPEdge e in Edges)
			e.Pheromone = VRPGraphLoader.Instance.InitialPheromone;
	}
}
