# This is a PowerShell script to generate a list of suggested passwords based on a dictionary.
Write-Host "Generating passwords using random strategy..."
. ..\..\..\pg-console-genpwd\bin\Debug\net8.0\genpwd.exe generate random -p 5 -l 16 -n 2 -s 2 -c 12 --Verbose

Write-Host "Generating passwords using dictionary strategy based on a plain text file (en-US), 2 words, length 6, depth 4, random strokes..."
. ..\..\..\pg-console-genpwd\bin\Debug\net8.0\genpwd.exe generate dictionary -d ".\..\..\bin\Debug\net8.0\Resources\Dictionaries\words_alpha_enUS.txt" -p 10 -w 2 -n 1 -s 1 -sc "-." -wl 6 -wd 4 --Verbose

Write-Host "Extracting word tree from plain text file (es-ES)..."
. ..\..\..\pg-console-genpwd\bin\Debug\net8.0\genpwd.exe extract plain -i ".\..\..\bin\Debug\net8.0\Resources\Dictionaries\words_alpha_esES.txt" -o ".\..\..\bin\Debug\net8.0\Resources\Dictionaries\word_tree_esES_test.dat.gz" --Overwrite

Write-Host "Generating passwords using dictionary strategy based on a word tree file (es-ES), 3 words, length 8, depth 4, alternating strokes..."
. ..\..\..\pg-console-genpwd\bin\Debug\net8.0\genpwd.exe generate dictionary -wt ".\..\..\bin\Debug\net8.0\Resources\Dictionaries\word_tree_esES_test.dat.gz" -p 5 -w 2 -wl 7 -wd 4 -ko AlternatingStroke --Verbose

Read-Host -Prompt "Press Enter to continue ::"