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

[System.Serializable]
public class VRPNode: Node {
	//--------------------------------------
	// Constants
	//--------------------------------------
	public const int MIN_DEMAND = 1;

	//--------------------------------------
	// Setting Attributes
	//--------------------------------------
	[SerializeField]
	private bool isDepot = false;

	[SerializeField]
	private int demand = MIN_DEMAND; //valor no negativo

	[SerializeField]
	private int processingTime;

	//--------------------------------------
	// Getters & Setters
	//--------------------------------------
	public bool IsDepot {
		get {
			return this.isDepot;
		}
		set {
			isDepot = value;
		}
	}

	public int Demand {
		get {
			return this.demand;
		}
		set {
			demand = Mathf.Clamp(value,MIN_DEMAND,int.MaxValue);
		}
	}

	public int ProcessingTime {
		get {
			return this.processingTime;
		}
		set {
			processingTime = value;
		}
	}

	//--------------------------------------
	// Constructors
	//--------------------------------------
	public VRPNode():this("","",0,0,false,false,MIN_DEMAND,0){}
	public VRPNode(VRPNode n):this(n.Id,n.Name, n.X, n.Y, n.Visited, false, n.Demand, 0){}
	public VRPNode(string id, int pDemand):this(id, id, 0, 0, false, false, pDemand, 0){}
	public VRPNode(string id, int pDemand, int pProcTime):this(id, id, 0, 0, false, false, pDemand, pProcTime){}
	public VRPNode(string id, bool pIsDepot, int pDemand):this(id,id, 0, 0, false, pIsDepot, pDemand, 0){}
	public VRPNode(string id, string name, int x, int y, bool visited, bool pIsDepot, int pDemand, int pProcTime):base(id,name, x, y, visited){
		isDepot = pIsDepot;
		Demand = pDemand;
		processingTime = pProcTime;
	}

	//--------------------------------------
	// Overriden Methods
	//--------------------------------------
	public override string ToString ()
	{
		return string.Format ("[VRPNode: name={0}, x={1}, y={2}, visited={3}, isDepot={4}, demand={5}, processingTime={6}]", Name, X, Y, Visited, IsDepot, Demand, processingTime);
	}
}