module Calculations

open System
open System.Collections.Concurrent
open System.Collections.Generic
open Utilities

type Digraph = char * char
type Letters = ConcurrentDictionary<char, int>
type Digraphs = ConcurrentDictionary<Digraph, int>
type Chars = ConcurrentDictionary<char, int>
type State = {
    letters: Letters
    digraphs: Digraphs
    chars: Chars
    totalLetters: int
    totalDigraphs: int
    totalChars: int }

type Counter<'TIn, 'TOut> = seq<'TIn> -> seq<'TOut * int>

let countLetters line =
    line 
    |> Seq.filter Char.IsLetter 
    |> Seq.countBy id

let countChars line =
    line
    |> Seq.countBy id

let countDigraphs line =
    line
    |> Seq.filter Char.IsLetter
    |> Seq.pairwise
    |> Seq.countBy id

let calculate<'TIn, 'TOut> line (counter: Counter<'TIn, 'TOut>) =
    let folder result (key, count) = addOrUpdate result key count (+)
    line
    |> counter
    |> Seq.fold folder (ConcurrentDictionary<'TOut, int>())

let collect line =
    let letters = calculate line countLetters
    let chars = calculate line countChars
    let digraphs = calculate line countDigraphs
    { 
        letters = letters
        digraphs = digraphs
        chars = chars
        totalLetters = letters.Values |> Seq.sum
        totalDigraphs = digraphs.Values |> Seq.sum
        totalChars = chars.Values |> Seq.sum
    }

let aggregator state from = {
    letters = sumValues from.letters state.letters
    digraphs = sumValues from.digraphs state.digraphs
    chars = sumValues from.chars state.chars
    totalLetters = from.totalLetters + state.totalLetters
    totalDigraphs = from.totalDigraphs + state.totalDigraphs
    totalChars = from.totalChars + state.totalChars }

let characters = HashSet<char>([' '..'~'] |> List.except ['A'..'Z'])

let calculateLines lines =
    let seed = {
        letters = Letters()
        digraphs = Digraphs()
        chars = Chars()
        totalLetters = 0
        totalDigraphs = 0
        totalChars = 0 }
    lines
    |> Seq.map (fun line -> line |> Seq.filter characters.Contains) 
    |> Seq.map collect
    |> Seq.fold aggregator seed
