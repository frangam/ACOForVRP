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
public class Edge {
	private Node nodeA;
	private Node nodeB;

	public Node NodeA {
		get {
			return this.nodeA;
		}
		set {
			nodeA = value;
		}
	}

	public Node NodeB {
		get {
			return this.nodeB;
		}
		set {
			nodeB = value;
		}
	}


	public float EuclideanDistance{
		get{
			return Mathf.Sqrt (Mathf.Pow(nodeB.X - nodeA.X,2) + Mathf.Pow(nodeB.Y - nodeA.Y,2));
		}
	}

	public Edge(){
		nodeA = new Node ();
		nodeB = new Node ();
	}

	public Edge(Node a, Node b){
		nodeA = a;
		nodeB = b;
	}

	public override string ToString ()
	{
		return string.Format ("[Edge: nodeA={0}, nodeB={1}]", nodeA, nodeB);
	}
	
}
