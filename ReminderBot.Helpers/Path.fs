namespace ReminderBot.Helpers

open System

module Path =
    let GetRoot =
        try
            AppContext.BaseDirectory.Substring (0, (AppContext.BaseDirectory.IndexOf "bin"))
        with
            __ -> AppContext.BaseDirectory