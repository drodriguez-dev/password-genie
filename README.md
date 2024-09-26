# Password *Genie*rator
Solution to generate passwords using different algorithms: randomized, dictionary. The purpose of this project is to provide a simple and easy to use console application to generate easy to remember but difficult to guess passwords.

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

## Examples
### Random Strategy
```bash
./genpwd generate random -p 5 -l 16 -n 2 -s 2 -c 12
```

### Dictionary Strategy
```bash
./genpwd generate dictionary -d /usr/share/dict/words -p 5 -w 3 -wl 8 -wd 2
```