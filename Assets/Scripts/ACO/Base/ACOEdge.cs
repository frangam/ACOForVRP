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

[System.Serializable]
public class ACOEdge<N> : WEdge<N> where N:Node {
	//--------------------------------------
	// Setting Attributes
	//--------------------------------------
	[SerializeField]
	private double pheromone;

	//--------------------------------------
	// Setting Attributes
	//--------------------------------------
	private double previousPheromone;

	//--------------------------------------
	// Getters & Setters
	//--------------------------------------
	public double Pheromone {
		get {
			return this.pheromone;
		}
		set {
			previousPheromone = pheromone;
			pheromone = value;
		}
	}

	public double PreviousPheromone {
		get {
			return this.previousPheromone;
		}
		set {
			previousPheromone = value;
		}
	}

	//--------------------------------------
	// Constructors
	//--------------------------------------
	public ACOEdge(N a, N b, float pWeight, double pPheromone):base(a, b, pWeight){
		previousPheromone = ACOSolver.Instance.InitialPheromone;
		pheromone = pPheromone;
	}

	//--------------------------------------
	// Overriden Methods
	//--------------------------------------
	public override string ToString ()
	{
		return string.Format ("[ACOEdge: nodeA={0}, nodeB={1}, weight={2}, pheromone={3}, prevPheromone={4}]", NodeA, NodeB, Weight, pheromone, previousPheromone);
	}
}
