namespace ReminderBot.Logic

open System

open Serilog

open ReminderBot.Helpers
open ReminderBot.Model

module Notifier =
    [<Literal>]
    let UserDelay = 5000
    
    let Notify cfg () =
        match Date.IsHolidayToday () with
        | true ->
            async { () }
        | _ ->
            async {
                let notifications =
                    Notification.This.FromToday
                    |> Seq.toList
                    
                let userTasks =
                    User.This.FindAll
                    |> Seq.cast<User.This>
                    |> Seq.filter(fun user -> DateTime.Parse(user.NotificationTime) <= DateTime.Now)
                    |> Seq.map(fun user ->
                        try
                            notifications
                            |> List.find(fun this -> this.UserId = user.Id)
                            |> ignore
                            
                            async { () }
                        with
                            | :? System.Collections.Generic.KeyNotFoundException ->
                                let messageToLog =
                                    ["Отправляем уведомление для пользователя {@Login} ({@Id})."
                                     "Назначенное время рассылки - {@NotificationTime}." ]
                                    |> String.concat " "
                                
                                Log.Debug(messageToLog, user.Login, user.Id, user.NotificationTime)
                                
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
                                |> Executor.UnwrapResult
                                |> Executor.SendAsync cfg user.Connectors.Telegram.Id
                                
                            | _ -> async { () }
                        )
                
                // TODO: https://github.com/fsharp/fslang-suggestions/issues/706
                do! async {
                    for userTask in userTasks do
                        do! userTask
                        do! Async.Sleep UserDelay
                }    
                
                return ()
            }
