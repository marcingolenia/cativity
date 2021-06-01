module TestsV1

open System
open CativityV1
open Xunit
open FsUnit.Xunit

let lazyCat name = LazyCat { Name = name; MiceEaten = FedThreshold  }

[<Fact>]
let ``Lazy cat when digests mouse then it becomes hungry`` () =
    // Arrange
    let mruczekBefore, digestionTime = lazyCat "mruczek", DateTime.UtcNow
    // Act
    let digestionResult = digestMouse mruczekBefore digestionTime
    // Assert
    let (hungryMruczek, event) = digestionResult
                                 |> function Ok (HungryCat cat, event) ->
                                                (cat, event) | _ -> failwith "Invalid cat"
    hungryMruczek.MiceEaten |> should equal (FedThreshold - 1)
    event |> should equal (MouseDigested digestionTime)
    event |> apply mruczekBefore |> should equal (Cat.HungryCat hungryMruczek)

[<Fact>]
let ``Hungry cat doesn't digests mouse if it hasn't eat anything`` () =
    // Arrange
    let mruczekBefore = Cat.zero "mruczek"
    // Act
    let result = digestMouse mruczekBefore DateTime.UtcNow
    // Assert
    result = Error CatHasNothingToDigest |> should equal true
