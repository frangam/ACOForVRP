using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ACOSolver : MonoBehaviour {
	//--------------------------------------
	// Setting Attributes
	//--------------------------------------


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
	// Private Methods
	//--------------------------------------


	//--------------------------------------
	// Events
	//--------------------------------------
	public virtual void OnVRPLoaded(VRP vrp){
		if (vrp != null) {
			
		}
	}

}
