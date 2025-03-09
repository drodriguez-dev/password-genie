# Password *Genie*rator
Solution to generate passwords using different algorithms: randomized, dictionary. The purpose of this project is to provide a way to generate easy to remember and type passwords but that are difficult to guess.

## Generation usage
The application can generate passwords using two strategies: Random and Dictionary. The Random strategy generates passwords using random characters, while the Dictionary strategy uses words from a dictionary file.

Usage:
```bash
genpwd generate <strategy> [options]
```

  - Random Strategy:
```bash
./genpwd generate random -p 5 -l 16 -n 2 -s 2 -c 12
```
```
D4,5e-iPhGxYlXzh
fL^63/vFvNKBwKzU
Pq,g-q88QBJXOnJV
LnT,iA8)4LfXjeSe
p06}eb@iOMuEnfIy
```
  - Dictionary Strategy
```bash
./genpwd generate dictionary -d /usr/share/dict/words -p 5 -w 3 -wl 8 -wd 2
```
```
Devu+PloltrierkbMseo9
Vssiveig"NhespobHf3
Lerndojiur(BmDbrldgavg4
Ukuarfs$PshfNactrtweri1
Xosluoel"XyzafPaml7
```

  - Author's favorite
```bash
./genpwd generate dictionary -d /usr/share/dict/words -p 5 -l 12 -w 2 -n 1 -s 1 -sc "-." -wl 6 -wd 4 -ko AlternatingStroke
```
```
Zoeheu.Gneu7
Yclepe.Ndod5
Hsuant.Jayan7
Dodoel-Zizy4
Blamsi.Epen4
```

## Extraction usage
With this application it's possible to create another word tree files to use it later in the generation process. This is usefull if the current version does not have your desired language or you prefeer to use another set of words.

Usage:
```bash
./genpwd extract plain -i /usr/share/dict/words -o /home/user/pass-genie/word-tree.dat.gz
./genpwd generate dictionary -wt /home/user/pass-genie/word-tree.dat.gz -p 10 -l 12 -w 2 -n 1 -s 1 -sc "-." -wl 6 -wd 4 -ko AlternatingStroke
```
# Documentation
More information can be found in the [Wiki](https://github.com/drodriguez-dev/password-genie/wiki) section.

# Road map
Check open issues with the tag "[enhacement](https://github.com/drodriguez-dev/password-genie/issues?q=is%3Aissue%20state%3Aopen%20label%3Aenhancement)".

# About the author
I am a self-taught software developer with a passion for creating useful applications that help people with recurrent tasks that can be done by a machine. This project was inspired because I needed a way to generate secure passwords that were easy to remember and type. I've realized words are easier to remember than random characters, so I've decided to create a way to generate fictional words from a dictionary file and use them as passwords.

I hope you find this project useful and that it helps you.