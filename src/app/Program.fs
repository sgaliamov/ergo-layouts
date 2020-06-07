open System.CommandLine
open System.CommandLine.Invocation
open MainModule
open System.Threading
open System

let private handler path = 
    printfn "Press any key to exit..."
    use cts = new CancellationTokenSource()
    processFolders path cts.Token // todo: find a way to be pure async
    Console.ReadKey false |> ignore
    cts.Cancel true
    printf "Done."

[<EntryPoint>]
let main argv =
    let root = RootCommand()
    Option("--path") |> root.AddOption
    root.Handler <- handler |> CommandHandler.Create 
    root.Invoke argv
