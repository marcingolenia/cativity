module Cativity

open System

[<Literal>]
let FedThreshold = 3

type Event =
    | MouseEaten of DateTime
    | MouseDigested of DateTime
    | CatRenamed of string
    
type Errors =
    | CatHasNothingToDigest

type HungryCat = {
    Name: string
    MiceEaten: int
}

type LazyCat = {
    Name: string
    MiceEaten: int
}

type Cat = 
    | HungryCat of HungryCat
    | LazyCat of LazyCat
    with static member zero name =
            HungryCat {
                Name = name
                MiceEaten = 0
            }
            
let apply cat event =
    match (cat, event) with
    | (cat, MouseDigested _) ->
        let (name, miceEaten) = cat |> function
            | LazyCat cat -> (cat.Name, cat.MiceEaten)
            | HungryCat cat -> (cat.Name, cat.MiceEaten)
        (HungryCat { Name = name; MiceEaten = miceEaten - 1}, event)
    | (HungryCat cat, MouseEaten _) ->
        let cat = 
            match cat.MiceEaten + 1 with
            | FedThreshold -> LazyCat { Name = cat.Name; MiceEaten = FedThreshold } 
            | _ -> HungryCat { cat with MiceEaten = cat.MiceEaten + 1 }
        (cat, event)
    | (cat, CatRenamed name) ->
        let cat = cat |> function
            | LazyCat cat -> LazyCat { cat with Name = name }
            | HungryCat cat -> HungryCat { cat with Name = name }
        (cat, event)
    | _ -> (cat, event) 
    |> fst
   
let eatMouse (cat: HungryCat) (ateAt: DateTime) =
    MouseEaten ateAt
    
let rename _ (name: string) =
    CatRenamed name
    
let digestMouse cat digestedAt =
    let miceEaten = cat |> function
            | LazyCat cat -> cat.MiceEaten
            | HungryCat cat -> cat.MiceEaten
    match miceEaten with
    | 0 -> Error Errors.CatHasNothingToDigest 
    | _ -> (MouseDigested digestedAt) |> Ok
