namespace ReminderBot.Logic

open System

open ReminderBot.Helpers
open ReminderBot.Model

module Notifier =
    let Notify cfg () =
        match Date.IsHolidayToday with
        | true -> async { () }
        | _ ->
            async {
                let notifications =
                    Notification.This.FromToday
                    |> Seq.toList
                    
                User.This.FindAll
                |> Seq.cast<User.This>
                |> Seq.filter(fun user -> DateTime.Parse(user.NotificationTime) <= DateTime.Now)
                |> Seq.iter(fun user ->
                    try
                        notifications
                        |> List.find(fun this -> this.UserId = user.Id)
                        |> ignore
                    with
                        | :? System.Collections.Generic.KeyNotFoundException ->
                            // Сохраняем запись о том, что отправка уже была
                            Notification.This.Insert {
                                Id = 0
                                UserId = user.Id
                                Date = Date.CurrentDateStr()
                                ExactTime = DateTime.UtcNow.ToString("s")
                                }
                            |> ignore                        
                            
                            // Собственно отправка
                            (user.Connectors.Telegram, Resolver.Command.Stats)
                            ||> Resolver.Resolve cfg
                            |> Executor.Execute
                            |> Executor.Send cfg user.Connectors.Telegram.Id
                            |> Async.Ignore
                            |> Async.Start
                    )
            }
