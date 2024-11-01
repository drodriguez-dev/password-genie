# Password *Genie*rator
Solution to generate passwords using different algorithms: randomized, dictionary. The purpose of this project is to provide a way to generate easy to remember and type passwords but that are difficult to guess.

## Generation usage
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
D4,5e-iPhGxYlXzh
fL^63/vFvNKBwKzU
Pq,g-q88QBJXOnJV
LnT,iA8)4LfXjeSe
p06}eb@iOMuEnfIy
```
### Dictionary Strategy
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
### Author's favorite

```bash
./genpwd generate dictionary -d /usr/share/dict/words -p 10 -l 12 -w 2 -n 1 -s 1 -sc "-." -wl 6 -wd 4 -ko AlternatingStroke
```
```
Zoeheu.Gneu7
Yclepe.Ndod5
Hsuant.Jayan7
Dodoel-Zizy4
Blamsi.Epen4
Yblentor-Iwis5
Jahantix-Udog5
Giti-Zytheo4
Iantheya-Mzu9
Flanchau-Tnt5
```

## Extraction usage
With this application it's possible to create a word tree file to use it later in the generation process. Word tree files could be smaller than dictionary files, they contain a tree structure of letters with all of the possible letter sequences extracted from the dictionary.

### Options
- `--Input`, `-i`: File path for the dictionary file.
- `--Output`, `-o`: File path for the generated word tree file.
- `--Overwrite`, `-ow`: Overwrite the output file if it already exists.

```bash
./genpwd extract plain -i /usr/share/dict/words -o /home/user/pass-genie/word-tree.dat.gz
./genpwd generate dictionary -wt /home/user/pass-genie/word-tree.dat.gz -p 10 -l 12 -w 2 -n 1 -s 1 -sc "-." -wl 6 -wd 4 -ko AlternatingStroke
```

# The Solution
## Architecture
This is a **n-layer** solution that provides a way to generate passwords using different strategies. The solution is divided into the following layers:
- **Crosscutting Layer**: Provides the common classes (logic & [POCO](https://en.wikipedia.org/wiki/Plain_old_CLR_object)) for the solution. Only non-specific classes are included in this layer.
  - *pg-shared*: This project contains the common classes for the solution.
  - *pg-entities*: This project contains the entities for the solution. Only the entities that must traverse all the layers are included here.
- **Data Access layer**: Provides the data access to the dictionary file. Its main purpose is to read the data requested by the logic layer.
  - *pg-data-files*: Data access methods for managing files (dictionaries, word trees, ...). Any other dictionary type can be implemented here.
- **Logic layer**: This layer contains only logic, external data and interactions are handled by the interface layer.
  - *pg-logic*: This project contains the main logic to generate the passwords. It's divided into the following sub-layers:
    - Generators: Contains the classes to generate the passwords using different strategies. New strategies can be implemented here.
    - Loaders: Contains the classes to load the data from the data access layer. The data layer is responsible to provide a list of words, and the logic layer will use them to create the necessary structure to generate the passwords. This structure is loaded only once and kept in memory.
- **Interface layer**: This layer is responsible for the communication between the external world and the logic layer. It will convert external data and interactions into entities and actions that the logic layer can understand. It will also convert the results from the logic layer into a format that the external world can understand.
  - *pg-interface-cmd*: Provides the command line parsing for the console application and defines the commands and options available. It will also handle the exceptions and pass a human-readable message to the caller.
  - *pg-wasm-pwdgen*: Web application that acts as an interface between the user and the logic layer.
- **Deployables**: This layer contains the final projects to be build. 
	- *pg-console-genpwd*: Entry point for the genpwd command. Provides the console application to generate the passwords and outputs the messages to the user. Any other console commands can be implemented here in the future.
- **Tests**: Provides the unit tests for the logic layer. This project is also divided into the same layers as the main solution.

Every layer contains its own specific exceptions to handle custom errors. Exceptions are only thrown for exceptional cases, expected cases are handled by the return values. Exceptions are catched in the layer that can handle them and rethrown (or not catched) if necessary. Rethrowning a new exception is allowed to provide more context to the caller, but always including the original exception as the inner exception.

## Strategies
### Random Strategy
This strategy generates passwords using random letters, numbers, and special characters. The generated passwords will have the specified number of letters, numbers, and special characters. Uses `System.Random` to generate the random characters based on the options provided. Depending on the type of character:
- Letters: Random letters from the alphabet (ASCII 65-90, 97-122).
- Numbers: Random numbers from 0 to 9 (ASCII 48-57).
- Special characters: Random special characters from the ASCII table (32-47, 58-64, 91-93, 95, 123-125).
  - Group symbols: `()[]{}<>`
  - Mark symbols: `!@#$%^*+=|;:\"'?`
  - Separator symbols: `-_ /\&,` *(space is included here)*

### Dictionary Strategy
This strategy consists of a tree structure of letters that will be used to generate the words. The loader will build the tree structure using the words from the dictionary file. Every letter (node) will have a list of possible next letters, and the tree will be traversed to generate the words. The depth level defines how many letters will be traversed and then, the next letter will be chosen randomly from the list of possible next letters. The next letter will be chosen based on the keystroke order.

## Entropy
The [password entropy]([url](https://en.wikipedia.org/wiki/Password_strength)) is a measure of the password strength. The higher the entropy, the stronger the password. Because of the nature of the strategies, the entropy is calculated based on the number of options on each decision point. For example, in a two letters password and one number, the entropy should be calculated as `log2(52^2 x 10)`; but, because the characters are then shuffled the entropy is `log2(52^2 x 10 x 2 x 1)`.

Additionally, because the length of the passwords is variable and the dictonary tree is traversed randomly, every password will have different entropy. The final value is calculated for the average length of the generated passwords. 

The entropy will be shown when the `--Verbose` option is used.

# Road map
Check open issues with the tag "[enhacement](issues?q=is%3Aissue+is%3Aopen+label%3Aenhancement+)".

# About the author
I am a self-taught software developer with a passion for creating useful applications that help people with recurrent tasks that can be done by a machine. This project was inspired because I needed a way to generate secure passwords that were easy to remember and type. I've realized words are easier to remember than random characters, so I've decided to create a way to generate passwords using fictional words from a dictionary file.

I hope you find this project useful and that it helps you.
