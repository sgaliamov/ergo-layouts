open System
open System.Threading
open System.Threading.Tasks
open System.CommandLine
open System.CommandLine.Invocation;
open Main

let private handler input pattern layout showProgress detailed =
    use cts = new CancellationTokenSource()
    let cancel () = cts.Cancel true

    Task.Run((fun () ->
        Console.ReadKey true |> ignore
        cancel ()), cts.Token) |> ignore

    let result = calculate showProgress input pattern detailed layout cts.Token cancel
    match result with
    | Ok ->
        0
    | Error error ->
        printf "\n\n%s" error
        -1

let [<EntryPoint>] main args =
    let root = RootCommand()
    root.AddOption (Option<string>([| "--input"; "-i" |]))
    root.AddOption (Option<string> ([| "--pattern"; "-p" |], fun () -> "*.txt"))
    root.AddOption (Option<string>([| "--layout"; "-l" |]))
    root.AddOption (Option<bool>([| "--show-progress"; "-sp" |], fun () -> true))
    root.AddOption (Option<bool>([| "--detailed"; "-d" |]))
    root.Handler <- CommandHandler.Create(fun input pattern layout showProgress detailed -> handler input pattern layout showProgress detailed)
    root.Invoke args
