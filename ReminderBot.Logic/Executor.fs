namespace ReminderBot.Logic

open Microsoft.Extensions.Configuration

open ReminderBot.Client.Bot
open ReminderBot.Errors
open ReminderBot.Model.Logic

module Executor =
    let Execute resolution =
        match resolution with
            | Resolution.Async result ->
                async {
                    try
                        return! result
                    with
                        | LogicError msg ->
                            return msg
                        | _ ->
                            return "Ошибка обработки запроса. Напишите в общий чат, что всё сломалось."
                    }
                |> Async.RunSynchronously
            | Resolution.Message msg ->
                msg
                
    let Send (cfg: IConfiguration) chatId message =
        Telegram.sendMessage (cfg.GetValue("App:Bot:Telegram:Token")) (string chatId) message