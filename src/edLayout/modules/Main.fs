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

let private appendLines (pairs: seq<KeyValuePair<'TKey, 'Value>>) getValue minValue (builder: StringBuilder) =
    let appendPair (sb: StringBuilder, i) (key, value) =
        if i % settings.columns = 0 then sb.AppendLine().Append("  ") |> ignore
        sb.AppendFormat("{0,-2} : {1,-10:0.###}", key, value), i + 1
    pairs
    |> Seq.map (fun pair -> pair.Key, getValue pair.Value)
    |> Seq.filter (fun (_, value) -> value >= minValue)
    |> Seq.sortByDescending (fun (_, value) -> value)
    |> Seq.fold appendPair (builder, 0)
    |> ignore
    builder

let calculate samplesPath search detailed (layoutPath: string) (token: CancellationToken) cancel =
    // todo: find better way to validate input parameters
    if not (Directory.Exists samplesPath) then
        cancel ()
        Error "Samples direcotry does not exist."
    else if not (File.Exists layoutPath) then
        cancel ()
        Error "Layout does not exist."
    else

    let div x y =
        match y with
        | 0. -> 0.0
        | _ -> x / y

    let appendValue (title: string) value (builder: StringBuilder) =
        let format = sprintf "\n{0}: {1,-%d:0,0.00}" (settings.columns * 15 - title.Length - 2)
        builder.AppendFormat(format, title, value)
        
    let yieldLines filePath = seq {
        use stream = File.OpenText filePath
        while not stream.EndOfStream && not token.IsCancellationRequested do
            // todo: use async
            yield stream.ReadLine() }

    let notCancelled _ = not token.IsCancellationRequested

    let percentFromTotal total value = (100. * div value total)
    let percentFromTotalInt total value = percentFromTotal (float total) (float value)

    let formatMain state (builder: StringBuilder) =
        let percentFromTotalInt = percentFromTotalInt state.TotalChars
        builder
        |> appendValue "Left fingers" (state.LeftFingers.Values.Sum())
        |> appendLines state.LeftFingers percentFromTotalInt 0.0
        |> appendValue "Right fingers" (state.RightFingers.Values.Sum())
        |> appendLines state.RightFingers percentFromTotalInt 0.0
        |> appendValue "Same finger" (percentFromTotalInt state.SameFinger)
        |> appendValue "Top keys" (percentFromTotalInt state.TopKeys)
        |> appendValue "Home keys" (percentFromTotalInt state.HomeKeys)
        |> appendValue "Bottom keys" (percentFromTotalInt state.BottomKeys)
        |> appendValue "Inward rolls" (percentFromTotalInt state.InwardRolls)
        |> appendValue "Outward rolls" (percentFromTotalInt state.OutwardRolls)
        |> appendValue "Left hand" (percentFromTotalInt state.LeftHandTotal)
        |> appendValue "Right hand" (percentFromTotalInt state.RightHandTotal)
        |> appendValue "Left hand continuous" (percentFromTotalInt state.LeftHandContinuous)
        |> appendValue "Right hand continuous" (percentFromTotalInt state.RightHandContinuous)
        |> appendValue "Hand switch" (percentFromTotalInt state.HandSwitch)
        |> appendValue "Efforts" state.Efforts
        |> appendValue "Distance" state.Distance
        |> appendLines state.HeatMap (fun value -> percentFromTotal state.Result value) 0.0
        |> appendValue "Result" state.Result

    let initialTop = Console.CursorTop
    let printState state =
        Console.SetCursorPosition(0, max (initialTop - 1) 0)
        let digraphs = state.Digraphs.ToDictionary((fun x -> Digraph.value x.Key), (fun y -> y.Value))
        let characters = state.Chars.ToDictionary((fun x -> Character.value x.Key), (fun y -> y.Value))
        let letters = state.Letters.ToDictionary((fun x -> Letter.value x.Key), (fun y -> y.Value))
        let builder = StringBuilder()
        if detailed then
            builder
            |> appendValue "Digraphs" state.TotalDigraphs
            |> appendLines digraphs (percentFromTotalInt state.TotalDigraphs) settings.minDigraphs
            |> appendValue "Letters" state.TotalLetters
            |> appendLines letters (percentFromTotalInt state.TotalLetters) 0.0
            |> appendValue "Characters" state.TotalChars
            |> appendLines characters (percentFromTotalInt state.TotalChars) 0.0
            |> appendValue "Shifts" (percentFromTotalInt state.TotalChars state.Shifts)
            |> ignore
        builder
        |> formatMain state
        |> Console.WriteLine

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
                cancel ()
        stateChangedStream.OnNext newState
        newState

    let start = DateTime.UtcNow
    let keyboard = Keyboard.load <| Layout.Load layoutPath

    Directory.EnumerateFiles(samplesPath, search, SearchOption.AllDirectories)
    |> PSeq.takeWhile notCancelled
    |> PSeq.map (yieldLines >> calculateLines keyboard)
    |> PSeq.fold folder initialState
    |> printState

    Ok (sprintf "Time spent: %s" ((DateTime.UtcNow - start).ToString("c")))
