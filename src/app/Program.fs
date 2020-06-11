open System
open System.Threading
open Main

let private handler path search = 
    printfn "Press any key to exit..."
    use cts = new CancellationTokenSource()
    calculate path search cts.Token |> Async.Start
    Console.ReadKey false |> ignore
    cts.Cancel true

[<EntryPoint>]
let main argv =
    match argv with
    | [| first; second |] -> handler first second
    | _ -> printfn "Wrong input."
    0
