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
open Utilities
open System.Globalization

let calculate showProgress samplesPath search detailed (layoutPath: string) output (token: CancellationToken) cancel =
    // todo: find better way to validate input parameters
    if not (Directory.Exists samplesPath) then
        cancel ()
        Error "Samples direcotry does not exist."
    else if not (File.Exists layoutPath) then
        cancel ()
        Error "Layout does not exist."
    else

    let keyboard = Keyboard.load <| Layout.Load layoutPath
    let appendNewLine (builder: StringBuilder) = builder.AppendLine()
    let append (text: string) (builder: StringBuilder) = builder.Append(text)
    let appendFormat format args (builder: StringBuilder) = builder.AppendFormat(format, args)

    let appendLines (pairs: seq<KeyValuePair<'TKey, 'Value>>) getValue minValue (builder: StringBuilder) =
        let appendPair (sb: StringBuilder, i) (key, value) =
            if i % settings.columns = 0 then
                if i = 0 then sb.Append("    ")
                else sb.AppendLine().Append("    ")
                |> ignore
            sb.AppendFormat("{0,2}: {1,-10:0.###}", key, value), i + 1
        pairs
        |> Seq.map (fun pair -> pair.Key, getValue pair.Value)
        |> Seq.filter (fun (_, value) -> value >= minValue)
        |> Seq.sortByDescending (fun (_, value) -> value)
        |> Seq.fold appendPair (builder, 0)
        |> first
        |> appendNewLine

    let div x y =
        match y with
        | 0. -> 0.0
        | _ -> x / y

    let appendValue (title: string) value (builder: StringBuilder) =
        let format = sprintf "{0,-11}: {1,-%d:0,0.##}\n" (settings.columns * 15 - title.Length - 2)
        builder.AppendFormat(format, title, value)
        
    let yieldLines filePath = seq {
        use stream = File.OpenText filePath
        while not stream.EndOfStream && not token.IsCancellationRequested do
            // todo: use async
            yield stream.ReadLine() }

    let notCancelled _ = not token.IsCancellationRequested
    let percentFromTotal total value = (100. * div value total)
    let percentFromTotalInt total value = percentFromTotal (float total) (float value)
    let percentFromTotalChars state = percentFromTotalInt state.TotalChars
    let combinePairedChars add (items: IDictionary<Character.Char, 'T>) =
        items
        |> Seq.map (fun (x: KeyValuePair<Character.Char, 'T>) -> x.Key, x.Value)
        |> Seq.map (fun (char, count) ->
            match keyboard.PairedChars.TryGetValue(char) with
            | (true, shifted) ->
                let chars =
                    [Character.value shifted; Character.value char]
                    |> List.sort
                let combinedCount =
                    if items.ContainsKey(shifted) 
                        then (add items.[shifted] count)
                        else count
                String.Join("", chars), combinedCount
            | _ -> (Character.value char).ToString(), count)
        |> Seq.distinct
        |> Map
        |> Dictionary

    let formatMain state (builder: StringBuilder) =
        let appendKeyboard builder =
            let appendKeysLine keys builder =
                keys
                |> Seq.fold (fun (sb: StringBuilder) key ->
                    let text = keyboard.Chars.[key] |> Array.map Character.value |> String
                    let sum =
                        keyboard.Chars.[key]
                        |> Seq.map (fun a -> state.HeatMap.TryGetValue(a))
                        |> Seq.filter (fun (has, _) -> has)
                        |> Seq.map second
                        |> Seq.sum
                    sb.AppendFormat("{0,2}: {1,-6:0.##}", text, (percentFromTotal state.Result sum)))
                    builder
            let appendRow keys builder =
                builder
                |> appendKeysLine (keys |> Seq.filter keyboard.LeftKeys.Contains |> Seq.sort)
                |> append "    "
                |> appendKeysLine (keys |> Seq.filter keyboard.RightKeys.Contains |> Seq.sort |> Seq.rev)
                |> appendNewLine
            builder
            |> appendRow keyboard.TopKeys
            |> appendRow keyboard.HomeKeys
            |> appendRow keyboard.BottomKeys

        let heatMap = combinePairedChars (+) state.HeatMap
        let leftFingersContinuous = state.LeftFingersContinuous.Values.Sum()
        let rightFingersContinuous = state.RightFingersContinuous.Values.Sum()
        let percentFromTotalChars = percentFromTotalChars state

        builder
        |> appendKeyboard
        |> appendFormat "Left      : {0:0,0.##}/{1:0,0.##}/{2:0,0.##} (total/hand continuous/fingers continuous)\n"
            [| (percentFromTotalChars state.LeftHandTotal)
               (percentFromTotalChars state.LeftHandContinuous)
               (percentFromTotalChars leftFingersContinuous) |]
        |> appendLines state.LeftFingers percentFromTotalChars 0.0
        |> appendLines state.LeftFingersContinuous percentFromTotalChars 0.0
        |> appendFormat "Right     : {0:0,0.##}/{1:0,0.##}/{2:0,0.##} (total/hand continuous/fingers continuous)\n"
            [| (percentFromTotalChars state.RightHandTotal)
               (percentFromTotalChars state.RightHandContinuous)
               (percentFromTotalChars rightFingersContinuous) |]
        |> appendLines state.RightFingers percentFromTotalChars 0.0
        |> appendLines state.RightFingersContinuous percentFromTotalChars 0.0
        |> appendValue "Efforts" state.Efforts
        |> appendValue "Distance" state.Distance
        |> appendLines heatMap (percentFromTotal state.Result) 0.0
        |> appendValue "Outward rolls" (percentFromTotalChars state.OutwardRolls)
        |> appendValue "Inward rolls" (percentFromTotalChars state.InwardRolls)
        |> appendValue "Hand switch" (percentFromTotalChars state.HandSwitch)
        |> appendValue "Same finger" (percentFromTotalChars state.SameFinger)
        |> appendValue "Result" state.Result
    
    let printState state =
        let digraphs = state.Digraphs.ToDictionary((fun x -> Digraph.value x.Key), (fun y -> y.Value))
        let characters = combinePairedChars (+) state.Chars
        let letters = state.Letters.ToDictionary((fun x -> Letter.value x.Key), (fun y -> y.Value))
        let builder = StringBuilder()
        if detailed then
            builder
            |> appendValue "Digraphs" state.TotalDigraphs
            |> appendLines digraphs (percentFromTotalInt state.TotalDigraphs) settings.minDigraphs
            |> appendValue "Characters" state.TotalChars
            |> appendLines characters (percentFromTotalInt state.TotalChars) 0.0
            |> appendValue "Letters" state.TotalLetters
            |> appendLines letters (percentFromTotalInt state.TotalLetters) 0.0
            |> appendValue "Shifts" (percentFromTotalInt state.TotalChars state.Shifts)
            |> appendValue "Top keys" (percentFromTotalInt state.TotalChars state.TopKeys)
            |> appendValue "Home keys" (percentFromTotalInt state.TotalChars state.HomeKeys)
            |> appendValue "Bottom keys" (percentFromTotalInt state.TotalChars state.BottomKeys)
            |> ignore
        builder
        |> formatMain state
        |> Console.Write
        state

    let onStateChanged state =
        if showProgress then
            let initialPosition = (Console.CursorLeft, Console.CursorTop)
            StringBuilder()
            |> formatMain state
            |> Console.Write
            Console.SetCursorPosition initialPosition

    use stateChangedStream = new Subject<State>()
    use subscription = stateChangedStream.Sample(TimeSpan.FromSeconds(0.500)).Subscribe onStateChanged
    let spacer = new string(' ', Console.WindowWidth)

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

    let save state =
        let percentFromTotalChars = percentFromTotalChars state
        if not (File.Exists output) then
            File.AppendAllText(output, "Title,Result,Hand switch,Same finger,Left hand,Right hand,Left hand continuos,Right hand continuos,Left finger continuos,Right finger continuos,Outward rolls,Inward rolls,Efforts,Distance\n")
        let title = Path.GetFileName layoutPath |> CultureInfo.InvariantCulture.TextInfo.ToTitleCase
        let leftFingersContinuous = state.LeftFingersContinuous.Values.Sum()
        let rightFingersContinuous = state.RightFingersContinuous.Values.Sum()
        let line =
            sprintf
                "%s,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f,%f\n"
                title
                state.Result
                (percentFromTotalChars state.HandSwitch)
                (percentFromTotalChars state.SameFinger)
                (percentFromTotalChars state.LeftHandTotal)
                (percentFromTotalChars state.RightHandTotal)
                (percentFromTotalChars state.LeftHandContinuous)
                (percentFromTotalChars state.RightHandContinuous)
                (percentFromTotalChars leftFingersContinuous)
                (percentFromTotalChars rightFingersContinuous)
                (percentFromTotalChars state.OutwardRolls)
                (percentFromTotalChars state.InwardRolls)
                state.Efforts
                state.Distance

        File.AppendAllText(output, line)

    let start = DateTime.UtcNow

    Directory.EnumerateFiles(samplesPath, search, SearchOption.AllDirectories)
    |> Seq.takeWhile notCancelled
    |> PSeq.map (yieldLines >> calculateLines keyboard)
    |> PSeq.fold folder initialState
    |> printState
    |> save

    Ok (sprintf "Time spent: %s" ((DateTime.UtcNow - start).ToString("c")))
