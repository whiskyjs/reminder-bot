namespace ReminderBot.Model

open LiteDB
open LiteDB.FSharp

open ReminderBot.Helpers

module Storage =
    module Defaults =
        [<Literal>]
        let DatabaseName =
            "storage.db"            
        
        let DatabasePath =
            Path.GetRoot + DatabaseName
            
    let Invoke fn =
        let mapper = FSharpBsonMapper()
        use db = new LiteDatabase(Defaults.DatabasePath, mapper)
        fn db
        