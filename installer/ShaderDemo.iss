#define MyAppName "ShaderDemo"
#define MyAppVersion "0.1.0"
#define MyAppPublisher "Patrick JAILLET"
#define MyAppExeName "ShaderDemo.exe"
#define MyPublishDir "..\publish"

[Setup]
AppId={{B37D6C64-9C1E-4C6C-9F1B-9E8F6C6B6E7D}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog commandline
UninstallDisplayIcon={app}\{#MyAppExeName}
OutputDir=.\output
OutputBaseFilename=ShaderDemo-Setup-{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
SetupIconFile=..\assets\icon.ico
DisableProgramGroupPage=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Create a &desktop icon"; GroupDescription: "Additional icons:"; Flags: unchecked

[Files]
Source: "{#MyPublishDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyPublishDir}\*.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "{#MyPublishDir}\shaders\*"; DestDir: "{app}\shaders"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall {#MyAppName}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}\shaders"
Type: files; Name: "{app}\settings.json"
Type: files; Name: "{app}\layers.json"
Type: files; Name: "{app}\timeline.json"
Type: files; Name: "{app}\presets.json"
Type: files; Name: "{app}\imgui.ini"
