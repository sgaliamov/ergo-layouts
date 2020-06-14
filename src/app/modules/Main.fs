module Main

open System
open System.IO
open System.Collections.Generic
open System.Text
open System.Threading
open Calculations
open Configs

type StringBuilder with
    member sb.AppendPair (key, value) = sb.AppendFormat("{0,-2} : {1,-10:0.###}", key, value)

    member sb.AppendLines<'T> (pairs: seq<KeyValuePair<'T, int>>) total filter =
        let getValue value =
            let div x y =
                match y with
                | 0 -> 0.0
                | _ -> (float x) / (float y)
            100.0 * div value total

        pairs
        |> Seq.map (fun pair -> ({| Key = pair.Key; Value = getValue pair.Value |}))
        |> Seq.filter (fun pair -> pair.Value >= filter)
        |> Seq.sortByDescending (fun pair -> pair.Value)
        |> Seq.fold (fun (sb: StringBuilder, i) pair ->
            if i % settings.columns = 0 && i <> 0 then sb.AppendLine() |> ignore
            (sb.AppendPair(pair.Key, pair.Value), i + 1)) (sb, 0)
        |> ignore
        sb

    member sb.Append (pairs, total, filter) = sb.AppendLines pairs total filter

let calculate path search (cts: CancellationTokenSource) = async {
    let yieldLines (token: CancellationToken) filePath = seq {
        use stream = File.OpenText filePath
        while not stream.EndOfStream && not token.IsCancellationRequested do
            yield stream.ReadLine () } // todo: use async
    
    let print state =
        let symbolsOnly = state.Chars |> Seq.filter (fun x -> Char.IsPunctuation(x.Key))
        StringBuilder()
            .AppendFormat("Letters: {0}\n", state.TotalLetters)
            .Append(state.Letters, state.TotalLetters, 0.0)
            .AppendFormat("\n\nSymbols from total: {0}\n", state.TotalChars)
            .Append(symbolsOnly, state.TotalChars, 0.0)
            .AppendFormat("\n\nDigraphs {0}:\n", state.TotalDigraphs)
            .Append(state.Digraphs, state.TotalDigraphs, 0.05)
        |> Console.WriteLine
    
    let folder state next =
        let newState = aggregator state next
        let digraphsFinished = isFinished newState.Digraphs settings.digraphs newState.TotalDigraphs settings.precision
        let lettersFinished = isFinished newState.Letters settings.letters newState.TotalLetters settings.precision
        if digraphsFinished && lettersFinished then cts.Cancel()
        newState

    Directory.EnumerateFiles(path, search, SearchOption.AllDirectories)
    |> Seq.filter (fun _ -> not cts.IsCancellationRequested)
    |> Seq.map (yieldLines cts.Token)
    |> Seq.map calculateLines // todo: can run in pararllel
    |> Seq.fold folder initialState
    |> print }
