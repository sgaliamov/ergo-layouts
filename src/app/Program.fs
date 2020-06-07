open System
open System.CommandLine
open System.CommandLine.Invocation
open System.Threading
open Main

let private handler path = 
    printfn "Press any key to exit..."
    use cts = new CancellationTokenSource()
    calculate path cts.Token // todo: find a way to be pure async
    Console.ReadKey false |> ignore
    cts.Cancel true
    printf "Done."

[<EntryPoint>]
let main argv =
    let root = RootCommand()
    Option("--path") |> root.AddOption
    root.Handler <- handler |> CommandHandler.Create
    root.Invoke argv