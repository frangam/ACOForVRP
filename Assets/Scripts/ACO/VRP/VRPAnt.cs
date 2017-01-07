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

public class VRPAnt : Ant<VRPVehicle,VRPNode, ACOVRPEdge> {
	//--------------------------------------
	// Getters && Setters
	//--------------------------------------
	public List<VRPNode> CompleteTourWithOutDepot{
		get{
			List<VRPNode> nodes = new List<VRPNode> ();

			foreach (ACOVRPEdge e in Paths) {
				VRPNode from = e.NodeA;
				VRPNode to = e.NodeB;

				if(checkConditionToAddNodeToCompleteTourWithOutDepot(nodes, from)){
					nodes.Add (from);
				}

				if(checkConditionToAddNodeToCompleteTourWithOutDepot(nodes, to)){
					nodes.Add (to);
				}
			}

			return nodes;
		}
	}

	public List<VRPNode> Customers{
		get{
			return CompleteTourWithOutDepot;
		}
	}

	public int TotalCustomers{
		get{
			return (Paths.Find(r=>r.NodeA.IsDepot && r.NodeB.IsDepot) != null) ? CompleteTourWithOutDepot.Count + 1 : CompleteTourWithOutDepot.Count;
		}
	}

	public int TotalDemandSatisfied{
		get{
			return Customers.Sum (c => c.Demand);
		}
	}

	//--------------------------------------
	// Constructors
	//--------------------------------------
	public VRPAnt(VRPVehicle v):base(v){
		
	}

	public VRPAnt(VRPAnt a):base(a){

	}

	//--------------------------------------
	// Virtual Methods
	//--------------------------------------
	protected virtual bool checkConditionToAddNodeToCompleteTourWithOutDepot (List<VRPNode> allNodes, VRPNode nodeToAdd)
	{
		return !nodeToAdd.IsDepot && base.checkConditionToAddNodeToCompleteTour (allNodes, nodeToAdd);
	}




	//--------------------------------------
	// Overriden Methods
	//--------------------------------------
	public override ACOVRPEdge createRoute (Graph<VRPNode, ACOVRPEdge> graph, VRPNode from, VRPNode to)
	{
		RouteProcessingTimeCost += to.ProcessingTime;
		return base.createRoute (graph, from, to);
	}

	protected override bool checkConditionToAddNodeToCompleteTour (List<VRPNode> allNodes, VRPNode nodeToAdd)
	{
		int totalDepotsInTour = allNodes.Where (d => d.IsDepot).Count<VRPNode> ();

		//only we can add a node if this is not present in the current tour or if it is the depot first time (start of the tour) or the second (end of the tour)
		return (nodeToAdd.IsDepot && totalDepotsInTour < 2) || base.checkConditionToAddNodeToCompleteTour (allNodes, nodeToAdd);
	}

	public override bool checkNodeConditionIn2Opt (List<VRPNode> tour, int index)
	{
		return base.checkNodeConditionIn2Opt (tour, index) && !tour[index].IsDepot;
	}


	public override List<VRPNode> getInitialCompleteTourForTwoOPT ()
	{
		List<VRPNode> res = CompleteTour;
//		res.RemoveAt (res.Count - 1); //remove last (depot)

		return res;
	}
}
