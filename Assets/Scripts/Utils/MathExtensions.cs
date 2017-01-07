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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class MathExtensions  {
	/// <summary>
	/// Custom Clamp method the specified val, min and max.
	/// 
	/// Based on: http://stackoverflow.com/questions/2683442/where-can-i-find-the-clamp-function-in-net
	/// </summary>
	/// <param name="val">Value.</param>
	/// <param name="min">Minimum.</param>
	/// <param name="max">Max.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
	{
		T res = val;

		if (val.CompareTo(min) < 0) 
			res = min;
		else if(val.CompareTo(max) > 0) 
			res = max;

		return res;
	}
}
