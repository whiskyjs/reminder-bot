namespace ReminderBot.Logic

open System.Text.RegularExpressions

open Microsoft.Extensions.Configuration

open ReminderBot.Client.Bot
open ReminderBot.Client.Service
open ReminderBot.Client.Service.Redmine
open ReminderBot.Model
open ReminderBot.Model.Telegram
open ReminderBot.Model.Logic
open ReminderBot.Errors
open ReminderBot.Helpers
open ReminderBot.Model.User

module Resolver =
    module Command =
        [<Literal>]
        let Start = "/start"
        
        [<Literal>]
        let IAm = "/iam"
        
        [<Literal>]
        let Time = "/time"
        
        [<Literal>]
        let Stats = "/stats"
    
    let FetchUsers telegramId =
        let rows =
            User.This.FindByConnector(fun connectors ->
                match connectors with
                | connectors when connectors.Telegram.Id = telegramId -> true
                | _ -> false)
            
        rows
        
    let FetchUser telegramId =
        let rows = FetchUsers telegramId
        
        let user =
            match rows with
            | _ when Seq.isEmpty rows ->
                LogicError "Пользователь не найден. Пожалуйста, зарегистрируйтесь в системе." |> raise
            | _ -> Seq.head rows
            
        user
        
    let GetIssueUrl baseUrl issueId commentIndex =
        Url(baseUrl)
            .Path(sprintf "/issues/%d/" issueId)
            .Fragment(if commentIndex > 0 then sprintf "note-%d" commentIndex else "")
            .ToString()

    let Resolve (cfg: IConfiguration) (connector: User.Telegram) (message: string): Resolution =
        match message.Split(" ") |> Array.toList with
        | [ Command.Start ] ->
            [ "Для подписки на уведомления укажите свой логин с помощью команды /iam ЛОГИН."
              "Для изменения времени рассылки уведомления используйте команду /time ЧЧ:ММ"
              "Время по умолчанию - 17:00." ]
            |> String.concat "\n"
            |> Resolution.Message

        | [ Command.IAm; login ] ->
            async {
                let! remoteUsers = Config.GetRedmineAuth cfg ||> Redmine.Users.getList Query

                let remoteUser =
                    try
                        remoteUsers |> Array.find (fun user -> user.login = login)
                    with
                        | :? System.Collections.Generic.KeyNotFoundException ->
                            LogicError"Пользователь с указанным логином не найден в базе."  |> raise                    
                
                // Если Telegram-аккаунт привязан к неким другим Redmine-пользователям - отвязываем и сообщаем
                // кто именно инициировал отвязку
                let notify telegramId login = async {
                    let unsubscribeMsg =
                        [ sprintf "Аккаунт отключен от получения уведомлений пользователя %s." login
                          sprintf
                            "Инициатор: %s (%s) "
                            connector.Name
                            connector.Username ]
                        |> String.concat "\n"
                    
                    do!
                        Telegram.sendMessage
                            (cfg.GetValue("App:Bot:Telegram:Token"))
                            (telegramId |> string)
                            unsubscribeMsg
                        |> Async.Ignore
                    }
                
                do!
                    let unsubscribe record login = async {
                        record.Connectors.SetTelegram Notification.EmptyConnector
                        |> record.SetConnectors
                        |> User.This.Update
                        |> ignore
                        
                        do! notify (record.Connectors.Telegram.Id) login
                        }
                        
                    User.This.FindAll
                        |> Seq.cast<User.This>
                        |> Seq.filter(fun u -> u.Connectors.Telegram.Id > 0)
                        |> Seq.map(fun user -> async {
                            match user with
                            | u when u.Connectors.Telegram.Id = connector.Id && remoteUser.id <> user.Id ->
                               do! unsubscribe user user.Login
                            | u when u.Connectors.Telegram.Id <> connector.Id && remoteUser.id = user.Id ->
                               do! unsubscribe user remoteUser.login
                            | _ ->
                                ()
                            })
                    |> Async.Parallel
                    |> Async.Ignore

                // Создаем или обновляем профиль Redmine-пользователя в БД                        
                match User.This.Get remoteUser.id with
                    | None ->
                        Notification.BuildUser remoteUser connector cfg
                        |> User.This.Insert
                    | Some(user) ->
                        user.Connectors.SetTelegram connector
                        |> user.SetConnectors
                        |> User.This.Update
                |> ignore
                    
                let message =
                    [ sprintf "Пользователь %s найден в БД." remoteUser.login
                      "Для изменения времени рассылки уведомления используйте команду /time ЧЧ:ММ"
                      "Время по умолчанию - 17:00." ]
                    |> String.concat "\n"

                return message
            }
            |> Resolution.Async

        | [ Command.Time; time ] ->
            if Regex.IsMatch(time, "\d{2}:\d{2}") |> not then LogicError "Некорректный формат времени." |> raise

            let user = FetchUser connector.Id

            { user with NotificationTime = time }
            |> User.This.Update
            |> ignore

            sprintf "Новое время отправки уведомлений: %s." time |> Resolution.Message

        | [ Command.Stats ] ->
            async {
                let user = FetchUser connector.Id
                
                // Запрашиваем трудозатраты за сегодня
                let! entries = Config.GetRedmineAuth cfg ||> Redmine.TimeEntries.getList ([
                    "from", Date.CurrentDateStr ();
                    "to", Date.CurrentDateStr ()
                    "user_id", user.Id |> string
                    ] |> Map.ofList)
                
                // Делим на 2 части - привязанные к задачам и нет
                let issueEntries, issuelessEntries =
                    entries
                    |> Array.partition(fun entry -> entry.issue.IsSome)
                    
                // Собираем ID связанных с трудозатратами задач
                let entryIssueIds =
                    issueEntries |> Array.fold (fun acc entry ->
                        match entry.issue with
                        | Some issue -> (acc, [|issue.id|]) ||> Array.append
                        | _ -> acc
                        ) [||]
                
                // Запрашиваем задачи, обновленные сегодня 
                let! updatedIssues = Config.GetRedmineAuth cfg ||> Redmine.Issues.getList ([
                    "updated_on", Date.CurrentDateStr ()
                    ] |> Map.ofList)
                
                // Собираем их ID
                let updatedIssueIds =
                    updatedIssues
                    |> Array.map(fun issue -> issue.id)
                    
                // Общий массив issue ID, по которым нужно вытащить все данные
                let totalIssueIds =
                    (entryIssueIds, updatedIssueIds)
                    ||> Array.append
                    |> Array.distinct
                    
                // Запрашиваем детальные данные пачками по 5 штук и добавляем задержку, чтобы Redmine не утонул
                let detailedIssues =
                    totalIssueIds
                    |> Array.chunk 5
                    |> Array.map(fun ids ->
                            ids
                            |> Array.map(fun id ->
                                Config.GetRedmineAuth cfg ||> Redmine.Issues.getDetail ([
                                        "include", "journals"
                                    ] |> Map.ofList) id
                                )
                            |> Async.Parallel
                            )
                    |> Array.fold (fun (acc: Redmine.Issue[]) chunk ->
                        async {
                            let! chunkIssues = chunk
                            
                            do! Async.Sleep(200)
                            
                            return (acc, chunkIssues) ||> Array.append
                        } |> Async.RunSynchronously) [||]
                    
                // Фильтруем полученный список и подготавливаем данные для отправки.
                // Issue должна содержать либо комментарий пользователя за сегодня, либо быть привязана
                // к трудозатратам за сегодня
                let preparedIssues =
                    detailedIssues
                    |> Array.fold(fun acc issue ->
                        let lastCommentIndexToday =
                            try
                                issue.journals
                                |> function
                                    | Some comments -> comments
                                    | _ -> [||]
                                |> Array.findIndexBack (fun comment ->
                                    comment.user.id = user.Id && (Date.IsToday (comment.created_on))     
                                    )
                                |> (+) 1
                            with 
                            | :? System.Collections.Generic.KeyNotFoundException ->
                                0
                                
                        let issueEntries =
                            issueEntries |> Array.filter(fun entry ->
                                match entry.issue with
                                | Some associatedIssue -> associatedIssue.id = issue.id
                                | _ -> false
                                )
                            
                        if issueEntries.Length > 0 || lastCommentIndexToday > 0 then
                            let issueHours = issueEntries |> Array.fold (fun acc entry -> acc + entry.hours) 0.0
                            
                            let issueUrl = GetIssueUrl (Config.GetRedmineUrl cfg) issue.id lastCommentIndexToday
                                
                            let issueLink = sprintf "[%s](%s)" issue.subject issueUrl
                            
                            [|(issueLink, issueHours)|] |> Array.append acc
                        else
                            acc
                        ) [||]
                    
                let hours =
                    entries
                    |> Array.fold (fun acc entry -> acc + entry.hours) 0.0
                    
                let issuelessHours =
                    issuelessEntries
                    |> Array.fold (fun acc entry -> acc + entry.hours) 0.0 
                    
                let message =
                    [| sprintf "*Всего часов за сегодня: %.2f ч.*" hours
                       "Проверьте, по всем ли задачам выставлены часы."
                       match detailedIssues with
                       | [||] ->
                           "Вы сегодня не отметились ни в одной задаче."
                       | _ ->
                            preparedIssues
                            |> Array.fold (fun acc (link, hours) ->
                                [| match hours with
                                   | 0.0 -> link
                                   | _ -> sprintf "%s - %.2f ч." link hours |]
                                |> Array.append acc                                
                                ) [|"*Активность за сегодня:*"|]
                            |> (fun lines ->
                                match issuelessEntries with
                                | [||] -> lines
                                | _ ->
                                    [| sprintf "*Также:* %.2f ч., не привязанных к задачам." issuelessHours |]
                                    |> Array.append lines
                                )
                            |> String.concat "\n"
                        |]
                    
                
                return message |> (String.concat "\n")
            }
            |> Resolution.Async
            
        | _ ->
            "Неизвестная команда или некорректный синтаксис." |> Resolution.Message
