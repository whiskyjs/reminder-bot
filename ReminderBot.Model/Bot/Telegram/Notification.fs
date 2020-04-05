namespace ReminderBot.Model.Telegram

open System.Text.Json.Serialization

open Microsoft.Extensions.Configuration

open ReminderBot.Model
open ReminderBot.Model.User

module Notification =
    [<JsonFSharpConverter>]
    type From =
        { id: int
          is_bot: bool
          first_name: string
          username: string option Skippable
          language_code: string }

    [<JsonFSharpConverter>]
    type Chat =
        { id: int
          first_name: string
          username: string option Skippable
          ``type``: string }

    [<JsonFSharpConverter>]
    type Message =
        { message_id: int
          from: From
          chat: Chat
          date: int
          text: string }

    [<JsonFSharpConverter>]
    type This =
        { update_id: int
          message: Message }

    let ExtractConnector notification =
        let { message = message } = notification

        { Id = message.from.id
          Name = message.from.first_name
          Username =
              match message.from.username with
              | Include option ->
                  match option with
                  | Some string -> string
                  | _ -> "Логин отсутствует"
              | _ -> "Логин отсутствует"
          Language = message.from.language_code }
        
    let EmptyConnector =
        { Id = 0
          Name = ""
          Username = ""
          Language = "" }
        
    let ExtractText notification =
        notification.message.text.Trim()

    let BuildUser (remoteUser: Redmine.User) relatedData (сfg: IConfiguration): User.This =
        { Id = remoteUser.id
          Login = remoteUser.login
          NotificationTime = сfg.GetValue("App:Defaults:NotificationTime")
          Connectors = { Telegram = relatedData } }
 
