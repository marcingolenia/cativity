module EatingMouseV1

open CativityV1
open Xunit
open FsUnit.Xunit
open System

[<Fact>]
let ``Hungry cat when eats mouse and reaches fed threshold then it becomes lazy`` () =
    // Arrange
    let miceEatenEvents = [ for i in 1 .. FedThreshold -> Event.MouseEaten(DateTime(2020, 01, 01, i, 0, 0)) ]
    let events = (create 1L "mruczek" |> snd) :: miceEatenEvents
    // Act
    let mruczekAfter = events |> List.fold apply Cat.zero 
    // Assert
    let lazyMruczek = mruczekAfter |> function LazyCat cat -> cat | _ -> failwith "Invalid cat"
    lazyMruczek.MiceEaten |> should equal (miceEatenEvents |> List.length)
