module Tests

open System
open Cativity
open Xunit
open FsUnit.Xunit

let lazyCat name = LazyCat {Id = 1L; Name = name; MiceEaten = FedThreshold  }

[<Fact>]
let ``Lazy cat when digests mouse then it becomes hungry`` () =
    // Arrange
    let mruczekBefore, digestionTime = lazyCat "mruczek", DateTime.UtcNow
    // Act
    let digestionResult = digestMouse mruczekBefore digestionTime
    // Assert
    let (event) = digestionResult  |> function Ok (event) ->  (event) | _ -> failwith "Invalid cat"
    let cat: HungryCat = event |> apply mruczekBefore |> function HungryCat cat -> cat | _ -> failwith "Invalid cat"
    cat.MiceEaten |> should equal (FedThreshold - 1)
    
[<Fact>]
let ``Hungry cat doesn't digests mouse if it hasn't eat anything`` () =
    // Arrange
    let mruczekBefore = Cat.zero
    // Act
    let result = digestMouse mruczekBefore DateTime.UtcNow
    // Assert
    result = Error CatHasNothingToDigest |> should equal true
