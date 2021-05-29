open System.Threading

[<EntryPoint>]
let main argv =
    for i in [0 .. 100] do
        printfn "%d" i
        Thread.Sleep 500
    0