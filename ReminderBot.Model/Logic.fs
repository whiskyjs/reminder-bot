namespace ReminderBot.Model.Logic

type Resolution =
    | Message of string
    | Async of Async<string>
