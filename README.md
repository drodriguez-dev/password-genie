# Password *Genie*rator
Solution to generate passwords using different algorithms: randomized, dictionary. The purpose of this project is to provide a way to generate easy to remember and type passwords but that are difficult to guess.

## Usage
To use the console application, run the following command:
```bash
genpwd generate <strategy> [options]
```

### Strategies
- `random` - Generates passwords using random letters, numbers, and special characters.
- `dictionary` - Generates easy to remember and difficult to guess passwords using words from a dictionary file. The generated passwords will never include any of the words from the dictionary but fictional words using parts of the words from it.

## Options
These options can be passed as command line arguments when running the console application. Depending on the strategy, some options may not be available.

### Common Options
- `--NumberOfPasswords`, `-p`: Specifies the number of passwords to generate. Default value is `1`.
- `--Length`, `-l`: Specifies the length of the password (approximate). Default value is `12`.
- `--NumberOfNumbers`, `-n`: Specifies the number of numbers in the password. Default value is `1`.
- `--NumberOfSpecialCharacters`, `-s`: Specifies the number of special characters in the password. Default value is `1`.
- `--IncludeGroupSymbols`, `-sg`: Includes group symbols ('()[]{}<>') in the password. Default value is `true`.
- `--IncludeMarkSymbols`, `-sm`: Includes mark symbols ('!@#$%^*+=|;:\""?') in the password. Default value is `true`.
- `--IncludeSeparatorSymbols`, `-sp`: Includes separator symbols (' -_/\&,.') in the password. Default value is `true`.
- `--CustomSymbols`, `-sc`: Uses a custom set of symbols. All 'Include' options are ignored. Default value is an empty string.
- `--RemoveHighAsciiTable`, `-r`: Removes characters of the high ASCII table (128-255) from the password. Default value is `false`.

### Random Strategy Options
- `--NumberOfLetters`, `-c`: Specifies the number of letters in the password. Default value is `10`.

### Dictionary Strategy Options
- `--Dictionary`, `-d`: Specifies the dictionary file path to use for generating the password.
- `--NumberOfWords`, `-w`: Specifies the number of words for generating the password. Default value is `2`.
- `--AverageWordLength`, `-wl`: Specifies the average word length in the password. Default value is `6`.
- `--DepthLevel`, `-wd`: Specifies the depth level for the word generation. Default value is `3`.
- `--KeystrokeOrder`, `-ko`: Specifies the keystroke order for the word generation. Default value is `Random`.
  - `Random`: Keystrokes are generated in a random order.
  - `Alternating`: Keystrokes are generated in an alternating pattern between left and right sides of the keyboard.
  - `AlternatingWord`: Keystrokes are generated in an alternating pattern between left and right sides of the keyboard after each word.
  - `OnlyLeft`: Keystrokes are generated only from the left side of the keyboard.
  - `OnlyRight`: Keystrokes are generated only from the right side of the keyboard.

When the same hand is used for a word, the keystrokes will avoid using the same finger for consecutive characters.

## Examples
### Random Strategy
```bash
./genpwd generate random -p 5 -l 16 -n 2 -s 2 -c 12
```
```
W}7IIrnPltDNV8D&
X8TmC\sKWRnV6,jb
M\iVXLbw0trVB9d)
bpCpV;|SSaW6xYF0
HjcAwh}_9uKVLr9d
```
### Dictionary Strategy
```bash
./genpwd generate dictionary -d /usr/share/dict/words -p 5 -w 3 -wl 8 -wd 2
```
```
Zuilkyu%EyipmkThwynao6
Blyc[TimphilyclPnxinu1
Luhupmk"IulipDjiulilih5
Xrewe%AgdseavdYnarqerabe3
Dypoilihril|IopilkPheohm5
```
### Author's favorite

```bash
./genpwd generate dictionary -d /usr/share/dict/words -p 10 -l 12 -w 2 -n 1 -s 1 -sc "-." -wl 6 -wd 4 -ko AlternatingStroke
```
```
Vorham-Peytor9
Skryen-Clayshf0
Naotocl.Iantheu9
Yakaneh-Lemanakt0
Thwitoth-Nahant9
Henbibi-Ovispr1
Qiblahs.Bkbndo2
Ovibot.Thwitl0
Vocodp-Oghamale8
Qiblauwi-Leuchan2
```

# About the author
I am a self-taught software developer with a passion for creating useful applications that help people with recurrent tasks that can be done by a machine.

# Road map
- [X] Create the base for a N-layer solution.
- [X] Implement the data access layer for the dictionary (text only).
- [X] Implement the Random strategy.
- [X] Implement the Dictionary strategy.
- [X] Implement the unit tests for the logic layer.
- [X] Implement the console application.
- [ ] Document the solution.
- [ ] Implement the CI pipeline (+ SonarQube)
- [ ] Implement the web application (GitHub Pages).
- [ ] Implement the CD pipeline (if necessary).
