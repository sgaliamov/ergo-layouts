module Program

open System.CommandLine
open System.CommandLine.Invocation;
open System

let [<EntryPoint>] main args =
    let root = new RootCommand()
    let option = new Option<string>("--test")
    root.AddOption option
    root.Handler <- CommandHandler.Create<string>(fun (test: string) -> Console.WriteLine test)
    root.Invoke args