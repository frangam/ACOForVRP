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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VRP {
	//--------------------------------------
	// Setting Attributes
	//--------------------------------------
	[SerializeField]
	private List<VRPNodeGameObject> nodeGOs;

	[SerializeField]
	private VRPGraph graph;

	[SerializeField]
	private List<VRPVehicle> vehicles;

	[SerializeField]
	private VRPNodeGameObject depot;

	//--------------------------------------
	// Getters & Setters
	//--------------------------------------
	/// <summary>
	/// Nodes GameObjects
	/// </summary>
	/// <value>The go nodes.</value>
	public List<VRPNodeGameObject> NodeGOs{
		get {
			return this.nodeGOs;
		}
		set{
			this.nodeGOs = value;
		}
	}

	public VRPGraph Graph {
		get {
			return this.graph;
		}
		set {
			graph = value;
		}
	}

	public List<VRPVehicle> Vehicles {
		get {
			return this.vehicles;
		}
		set {
			vehicles = value;
		}
	}

	public VRPNodeGameObject Depot {
		get {
			return this.depot;
		}
		set {
			depot = value;
		}
	}
		

	//--------------------------------------
	// Constructors
	//--------------------------------------
	public VRP(VRPGraph pGraph, List<VRPVehicle> pVehicles, VRPNodeGameObject pDepot):this(pGraph, pVehicles, pDepot, new List<VRPNodeGameObject>()){}
	public VRP(VRPGraph pGraph, List<VRPVehicle> pVehicles, VRPNodeGameObject pDepot, List<VRPNodeGameObject> go){
		nodeGOs = go;
		graph = pGraph;
		vehicles = pVehicles;
		depot = pDepot;
	}

	//--------------------------------------
	// Overriden Methods
	//--------------------------------------
	public override string ToString ()
	{
		return string.Format ("[VRP: graph={0}, vehicles={1}, depots={2}]", graph, vehicles, depot);
	}
	
}
