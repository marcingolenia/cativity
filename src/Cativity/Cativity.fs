module Cativity

open System

[<Literal>]
let FedThreshold = 3
type CatCreated = { Id: int64; Name:string }
type Event =
    | MouseEaten of DateTime
    | MouseDigested of DateTime
    | CatRenamed of string
    | CatCreated of CatCreated
    
type Errors =
    | CatHasNothingToDigest

type HungryCat = {
    Id : int64
    Name: string
    MiceEaten: int
}

type LazyCat = {
    Id: int64
    Name: string
    MiceEaten: int
}

type Cat = 
    | HungryCat of HungryCat
    | LazyCat of LazyCat
    with static member zero =
            HungryCat {
                Id = 0L
                Name = ""
                MiceEaten = 0
            }
            
let apply cat event =
    match (cat, event) with
    | (_, CatCreated created) ->
        HungryCat { Id = created.Id; Name = created.Name; MiceEaten = 0 }
    | (cat, MouseDigested _) ->
        let (name, miceEaten, id) = cat |> function
            | LazyCat cat -> (cat.Name, cat.MiceEaten, cat.Id)
            | HungryCat cat -> (cat.Name, cat.MiceEaten, cat.Id)
        HungryCat { Id = id; Name = name; MiceEaten = miceEaten - 1}
    | (HungryCat cat, MouseEaten _) ->
        match cat.MiceEaten + 1 with
        | FedThreshold -> LazyCat { Id = cat.Id; Name = cat.Name; MiceEaten = FedThreshold } 
        | _ -> HungryCat { cat with MiceEaten = cat.MiceEaten + 1 }
    | (cat, CatRenamed name) ->
        cat |> function
            | LazyCat cat -> LazyCat { cat with Name = name }
            | HungryCat cat -> HungryCat { cat with Name = name }
    | _ -> cat
   
let eatMouse _ (ateAt: DateTime) =
    MouseEaten ateAt

let create id name =
    CatCreated { Id = id; Name = name }

let rename _ (name: string) =
    CatRenamed name
    
let digestMouse cat digestedAt =
    let miceEaten = cat |> function
            | LazyCat cat -> cat.MiceEaten
            | HungryCat cat -> cat.MiceEaten
    match miceEaten with
    | 0 -> Error Errors.CatHasNothingToDigest 
    | _ -> (MouseDigested digestedAt) |> Ok
