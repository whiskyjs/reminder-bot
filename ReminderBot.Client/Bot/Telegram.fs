namespace ReminderBot.Client.Bot

open FSharp.Data

open ReminderBot.Helpers

module Telegram =
    module Defaults =
        [<Literal>]
        let baseUrl = "https://api.telegram.org/"
        
    let buildUrl token method chatId text =
        Url
            .Build(Defaults.baseUrl)
            .Path(sprintf "bot%s/%s" token method)
            .Query(["chat_id", chatId; "text", text; "parse_mode", "Markdown"] |> Map.ofList)
            .Uri()
            .ToString()

    let sendMessage token chatId text =
         Http.AsyncRequestString(buildUrl token "sendMessage" chatId text)
         