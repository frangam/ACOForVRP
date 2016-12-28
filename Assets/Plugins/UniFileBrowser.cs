// UniFileBrowser 2.5
// © 2014 Starscene Software. All rights reserved. Redistribution without permission not allowed.

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

public class UniFileBrowser : MonoBehaviour {

	public enum SortType {Name, DateNewest, DateOldest}

	public bool filterFiles = false;			// Filter file names by the items in the filterFileExtensions array
	public string[] filterFileExtensions;		// When filterFiles is true, show only the files with these extensions
	public bool autoAddExtension = false;		// When saving, automatically add this extension to file names
	public string addedExtension;				// Extension to use if automatically adding when saving
	public bool useFilterButton = false;		// Have a button which allows filtering to be turned on and off
	public bool useDeleteButton = false;		// Have a button which allows the deletion of files from within the filebrowser
	// WARNING! This can be destructive. You should probably enable limitToInitialFolder if enabling useDeleteButton
	public string defaultFileName;				// Default string to be used when the file window is opened
	public int maxFileNameLength = 32;			// The maximum filename length when saving, in characters, not including the added extension (min 3, max 128)
	public bool limitToInitialFolder = false;	// Navigating away from the default path is not allowed and no other folders are displayed
	public bool showVolumes = false;			// If true, show all volumes on OS X or drive letters on Windows at the top of the folder list
	public bool volumesAreSeparate = false;		// If true, and volumes are shown, then they will be listed separately from the folders and files
	public bool showDate = false;				// If true, the date when files were last modified is printed next to the file name
	public int dateWidthAdd = 8;				// Pixels to add to the date field, if used
	public SortType sortType = SortType.Name;		// Whether the file list is sorted by name or date
	public bool m_allowMultiSelect = false;		// If true, shift-click and Command-click will allow multiple files to be selected
	public bool allowAppBundleBrowsing = false;	// Allow browsing .app bundles on OS X as folders (which is what they actually are)
	public bool showHiddenOSXFiles = false;		// Shows files and folders starting with "." on OS X
	public bool preventIllegalCharInput = true;	// Prevent "illegal" characters in file names
	public string illegalChars = "\\/|:<>?\"+[]";	// The list of illegal characters; this differs depending on the file system, but doesn't hurt to include them all
	public bool allowWindowResize = true;		// Whether the window can be resized or not
	public bool allowWindowDrag = true;			// Whether the window can be dragged or not
	public int fileWindowID = 50;				// The window ID for the file requester window
	public int messageWindowID = 51;			// The window ID for the dialog message window
	public float doubleClickTime = 0.5f;			// Only used for Unity 3 and Linux builds; time in seconds where subsequent clicks count as a double-click
	public int fileWindowInset = 25;					// Size in pixels of space around the inside of the file browser window
	public Rect defaultFileWindowRect = new Rect(400, 150, 500, 600);	// Default position of the window when it's opened the first time
	public int minWindowWidth = 435;				// Window can't be less wide than this number of pixels
	public int minWindowHeight = 300;			// Window can't be less high than this number of pixels
	public Vector2 messageWindowSize = new Vector2(400, 160);	// Width and height of message window (dialog box)
	public Rect popupRect = new Rect(0, 0, 300, 33);		// Position of the popup button relative to the inset border
	public Vector2 buttonSize = new Vector2(72, 35);			// Size of OK/Cancel/Delete buttons
	public Vector2 windowTabOffset = new Vector2(0, 3);		// File filter window tab offset
	public int guiDepth = -1;							// Sets drawing order compared to other OnGUI calls
	public GUISkin guiSkin;
	public Texture highlightTexture;
	public Color highlightTextColor = Color.black;
	public Texture windowTab;
	public Texture messageWindowTexture;
	public Texture driveIcon;
	public Texture folderIcon;
	public Texture fileIcon;

	private bool _allowMultiSelect;
	public bool AllowMultiSelect {
		get {
			return this._allowMultiSelect;
		}
		set {
			if (fileWindowOpen)
				return;
			_allowMultiSelect = value;
		}
	}

	public enum FileType {Open=0, Save=1, Folder=2}

	private Vector2 scrollPos;
	private int selectedFileNumber = -1;
	private int oldSelectedFileNumber = -1;
	private int anchorFileNumber = 0;
	private List<FileData> dirList;
	private List<FileData> fileList;
	private Texture[] iconDisplayList;
	private string[] fileDisplayList;
	private string[] dateDisplayList;
	private List<int> multiFileList;

	private GUIStyle scrollViewStyle;
	private GUIStyle scrollViewStyleSelected;
	private GUIStyle popupListStyle;
	private GUIStyle popupButtonStyle;
	private GUIStyle popupBoxStyle;
	private GUIStyle messageWindowStyle;

	private string filePath ;
	private string fileName = "";
	private string oldFileName = "";
	private bool frameDone = true;
	private GUIContent[] pathList;
	private RefBool showPopup;
	private RefInt selectedPath;
	private char pathChar = "/"[0];
	private bool windowsSystem = false;
	private bool linuxSystem = false;
	private bool osxSystem = false;
	private bool androidSystem = false;
	private int numberOfVolumes;

	private bool fileWindowOpen = false;
	private Rect fileWindowRect;
	private string windowTitle;
	private string[] fileWindowTitles = new string[]{"Open file", "Save file", "Select folder"};
	private string[] selectButtonText = new string[]{"Open", "Save", "Open"};
	private FileType fileType = FileType.Open;
	private bool handleClicked = false;
	private Vector3 clickedPosition;
	private bool doubleClicked = false;
	private Rect originalWindowRect;
	private KeyCode cmdKey1;
	private KeyCode cmdKey2;
	private Vector3 mousePos;
	private float linePixelHeight;
	private float iconWidth;
	private float dateWidth;
	private bool selectFileInProgress = false;
	private bool showFiles = true;
	private bool sendCloseMessage = false;
	private bool sendChangeMessage = false;
	private Rect lastRect;
	public static UniFileBrowser use;
	private float clickTimer = 0.0f;
	private bool clicked = false;
	private int keyboardControlID;
	private int windowOpenedCounter = 0;
	private GUISkin defaultSkin;
	private Rect textfieldRect;
	private Rect fileBoxRect;
	private Rect fileAreaRect;
	private Rect volumeBoxRect;
	private Rect saveFileAreaRect;
	private Rect viewRect;
	private float bottomAreaSpace;
	private float xIndent;
	private float yIndent;
	private float scrollbarWidth;
	private float buttonPositionX;
	private float buttonPositionY;
	private bool multi;
	private float windowOffset;
	private float storedPopupHeight;
	private bool runCustomFunction = false;
	private bool doFolderFunction = false;
	private bool doFileFunction = false;

	//--------------------------------------
	// Delegates
	//--------------------------------------
	public delegate void FileOpened(string filePath);
	public delegate void MultiFilesOpened(string[] filePaths);
	public delegate void CloseWindowFunction();
	public delegate void ChangeWindowFunction();
	public delegate void CustomFunction();
	public delegate bool FolderFunction(FileInfo[] fileInfo);
	public delegate bool FileFunction(string path);

