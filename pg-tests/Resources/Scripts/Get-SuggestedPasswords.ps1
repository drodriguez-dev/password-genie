# This is a PowerShell script to generate a list of suggested passwords based on a dictionary.
. ..\..\..\pg-console-genpwd\bin\Debug\net8.0\genpwd.exe generate random -p 5 -l 16 -n 2 -s 2 -c 12

. ..\..\..\pg-console-genpwd\bin\Debug\net8.0\genpwd.exe generate dictionary -wt ".\..\..\bin\Debug\net8.0\Resources\Dictionaries\word_tree_enUS.dat.gz" -p 5 -w 3 -wl 8 -wd 4

. ..\..\..\pg-console-genpwd\bin\Debug\net8.0\genpwd.exe generate dictionary -d ".\..\..\bin\Debug\net8.0\Resources\Dictionaries\words_alpha_enUS.txt" -p 10 -w 2 -n 1 -s 1 -sc "-." -wl 6 -wd 4 -ko AlternatingStroke

Read-Host -Prompt "Press Enter to continue ::"