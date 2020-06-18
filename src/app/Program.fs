﻿open System
open System.Threading
open Main

let private handler path search keyboard =
    printf "Press any key to finish..."
    use cts = new CancellationTokenSource()
    let task = calculate path search keyboard cts |> Async.StartAsTask
    Console.ReadKey true |> ignore
    cts.Cancel true
    // todo: better awaiting
    task.GetAwaiter().GetResult()

[<EntryPoint>]
let main argv =
    match argv with
    | [| first; second; third |] -> handler first second third
    | _ -> printf "Wrong input."
    0
