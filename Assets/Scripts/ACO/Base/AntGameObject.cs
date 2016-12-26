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

public class AntGameObject<T,N,E,P> : MonoBehaviour where N:Node where E:ACOEdge<N> where P:PheromoneGameObject<N,E> {
	//--------------------------------------
	// Setting Attributes
	//--------------------------------------
	[SerializeField]
	private Ant<T,N,E> ant;

	[SerializeField]
	private float movSpeed = 0.5f;

	[SerializeField]
	private bool destroyWhenDepotReached = false;

	//--------------------------------------
	// Private Attributes
	//--------------------------------------
	private Transform depot;
	private Transform destination;
	private bool canMove = false;
	private bool isCommingBack = false;
	private bool destinationReached = false;
	private double currentPheromoneInfluence = 0.1f;
	private bool canSpawnPheromone = true;
	private Color pheromoneColor;
	private E currentEdgeThrough;
	private bool isInitializationAnt = false;

	//--------------------------------------
	// Events
	//--------------------------------------
	public delegate void PheromoneSpawn(P pheromoneGO);
	public static event PheromoneSpawn OnPheromoneSpawned;

	//--------------------------------------
	// Getters & Setters
	//--------------------------------------
	public Ant<T, N, E> Ant {
		get {
			return this.ant;
		}
		set {
			ant = value;
		}
	}
	public bool IsCommingBack {
		get {
			return this.isCommingBack;
		}
	}

	public bool DestinationReached {
		get {
			return this.destinationReached;
		}
	}

	public bool DestroyWhenDepotReached {
		get {
			return this.destroyWhenDepotReached;
		}
	}

	public double CurrentPheromoneInfluence {
		get {
			return this.currentPheromoneInfluence;
		}
		set {
			currentPheromoneInfluence = value;
		}
	}

	//--------------------------------------
	// Unity Methods
	//--------------------------------------
	protected virtual void Awake(){
		
	}

	protected virtual void OnEnable(){
		
	}

	protected virtual void OnDisable(){

	}

	protected virtual void OnDistroy(){

	}

	protected virtual void Start () {
		
	}

	protected virtual void FixedUpdate () {
		if (canMove) {
			transform.position = Vector3.MoveTowards (transform.position, destination.position, Time.deltaTime * movSpeed);
			transform.rotation = Quaternion.LookRotation(Vector3.forward, destination.position - transform.position);
			destinationReached = Vector3.Distance (transform.position, destination.position) <= 0.1f;

			if (DestinationReached)
				canMove = false;
			
			if (isCommingBack && destinationReached) { //arrived to depot
				this.CancelInvoke();
				resetAnt ();

				if (destroyWhenDepotReached)
					Destroy (gameObject);
				else
					gameObject.SetActive (false);
			}
			else if (canSpawnPheromone)
				StartCoroutine (spawnPheromone ());
		}
	}

	//--------------------------------------
	// Public Methods
	//--------------------------------------
	public virtual void loadAnt(Ant<T,N,E> a, Transform pDestination, float pSpeed, Transform pDepot, bool pIsInitializationAnt=false){
		currentPheromoneInfluence = ACOSolver.Instance.InitialPheromone;
		isInitializationAnt = pIsInitializationAnt;
		destinationReached = false;
		ant = a;
		destination = pDestination;
		canMove = !isInitializationAnt;
		isCommingBack = false;
		movSpeed = !isInitializationAnt ? pSpeed : (pSpeed * 5);
		depot = pDepot;

		if (isInitializationAnt) {
			pheromoneColor = Color.white;

			GetComponent<SpriteRenderer> ().enabled = false;
		}
		else
			pheromoneColor = createPheromoneColor ();
	}

	public virtual void resetAnt(){
		isCommingBack = false;
		destinationReached = false;
		canMove = false;
		canSpawnPheromone = true;
	}

	public virtual E createRoute(Graph<N, E> graph, N from, N to){
		E edge = ant.createRoute (graph, from, to);
		return edge;
	}

	public virtual void comeBackToDepot(E edgeThrough){
		currentEdgeThrough = edgeThrough;
		destinationReached = false;
		isCommingBack = true;
		destination = depot;
		canMove = true;
	}

	public virtual void goToNextNode(E edgeThrough, Transform newDestination){
		currentEdgeThrough = edgeThrough;
		destinationReached = false;
		destination = newDestination;
		canMove = true;
	}

	public virtual IEnumerator checkDestinationReached(){
		while(!DestinationReached)
			yield return DestinationReached;
	}

	public virtual IEnumerator spawnPheromone(){
		canSpawnPheromone = false;
		yield return new WaitForSeconds ((float) currentPheromoneInfluence*((1/movSpeed)*(isInitializationAnt ? 0.000000000001f:1f)));
		P p = Instantiate (ACOSolver.Instance.PbPheromone, transform.position, Quaternion.identity) as P;
		p.Edge = currentEdgeThrough;
		p.GetComponent<SpriteRenderer> ().color = pheromoneColor;
		OnPheromoneSpawned (p);
		canSpawnPheromone = true;
	}

	//--------------------------------------
	// Private Methods
	//--------------------------------------
	private Color createPheromoneColor(){
		//create a random color
		Color res = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
		float hue = 0f, contrast = 0f, brigVal = 0f;

		//get hue,constrast,and brigneths values
		Color.RGBToHSV(res, out hue, out contrast, out brigVal);

		//prevent light and brigthness color
		if (brigVal >= 0.7f){
			brigVal = 0.7f;
		}
		else if (brigVal<=0.3f){
			brigVal = 0.3f;
		}
		if (contrast <= 0.8f){
			contrast = 0.8f;
		}
			
		//reassign new color values
		res =Color.HSVToRGB(hue,contrast,brigVal);

		return res;
	}
}
