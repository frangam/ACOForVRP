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
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager> {
	//--------------------------------------
	// Setting Attributes
	//--------------------------------------
	[SerializeField]
	private Text curAntQuantity;

	[SerializeField]
	private Text curIteration;

	[SerializeField]
	private Button startBtn;

	[SerializeField]
	private Button restartBtn;

	[SerializeField]
	private Button pauseBtn;

	[SerializeField]
	private Button resumeBtn;

	[SerializeField]
	private GameObject settingsPnl;

	[SerializeField]
	private SettingSlider[] sliders;

	//--------------------------------------
	// Private Attributes
	//--------------------------------------
	private float initialAntSpeed;
	private bool settingsPnlOpened = false;

	//--------------------------------------
	// Unity Methods
	//--------------------------------------
	public void Awake(){
		sliders = FindObjectsOfType<SettingSlider> ();
		startBtn.gameObject.SetActive (true);
		restartBtn.gameObject.SetActive (false);
		pauseBtn.gameObject.SetActive (false);
		resumeBtn.gameObject.SetActive (false);
		settingsPnlOpened = false;
		settingsPnl.SetActive (false);
	}

	public void OnEnable(){
		UniFileBrowser.OnFileOpened += OpenFile;
	}
	public void OnDisable(){
		UniFileBrowser.OnFileOpened -= OpenFile;
	}
	public void OnDistroy(){
		UniFileBrowser.OnFileOpened -= OpenFile;
	}

	public void Start(){
		initialAntSpeed = ACOSolver.Instance.AntSpeed;
	}

	//--------------------------------------
	// Public Methods
	//--------------------------------------
	public void updateAntQuantity(int quantity, int initial){
		if (curAntQuantity)
			curAntQuantity.text = "Q: "+Mathf.Clamp(quantity, 0, int.MaxValue).ToString () +" / "+initial.ToString();
	}

	public void updateIteration(int iteration){
		if (curIteration)
			curIteration.text = "Iteration: "+(iteration+1).ToString ();
	}

	public void startButton(){
		ACOSolver.Instance.startRun ();

		startBtn.gameObject.SetActive (false);
		restartBtn.gameObject.SetActive (true);
		pauseBtn.gameObject.SetActive (true);
	}

	public void restartButton(){
		SceneManager.LoadScene (SceneManager.GetActiveScene ().name);
	}

	public void pauseButton(bool pause=true){
		Time.timeScale = pause ? 0f : 1f;

		resumeBtn.gameObject.SetActive (pause);
		pauseBtn.gameObject.SetActive (!pause);
	}

	public void settingsButton(){
		settingsPnlOpened = !settingsPnlOpened;
		settingsPnl.SetActive (settingsPnlOpened);
	}

	public void restoreSettings(){
		foreach (SettingSlider s in sliders)
			s.restoreValue ();
	}


	char pathChar = '/';
	public void openFileBrowser(){

		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) {
			pathChar = '\\';
		}

		if (UniFileBrowser.use.AllowMultiSelect) {
			UniFileBrowser.use.OpenMultiFileWindow ();
		}
		else {
			UniFileBrowser.use.OpenFileWindow ();
		}
	}


	//--------------------------------------
	// Events
	//--------------------------------------
	public void OpenFile (string pathToFile) {
		var fileIndex = pathToFile.LastIndexOf (pathChar);
		string message = "You selected file: " + pathToFile.Substring (fileIndex+1, pathToFile.Length-fileIndex-1);
//		Debug.Log (message);
		ACOVRPGraphLoader.Instance.load (pathToFile);
	}

	public void OpenFiles (string[] pathsToFiles) {
		string message = "You selected these files:\n";
		for (var i = 0; i < pathsToFiles.Length; i++) {
			var fileIndex = pathsToFiles[i].LastIndexOf (pathChar);
			message += pathsToFiles[i].Substring (fileIndex+1, pathsToFiles[i].Length-fileIndex-1) + "\n";
		}
		Debug.Log (message);

	}
}