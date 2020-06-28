open System
open System.CommandLine
open System.CommandLine.Invocation;

[<EntryPoint>]
let main args =
    let root = new RootCommand()
    let option = new Option<string>("--test")
    root.AddOption option
    root.Handler <- CommandHandler.Create<string>(fun (test: string) -> Console.WriteLine test)
    root.Invoke args