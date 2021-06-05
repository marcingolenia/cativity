module Tests

open System
open System.Threading
open System.Threading.Tasks
open Xunit
open CosmoStore.Marten
open CosmoStore
open FsUnit.Xunit

type DummyEvent = { IncrementedBy: int }

[<Fact>]
let ``Basic event appending to postgres stream with optimistic concurrency versioning`` () =
    // Arrange
    let streamId = "MyAmazingStream"

    let config: Configuration =
        { Host = "localhost"
          Username = "postgres"
          Password = "Secret!Passw0rd"
          Database = "cativity" }

    let es = EventStore.getEventStore config

    let eventWrite: EventWrite<DummyEvent> =
        { Id = Guid.NewGuid()
          CorrelationId = None
          CausationId = None
          Name = nameof DummyEvent
          Data = { IncrementedBy = 4 }
          Metadata = None }

    let events =
        EventsReadRange.AllEvents
        |> es.GetEvents "MyAmazingStream"
        |> Async.AwaitTask
        |> Async.RunSynchronously
    let previousEvent = events |> List.tryHead
    let version = previousEvent |> function Some item -> item.Version + 1L | _ -> 1L
    // Act
    let newestEvent =
        eventWrite
        |> es.AppendEvent streamId (version |> ExpectedVersion.Exact)
        |> Async.AwaitTask
        |> Async.RunSynchronously
    newestEvent.Data |> should equal eventWrite.Data

[<Fact>]
let ``Basic event appending to postgres stream without optimistic concurrency versioning`` () =
    // Arrange
    let streamId = "MyAmazingStream"

    let config: Configuration =
        { Host = "localhost"
          Username = "postgres"
          Password = "Secret!Passw0rd"
          Database = "cativity" }

    let es = EventStore.getEventStore config

    let eventWrite: EventWrite<DummyEvent> =
        { Id = Guid.NewGuid()
          CorrelationId = None
          CausationId = None
          Name = nameof DummyEvent
          Data = { IncrementedBy = 4 }
          Metadata = None }
    // Act
    let newestEvent =
        eventWrite
        |> es.AppendEvent streamId ExpectedVersion.Any
        |> Async.AwaitTask
        |> Async.RunSynchronously
    // Assert
    newestEvent.Data |> should equal eventWrite.Data
    
[<Fact>]
let ``Basic event appending to postgres stream and subscription to stream`` () =
    // Arrange
    let streamId = "MyAmazingStream"
    let tcs = TaskCompletionSource<Unit>()
    let mutable consumedEvents: DummyEvent list = []
    let eventsCount = 5

    let config: Configuration =
        { Host = "localhost"
          Username = "postgres"
          Password = "Secret!Passw0rd"
          Database = "cativity" }

    let es = EventStore.getEventStore config
    use subsription = es.EventAppended.Subscribe(fun evt ->
        printfn "%A" evt.Data
        consumedEvents <- evt.Data :: consumedEvents
        if consumedEvents.Length = eventsCount then tcs.SetResult(())
        )

    let eventWrite i: EventWrite<DummyEvent> =
        { Id = Guid.NewGuid()
          CorrelationId = None
          CausationId = None
          Name = nameof DummyEvent
          Data = { IncrementedBy = i }
          Metadata = None }
    // Act
    [ 1 .. eventsCount ] |> List.iter(fun i ->
        eventWrite i
            |> es.AppendEvent streamId ExpectedVersion.Any
            |> Async.AwaitTask
            |> Async.RunSynchronously
            |> ignore
        Async.Sleep 100 |> Async.RunSynchronously
        )
    // Assert
    tcs.Task |> Async.AwaitTask |> Async.RunSynchronously
    consumedEvents.Length |> should equal eventsCount
    printfn "Done!"