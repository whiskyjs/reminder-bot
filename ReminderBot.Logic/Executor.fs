namespace ReminderBot.Logic

open Microsoft.Extensions.Configuration

open Serilog

open ReminderBot.Client.Bot
open ReminderBot.Errors
open ReminderBot.Model.Logic

module Executor =
    let UnwrapResult resolution =
        match resolution with
            | Resolution.Async result ->
                async {
                    try
                        return! result
                    with
                        | LogicError msg ->
                            return msg
                        | err ->
                            Log.Error(err, "Ошибка обработки запроса.")
                            
                            return "Ошибка обработки запроса. Напишите в общий чат, что всё сломалось."
                    }
            | Resolution.Message msg ->
                async { return msg }
                
    let SendAsync (cfg: IConfiguration) chatId message =
        async {
            let! str = message
            
            do! Telegram.sendMessage (cfg.GetValue("App:Bot:Telegram:Token")) (string chatId) str
        }