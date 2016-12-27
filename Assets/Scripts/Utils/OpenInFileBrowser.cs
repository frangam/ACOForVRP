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
//http://wiki.unity3d.com/index.php/OpenInFileBrowser
using UnityEngine;

public static class OpenInFileBrowser{
	public static bool IsInMacOS
	{
		get
		{
			return Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor;
		}
	}

	public static bool IsInWinOS
	{
		get
		{
			return Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor;
		}
	}

	[UnityEditor.MenuItem("Window/Test OpenInFileBrowser")]
	public static void Test()
	{
		Open(UnityEngine.Application.dataPath);
	}

	public static void OpenInMac(string path)
	{
		bool openInsidesOfFolder = false;

		// try mac
		string macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes

		if ( System.IO.Directory.Exists(macPath) ) // if path requested is a folder, automatically open insides of that folder
		{
			openInsidesOfFolder = true;
		}

		if ( !macPath.StartsWith("\"") )
		{
			macPath = "\"" + macPath;
		}

		if ( !macPath.EndsWith("\"") )
		{
			macPath = macPath + "\"";
		}

		string arguments = (openInsidesOfFolder ? "" : "-R ") + macPath;

		try
		{
			System.Diagnostics.Process.Start("open", arguments);
		}
		catch ( System.ComponentModel.Win32Exception e )
		{
			// tried to open mac finder in windows
			// just silently skip error
			// we currently have no platform define for the current OS we are in, so we resort to this
			e.HelpLink = ""; // do anything with this variable to silence warning about not using it
		}
	}

	public static void OpenInWin(string path)
	{
		bool openInsidesOfFolder = false;

		// try windows
		string winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes

		if ( System.IO.Directory.Exists(winPath) ) // if path requested is a folder, automatically open insides of that folder
		{
			openInsidesOfFolder = true;
		}

		try
		{
			System.Diagnostics.Process.Start("explorer.exe", (openInsidesOfFolder ? "/root," : "/select,") + winPath);
		}
		catch ( System.ComponentModel.Win32Exception e )
		{
			// tried to open win explorer in mac
			// just silently skip error
			// we currently have no platform define for the current OS we are in, so we resort to this
			e.HelpLink = ""; // do anything with this variable to silence warning about not using it
		}
	}

	public static void Open(string path)
	{
		if ( IsInWinOS ){
			OpenInWin(path);
		}
		else if ( IsInMacOS )
		{
			OpenInMac(path);
		}
//		else // couldn't determine OS
//		{
//			OpenInWin(path);
//			OpenInMac(path);
//		}
	}
}