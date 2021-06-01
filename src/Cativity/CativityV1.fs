module CativityV1

open System

[<Literal>]
let FedThreshold = 3

type Errors =
    | CatHasNothingToDigest

type Event =
    | MouseEaten of DateTime
    | MouseDigested of DateTime
    | CatRenamed of string

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
   
let eatMouse (cat: HungryCat) (ateAt: DateTime) =
    let cat = 
        match cat.MiceEaten + 1 with
        | FedThreshold -> LazyCat { Name = cat.Name ;MiceEaten = FedThreshold } 
        | _ -> HungryCat { cat with MiceEaten = cat.MiceEaten + 1 } 
    (cat, MouseEaten ateAt)

let changeName cat name =
    let cat = cat |> function
            | LazyCat cat -> LazyCat { cat with Name = name }
            | HungryCat cat -> HungryCat { cat with Name = name }
    (cat, CatRenamed name)
    
let digestMouse cat digestedAt =
    let (name, miceEaten) = cat |> function
        | LazyCat cat -> (cat.Name, cat.MiceEaten)
        | HungryCat cat -> (cat.Name, cat.MiceEaten)
    match miceEaten with
    | 0 -> Error CatHasNothingToDigest
    | _ -> 
        Ok (HungryCat {
            Name = name
            MiceEaten = miceEaten - 1
        }, MouseDigested digestedAt)
    
let apply cat event =
    match (cat, event) with
    | (HungryCat cat, MouseEaten ateAt) -> eatMouse cat ateAt
    | (cat, MouseDigested digestedAt) -> digestMouse cat digestedAt
                                         |> function Ok result -> result | _ -> (cat, event)
    | (cat, CatRenamed name) -> changeName cat name
    | _ -> (cat, event) 
    |> fst