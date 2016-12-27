/***************************************************************************
Project:     Game Template
Copyright (c) Frills Games
Author:       Francisco Manuel Garcia Moreno (garmodev@gmail.com)
***************************************************************************/
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
using System.Security.Cryptography;

public static class ListsUtil {
	/// <summary>
	/// Shuffle the specified list.
	/// 
	/// Usage:
	/// List<Product> products = GetProducts();
	/// products.Shuffle();
	/// 
	/// http://stackoverflow.com/questions/273313/randomize-a-listt
	/// </summary>
	/// <param name="list">List.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static void Shuffle<T>(this IList<T> list)
	{
		RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
		int n = list.Count;
		while (n > 1)
		{
			byte[] box = new byte[1];
			do provider.GetBytes(box);
			while (!(box[0] < n * (Byte.MaxValue / n)));
			int k = (box[0] % n);
			n--;
			T value = list[k];
			list[k] = list[n];
			list[n] = value;
		}
	}
}