	//--------------------------------------
	// Events
	//--------------------------------------
	public static event FileOpened OnFileOpened;
	public static event MultiFilesOpened OnMultiFilesOpened;
	public static event CloseWindowFunction OnCloseWindowFunction;
	public static event ChangeWindowFunction OnChangeWindowFunction;
	public static event CustomFunction OnCustomFunction;
	public static event FolderFunction OnFolderFunction;
	public static event FileFunction OnFileFunction;

	#if UNITY_EDITOR
	void Reset () {
		string[] paths = UnityEditor.AssetDatabase.GetAllAssetPaths();
		foreach (string path in paths) {
			string pathString = path.ToLower();
			if (pathString.Contains ("unifilebrowserguiskin")) {
				GUISkin skin = (GUISkin) UnityEditor.AssetDatabase.LoadAssetAtPath (path, typeof(GUISkin));
				if (skin != null && skin.name.ToLower() == "unifilebrowserguiskin") {
					guiSkin = skin;
				}
			}
			else if (pathString.Contains ("ufb")) {
				Texture tex = (Texture) UnityEditor.AssetDatabase.LoadAssetAtPath (path, typeof(Texture));
				if (tex != null) {
					switch (tex.name.ToLower()) {
					case "ufbhighlight": highlightTexture = tex; break;
					case "ufbwindowtab": windowTab = tex; break;
					case "ufbdriveicon": driveIcon = tex; break;
					case "ufbfoldericon": folderIcon = tex; break;
					case "ufbfileicon": fileIcon = tex; break;
					case "ufbwindow2": messageWindowTexture = tex; break;
					}
				}
			}
		}
	}
	#endif

	void Awake () {
		enabled = false;
		if (!guiSkin) {
			Debug.LogError ("UniFileBrowser GUI skin missing");
			return;
		}

		if (m_allowMultiSelect) {
			multiFileList = new List<int>();
			_allowMultiSelect = m_allowMultiSelect;
		}

		if (use != null) {
			Destroy (this);
			return;
		}
		use = this;
		DontDestroyOnLoad (this);
		showPopup = new RefBool(false);
		selectedPath = new RefInt(-1);

		SetDefaultPath();
		// Set up file window position
		fileWindowRect = defaultFileWindowRect;
		fileWindowRect.x = Mathf.Min (fileWindowRect.x, Screen.width - fileWindowRect.width);	// In case small resolutions make it go partially off screen

		// Styles are packaged in the GUI skin
//		try {
			scrollViewStyle = guiSkin.GetStyle ("listScrollview");
			scrollViewStyle.clipping = TextClipping.Clip;
//		}
//		catch (err) {
//			Debug.LogError ("The GUISkin for UniFileBrowser must contain a style called \"listScrollview\"");
//			return;
//		}
		scrollViewStyleSelected = new GUIStyle (scrollViewStyle);
		scrollViewStyleSelected.normal.background = highlightTexture as Texture2D;
		scrollViewStyleSelected.normal.textColor = highlightTextColor;
//		try {
			popupListStyle = guiSkin.GetStyle ("popupList");
//		}
//		catch (err) {
//			Debug.LogError ("The GUISkin for UniFileBrowser must contain a style called \"popupList\"");
//			return;
//		}
//		try {
			popupButtonStyle = guiSkin.GetStyle ("popupButton");
//		}
//		catch (err) {
//			Debug.LogError ("The GUISkin for UniFileBrowser must contain a style called \"popupButton\"");
//			return;
//		}
//		try {
			popupBoxStyle = guiSkin.GetStyle ("popupBox");
//		}
//		catch (err) {
//			Debug.LogError ("The GUISkin for UniFileBrowser must contain a style called \"popupBox\"");
//			return;
//		}
		messageWindowStyle = new GUIStyle (guiSkin.window);
		messageWindowStyle.normal.background = messageWindowTexture as Texture2D;
		messageWindowStyle.onHover.background = messageWindowTexture as Texture2D;

		SetupExtensions();
		if (autoAddExtension && !addedExtension.StartsWith (".")) {
			addedExtension = "." + addedExtension;
		}
		maxFileNameLength = Mathf.Clamp (maxFileNameLength, 3, 128);

		windowOffset = guiSkin.GetStyle ("window").padding.top;
		popupRect.x += fileWindowInset;
		popupRect.y += fileWindowInset + windowOffset;
		storedPopupHeight = popupRect.height;
		if (limitToInitialFolder) {
			popupRect.height = 0;
		}
		xIndent = guiSkin.GetStyle ("box").padding.left;
		yIndent = guiSkin.GetStyle ("box").padding.top;
		linePixelHeight = scrollViewStyle.CalcHeight (new GUIContent(folderIcon), 1.0f);
		iconWidth = scrollViewStyle.CalcSize (new GUIContent(folderIcon)).x;
		dateWidth = scrollViewStyle.CalcSize (new GUIContent(System.DateTime.MinValue.ToString())).x + dateWidthAdd;
		var scrollbarStyle = guiSkin.GetStyle ("verticalscrollbar");
		scrollbarWidth = scrollbarStyle.fixedWidth + scrollbarStyle.margin.left + scrollbarStyle.margin.right;

		dirList = new List<FileData>();
		fileList = new List<FileData>();
	}

	public void SetFileExtensions (string[] extensions) {
		filterFileExtensions = extensions;
		if (extensions == null || extensions.Length == 0) {
			filterFiles = false;
		}
		else {
			filterFiles = true;
		}
		SetupExtensions();
	}

	private void SetupExtensions () {
		// Add "." to file extensions if not already there
		for (int i=0; i<filterFileExtensions.Length; i++) {
			if (!filterFileExtensions[i].StartsWith (".")) {
				filterFileExtensions[i] = "." + filterFileExtensions[i];
			}
		}
	}

	void SetAutoAddedExtension (string extension) {
		if (!extension.StartsWith (".")) {
			extension = "." + extension;
		}
		addedExtension = extension;
	}

	void RefreshFileList () {
		GetCurrentFileInfo();
		if (autoAddExtension) {
			fileName = Path.GetFileNameWithoutExtension(fileName) + addedExtension;
		}
	}

	void SetDefaultPath () {
		filePath = Application.dataPath;
		switch (Application.platform) {
		case RuntimePlatform.OSXEditor:
			filePath = filePath.Substring (0, filePath.LastIndexOf (pathChar)) + pathChar;
			cmdKey1 = KeyCode.LeftApple; cmdKey2 = KeyCode.RightApple;
			osxSystem = true;
			break;
		case RuntimePlatform.OSXPlayer:
			filePath = filePath.Substring (0, filePath.LastIndexOf (pathChar));
			filePath = filePath.Substring (0, filePath.LastIndexOf (pathChar)) + pathChar;
			cmdKey1 = KeyCode.LeftApple; cmdKey2 = KeyCode.RightApple;
			osxSystem = true;
			break;
		case RuntimePlatform.WindowsEditor:
		case RuntimePlatform.WindowsPlayer:
			pathChar = "\\"[0];	// A forward slash should work, but one user had some problems and this seemed part of the solution
			filePath = filePath.Replace ("/", "\\");
			filePath = filePath.Substring (0, filePath.LastIndexOf (pathChar)) + pathChar;
			cmdKey1 = KeyCode.LeftControl; cmdKey2 = KeyCode.RightControl;
			windowsSystem = true;
			break;
		case RuntimePlatform.LinuxPlayer:
			filePath = filePath.Substring (0, filePath.LastIndexOf (pathChar)) + pathChar;
			cmdKey1 = KeyCode.LeftControl; cmdKey2 = KeyCode.RightControl;
			linuxSystem = true;
			break;
		case RuntimePlatform.IPhonePlayer:
			filePath = Application.persistentDataPath + pathChar;
			break;
		case RuntimePlatform.Android:
			filePath = Application.persistentDataPath + pathChar;
			androidSystem = true;
			break;
		default:
			Debug.LogError ("You are not using a supported platform");
			Application.Quit();
			break;
		}
	}

