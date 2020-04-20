namespace ReminderBot.Model.Telegram

open Microsoft.Extensions.Configuration

open ReminderBot.Model
open ReminderBot.Model.User

module Notification =
    type From =
        { id: int
          is_bot: bool
          first_name: string option
          last_name: string option
          username: string option
          language_code: string option }

    type Chat =
        { id: int
          first_name: string option
          last_name: string option
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
          message: Message option
          edited_message: Message option }
        
    let ExtractMessage notification =
        match notification.message with
            | Some message -> message
            | None ->
                match notification.edited_message with
                | Some message -> message
                | None -> failwith "Невозможно извлечь сообщение из уведомления"

    let ExtractConnector notification =
        let message = ExtractMessage notification

        { Id = message.from.id
          Name =
              match message.from.first_name with
              | Some value -> value
              | None -> ""
          Username =
              match message.from.username with
              | Some string -> string
              | _ -> "Логин отсутствует"
          Language =
              match message.from.language_code with
              | Some value -> value
              | None -> "" }
        
    let EmptyConnector =
        { Id = 0
          Name = ""
          Username = ""
          Language = "" }
        
    let ExtractText notification =
        let message = ExtractMessage notification
        
        message.text.Trim()

    let BuildUser (remoteUser: Redmine.User) relatedData (сfg: IConfiguration): User.This =
        { Id = remoteUser.id
          Login = remoteUser.login
          NotificationTime = сfg.GetValue("App:Defaults:NotificationTime")
          Connectors = { Telegram = relatedData } }
 