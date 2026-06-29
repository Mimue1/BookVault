# BookVault – Installer erstellen (Inno Setup)

## 1. Projekt publishen

Im Projektordner ausführen:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

**Alternativ:** 
Projekt in VisualStudio öffnen -> rechtsklick auf projekt datei -> "publish" -> neues profil:
```
Configuration: Release|Any CPU
Target Framework: net8.0
Deployment mode: Self-contained
Target Runtime: win-x64
Target location: \bin\release\net8.0\publish\win-x64

File publich options:
  Produce single file
```

-> Output:
```
bin\Release\net8.0\publish\win-x64\
```
Benötigt wird BookVault.exe und e_sqlite3.dll

## 2. Inno Setup installieren
Download:
https://jrsoftware.org/isdl.php

## 3. Inno Setup Script (BookVault.iss)
BookVault.iss im BaseFolder erstellen
```ini
[Setup]
AppName=BookVault
AppVersion=1.0.0
DefaultDirName={pf}\BookVault
DefaultGroupName=BookVault
OutputBaseFilename=BookVault-Setup
Compression=lzma
SolidCompression=yes

[Files]
Source: "bin\Release\net8.0\publish\win-x64\*"; DestDir: "{app}"; Flags: recursesubdirs

[Icons]
Name: "{group}\BookVault"; Filename: "{app}\BookVault.exe"
Name: "{commondesktop}\BookVault"; Filename: "{app}\BookVault.exe"

[UninstallDelete]
Type: filesandordirs; Name: "{localappdata}\BookVault"
```

## 4. Installer erstellen
- Script in Inno Setup öffnen
- "Compile" klicken
--> Ergebnis:
  BookVault-Setup.exe 
