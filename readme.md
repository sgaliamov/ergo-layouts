# Utilities for ErgoDox EZ layouts

## edLayout

Is a utility to examine layouts for ErgoDox keyboard.

[TBD] link to the post.

## edText

A utility that helps to minify a sampling texts without loosing statistical characteristic's of digraphs.

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
