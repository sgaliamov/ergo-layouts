module Models

open System.Collections.Concurrent

type Char = char
type Letter = char
type Chars = ConcurrentDictionary<Char, int>
type Letters = ConcurrentDictionary<Letter, int>
type Digraph = string
type Digraphs = ConcurrentDictionary<Digraph, int>

type State =
    { Letters: Letters
      Digraphs: Digraphs
      Chars: Chars
      TotalLetters: int
      TotalDigraphs: int
      TotalChars: int }
