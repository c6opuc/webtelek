; The name of the installer
Name "WEBTELEK+ frontend plugin"

; The file to write
OutFile "webtelek+.exe"

; The default installation directory
InstallDir "$PROGRAMFILES\Team MediaPortal\MediaPortal"


; Pages

Page directory
Page instfiles

; The stuff to install
Section "" ;No components page, name is not important


  ; Set output path to the installation directory.
  SetOutPath $INSTDIR
  
  ; Put file there
  File /r "[mediaportal root]\*.*"
  
SectionEnd ; end the section
