﻿open System
open System.Threading
open System.Threading.Tasks
open System.CommandLine
open System.CommandLine.Invocation;
open Main

let private handler input pattern layout output showProgress detailed =
    use cts = new CancellationTokenSource()
    let cancel () = cts.Cancel true

    Task.Run((fun () ->
        Console.ReadKey true |> ignore
        cancel ()), cts.Token) |> ignore

    let result = calculate showProgress input pattern detailed layout output cts.Token cancel
    match result with
    | Error error ->
        printf "\n\n%s" error
        -1
    | _ -> 0

let [<EntryPoint>] main args =
    let root = RootCommand()
    root.AddOption (Option<string>([| "--input"; "-i" |], "Path to a directory with sampling texts."))
    root.AddOption (Option<string> ([| "--pattern"; "-p" |], (fun () -> "*.txt"), "Pattern to filter sampling files."))
    root.AddOption (Option<string>([| "--layout"; "-l" |], "Path to a layout file."))
    root.AddOption (Option<string>([| "--output"; "-o" |], "Path to the output file."))
    root.AddOption (Option<bool>([| "--show-progress"; "-sp" |], fun () -> true))
    root.AddOption (Option<bool>([| "--detailed"; "-d" |], "Show sample texts statistics."))
    root.Handler <- CommandHandler.Create(fun input pattern layout output showProgress detailed -> handler input pattern layout output showProgress detailed)
    root.Invoke args
