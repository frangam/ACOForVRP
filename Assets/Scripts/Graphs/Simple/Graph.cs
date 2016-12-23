﻿/* 
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

public class Graph {
	private List<Node> nodes;
	private List<Edge> edges;
	private bool isComplete;

	public List<Node> Nodes {
		get {
			return this.nodes;
		}
		set {
			nodes = value;
		}
	}

	public bool IsComplete {
		get {
			return this.isComplete;
		}
		set {
			isComplete = value;
		}
	}

	public List<Edge> Edges {
		get {
			return this.edges;
		}
		set {
			edges = value;
		}
	}

	public Graph():this(new List<Node> (), new List<Edge> (), false){}
	public Graph(Graph g):this(g.Nodes, g.Edges, g.IsComplete){}
	public Graph(List<Node> pNodes, List<Edge> pEdges, bool pIsComplete){
		nodes = pNodes;
		edges = pEdges;
		isComplete = pIsComplete;
	}
}
