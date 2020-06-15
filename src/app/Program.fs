open System
open System.Threading
open Main

let private handler path search =
    printfn "Press any key to finish..."
    use cts = new CancellationTokenSource()
    let task = calculate path search cts |> Async.StartAsTask
    Console.ReadKey true |> ignore
    cts.Cancel true
    // todo: better awaiting
    task.GetAwaiter().GetResult()

[<EntryPoint>]
let main argv =
    match argv with
    | [| first; second |] -> handler first second
    | _ -> printfn "Wrong input."
    0
