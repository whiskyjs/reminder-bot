namespace ReminderBot.Model

module Json =
    type Skippable<'T> =
        | Skip
        | Include of 'T
