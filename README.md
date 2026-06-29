# BookVault – Create Installer (Inno Setup)

## 1. Publish project

Run in the project folder:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

**Alternatively:**
Open the project in VisualStudio -> right-click on project file -> "publish" -> new profile:
```
Configuration: Release|Any CPU
Target Framework: net8.0
Deployment mode: Self-contained
Target Runtime: win-x64
Target location: \bin\release\net8.0\publish\win-x64

File publish options:
  Produce single file
```

-> Output:
```
bin\Release\net8.0\publish\win-x64\
```
Required: BookVault.exe and e_sqlite3.dll

## 2. Install Inno Setup
Download:
https://jrsoftware.org/isdl.php

## 3. Inno Setup Script (BookVault.iss)
Create BookVault.iss in the BaseFolder
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

## 4. Create Installer
- Open script in Inno Setup
- Click "Compile"
--> Result:
  BookVault-Setup.exe
