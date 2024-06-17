F9::
{
	DetectHiddenWindows, On
	if !WinExist("ahk_exe SteamAchievmentViewer.exe")
	{
		run YourPathToSteamAchievmentViewer.exe
		visible := true 
		Sleep, 2000 
		WinSet, AlwaysOnTop, On, ahk_exe SteamAchievmentViewer.exe
		return 
	}

	if (visible)
	{
		;WinHide, ahk_exe SteamAchievmentViewer.exe
		WinMinimize, Steam Achievement Viewer
		visible := false 
	}
	else 
	{
		;WinShow, ahk_exe SteamAchievmentViewer.exe
		WinRestore, Steam Achievement Viewer
		visible := true 
	}

	return
}