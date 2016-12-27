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

[System.Serializable]
public class Ant<T,N,E> where N:Node where E:ACOEdge<N> {
	//--------------------------------------
	// Setting Attributes
	//--------------------------------------
	[SerializeField]
	private T theObject;

	//--------------------------------------
	// Private Attributes
	//--------------------------------------
	private List<E> paths;
	private int routeDistanceCost;
	private int routeProcessingTimeCost;

	//--------------------------------------
	// Getters & Setters
	//--------------------------------------
	public T TheObject {
		get {
			return this.theObject;
		}
		set {
			theObject = value;
		}
	}

	/// <summary>
	/// Sorted based on the way the nodes of paths are visited
	/// </summary>
	/// <value>The paths.</value>
	public List<E> Paths {
		get {
			return this.paths;
		}
		set{
			this.paths = value;
		}
	}

	public int TotalRouteWeight{
		get{
			return Mathf.Clamp(paths.Sum (r => r.Weight), 1, int.MaxValue);
		}
	}

	public List<N> CompleteTour{
		get{
			List<N> nodes = new List<N> ();

			foreach (E e in paths) {
				N from = (N) e.NodeA;
				N to = (N) e.NodeB;

				if(checkConditionToAddNodeToCompleteTour(nodes, from)){
					nodes.Add (from);
				}

				if(checkConditionToAddNodeToCompleteTour(nodes, to)){
					nodes.Add (to);
				}
			}

			return nodes;
		}
	}



	public int RouteDistanceCost {
		get {
			return this.routeDistanceCost;
		}
		set {
			routeDistanceCost = value;
		}
	}

	public int RouteProcessingTimeCost {
		get {
			return this.routeProcessingTimeCost;
		}
		set {
			routeProcessingTimeCost = value;
		}
	}

	//--------------------------------------
	// Constructors
	//--------------------------------------
	public Ant(T pTheObject){
		theObject = pTheObject;
		routeDistanceCost = 0;
		routeProcessingTimeCost = 0;
		paths = new List<E>();
	}

	//--------------------------------------
	// Public Methods
	//--------------------------------------


	//--------------------------------------
	// Virtual Methods
	//--------------------------------------
	public virtual E createRoute(Graph<N, E> graph, N from, N to){
		E edge = graph.Edges.Find (e => e.NodeA.Id.Equals(from.Id) && e.NodeB.Id.Equals(to.Id));
		routeDistanceCost += edge.Weight;
		paths.Add(edge);
		return edge;
	}

	protected virtual bool checkConditionToAddNodeToCompleteTour(List<N> allNodes, N nodeToAdd){
		return !allNodes.Contains (nodeToAdd);
	}


	//--------------------------------------
	// Private Methods
	//--------------------------------------


	//--------------------------------------
	// Overriden Methods
	//--------------------------------------
	public override string ToString ()
	{
		return string.Format ("[Ant: theObject={0}]", theObject);
	}
	
}
