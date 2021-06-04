module Tests

open System
open Xunit
open CosmoStore.Marten
open CosmoStore
open FsUnit.Xunit

type DummyEvent = { IncrementedBy: int }

[<Fact>]
let ``Appending event to postgres stream`` () =
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
