module CativityV1

open System

[<Literal>]
let FedThreshold = 3

type Errors =
    | CatHasNothingToDigest

type CatCreated = { Id: int64; Name:string }

type Event =
    | MouseEaten of DateTime
    | MouseDigested of DateTime
    | CatRenamed of string
    | CatCreated of CatCreated

type HungryCat = {
    Id: int64
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

let create id name =
    let cat = HungryCat { Id = id; Name = name; MiceEaten = 0 }
    (cat, CatCreated { Id = id; Name = name })

let eatMouse (cat: HungryCat) (ateAt: DateTime) =
    let cat = 
        match cat.MiceEaten + 1 with
        | FedThreshold -> LazyCat { Id = cat.Id; Name = cat.Name ;MiceEaten = FedThreshold } 
        | _ -> HungryCat { cat with MiceEaten = cat.MiceEaten + 1 } 
    (cat, MouseEaten ateAt)

let changeName cat name =
    let cat = cat |> function
            | LazyCat cat -> LazyCat { cat with Name = name }
            | HungryCat cat -> HungryCat { cat with Name = name }
    (cat, CatRenamed name)
    
let digestMouse cat digestedAt =
    let (name, miceEaten, id) = cat |> function
        | LazyCat cat -> (cat.Name, cat.MiceEaten, cat.Id)
        | HungryCat cat -> (cat.Name, cat.MiceEaten, cat.Id)
    match miceEaten with
    | 0 -> Error CatHasNothingToDigest
    | _ -> 
        Ok (HungryCat {
            Id = id
            Name = name
            MiceEaten = miceEaten - 1
        }, MouseDigested digestedAt)
    
let apply cat event =
    match (cat, event) with
    | (_, CatCreated created) -> create created.Id created.Name
    | (HungryCat cat, MouseEaten ateAt) -> eatMouse cat ateAt
    | (cat, MouseDigested digestedAt) -> digestMouse cat digestedAt
                                         |> function Ok result -> result | _ -> (cat, event)
    | (cat, CatRenamed name) -> changeName cat name
    | _ -> (cat, event) 
    |> fst