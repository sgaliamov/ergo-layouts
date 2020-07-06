open System
open System.Threading
open Main
open System.Threading.Tasks

let private handler path search showProgress detailed layout =
    use cts = new CancellationTokenSource()
    let cancel () = cts.Cancel true

    Task.Run((fun () ->
        Console.ReadKey true |> ignore
        cancel ()), cts.Token) |> ignore

    let result = calculate showProgress path search detailed layout cts.Token cancel
    match result with
    | Ok ->
        0
    | Error error ->
        printf "\n\n%s" error
        -1

[<EntryPoint>]
let main argv =
    match argv with
    | [| first; second; third; fourth; fifth |] -> handler first second (bool.Parse third) (bool.Parse fourth) fifth
    | _ -> printf "Wrong input."; -1
