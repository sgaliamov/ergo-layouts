module Main

open System
open System.IO
open System.Collections.Generic
open System.Text
open System.Threading
open Calculations
open Configs

type StringBuilder with
    member sb.AppendPair (key, value, total) =
        let div x y =
            match y with
            | 0 -> 0.0
            | _ -> (float x) / (float y)
        let getValue value total = 100.0 * div value total
        sb.AppendFormat("{0} : {1:0.####}\t", key, (getValue value total))
    member sb.AppendLines<'T> (pairs: seq<KeyValuePair<'T, int>>) total =
        pairs
        |> Seq.sortByDescending (fun pair -> pair.Value)
        |> Seq.fold (fun (sb: StringBuilder, i) pair ->
            if i % 5 = 0 && i <> 0 then
                sb.AppendLine() |> ignore
            (sb.AppendPair(pair.Key, pair.Value, total), i + 1)) (sb, 0)
        |> ignore
        sb
    member sb.Append (pairs, total) = sb.AppendLines pairs total

let calculate path search (cts: CancellationTokenSource) = async {
    let yieldLines (token: CancellationToken) filePath = seq {
        use stream = File.OpenText filePath
        while not stream.EndOfStream && not token.IsCancellationRequested do
            yield stream.ReadLine() } // todo: use async
    
    let print state =
        let symbolsOnly = state.chars |> Seq.filter (fun x -> Char.IsPunctuation(x.Key))
        StringBuilder()
            .AppendFormat("Letters: {0}\n", state.totalLetters)
            .Append(state.letters, state.totalLetters)
            .AppendFormat("\n\nSymbols from total: {0}\n", state.totalChars)
            .Append(symbolsOnly, state.totalChars)
            .AppendFormat("\n\nDigraphs {0}:\n", state.totalDigraphs)
            .Append(state.digraphs, state.totalDigraphs)
        |> Console.WriteLine
    
    let folder (cts: CancellationTokenSource) state next =
        let newState = aggregator state next
        let digraphsFinished = isFinished newState.digraphs stats.digraphs newState.totalDigraphs stats.precision
        let lettersFinished = isFinished newState.letters stats.letters newState.totalLetters stats.precision
        if digraphsFinished && lettersFinished then cts.Cancel()
        newState

    Directory.EnumerateFiles(path, search, SearchOption.AllDirectories)
    |> Seq.filter (fun _ -> not cts.IsCancellationRequested)
    |> Seq.map (yieldLines cts.Token)
    |> Seq.map calculateLines // todo: can run in pararllel
    |> Seq.fold (folder cts) stateSeed
    |> print }
