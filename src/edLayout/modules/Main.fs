﻿module Main

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Reactive.Subjects
open System.Reactive.Linq
open System.Text
open System.Threading
open Calculations
open Configs
open KeyboardModelds
open StateModels

let private spacer = new string(' ', Console.WindowWidth)

let private appendLines<'T> (pairs: seq<KeyValuePair<'T, int>>) total minValue (builder: StringBuilder) =
    let appendPair (key, value) = builder.AppendFormat("{0,-2} : {1,-10:0.###}", key, value)
    let getValue value =
        let div x y =
            match y with
            | 0 -> 0.0
            | _ -> float x / float y
        100.0 * div value total

    builder.AppendLine() |> ignore
    pairs
    |> Seq.map (fun pair -> {| Key = pair.Key; Value = getValue pair.Value |})
    |> Seq.filter (fun pair -> pair.Value >= minValue)
    |> Seq.sortByDescending (fun pair -> pair.Value)
    |> Seq.fold (fun (sb: StringBuilder, i) pair ->
        match i % settings.columns = 0 with
        | true -> sb.AppendLine().Append("  "), i
        | _ -> appendPair(pair.Key, pair.Value), i + 1)
        (builder, 0)
    |> ignore
    builder

let calculate path search detailed (layout: string) (cts: CancellationTokenSource) =
    // todo: find better way to validate input parameters
    if not (Directory.Exists path) then
        cts.Cancel true
        Error "Samples direcotry does not exist."
    else if not (File.Exists layout) then
        cts.Cancel true
        Error "Layout file does not exist."
    else

    let appendSpacer (builder: StringBuilder) =
        builder.AppendFormat("\n{0}", spacer)

    let appendValue (title: string) value (builder: StringBuilder) =
        let format = sprintf "\n{0}: {1,-%d:0.###}" (settings.columns * 15 - title.Length - 2)
        builder.AppendFormat(format, title, value)
        
    let yieldLines (token: CancellationToken) filePath = seq {
        use stream = File.OpenText filePath
        while not stream.EndOfStream && not token.IsCancellationRequested do
            // todo: use async
            yield stream.ReadLine() }

    let formatMain state (builder: StringBuilder) =
        let percentFromTotal value = (100. * float value / float state.TotalChars)

        builder
        |> appendValue "Left fingers" (state.LeftFingers.Values.Sum())
        |> appendLines state.LeftFingers state.TotalChars 0.0
        |> appendValue "Right fingers" (state.RightFingers.Values.Sum())
        |> appendLines state.RightFingers state.TotalChars 0.0
        |> appendValue "Same finger" (percentFromTotal state.SameFinger)
        |> appendSpacer
        |> appendValue "Shifts" (percentFromTotal state.Shifts)
        |> appendValue "Top keys" (percentFromTotal state.TopKeys)
        |> appendValue "Home keys" (percentFromTotal state.HomeKeys)
        |> appendValue "Bottom keys" (percentFromTotal state.BottomKeys)
        |> appendValue "Inward rolls" (percentFromTotal state.InwardRolls)
        |> appendValue "Outward rolls" (percentFromTotal state.OutwardRolls)
        |> appendValue "Left hand" (percentFromTotal state.LeftHandTotal)
        |> appendValue "Right hand" (percentFromTotal state.RightHandTotal)
        |> appendValue "Left hand continuous" (percentFromTotal state.LeftHandContinuous)
        |> appendValue "Right hand continuous" (percentFromTotal state.RightHandContinuous)
        |> appendValue "Efforts" state.Efforts
        |> appendSpacer

    let formatState state =
        Console.SetCursorPosition(0, Console.CursorTop)
        let digraphs = state.Digraphs.ToDictionary((fun x -> Digraph.value x.Key), (fun y -> y.Value))
        let symbolsOnly =
            state.Chars.ToDictionary((fun x -> Character.value x.Key), (fun y -> y.Value))
            |> Seq.filter (fun x -> Char.IsPunctuation(x.Key) || x.Key = ' ')
        let letters = state.Letters.ToDictionary((fun x -> Letter.value x.Key), (fun y -> y.Value))
        let builder = StringBuilder()

        if detailed then
            builder
            |> appendValue "Digraphs" state.TotalDigraphs
            |> appendLines digraphs state.TotalDigraphs settings.minDigraphs
            |> appendValue "Letters" state.TotalLetters
            |> appendLines letters state.TotalLetters 0.0
            |> appendValue "Symbols from total" state.TotalChars
            |> appendLines symbolsOnly state.TotalChars 0.0
            |> ignore
        builder
        |> appendSpacer
        |> formatMain state
        |> appendSpacer

    let onStateChanged state =
        let initialPosition = (Console.CursorLeft, Console.CursorTop)
        StringBuilder()
        |> formatMain state
        |> appendSpacer
        |> Console.Write
        Console.SetCursorPosition initialPosition

    use stateChangedStream = new Subject<State>()
    use subscription = stateChangedStream.Sample(TimeSpan.FromSeconds(0.500)).Subscribe onStateChanged

    let folder state next =
        let newState = aggregator state next
        let digraphsFinished = isFinished newState.Digraphs settings.digraphs newState.TotalDigraphs settings.precision
        let lettersFinished = isFinished newState.Letters settings.letters newState.TotalLetters settings.precision
        if digraphsFinished || lettersFinished then
            Console.SetCursorPosition(0, Console.CursorTop)
            Console.Write spacer
            printfn "\rCollected enough data."
            subscription.Dispose()
            cts.Cancel true
        stateChangedStream.OnNext newState
        newState

    let start = DateTime.UtcNow
    let keyboard = Keyboard.load <| Layout.Load layout

    Directory.EnumerateFiles(path, search, SearchOption.AllDirectories).AsParallel()
    |> Seq.filter (fun _ -> not cts.IsCancellationRequested)
    |> Seq.map (yieldLines cts.Token >> calculateLines keyboard)
    |> Seq.fold folder initialState
    |> formatState
    |> Console.Write

    Ok (sprintf "Time spent: %s" ((DateTime.UtcNow - start).ToString("c")))
