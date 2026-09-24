@echo off
rem Build the decoy and the toggle switch with the .NET Framework compiler.
set CSC=C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe
"%CSC%" /nologo /target:winexe /out:Cyberpunk2077.exe dummy.cs
"%CSC%" /nologo /target:winexe /out:DSSwitch.exe dsswitch.cs /r:System.Windows.Forms.dll /r:System.Drawing.dll
echo.
echo Build done: Cyberpunk2077.exe + DSSwitch.exe
