module EatingMouse

open Cativity
open Xunit
open FsUnit.Xunit
open System

[<Fact>]
let ``Hungry cat when eats mouse and reaches fed threshold then it becomes lazy`` () =
    // Arrange
    let mruczekBefore = Cat.zero "mruczek"
    let events = [ for i in 1 .. FedThreshold -> Event.MouseEaten(DateTime(2020, 01, 01, i, 0, 0)) ]
    // Act
    let mruczekAfter = events |> List.fold apply mruczekBefore 
    // Assert
    let lazyMruczek = mruczekAfter |> function LazyCat cat -> cat | _ -> failwith "Invalid cat"
    lazyMruczek.MiceEaten |> should equal (events |> List.length)
