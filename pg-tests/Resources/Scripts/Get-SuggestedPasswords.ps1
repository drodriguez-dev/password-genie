# This is a PowerShell script to generate a list of suggested passwords for a user.
. ..\..\..\pg-console-genpwd\bin\Debug\net8.0\genpwd.exe generate dictionary -d ".\..\Dictionaries\words_alpha_enUS.txt" -p 10 -l 12 -w 2 -n 1 -s 1 -sc "-" -wl 6 -wd 4 -r

Read-Host -Prompt "Press Enter to continue..."