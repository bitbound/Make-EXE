# Make-EXE
Easily compile your PowerShell or batch scripts into EXEs and embed data files (C# WPF).

### Instructions:
* After installing Make-EXE, there should be a "Make EXE" option when right-clicking PS1 or BAT files.
  * If the option is missing, reset your program defaults and reinstall Make-EXE.
  * To reset program defaults in Windows 10, go to Settings - System - Default apps.  The Reset button is at the bottom.
* You can also use Make-EXE from the command line by supplying the path to the script as the only argument.
* The EXE will be created in the same directory as the script.

### Embedding Files:
When compiling a script to EXE, you'll be asked if you want to embed files into the EXE.  This will allow you to include data files and other resources that your script calls.

If you select yes, all files in the same directory as the script file will be included (no subdirectories).  When the EXE is run, the embedded files will be availabe.  Your script must call these files from the current working directory.  Do not change your working directory within the script if you wish to call these files afterward.

### Icons:
You can set a custom icon for your EXE by putting the ICO file in the same directory as your script and choosing yes to embed files.  Make sure there aren't other files in the script's directory if you don't want to embed them!
