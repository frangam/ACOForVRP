using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACOSolver : MonoBehaviour{
	//--------------------------------------
	// Setting Attributes
	//--------------------------------------
	[SerializeField]
	[Tooltip("Ant Prefab")]
	private VRPAntGameObject pbAnt;

	[SerializeField]
	private float antSpeed = 2;

	//--------------------------------------
	// Private Attributes
	//--------------------------------------
	private VRP vrp;

	//--------------------------------------
	// Unity Methods
	//--------------------------------------
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnEnable (){
		VRPGraphLoader.OnVRPLoaded += OnVRPLoaded;
	}

	public void OnDisable (){
		VRPGraphLoader.OnVRPLoaded -= OnVRPLoaded;
	}

	public void OnDestroy (){
		VRPGraphLoader.OnVRPLoaded -= OnVRPLoaded;
	}

	//--------------------------------------
	// Protected Methods
	//--------------------------------------
	protected virtual void spawnAnts(){
		foreach (VRPVehicle v in vrp.Vehicles) {
			VRPAntGameObject antGO = Instantiate (pbAnt, vrp.Depot.transform.position, Quaternion.identity) as VRPAntGameObject; //the spawn
			VRPAnt ant = new VRPAnt(v);

			//TODO for testing
			int nodeIndex = Random.Range (0, vrp.NodeGOs.Count - 1);
			antGO.loadAnt(ant, vrp.NodeGOs[nodeIndex].transform, antSpeed, vrp.Depot.transform);
		}
	}


	//--------------------------------------
	// Events
	//--------------------------------------
	public virtual void OnVRPLoaded(VRP pVRP){
		if (pVRP != null) {
			vrp = pVRP;

			spawnAnts ();
		}
	}

}
