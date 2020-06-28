module Main

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Reactive.Subjects
open System.Reactive.Linq
open System.Text
open System.Threading
open FSharp.Collections.ParallelSeq
open Calculations
open Configs
open KeyboardModelds
open StateModels

let private spacer = new string(' ', Console.WindowWidth)

let private appendLines<'T> (pairs: seq<KeyValuePair<'T, int>>) total minValue (builder: StringBuilder) =
    let appendPair (sb: StringBuilder, i) (key, value) =
        if i % settings.columns = 0 then sb.AppendLine().Append("    ") |> ignore
        sb.AppendFormat("{0,-2} : {1,-10:0.###}", key, value), i + 1
    let getValue value =
        let div x y =
            match y with
            | 0 -> 0.0
            | _ -> float x / float y
        100.0 * div value total
    pairs
    |> Seq.map (fun pair -> pair.Key, getValue pair.Value)
    |> Seq.filter (fun (_, value) -> value >= minValue)
    |> Seq.sortByDescending (fun (_, value) -> value)
    |> Seq.fold appendPair (builder, 0)
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

    let appendValue (title: string) value (builder: StringBuilder) =
        let format = sprintf "\n{0}: {1,-%d:0,0.00}" (settings.columns * 15 - title.Length - 2)
        builder.AppendFormat(format, title, value)
        
    let yieldLines (token: CancellationToken) filePath = seq {
        use stream = File.OpenText filePath
        while not stream.EndOfStream && not token.IsCancellationRequested do
            // todo: use async
            yield stream.ReadLine() }

    let percentFromTotal total value = (100. * float value / float total)

    let formatMain state (builder: StringBuilder) =
        let percentFromTotal = percentFromTotal state.TotalChars
        builder
        |> appendValue "Left fingers" (state.LeftFingers.Values.Sum())
        |> appendLines state.LeftFingers state.TotalChars 0.0
        |> appendValue "Right fingers" (state.RightFingers.Values.Sum())
        |> appendLines state.RightFingers state.TotalChars 0.0
        |> appendValue "Same finger" (percentFromTotal state.SameFinger)
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
        |> appendValue "Distance" state.Distance
        |> appendValue "Result" state.Result

    let formatState state =
        let digraphs = state.Digraphs.ToDictionary((fun x -> Digraph.value x.Key), (fun y -> y.Value))
        let characters = state.Chars.ToDictionary((fun x -> Character.value x.Key), (fun y -> y.Value))
        let letters = state.Letters.ToDictionary((fun x -> Letter.value x.Key), (fun y -> y.Value))
        let builder = StringBuilder()
        if detailed then
            builder
            |> appendValue "Digraphs" state.TotalDigraphs
            |> appendLines digraphs state.TotalDigraphs settings.minDigraphs
            |> appendValue "Letters" state.TotalLetters
            |> appendLines letters state.TotalLetters 0.0
            |> appendValue "Characters" state.TotalChars
            |> appendLines characters state.TotalChars 0.0
            |> appendValue "Shifts" (percentFromTotal state.TotalChars state.Shifts)
            |> ignore
        builder
        |> formatMain state

    let onStateChanged state =
        let initialPosition = (Console.CursorLeft, Console.CursorTop)
        StringBuilder()
        |> formatMain state
        |> Console.Write
        Console.SetCursorPosition initialPosition

    use stateChangedStream = new Subject<State>()
    use subscription = stateChangedStream.Sample(TimeSpan.FromSeconds(0.500)).Subscribe onStateChanged

    let folder state next =
        let newState = aggregator state next
        if Probability.value settings.precision > 0. then
            let digraphsFinished = isFinished newState.Digraphs settings.digraphs newState.TotalDigraphs settings.precision
            let lettersFinished = isFinished newState.Letters settings.letters newState.TotalLetters settings.precision
            if digraphsFinished && lettersFinished then
                Console.SetCursorPosition(0, Console.CursorTop)
                Console.Write spacer
                printfn "\rCollected enough data."
                subscription.Dispose()
                cts.Cancel true
        stateChangedStream.OnNext newState
        newState

    let start = DateTime.UtcNow
    let keyboard = Keyboard.load <| Layout.Load layout

    Directory.EnumerateFiles(path, search, SearchOption.AllDirectories)
    |> Seq.takeWhile (fun _ -> not cts.IsCancellationRequested)
    |> PSeq.map (yieldLines cts.Token >> calculateLines keyboard)
    |> PSeq.fold folder initialState
    |> formatState
    |> Console.WriteLine

    Ok (sprintf "Time spent: %s" ((DateTime.UtcNow - start).ToString("c")))
