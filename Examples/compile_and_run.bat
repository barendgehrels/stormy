@echo off

goto quickstart4

:quickstart1
echo ---------------------------------------------------------------------------
echo QuickStart 1
echo ---------------------------------------------------------------------------
del QuickStart1.exe > nul
csc quickstart1.cs ..\Stormy.cs
QuickStart1.exe


:quickstart2
echo ---------------------------------------------------------------------------
echo QuickStart 2
echo ---------------------------------------------------------------------------
del QuickStart2.exe > nul
csc quickstart2.cs ..\Stormy.cs /nologo /reference:"c:\Program Files (x86)\Microsoft SQL Server\100\SDK\Assemblies\Microsoft.SqlServer.Types.dll"
QuickStart2.exe


:quickstart3
echo ---------------------------------------------------------------------------
echo QuickStart 3
echo ---------------------------------------------------------------------------
del QuickStart3.exe > nul
csc quickstart3.cs ..\Stormy.cs /nologo /reference:"c:\Program Files (x86)\System.Data.SQLite\2010\bin\System.Data.SQLite.dll" /lib:"c:\Program Files (x86)\System.Data.SQLite\2010\bin"
:: This currently compiles but does not run - it does not find the SQLite assembly...
:: QuickStart3.exe

:quickstart4
echo ---------------------------------------------------------------------------
echo QuickStart 4
echo ---------------------------------------------------------------------------
del QuickStart4.exe > nul
csc quickstart4.cs ..\Stormy.cs
QuickStart4.exe

