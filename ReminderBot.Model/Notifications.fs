namespace ReminderBot.Model

open LiteDB
open LiteDB.FSharp

open ReminderBot.Helpers

module Notification =
    [<Literal>]
    let Collection = "notifications"

    [<CLIMutable>]
    type This =
        { Id: int
          UserId: int
          Date: string
          ExactTime: string }

        static member GetCollection = Storage.Invoke(fun db -> db.GetCollection<This>(Collection))

        static member Insert(this: This) =
            This.GetCollection.Insert this |> ignore
            this

        static member Query(query: LiteDB.Query) = This.GetCollection.Find(query)

        static member FindAll = Query.All() |> This.Query

        static member FromToday =
            Query.Where("Date", (fun bsonValue -> Bson.deserializeField<string> bsonValue |> Date.IsToday))
            |> This.Query
