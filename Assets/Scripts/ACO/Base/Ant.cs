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

	public float TotalRouteWeight{
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
		routeProcessingTimeCost = 0;
		paths = new List<E>();
	}

	//--------------------------------------
	// Public Methods
	//--------------------------------------
	public void resetRoute(){
		paths = new List<E> ();
	}

	//--------------------------------------
	// Virtual Methods
	//--------------------------------------
	public virtual E createRoute(Graph<N, E> graph, N from, N to){
		E edge = graph.Edges.Find (e => e.NodeA.Id.Equals(from.Id) && e.NodeB.Id.Equals(to.Id));
		paths.Add(edge);
		return edge;
	}

	protected virtual bool checkConditionToAddNodeToCompleteTour(List<N> allNodes, N nodeToAdd){
		return !allNodes.Contains (nodeToAdd);
	}

	/// <summary>
	/// Gets the 2-OPT path.
	/// </summary>
	/// <returns>The two OPT paht.</returns>
	public virtual List<N> improveCurrentRouteWithTwoOPT(Graph<N, E> graph){
		List<N> curTour = new List<N>(CompleteTour);
		float minDist = TotalRouteWeight; //current distance cost
		float curCost = 0;

		for (int i = 0; i < curTour.Count; i++) {
			if (checkNodeConditionIn2Opt(curTour, i)) {
				for (int j = i + 1; j < curTour.Count; j++) {
					if(checkNodeConditionIn2Opt(curTour, j)){
						exchangeTwoNodes (curTour, curTour [i], curTour [j]); //do the exchange 2-opt
						curCost = totalWeightOfThisTour (graph, curTour);

						if (curCost < minDist) {
							minDist = curCost;
						} else { //restore
							exchangeTwoNodes (curTour, curTour [i], curTour [j]);
						}
					}
				}
			}
		}

		return curTour;
	}

	public virtual bool checkNodeConditionIn2Opt(List<N> tour, int index){
		return tour[index] != null;
	}

	public virtual float totalWeightOfThisTour(Graph<N, E> graph, List<N> tour){
		float res = 0f;

		for (int i = 0; i < tour.Count-1; i++) {
			N a = tour [i];
			N b = tour [i + 1];
			float w = graph.Edges.Find (e=>e.NodeA.Id.Equals(a.Id) && e.NodeB.Id.Equals(b.Id)).Weight;
			res += w;
		}

		return res;
	}

	public virtual void exchangeTwoNodes(List<N> tour, N a, N b){
		N aux = (N) System.Activator.CreateInstance (typeof(N), a);
		tour[tour.IndexOf (a)] = (N) System.Activator.CreateInstance (typeof(N), b);
		tour[tour.IndexOf (b)] = (N) System.Activator.CreateInstance (typeof(N), aux);
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
