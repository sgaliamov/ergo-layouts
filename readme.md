# Utilities for ErgoDox EZ

## edLayout

Is a utility to examine layouts for ErgoDox keyboard.

``` pwsh
❯ .\edLayout.exe -i .\samples -l .\layouts\qwerty.json -sp false -o .\publish\edLayout\results.csv

Usage:
  edLayout [options]

Options:
  -i, --input <input>        Path to a directory with sampling texts.
  -p, --pattern <pattern>    Pattern to filter sampling files. [default: *.txt]
  -l, --layout <layout>      Path to a layout file.
  -o, --output <output>      Path to the output file.
  -sp, --show-progress       [default: True]
  -d, --detailed             Show sample texts statistics.
  --version                  Show version information
  -?, -h, --help             Show help and usage information
```

More details you can fined [here](https://sgaliamov.medium.com/evaluating-keyboard-layouts-for-ergodox-ez-cf70042c4865).

## edText

A utility that helps to minify sampling texts without loosing statistical characteristic's of digraphs.

In case when you have a lot of texts for some algorithm, you can save some time running it on smaller version of those texts which have same quality.

``` pwsh
edText.exe .\samples 0.01 .\results 4
```

### Parameters

1. `.\samples` path to a folder with original texts. It should be `txt` files.
1. `0.01` target precision.
1. `.\results` output folder.
1. `4` defines the way how to split the set of lines in the algorithm.

### Algorithm

1. Load all texts.
1. Shuffle the current set of lines.
1. If shuffled 10 times already, save text and exit.
1. Split into 2 parts using the last input parameter.
1. Calculate a score for both parts.
1. If both bad according chosen precision, go to 2.
1. Get the best part.
1. Go to 3.