	public bool FileWindowOpen () {
		return fileWindowOpen;
	}

	public Rect GetFileWindowRect () {
		return fileWindowRect;
	}

	private void UpdateRects () {
		fileBoxRect = new Rect(fileWindowInset, textfieldRect.y + (fileType == FileType.Save? (textfieldRect.height + 10) : 0), 0, 0);
		fileBoxRect.width = fileWindowRect.width - fileWindowInset*2;

		if (showVolumes && volumesAreSeparate && !limitToInitialFolder) {
			volumeBoxRect = fileBoxRect;
			volumeBoxRect.height = numberOfVolumes * linePixelHeight + yIndent*2;
			fileBoxRect.y += volumeBoxRect.height + 10;
			fileBoxRect.height = fileWindowRect.height - fileBoxRect.y - bottomAreaSpace;
			fileAreaRect = fileBoxRect;
			fileAreaRect.x += xIndent;
			fileAreaRect.y += yIndent;
			fileAreaRect.width -= xIndent*2;
			fileAreaRect.height -= yIndent*2;
			viewRect = new Rect(fileAreaRect.x, fileAreaRect.y, fileAreaRect.width - (scrollbarWidth + xIndent),
				(fileDisplayList.Length - numberOfVolumes) * linePixelHeight);
		}
		else {
			fileBoxRect.height = fileWindowRect.height - fileBoxRect.y - bottomAreaSpace;
			fileAreaRect = fileBoxRect;
			fileAreaRect.x += xIndent;
			fileAreaRect.y += yIndent;
			fileAreaRect.width -= xIndent*2;
			fileAreaRect.height -= yIndent*2;
			viewRect = new Rect(fileAreaRect.x, fileAreaRect.y, fileAreaRect.width - (scrollbarWidth + xIndent), fileDisplayList.Length * linePixelHeight);
		}
	}

	// Touch scrolling in file area rect
	#if UNITY_IPHONE || UNITY_ANDROID || UNITY_BLACKBERRY || UNITY_WP8
	private Vector2 touchPos;
	private bool touchScrolling = false;

