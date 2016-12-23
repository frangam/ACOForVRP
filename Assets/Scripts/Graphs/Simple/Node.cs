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
public class Node {
	//--------------------------------------
	// Setting Attributes
	//--------------------------------------
	[SerializeField]
	private string id;

	[SerializeField]
	private string name;

	[SerializeField]
	private int x;

	[SerializeField]
	private int y;

	[SerializeField]
	private bool visited = false;

	//--------------------------------------
	// Getters & Setters
	//--------------------------------------
	public string Id {
		get {
			return this.id;
		}
		set {
			id = value;
		}
	}

	public string Name {
		get {
			return this.name;
		}
		set {
			name = value;
		}
	}

	public int X {
		get {
			return this.x;
		}
		set {
			x = value;
		}
	}

	public int Y {
		get {
			return this.y;
		}
		set {
			y = value;
		}
	}

	public bool Visited {
		get {
			return this.visited;
		}
		set {
			visited = value;
		}
	}

	//--------------------------------------
	// Constructors
	//--------------------------------------
	public Node():this("","",0,0,false){}
	public Node(Node n):this(n.id, n.name, n.x, n.y, n.visited){}
	public Node(int pX, int pY):this("","",pX,pY,false){}
	public Node(int pX, int pY, bool pVisited):this("","",pX,pY,pVisited){}
	public Node(string pId, int pX, int pY, bool pVisited):this(pId,"",pX,pY,pVisited){}
	public Node(string pId, string pName, int pX, int pY, bool pVisited){
		id = pId;
		name = pName;
		x = pX;
		y = pY;
		visited = pVisited;
	}

	//--------------------------------------
	// Overriden Methods
	//--------------------------------------
	public override string ToString ()
	{
		return string.Format ("[Node: name={0}, x={1}, y={2}, visited={3}]", name, x, y, visited);
	}
}
