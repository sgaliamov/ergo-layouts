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

let private appendLines<'T> (pairs: seq<KeyValuePair<'T, int>>) total minValue (builder: StringBuilder) =
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
    |> Seq.filter (fun pair -> pair.Value >= minValue)
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
        let spacer = new string(' ', Console.WindowWidth)

        let appendValue (title: string) value (builder: StringBuilder) =
            let format = sprintf "\n%s\n{0}: {1,-%d}\n" spacer (settings.columns * 15 - title.Length - 2)
            builder.AppendFormat(format, title, value)
        
        let yieldLines (token: CancellationToken) filePath =
            seq {
                use stream = File.OpenText filePath
                while not stream.EndOfStream
                      && not token.IsCancellationRequested do
                    yield stream.ReadLine() // todo: use async
            }

        let formatMain state  (builder: StringBuilder) =
            let symbolsOnly =
                state.Chars
                |> Seq.filter (fun x -> Char.IsPunctuation(x.Key) || x.Key = ' ')
            builder
            |> (appendValue "Letters" state.TotalLetters)
            |> (appendLines state.Letters state.TotalLetters 0.0)
            |> (appendValue "Symbols from total" state.TotalChars)
            |> (appendLines symbolsOnly state.TotalChars 0.0)

        let formatState state =
            Console.SetCursorPosition (0, Console.CursorTop)
            StringBuilder()
            |> (appendValue "Digraphs" state.TotalDigraphs)
            |> (appendLines state.Digraphs state.TotalDigraphs 0.05)
            |> formatMain state
            |> Console.WriteLine


        let onStateChanged state =
            let initialPosition = (Console.CursorLeft, Console.CursorTop)
            StringBuilder()
            |> formatMain state
            |> Console.WriteLine
            Console.SetCursorPosition initialPosition

        use stateChangedStream = new Subject<State>()
        use subscription = stateChangedStream.Sample(TimeSpan.FromSeconds(0.500)).Subscribe onStateChanged

        let folder state next =
            let newState = aggregator state next
            let digraphsFinished = isFinished newState.Digraphs settings.digraphs newState.TotalDigraphs settings.precision
            let lettersFinished = isFinished newState.Letters settings.letters newState.TotalLetters settings.precision
            if digraphsFinished && lettersFinished then
                Console.SetCursorPosition(0, Console.CursorTop)
                Console.Write spacer
                printf "\rCollected enough data."
                subscription.Dispose()
                cts.Cancel()
            stateChangedStream.OnNext newState
            newState

        let start = DateTime.UtcNow

         // todo: run in pararllel
        Directory.EnumerateFiles(path, search, SearchOption.AllDirectories)
        |> Seq.filter (fun _ -> not cts.IsCancellationRequested)
        |> Seq.map ((yieldLines cts.Token) >> calculateLines)
        |> Seq.fold folder initialState
        |> formatState

        printf "\nTime: %s\n" ((DateTime.UtcNow - start).ToString("c"))
    }
