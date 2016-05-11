# Tools-Windows-Build-HardCodedStringCheckerSharp
A program to find hard-coded strings in CSharp files.

To use you you specify the source directory to scan and whether to fix or report hard coded strings.  If you are unsure use
report.  Example:
HardCodedStringCheckerSharp.exe "E:\Git\WPFCommonControls" Report

By default it won't return failure unless you add the option --FailOnHCS
This will cause a failure exit code.
