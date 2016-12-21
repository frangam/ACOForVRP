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

public class VRPNode: Node {
	public const int MIN_DEMAND = 1;

	private int demand = MIN_DEMAND; //valor no negativo

	public int Demand {
		get {
			return this.demand;
		}
		set {
			demand = Mathf.Clamp(value,MIN_DEMAND,int.MaxValue);
		}
	}

	public VRPNode():base(){
		Demand = MIN_DEMAND;
	}
	public VRPNode(VRPNode n):base(n){
		Demand = n.Demand;
	}
	public VRPNode(string name, int x, int y, bool visited, int pDemand):base(name, x, y, visited){
		Demand = pDemand; 
	}

	public override string ToString ()
	{
		return string.Format ("[VRPNode: name={0}, x={1}, y={2}, visited={3}, demand={4}]", Name, X, Y, Visited, Demand);
	}
}