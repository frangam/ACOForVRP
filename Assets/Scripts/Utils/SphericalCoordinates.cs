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

//http://wiki.unity3d.com/index.php/SphericalCoordinates
//http://blog.nobel-joergensen.com/2010/10/22/spherical-coordinates-in-unity/
//http://en.wikipedia.org/wiki/Spherical_coordinate_system
[System.Serializable]
public class SphericalCoordinates
{
	public float radius
	{ 
		get{ return _radius; }
		private set{ _radius = Mathf.Clamp( value, _minRadius, _maxRadius ); }
	}
	public float polar
	{ 
		get{ return _polar; }
		private set
		{ 
			_polar = loopPolar ? Mathf.Repeat( value, _maxPolar - _minPolar )
				: Mathf.Clamp( value, _minPolar, _maxPolar ); 
		}
	}
	public float elevation
	{ 
		get{ return _elevation; }
		private set
		{ 
			_elevation = loopElevation ? Mathf.Repeat( value, _maxElevation - _minElevation )
				: Mathf.Clamp( value, _minElevation, _maxElevation ); 
		}
	}

	// Determine what happen when a limit is reached, repeat or clamp.
	public bool loopPolar = true, loopElevation = false;

	[SerializeField]
	private float _radius, _polar, _elevation;

	[SerializeField]
	private float _minRadius, _maxRadius, _minPolar, _maxPolar, _minElevation, _maxElevation;

	public SphericalCoordinates(){}
	public SphericalCoordinates( float r, float p, float s,
		float minRadius = 1f, float maxRadius = 20f,
		float minPolar = 0f, float maxPolar = (Mathf.PI*2f),
		float minElevation = 0f, float maxElevation = (Mathf.PI / 3f) )
	{
		_minRadius = minRadius;
		_maxRadius = maxRadius;
		_minPolar = minPolar;
		_maxPolar = maxPolar;
		_minElevation = minElevation;
		_maxElevation = maxElevation;

		SetRadius(r);
		SetRotation(p, s);
	}

	public SphericalCoordinates(Transform T,
		float minRadius = 1f, float maxRadius = 20f,
		float minPolar = 0f, float maxPolar = (Mathf.PI*2f),
		float minElevation = 0f, float maxElevation = (Mathf.PI / 3f)) :
	this(T.position, minRadius, maxRadius, minPolar, maxPolar, minElevation, maxElevation) 
	{ }

	public SphericalCoordinates(Vector3 cartesianCoordinate,
		float minRadius = 1f, float maxRadius = 20f,
		float minPolar = 0f, float maxPolar = (Mathf.PI*2f),
		float minElevation = 0f, float maxElevation = (Mathf.PI / 3f))
	{
		_minRadius = minRadius;
		_maxRadius = maxRadius;
		_minPolar = minPolar;
		_maxPolar = maxPolar;
		_minElevation = minElevation;
		_maxElevation = maxElevation;


		FromCartesian( cartesianCoordinate );
	}

	public Vector3 toCartesian
	{
		get
		{
			float a = radius * Mathf.Cos(elevation);
			return new Vector3(a * Mathf.Cos(polar), radius * Mathf.Sin(elevation), a * Mathf.Sin(polar));
		}
	}

	public SphericalCoordinates FromCartesian(Vector3 cartesianCoordinate)
	{
		if( cartesianCoordinate.x == 0f )
			cartesianCoordinate.x = Mathf.Epsilon;
		radius = cartesianCoordinate.magnitude;

		polar = Mathf.Atan(cartesianCoordinate.z / cartesianCoordinate.x);

		if( cartesianCoordinate.x < 0f )
			polar += Mathf.PI;
		elevation = Mathf.Asin(cartesianCoordinate.y / radius);

		return this;
	}

	public SphericalCoordinates RotatePolarAngle(float x) { return Rotate(x, 0f); }
	public SphericalCoordinates RotateElevationAngle(float x) { return Rotate(0f, x); }
	public SphericalCoordinates Rotate(float newPolar, float newElevation){ return SetRotation( polar + newPolar, elevation + newElevation ); }
	public SphericalCoordinates SetPolarAngle(float x) { return SetRotation(x, elevation); }
	public SphericalCoordinates SetElevationAngle(float x) { return SetRotation(x, elevation); }
	public SphericalCoordinates SetRotation(float newPolar, float newElevation)
	{
		polar = newPolar;		
		elevation = newElevation;

		return this;
	}

	public SphericalCoordinates TranslateRadius(float x) { return SetRadius(radius + x); }
	public SphericalCoordinates SetRadius(float rad)
	{
		radius = rad;
		return this;
	}

	public override string ToString()
	{
		return "[Radius] " + radius + ". [Polar] " + polar + ". [Elevation] " + elevation + ".";
	}
}