	void Update () {
		if (Input.touchCount > 0) {
		var touch = Input.GetTouch(0);
		var relativeTouchPos = touch.position;
		relativeTouchPos.y = Screen.height - relativeTouchPos.y;
		relativeTouchPos -= Vector2(fileWindowRect.x, fileWindowRect.y);

		if (touch.phase == TouchPhase.Began && fileAreaRect.Contains (relativeTouchPos)) {
		touchPos = relativeTouchPos;
		touchScrolling = true;
		return;
		}
		if (touchScrolling && touch.phase == TouchPhase.Moved) {
		scrollPos -= relativeTouchPos - touchPos;
		touchPos = relativeTouchPos;
		}
		if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) {
		touchScrolling = false;
		}
		}
		else if (touchScrolling) {
		touchScrolling = false;
		}
	}
	#endif

	void OnGUI () {
		defaultSkin = GUI.skin;
		GUI.skin = guiSkin;
		GUI.depth = guiDepth;

		// Keyboard input in editor and non-touch platforms
		#if UNITY_EDITOR || (!UNITY_IPHONE && !UNITY_ANDROID && !UNITY_BLACKBERRY && !UNITY_WP8)
		if (Event.current.type == EventType.KeyDown) {
			FileWindowKeys();
		}
		#endif

		fileWindowRect = GUI.Window (fileWindowID, fileWindowRect, DrawFileWindow, windowTitle);
		if (!showMessageWindow) {
			GUI.FocusWindow (fileWindowID);
		}
		else {
			// Double-clicking can cause an error, probably related to window focus, if the message window is drawn in the same frame when the
			// mouse pointer is over the message window.  So we wait until the next OnGUI frame if this is the case: messageWindowDelay is set
			// true in DrawFileWindow if a double-click is detected.
			if (!messageWindowDelay || !messageWindowRect.Contains (Event.current.mousePosition)) {
				GUI.Window (messageWindowID, messageWindowRect, MessageWindow, messageWindowTitle, messageWindowStyle);
				GUI.BringWindowToFront (messageWindowID);
			}
		}
		messageWindowDelay = false;

		// Resize window by dragging corner...must be done outside window code, or else mouse drag events outside the window are unrecognized
		if (allowWindowResize) {
			if (Event.current.type == EventType.MouseDown && new Rect(fileWindowRect.width-25, fileWindowRect.height-25, 25, 25).Contains (mousePos)) {
				handleClicked = true;
				clickedPosition = mousePos;
				originalWindowRect = fileWindowRect;
			}
			if (handleClicked) {
				if (Event.current.type == EventType.MouseDrag) {
					fileWindowRect.width = Mathf.Clamp (originalWindowRect.width + (mousePos.x - clickedPosition.x), minWindowWidth, 1600);
					fileWindowRect.height = Mathf.Clamp (originalWindowRect.height + (mousePos.y - clickedPosition.y), minWindowHeight, 1200);
					UpdateRects();
				}
				else if (Event.current.type == EventType.MouseUp) {
					handleClicked = false;
				}
			}
		}

		if (sendChangeMessage && Event.current.type == EventType.Repaint) {
			if (lastRect != fileWindowRect) {
				SendWindowChangeMessage();
			}
			lastRect = fileWindowRect;
		}

		GUI.skin = defaultSkin;
	}

	private void DrawFileWindow (int windowID) {
		if (allowWindowDrag) {
			GUI.DragWindow (new Rect(0, 0, 10000, windowOffset));
		}
		mousePos = Event.current.mousePosition;

		if (showMessageWindow) {
			GUI.enabled = false;
		}

		// Folder use button if this is a FileType.Folder window
		if (fileType == FileType.Folder && GUI.Button (new Rect(fileWindowRect.width - buttonSize.x - fileWindowInset, popupRect.y, buttonSize.x, popupRect.height),
			"Select")) {
			if (selectedFileNumber > -1 && selectedFileNumber < dirList.Count) {
				OnFileOpened(filePath + fileDisplayList[selectedFileNumber] + pathChar);
			}
			else {
				OnFileOpened (filePath);
			}
			CloseFileWindow();
			return;
		}

		// Editable file name if saving
		if (fileType == FileType.Save) {
			if (preventIllegalCharInput) {
				for (var i = 0; i < illegalChars.Length; i++) {
					if (Event.current.character == illegalChars[i]) {
						Event.current.character = "\0"[0];
						break;
					}
				}
			}
			// Don't exceed max file name length (the delete keys still work)
			if (fileName.Length == maxFileNameLength + addedExtension.Length) {
				Event.current.character = "\0"[0];
			}
			textfieldRect.width = fileWindowRect.width - 90 - fileWindowInset*2;
			GUI.SetNextControlName ("TextEntry");
			fileName = GUI.TextField (textfieldRect, fileName, 60);
			// Prevent deleting extension
			if (autoAddExtension) {
				if (!fileName.EndsWith (addedExtension)) {
					fileName = oldFileName;
				}
				else {
					oldFileName = fileName;
				}
			}
			// Initialize when window is opened
			if (windowOpenedCounter > 0) {
				if (--windowOpenedCounter == 1) {
					GUI.FocusControl ("TextEntry");
				}
				if (windowOpenedCounter == 0) {
					keyboardControlID = GUIUtility.keyboardControl;
					fileName = defaultFileName;
					if (autoAddExtension) {
						fileName += addedExtension;
					}
				}
			}
		}

		if (fileType == FileType.Save) {
			GUI.Label (new Rect(fileWindowInset, textfieldRect.y, 90, textfieldRect.height), "Save as:");
		}

		// List of folders/files
		GUI.SetNextControlName ("Area");
		GUI.Box (fileBoxRect, "");
		var doScrollView = false;
		Rect buttonRect = default(Rect);
		if (showVolumes && volumesAreSeparate && !limitToInitialFolder) {
			GUI.Box (volumeBoxRect, "");
			buttonRect = new Rect(fileAreaRect.x, volumeBoxRect.y + yIndent, iconWidth, linePixelHeight);
		}
		else {
			scrollPos = GUI.BeginScrollView (fileAreaRect, scrollPos, viewRect);
			buttonRect = new Rect(fileAreaRect.x, fileAreaRect.y, iconWidth, linePixelHeight);
			doScrollView = true;
		}

		bool commandDown = false;
		bool shiftDown = false;
		if (multi) {
			commandDown = (osxSystem && Event.current.command) || (!osxSystem && Event.current.control);
			shiftDown = Event.current.shift;
		}

		float scrollbarOffset = 0;
		if (showDate && fileAreaRect.height < (fileDisplayList.Length - ((showVolumes && volumesAreSeparate)? numberOfVolumes : 0)) * linePixelHeight) {
			scrollbarOffset = scrollbarWidth;
		}

		for (int i = 0; i < fileDisplayList.Length; i++) {
			if (showVolumes && volumesAreSeparate && !limitToInitialFolder && i == numberOfVolumes) {
				scrollPos = GUI.BeginScrollView (fileAreaRect, scrollPos, viewRect);
				buttonRect.y = fileAreaRect.y;
				doScrollView = true;
			}

			var selected = ((multi && multiFileList.Contains (i)) || (!multi && i == selectedFileNumber));

			// Icon
			buttonRect.x = fileAreaRect.x;
			GUI.Label (buttonRect, iconDisplayList[i], selected? scrollViewStyleSelected : scrollViewStyle);
			buttonRect.x += iconWidth;
			buttonRect.width = fileAreaRect.width - iconWidth - (showDate? dateWidth : 0) - scrollbarOffset;

			// File/folder name
			if (GUI.Button (buttonRect, fileDisplayList[i], selected? scrollViewStyleSelected : scrollViewStyle) && frameDone) {
				GUI.FocusControl ("Area");
				selectedFileNumber = i;
				if (multi) {
					if (commandDown) {
						if (multiFileList.Contains (i)) {
							multiFileList.Remove (i);
						}
						else {
							multiFileList.Add (i);
						}
					}
					else {
						if (!multiFileList.Contains (i)) {
							multiFileList.Add (i);
						}
					}
					if (!shiftDown) {
						anchorFileNumber = i;
					}
					fileName = (multiFileList.Count == 1 && i >= dirList.Count && i - dirList.Count < fileList.Count)?
						fileDisplayList[multiFileList[0]] : "";
				}
			}

			// Date
			if (showDate) {
				buttonRect.x += buttonRect.width;
				buttonRect.width = dateWidth;
				GUI.Label (buttonRect, dateDisplayList[i], selected? scrollViewStyleSelected : scrollViewStyle);
			}
			buttonRect.y += linePixelHeight;
		}
		if (doScrollView) {
			GUI.EndScrollView();
		}

		// Remove any file list highlight if the text field is selected
		if (GUIUtility.keyboardControl == keyboardControlID) {
			selectedFileNumber = -1;
		}

		// See if a different file name was chosen, so we don't overwrite any user input in the text box except when needed
		if (selectedFileNumber != oldSelectedFileNumber && frameDone) {
			// Do multi-select if the appropriate key is held...Command/control adds to selection, shift adds a range to selection
			// (or removes from selection as appropriate)
			if (multi) {
				if (shiftDown) {
					if (selectedFileNumber != anchorFileNumber) {
						var add = (selectedFileNumber > anchorFileNumber)? 1 : -1;
						for (int i = anchorFileNumber; i != selectedFileNumber; i += add) {
							if (!multiFileList.Contains (i)) {
								multiFileList.Add (i);
							}
						}
					}

					if (selectedFileNumber <= anchorFileNumber && oldSelectedFileNumber > anchorFileNumber) {
						for (int i = anchorFileNumber + 1; i <= oldSelectedFileNumber; i++) {
							multiFileList.Remove (i);
						}
					}
					else if (selectedFileNumber >= anchorFileNumber && oldSelectedFileNumber < anchorFileNumber) {
						for (int i = anchorFileNumber - 1; i >= oldSelectedFileNumber; i--) {
							multiFileList.Remove (i);
						}
					}

					if (selectedFileNumber > anchorFileNumber && selectedFileNumber < oldSelectedFileNumber) {
						for (int i = selectedFileNumber; i <= oldSelectedFileNumber; i++) {
							multiFileList.Remove (i);
						}
					}
					else if (selectedFileNumber < anchorFileNumber && selectedFileNumber > oldSelectedFileNumber) {
						for (int i = selectedFileNumber; i >= oldSelectedFileNumber; i--) {
							multiFileList.Remove (i);
						}
					}

					if (!multiFileList.Contains (selectedFileNumber)) {
						multiFileList.Add (selectedFileNumber);
					}
				}
				else {
					if (!commandDown) {
						if (multiFileList.Count > 0) {
							multiFileList.Clear();
						}
						multiFileList.Add (selectedFileNumber);
						anchorFileNumber = selectedFileNumber;
					}
				}
				if (!commandDown) {
					fileName = (multiFileList.Count == 1 && selectedFileNumber >= dirList.Count && selectedFileNumber - dirList.Count < fileList.Count)?
						fileDisplayList[multiFileList[0]] : "";
				}
			}
			else {
				if (fileType == FileType.Save) {
					if (selectedFileNumber >= dirList.Count && selectedFileNumber - dirList.Count < fileList.Count) {
						fileName = fileList[selectedFileNumber - dirList.Count].name;
					}
				}
				else {
					fileName = (selectedFileNumber >= dirList.Count && selectedFileNumber - dirList.Count < fileList.Count)?
						fileList[selectedFileNumber - dirList.Count].name : ""; // No file name if directory is selected
				}
			}

			oldSelectedFileNumber = selectedFileNumber;

			if (fileType == FileType.Save && autoAddExtension && !fileName.EndsWith (addedExtension)) {
				fileName += addedExtension;
			}
		}

		// Double-click - only in file selection area
		// Mouse input for editor and non-touch platforms
		#if UNITY_EDITOR || (!UNITY_IPHONE && !UNITY_ANDROID && !UNITY_BLACKBERRY && !UNITY_WP8)
		#if !UNITY_EDITOR && (UNITY_3_4 || UNITY_3_5 || UNITY_STANDALONE_LINUX)
		if (Input.GetMouseButtonDown(0) && !clicked) {
		if (Time.time - clickTimer < doubleClickTime && (fileAreaRect.Contains (mousePos) || volumeBoxRect.Contains (mousePos)) && frameDone) {
		doubleClicked = true;
		}
		clickTimer = Time.time;
		clicked = true;
		}
		if (Input.GetMouseButtonUp(0)) {
		clicked = false;
		}
		#else
		if (Event.current.clickCount == 2 && (fileAreaRect.Contains (mousePos) || volumeBoxRect.Contains (mousePos)) && frameDone) {
			doubleClicked = true;
		}
		#endif

		if (doubleClicked && Input.GetMouseButtonUp(0) && frameDone) {	// EventType.MouseUp doesn't work here because the buttons eat it
			doubleClicked = false;
			StartCoroutine(SelectFile());
			StartCoroutine(WaitForFrame());
			messageWindowDelay = true;
		}
		// Touch input for mobile
		#else
		for (var touch in Input.touches) {
		if (touch.tapCount == 2 && (fileAreaRect.Contains (mousePos) || volumeBoxRect.Contains (mousePos)) && frameDone) {
		StartCoroutine(SelectFile());
		StartCoroutine(WaitForFrame());
		messageWindowDelay = true;
		break;
		}
		}
		#endif

		// Filter button
		if (useFilterButton) {
			GUI.Label (new Rect(40 + windowTabOffset.x, fileWindowRect.height-76 + windowTabOffset.y, 120, 50), windowTab);
			if (GUI.Button (new Rect(50, fileWindowRect.height-76 + windowTabOffset.y + 8, 80, 33), filterFiles? "Show all" : "Filter") ) {
				filterFiles = !filterFiles;
				GetCurrentFileInfo();
			}
		}

		// Delete button
		if (useDeleteButton && fileType != FileType.Folder) {
			if (fileType == FileType.Open) {
				if ( (!_allowMultiSelect && fileName == "") || (_allowMultiSelect && multiFileList.Count == 0) ) {
					GUI.enabled = false;
				}
			}
			else {
				if (fileName == "" || (autoAddExtension && fileName == addedExtension)) {
					GUI.enabled = false;
				}
			}
			if (GUI.Button(new Rect(fileWindowRect.width - buttonPositionX - (buttonSize.x+25)*2, fileWindowRect.height-buttonPositionY, buttonSize.x, buttonSize.y),
				"Delete") ) {
				StartCoroutine(DeleteFile());
			}
		}
		if (!showMessageWindow) {
			GUI.enabled = true;
		}

		// Cancel button
		if (GUI.Button (new Rect(fileWindowRect.width - buttonPositionX - (buttonSize.x+15), fileWindowRect.height - buttonPositionY, buttonSize.x, buttonSize.y),
			"Cancel") ) {
			CloseFileWindow();
		}

		// Open/Save button
		if (fileType == FileType.Open) {
			if ( (!multi && selectedFileNumber == -1) || (multi && multiFileList.Count == 0) ) {
				GUI.enabled = false;
			}
		}
		else if (fileType == FileType.Folder) {
			if (selectedFileNumber == -1 || selectedFileNumber >= dirList.Count) {
				GUI.enabled = false;
			}
		}
		else {
			if ((selectedFileNumber == -1 || selectedFileNumber >= dirList.Count) &&
				((!autoAddExtension && fileName == "") || (autoAddExtension && fileName == addedExtension)) ) {
				GUI.enabled = false;
			}
		}
		if (GUI.Button (new Rect(fileWindowRect.width - buttonPositionX, fileWindowRect.height - buttonPositionY, buttonSize.x, buttonSize.y), (fileType == FileType.Save && (selectedFileNumber > -1 && selectedFileNumber < dirList.Count))? selectButtonText[(int)FileType.Open] : selectButtonText[(int)fileType])) {
			StartCoroutine(SelectFile());
		}

		if (!showMessageWindow) {
			GUI.enabled = true;
		}

		if (runCustomFunction) {
			CustomFunction d = SetCustomFunction;
			d ();
		}

		// Path list popup -- done last so it's drawn on top of other stuff
		if (!limitToInitialFolder) {
			if (pathList.Length > 0 && Popup.List (popupRect, showPopup, selectedPath, pathList[0], pathList, popupButtonStyle, popupBoxStyle, popupListStyle)) {
				if (selectedPath.i > 0) {
					BuildPathList (selectedPath.i);
				}
			}
		}
	}

	enum MessageWindowType {Error, Confirm}
	private MessageWindowType messageWindowType;
	private string button1Text;
	private string button2Text;
	private string message;
	private bool showMessageWindow = false;
	private string messageWindowTitle;
	private Rect messageWindowRect;
	private bool confirm = true;
	private bool messageWindowDelay = false;

	private void ShowError (string msg) {
		message = msg;
		messageWindowTitle = "Error";
		showMessageWindow = true;
		messageWindowType = MessageWindowType.Error;
		fileName = "";
	}

	private void ShowConfirmMessage (string title, string msg, string b1Text, string b2Text) {
		// Set up message window position to be in the middle of the screen
		messageWindowRect = new Rect(Screen.width/2-messageWindowSize.x/2, Screen.height/2-messageWindowSize.y/2, messageWindowSize.x, messageWindowSize.y);
		message = msg;
		button1Text = b1Text;
		button2Text = b2Text;
		messageWindowTitle = title;
		showMessageWindow = true;
		messageWindowType = MessageWindowType.Confirm;
	}

	private void MessageWindow (int messageWindowId) {
		GUI.Label (new Rect(fileWindowInset, windowOffset + fileWindowInset, messageWindowRect.width - fileWindowInset*2,
			messageWindowRect.height - (windowOffset + fileWindowInset + buttonSize.y + 25)), message);

		if (messageWindowType == MessageWindowType.Error) {
			if (GUI.Button (new Rect(messageWindowSize.x - (fileWindowInset + buttonSize.x), messageWindowSize.y - (buttonSize.y + fileWindowInset),
				buttonSize.x, buttonSize.y), "OK") && frameDone) {
				CloseMessageWindow (false);
			}
		}
		else if (messageWindowType == MessageWindowType.Confirm) {
			if (GUI.Button (new Rect(messageWindowSize.x - (fileWindowInset + buttonSize.x*2 + 25), messageWindowSize.y - (buttonSize.y + fileWindowInset),
				buttonSize.x, buttonSize.y), button1Text) && frameDone) {
				CloseMessageWindow (false);
			}
			if (GUI.Button (new Rect(messageWindowSize.x - (fileWindowInset + buttonSize.x), messageWindowSize.y - (buttonSize.y + fileWindowInset),
				buttonSize.x, buttonSize.y), button2Text) && frameDone) {
				CloseMessageWindow (true);
			}
		}
	}

	private void CloseMessageWindow (bool isConfirmed) {
		showMessageWindow = false;
		confirm = isConfirmed;
	}

	// Work-around for behavior where double-clicking selects files it shouldn't
	private IEnumerator WaitForFrame () {
		frameDone = false;
		selectedFileNumber = -1;
		yield return null;
		frameDone = true;
		selectedFileNumber = -1;
	}

	// Keyboard input for editor and non-touch platforms
	#if UNITY_EDITOR || (!UNITY_IPHONE && !UNITY_ANDROID && !UNITY_BLACKBERRY && !UNITY_WP8)
	private void FileWindowKeys () {
		int arrowKey = 0;
		switch (Event.current.keyCode) {
		case KeyCode.DownArrow: arrowKey = 1; break;
		case KeyCode.UpArrow: arrowKey = -1; break;
		case KeyCode.Return: ReturnHit(); break;
		case KeyCode.Escape: EscapeHit(); break;
		}
		if (arrowKey == 0) return;

		// Go to top or bottom of list if alt key is down
		if (Input.GetKey (KeyCode.LeftAlt) || Input.GetKey (KeyCode.RightAlt)) {
			if (arrowKey == -1) {selectedFileNumber = 0;}
			else {selectedFileNumber = fileDisplayList.Length-1;}
		}
		// Go up or down a folder hierarchy level if command key is down (command/apple on OS X, control on Windows)
		else if (!limitToInitialFolder && (Input.GetKey (cmdKey1) || Input.GetKey (cmdKey2)) ) {
			if (arrowKey == -1 && pathList.Length > 1) {
				BuildPathList (1);
				return;
			}
			else if (selectedFileNumber >= 0 && selectedFileNumber < dirList.Count) {
				StartCoroutine(SelectFile());
				return;
			}
		}
		// Move file selection up or down
		else {
			selectedFileNumber += arrowKey;
			if (selectedFileNumber < -1) {
				selectedFileNumber = fileDisplayList.Length-1;
			}
			selectedFileNumber = Mathf.Clamp (selectedFileNumber, 0, fileDisplayList.Length-1);
		}

		// Handle keyboard scrolling of the list view properly
		var wantedPos = linePixelHeight * selectedFileNumber;
		var otherSpace = (fileWindowRect.height - fileAreaRect.height) + linePixelHeight + linePixelHeight/2;
		if (wantedPos < scrollPos.y) {
			scrollPos.y = wantedPos;
		}
		else if (wantedPos > scrollPos.y + fileWindowRect.height - otherSpace) {
			scrollPos.y = wantedPos - (fileWindowRect.height - otherSpace);
		}
	}

	private void ReturnHit () {
		if (showMessageWindow) {
			CloseMessageWindow (true);
		}
		else {
			StartCoroutine(SelectFile());
		}
	}

	private void EscapeHit () {
		if (showMessageWindow) {
			CloseMessageWindow (false);
		}
		else {
			CloseFileWindow();
		}
	}
	#endif

	private void BuildPathList (int pathEntry) {
		filePath = "";
		for (var i = pathList.Length-1; i >= pathEntry; i--) {
			filePath += pathList[i].text;
			if (i < pathList.Length-1 || windowsSystem) {
				filePath += pathChar;
			}
		}
		selectedPath.i = -1;
		GetCurrentFileInfo();
	}

	private void GetCurrentFileInfo () {
		dirList.Clear();
		fileList.Clear();

		bool infoExists = true;
		DirectoryInfo info = new DirectoryInfo(filePath);
		FileInfo[] fileInfo;
		DirectoryInfo[] dirInfo;

		if (!info.Exists) {
			infoExists = false;
		}

		if (infoExists) {
//			try {
				fileInfo = info.GetFiles();
				dirInfo = info.GetDirectories();
//			}
//			catch (err) {
//				HandleError (err.Message);
//				return;
//			}
		}
		else {
			fileInfo = new FileInfo[0];
			dirInfo = new DirectoryInfo[0];
		}

		// Put folder names into a sorted array
		if (!limitToInitialFolder && dirInfo.Length > 0) {
			for (var i = 0; i < dirInfo.Length; i++) {
				// Don't include ".app" folders or hidden folders, if set
				if (dirInfo[i].Name.EndsWith (".app") && !allowAppBundleBrowsing) continue;
				if (dirInfo[i].Name.StartsWith (".") && !showHiddenOSXFiles) continue;
				// Custom folder filter, if set
				if (doFolderFunction) {
					FileInfo[] filesInFolder = new DirectoryInfo(dirInfo[i].FullName).GetFiles();
					OpenFolderWindow ();
					if (!OnFolderFunction(filesInFolder)) continue;
				}

				var dirDate = showDate? dirInfo[i].LastWriteTime : System.DateTime.MinValue;
				dirList.Add (new FileData(dirInfo[i].Name, dirDate));
			}
			if (sortType == SortType.Name) {
				dirList.Sort ((FileData a, FileData b) => a.name.CompareTo (b.name));
			}
			else {
				dirList.Sort ((FileData a, FileData b) => a.date.CompareTo (b.date));
				if (sortType == SortType.DateNewest) {
					dirList.Reverse();
				}
			}
		}

		// Get volumes/drives, if set
		if (showVolumes && !limitToInitialFolder) {
//			try {
				int idx = 0;
				if (windowsSystem || linuxSystem) {
					string[] drives = Directory.GetLogicalDrives();
			
					for (int i = 0; i < drives.Length; i++) {
						if (drives[i].Length > 1) {
							dirList.Insert (idx++, new FileData(drives[i].Substring(0, drives[i].Length-1), System.DateTime.MinValue));
						}
					}
					numberOfVolumes = idx;
					if (windowsSystem) {
						filePath = filePath.Replace (":" + pathChar + pathChar, ":" + pathChar);
					}
				}
				else {
					if (androidSystem) {
						info = new DirectoryInfo("/mnt");
					}
					else {
						info = new DirectoryInfo("/Volumes");
					}
					dirInfo = info.GetDirectories();
					idx = 0;
					for (int i = 0; i < dirInfo.Length; i++) {
						if (dirInfo[i].GetFiles().Length != 0 || dirInfo[i].GetDirectories().Length != 0) {
							dirList.Insert (idx++, new FileData(dirInfo[i].Name, System.DateTime.MinValue));
						}
					}
					numberOfVolumes = idx;
				}
//			}
//			catch (err) {
//				HandleError (err.Message);
//			}
		}

		// Put file names into a sorted array
		if (showFiles && fileInfo.Length > 0) {
			for (int i = 0; i < fileInfo.Length; i++) {
				// Don't include hidden files, if set
				if (fileInfo[i].Name.StartsWith (".") && !showHiddenOSXFiles) continue;
				if (filterFiles && filterFileExtensions.Length > 0) {
					// Go through all extensions for this file type
					var dontAddFile = true;
					for (var j = 0; j < filterFileExtensions.Length; j++) {
						if (fileInfo[i].Name.EndsWith (filterFileExtensions[j])) {
							dontAddFile = false;
							break;
						}
					}
					if (dontAddFile) continue;
				}
				// Custom file filter, if set
				if (doFileFunction) {
					FileFunction del = UseFileFilterFunction;
					if (! del(fileInfo[i].Name)) continue;
				}
				var fileDate = showDate? fileInfo[i].LastWriteTime : System.DateTime.MinValue;
				fileList.Add (new FileData(fileInfo[i].Name, fileDate));
			}
			if (sortType == SortType.Name) {
				fileList.Sort ((FileData a, FileData b) => a.name.CompareTo (b.name));
			}
			else {
				fileList.Sort ((FileData a, FileData b) => a.date.CompareTo (b.date));
				if (sortType == SortType.DateNewest) {
					fileList.Reverse();
				}
			}
		}

		// Create the combined folder + file lists that are actually displayed
		iconDisplayList = new Texture[dirList.Count + fileList.Count];
		fileDisplayList = new String[iconDisplayList.Length];
		dateDisplayList = new String[iconDisplayList.Length];
		for (int i = 0; i < dirList.Count; i++) {
			if (showVolumes && i < numberOfVolumes) {
				iconDisplayList[i] = driveIcon;
				fileDisplayList[i] = dirList[i].name;
				dateDisplayList[i] = "";
			}
			else {
				iconDisplayList[i] = folderIcon;
				fileDisplayList[i] = dirList[i].name;
				dateDisplayList[i] = "  " + dirList[i].date;
			}
		}
		for (int i = 0; i < fileList.Count; i++) {
			iconDisplayList[i + dirList.Count] = fileIcon;
			fileDisplayList[i + dirList.Count] = fileList[i].name;
			dateDisplayList[i + dirList.Count] = "  " + fileList[i].date;
		}

		// Get path list
		var currentPathList = filePath.Split (pathChar);
		var pathListArray = new List<string>();
		for (int i = 0; i < currentPathList.Length-1; i++) {
			if (currentPathList[i] == "") {
				pathListArray.Add (pathChar.ToString());
			}
			else {
				pathListArray.Add (currentPathList[i]);
			}
		}
		pathListArray.Reverse();
		pathList = new GUIContent[pathListArray.Count];
		for (int i = 0; i < pathList.Length; i++) {
			pathList[i] = new GUIContent(pathListArray[i], folderIcon);
		}

		// Reset stuff so no filenames are selected and the scrollbar is always at the top
		selectedFileNumber = oldSelectedFileNumber = -1;
		scrollPos = Vector2.zero;
		if (_allowMultiSelect) {
			multiFileList.Clear();
			anchorFileNumber = 0;
		}
		UpdateRects();
	}

	private void HandleError (string errorMessage) {
		// Set up message window position to be in the middle of the screen
		messageWindowRect = new Rect(Screen.width/2-messageWindowSize.x/2, Screen.height/2-messageWindowSize.y/2, messageWindowSize.x, messageWindowSize.y);
		ShowError (errorMessage);
		SetDefaultPath();
		fileDisplayList = new String[0];
		pathList = new GUIContent[0];
	}

	public void SetPath (string thisPath) {
		filePath = thisPath;
		if (!filePath.EndsWith (pathChar.ToString())) {
			filePath += pathChar;
		}
		if (windowsSystem) {
			filePath = filePath.Replace ("/", "\\");
		}
	}

	public void OpenFileWindow () {
		if (fileWindowOpen) return;
		if (_allowMultiSelect) {
			Debug.LogError ("When using allowMultiSelect, you must supply a function that accepts a string array");
			return;
		}
			
		showFiles = true;
		fileType = FileType.Open;
		ShowFileWindow();

	}

	public void OpenMultiFileWindow () {
		if (fileWindowOpen) return;
		if (!_allowMultiSelect) {
			Debug.LogError ("When not using allowMultiSelect, you must supply a function that accepts a string, not a string array");
			return;
		}
			
//		showFiles = true;
		fileType = FileType.Open;
		ShowFileWindow();
	}

	public void OpenFolderWindow () {
		if (!fileWindowOpen) {

			this.showFiles = true;
			fileType = FileType.Folder;
			limitToInitialFolder = false;
			popupRect.height = storedPopupHeight;
			ShowFileWindow ();
		}
	}

	public void SaveFileWindow () {
		if (fileWindowOpen) return;

		showFiles = true;
		fileType = FileType.Save;
		ShowFileWindow();
		windowOpenedCounter = 2;
	}

	private void ShowFileWindow () {
		GetCurrentFileInfo();
		windowTitle = fileWindowTitles[(int) fileType];
		fileWindowOpen = true;
		lastRect = fileWindowRect;
		enabled = true;
		fileName = "";
		multi = _allowMultiSelect && fileType == FileType.Open;

		var textfieldStyle = guiSkin.GetStyle ("textfield");
		textfieldRect = new Rect(fileWindowInset + 90, limitToInitialFolder? fileWindowInset + windowOffset : popupRect.y + popupRect.height + 15, 0, 0);
		if (fileType != FileType.Folder) {
			textfieldRect.height = textfieldStyle.CalcHeight(new GUIContent(" "), 1.0f) + textfieldStyle.padding.top;
		}
		bottomAreaSpace = fileWindowInset + 15 + buttonSize.y;
		buttonPositionX = fileWindowInset + buttonSize.x;
		buttonPositionY = fileWindowInset + buttonSize.y;
		UpdateRects();
	}

	public void SendWindowCloseMessage () {
		sendCloseMessage = true;
	}

	public void DontSendWindowCloseMessage () {
		sendCloseMessage = false;
	}

	public void SendWindowChangeMessage () {
		sendChangeMessage = true;
	}

	public void DontSendWindowChangeMessage () {
		sendChangeMessage = false;
	}

	public void SetCustomFunction () {
		runCustomFunction = true;
	}

	public void RemoveCustomFunction () {
		runCustomFunction = false;
	}

	public void UseFolderFilterFunction () {
		doFolderFunction = true;
	}



	public void DontUseFolderFilterFunction () {
		doFolderFunction = false;
	}

	public bool UseFileFilterFunction (string path) {
		doFileFunction = true;
		return doFileFunction;
	}

	public void DontUseFileFilterFunction () {
		doFileFunction = false;
	}

	public void CloseFileWindow () {
		if (showMessageWindow) return;	// Don't let window close if error/confirm window is open

		fileWindowOpen = false;
		selectedFileNumber = oldSelectedFileNumber = -1;
		fileName = "";
		if (sendCloseMessage) {
			CloseWindowFunction d = delegate() {
				
			};
			d ();
		}
		// For maximum efficiency, the OnGUI function in this script doesn't run at all when the file browser window isn't open,
		// but is enabled in ShowFileWindow when necessary
		enabled = false;
	}

	public void SetWindowTitle (string title) {
		windowTitle = title;
	}

	public void SetFileWindowPosition (Vector2 windowPosition) {
		windowPosition.x = Mathf.Max (0, windowPosition.x);
		windowPosition.y = Mathf.Max (0, windowPosition.y);
		fileWindowRect = GetClampedWindowRect (new Rect(windowPosition.x, windowPosition.y, fileWindowRect.width, fileWindowRect.height));
		if (fileDisplayList != null) {
			UpdateRects();
		}
	}

	public void SetFileWindowSize (Vector2 windowSize) {
		windowSize.x = Mathf.Max (0, windowSize.x);
		windowSize.y = Mathf.Max (0, windowSize.y);
		fileWindowRect = GetClampedWindowRect (new Rect(fileWindowRect.x, fileWindowRect.y, windowSize.x, windowSize.y));
		if (fileDisplayList != null) {
			UpdateRects();
		}
	}

	private Rect GetClampedWindowRect (Rect newWindowRect) {
		var thisWidth = Screen.width - newWindowRect.x;
		if (thisWidth < minWindowWidth) {
			newWindowRect.x -= minWindowWidth - thisWidth;
			thisWidth = minWindowWidth;
		}
		var thisHeight = Screen.height - newWindowRect.y;
		if (thisHeight < minWindowHeight) {
			newWindowRect.y -= minWindowHeight - thisHeight;
			thisHeight = minWindowHeight;
		}
		return new Rect (newWindowRect.x, newWindowRect.y,
			Mathf.Clamp (newWindowRect.width, minWindowWidth, thisWidth), Mathf.Clamp (newWindowRect.height, minWindowHeight, thisHeight));
	}

	private IEnumerator DeleteFile () {
		if (!(showMessageWindow || selectFileInProgress || (selectedFileNumber >= 0 && selectedFileNumber < dirList.Count))) {

			selectFileInProgress = true;

			if (!_allowMultiSelect) {
				if (File.Exists (filePath + fileName)) {
					ShowConfirmMessage ("Warning", "Are you sure you want to delete " + fileName + "?", "Cancel", "Delete");
					while (showMessageWindow) {
						yield return null;
					}
					if (!confirm) {
						selectFileInProgress = false;
					}
				} else {
					selectFileInProgress = false;
				}

				if (selectFileInProgress) {
//					try {
					File.Delete (filePath + fileName);
//					} catch (err) {
//					ShowError (err.Message);
//					}
					GetCurrentFileInfo ();
					selectFileInProgress = false;
				}
			} else {
				var files = GetMultiFileNames ();
				for (var i = 0; i < files.Count; i++) {
					if (File.Exists (files [i])) {
						string thisFileName = Path.GetFileName (files [i]);
						ShowConfirmMessage ("Warning", "Are you sure you want to delete " + thisFileName + "?", "Cancel", "Delete");
						while (showMessageWindow) {
							yield return null;
						}
						if (!confirm) {
							continue;
						}

//						try {
							File.Delete (files [i]);
//						} catch (err) {
//							ShowError (err.Message);
//							GetCurrentFileInfo ();
//							selectFileInProgress = false;
//							return;
//						}
					}
				}
				GetCurrentFileInfo ();
				selectFileInProgress = false;
			}
		}
	}

	private List<string> GetMultiFileNames () {
		var files = new List<string>();
		for (var i = 0; i < multiFileList.Count; i++) {
			if (multiFileList[i] >= dirList.Count) {
				files.Add (filePath + fileList[multiFileList[i] - dirList.Count].name);
			}
		}
		return files;
	}

	private IEnumerator SelectFile () {
		if (!(showMessageWindow || selectFileInProgress)) {

			// If user opened a folder, change directories
			if (selectedFileNumber >= 0 && selectedFileNumber < dirList.Count) {
				filePath += dirList [selectedFileNumber].name + pathChar;
				if (showVolumes && selectedFileNumber < numberOfVolumes) {
					if (windowsSystem) {
						filePath = dirList [selectedFileNumber].name + pathChar + pathChar;
					} else if (linuxSystem) {
						filePath = dirList [selectedFileNumber].name + pathChar;
					} else {
						filePath = "/Volumes/" + dirList [selectedFileNumber].name + pathChar;
					}
				}
				GetCurrentFileInfo ();
			} else {
				// Do nothing if there's no file name, or if saving and no real filename has been supplied
				if (!((fileType != FileType.Save && ((!_allowMultiSelect && fileName == "") || (_allowMultiSelect && multiFileList.Count < 1))) ||
				    (fileType == FileType.Save && autoAddExtension && fileName == addedExtension))) {
				
					selectFileInProgress = true;
					var thisFileName = fileName;	// Make sure to keep the file name as it was when selected, since it can change later
					// Check for duplicate file name when saving
					if (fileType == FileType.Save) {
						for (var i = 0; i < fileList.Count; i++) {
							if (fileList [i].name == fileName) {
								ShowConfirmMessage ("Warning", "A file with that name already exists. Are you sure you want to replace it?", "Cancel", "Replace");
								while (showMessageWindow) {
									fileName = thisFileName;
									yield return null;
								}
								if (!confirm) {
									selectFileInProgress = false;
									break;
								}
							}
						}
					}

					Debug.Log ("Select file in progress: "+selectFileInProgress);

					if (selectFileInProgress) {
						selectFileInProgress = false;

						// If user selected a name, load/save that file
						if (fileType == FileType.Open || fileType == FileType.Save) {
							CloseFileWindow ();
							if (fileType == FileType.Open && _allowMultiSelect) {
								OnMultiFilesOpened (GetMultiFileNames ().ToArray ());
							} else {
								OnFileOpened (filePath + thisFileName);
							}
							if (fileType == FileType.Save) {
								GetCurrentFileInfo ();	// Refresh with new file in case of error
							}
						}
					}
				}
			}
		}
	}

	class RefBool {
		public bool b ;
		public RefBool (bool b) {
			this.b = b;
		}
	}

	class RefInt {
		public int i;
		public RefInt (int i) {
			this.i = i;
		}
	}

	class Popup {
		public static int popupListHash = "PopupList".GetHashCode();

		public static bool List (Rect position , RefBool showList , RefInt listEntry, GUIContent buttonContent, GUIContent[] listContent,
			GUIStyle listStyle) {
			return List (position, showList, listEntry, buttonContent, listContent, "button", "box", listStyle);
		}

		public static bool List (Rect position, RefBool showList, RefInt listEntry, GUIContent buttonContent, GUIContent[] listContent,
			GUIStyle buttonStyle, GUIStyle boxStyle, GUIStyle listStyle) {
			int controlID = GUIUtility.GetControlID (popupListHash, FocusType.Passive);
			bool done = false;
			switch (Event.current.GetTypeForControl (controlID)) {
			case EventType.MouseDown:
				if (position.Contains (Event.current.mousePosition)) {
					showList.b = true;
				}
				break;
			case EventType.MouseUp:
				if (showList.b) {
					done = true;
				}
				break;
			}

			GUI.Label (position, buttonContent, buttonStyle);
			if (showList.b) {
				var listRect = new Rect(position.x, position.y, position.width, listStyle.CalcHeight (listContent[0], 1.0f)*listContent.Length);
				GUI.Box (listRect, "", boxStyle);
				listEntry.i = GUI.SelectionGrid (listRect, listEntry.i, listContent, 1, listStyle);
			}
			if (done) {
				showList.b = false;
			}
			return done;
		}
	}

	class FileData {
		public string name;
		public System.DateTime date; 

		public FileData (string name, System.DateTime date) {
			this.name = name;
			this.date = date;
		}
	}

}