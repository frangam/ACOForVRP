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
public class WEdge<N> : Edge<N>, System.IComparable  where N:Node{
	//--------------------------------------
	// Setting Attributes
	//--------------------------------------
	[SerializeField]
	private int weight;

	//--------------------------------------
	// Getters & Setters
	//--------------------------------------
	public int Weight {
		get {
			return this.weight;
		}
		set {
			weight = value;
		}
	}

	//--------------------------------------
	// Constructors
	//--------------------------------------
	public WEdge(N a, N b, int pWeight):base(a, b){
		weight = pWeight;
	}

	//--------------------------------------
	// Overriden Methods
	//--------------------------------------
	public override string ToString ()
	{
		return string.Format ("[WEdge: nodeA={0}, nodeB={1}, weight={2}]", NodeA, NodeB, weight);
	}

	int System.IComparable.CompareTo(object b)
	{
		WEdge<N> e2=(WEdge<N>)b;
		if (this.weight > e2.weight)
			return 1;
		if (this.weight < e2.weight)
			return -1;
		else
			return 0;
	}
}
