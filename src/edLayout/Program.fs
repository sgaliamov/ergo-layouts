open System
open System.Threading
open Main
open System.Threading.Tasks

let private handler path search layout =
    use cts = new CancellationTokenSource()
    let task = Task.Run(fun () ->
        Console.ReadKey true |> ignore
        cts.Cancel true, cts.Token)

    printf "Press any key to finish..."
    calculate path search layout cts
    Task.WaitAll task

[<EntryPoint>]
let main argv =
    match argv with
    | [| first; second; third |] -> handler first second third
    | _ -> printf "Wrong input."
    0
