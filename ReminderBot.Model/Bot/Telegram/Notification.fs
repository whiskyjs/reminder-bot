namespace ReminderBot.Model.Telegram

open System.Text.Json.Serialization

open Microsoft.Extensions.Configuration

open ReminderBot.Model
open ReminderBot.Model.User

module Notification =
    type From =
        { id: int
          is_bot: bool
          first_name: string
          username: string option
          language_code: string }

    type Chat =
        { id: int
          first_name: string
          username: string option
          ``type``: string }

    type Message =
        { message_id: int
          from: From
          chat: Chat
          date: int
          text: string }

    type This =
        { update_id: int
          message: Message }

    let ExtractConnector notification =
        let { message = message } = notification

        { Id = message.from.id
          Name = message.from.first_name
          Username =
              match message.from.username with
              | Some string -> string
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
 