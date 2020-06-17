module Main

open System
open System.IO
open System.Collections.Generic
open System.Reactive.Subjects
open System.Reactive.Linq
open System.Text
open System.Threading
open Calculations
open Configs
open Models

let appendLines<'T> (pairs: seq<KeyValuePair<'T, int>>) total filter (builder: StringBuilder) =
    let appendPair (key, value) = builder.AppendFormat("{0,-2} : {1,-10:0.###}", key, value)
    let getValue value =
        let div x y =
            match y with
            | 0 -> 0.0
            | _ -> (float x) / (float y)
        100.0 * div value total

    pairs
    |> Seq.map (fun pair ->
        ({| Key = pair.Key
            Value = getValue pair.Value |}))
    |> Seq.filter (fun pair -> pair.Value >= filter)
    |> Seq.sortByDescending (fun pair -> pair.Value)
    |> Seq.fold (fun (sb: StringBuilder, i) pair ->
        match i % settings.columns = 0 && i <> 0 with
        | true -> sb.AppendLine(), 0
        | _ -> appendPair(pair.Key, pair.Value), i + 1)
        (builder, 0)
    |> ignore
    builder

let calculate path search (cts: CancellationTokenSource) =
    async {
        let appendValue (format: string) (value: int) (builder: StringBuilder) = builder.AppendFormat(format, value)

        let yieldLines (token: CancellationToken) filePath =
            seq {
                use stream = File.OpenText filePath
                while not stream.EndOfStream
                      && not token.IsCancellationRequested do
                    yield stream.ReadLine() // todo: use async
            } 

        let printMain state =
            let symbolsOnly =
                state.Chars
                |> Seq.filter (fun x -> Char.IsPunctuation(x.Key) || x.Key = ' ')

            StringBuilder()
            |> (appendValue "Letters: {0}\n" state.TotalLetters)
            |> (appendLines state.Letters state.TotalLetters 0.0)
            |> (appendValue "\n\nSymbols from total: {0}\n" state.TotalChars)
            |> (appendLines symbolsOnly state.TotalChars 0.0)

        let print state =
            printMain(state)
            |> (appendValue "\n\nDigraphs {0}:\n" state.TotalDigraphs)
            |> (appendLines state.Digraphs state.TotalDigraphs 0.05)
            |> Console.WriteLine

        use stateChangedStream = new Subject<State>()

        let onStateChanged state =
            let initialPosition = (Console.CursorLeft, Console.CursorTop)
            printMain state
            |> Console.WriteLine
            Console.SetCursorPosition initialPosition

        use _ = Observable.subscribe onStateChanged (stateChangedStream.Throttle(TimeSpan.FromSeconds(5.)))

        let folder state next =
            let newState = aggregator state next
            let digraphsFinished = isFinished newState.Digraphs settings.digraphs newState.TotalDigraphs settings.precision
            let lettersFinished = isFinished newState.Letters settings.letters newState.TotalLetters settings.precision
            if digraphsFinished && lettersFinished then
                printfn "Collected enough data."
                cts.Cancel()
            stateChangedStream.OnNext newState
            newState

        let start = DateTime.UtcNow

        Directory.EnumerateFiles(path, search, SearchOption.AllDirectories)
        |> Seq.filter (fun _ -> not cts.IsCancellationRequested)
        |> Seq.map (yieldLines cts.Token)
        |> Seq.map calculateLines // todo: can run in pararllel
        |> Seq.fold folder initialState
        |> print

        stateChangedStream.OnCompleted()

        printf "\nTime: %s." ((DateTime.UtcNow - start).ToString("c"))
    }
