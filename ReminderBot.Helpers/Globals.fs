namespace ReminderBot

[<AutoOpen>]
module Globals =
    let testNull value =
        obj.ReferenceEquals(value, null)
