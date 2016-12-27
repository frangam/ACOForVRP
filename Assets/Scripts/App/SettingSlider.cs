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
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class SettingSlider : MonoBehaviour {
	//--------------------------------------
	// Setting Attributes
	//--------------------------------------
	[SerializeField]
	private SettingType type;

	[SerializeField]
	private Text textValue;

	[SerializeField]
	private string PreText = "Value:";

	//--------------------------------------
	// Private Attributes
	//--------------------------------------
	private Slider slider;
	private float initialValue;

	//--------------------------------------
	// Getters & Setters
	//--------------------------------------
	public Slider Slider {
		get {
			return this.slider;
		}
	}
	public SettingType Type {
		get {
			return this.type;
		}
	}

	public float Value{
		get{
			return slider.value;
		}
		set{
			slider.value = value;
			TextValue.text = PreText + " " + StringValue;
		}
	}

	public string StringValue{
		get{
			string v = Value.ToString (); 

			switch (type) {
			case SettingType.ANT_SPEED:
				v = string.Format ("{0:0.00}", Value);
				break;
			}
			return v;
		}
	}

	public Text TextValue {
		get {
			return this.textValue;
		}
		set {
			textValue = value;
		}
	}
		
	//--------------------------------------
	// Unity Methods
	//--------------------------------------
	public void Awake(){
		slider = GetComponent<Slider> ();
	}

	public void Start(){
		automaticLoadValue ();
	}

	//--------------------------------------
	// Unity Methods
	//--------------------------------------
	public void automaticLoadValue(){
		switch(type){
		case SettingType.ANT_SPEED:
			slider.wholeNumbers = false;
			slider.minValue = 1f;
			slider.maxValue = 20f;
			Value = ACOSolver.Instance.AntSpeed;
			break;
		case SettingType.RO:
			slider.wholeNumbers = false;
			slider.minValue = 0.0000001f;
			slider.maxValue = 1f;
			Value = ACOSolver.Instance.Ro;
			break;
		case SettingType.ALPHA:
			slider.wholeNumbers = false;
			slider.minValue = 0.0000001f;
			slider.maxValue = 20f;
			Value = ACOSolver.Instance.PheromoneInfluence;
			break;
		case SettingType.BETA:
			slider.wholeNumbers = false;
			slider.minValue = 0.0000001f;
			slider.maxValue = 20f;
			Value = ACOSolver.Instance.VisibilityInfluence;
			break;
		case SettingType.Q:
			slider.wholeNumbers = false;
			slider.minValue = 1f;
			slider.maxValue = 100000f;
			Value = ACOSolver.Instance.ImpIACO_Q;
			break;
		case SettingType.ITERATION:
			slider.wholeNumbers = true;
			slider.minValue = 1;
			slider.maxValue = int.MaxValue;
			Value = ACOSolver.Instance.CurrentIteration;
			break;
		}
		initialValue = Value;
	}

	public void automaticChangeValue(){
		switch (type) {
		case SettingType.ANT_SPEED:
			ACOSolver.Instance.AntSpeed = Value;

			if (ACOSolver.Instance.CurAntGO != null)
				ACOSolver.Instance.CurAntGO.MovSpeed = Value;
			break;
		case SettingType.RO:
			ACOSolver.Instance.Ro = Value;
			break;
		case SettingType.ALPHA:
			ACOSolver.Instance.PheromoneInfluence = Value;
			break;
		case SettingType.BETA:
			ACOSolver.Instance.VisibilityInfluence = Value;
			break;
		case SettingType.Q:
			ACOSolver.Instance.ImpIACO_Q = Value;
			break;
		case SettingType.ITERATION:
			ACOSolver.Instance.CurrentIteration = (int) Value;
			break;
		}
		TextValue.text = PreText + " " + StringValue;
	}

	public void restoreValue(){
		Value = initialValue;
		automaticChangeValue ();
	}
}
