namespace ReminderBot.Model

open LiteDB
open LiteDB.FSharp

open ReminderBot

module User =
    [<Literal>]
    let Collection = "users"
    
    type Telegram =
        { Id: int
          Name: string
          Username: string
          Language: string }

    type Connectors =
        { Telegram: Telegram }

        member this.SetTelegram value = { this with Telegram = value }

    type This =
        { Id: int
          Login: string
          NotificationTime: string
          Connectors: Connectors }

        static member GetCollection = Storage.Invoke(fun db -> db.GetCollection<This>(Collection))

        static member Get id =
            let result =
                This.GetCollection.FindOne(fun this -> this.Id = id)

            match testNull result with
            | true -> None
            | _ -> Some(result)

        static member Insert(user: This) =
            This.GetCollection.Insert user |> ignore
            user

        static member Update(user: This) =
            This.GetCollection.Update user |> ignore
            user

        static member Query(query: LiteDB.Query) = This.GetCollection.Find(query)
        
        static member FindAll =
            Query.All()
            |> This.Query

        static member FindByConnector fn =
            Query.Where
                ("Connectors",
                 (fun bsonValue ->
                     let connectors = Bson.deserializeField<Connectors> bsonValue
                     fn connectors))
            |> This.Query

        member this.SetConnectors value = { this with Connectors = value }
