namespace ReminderBot.Controller.API.V1.Telegram

open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Mvc

open ReminderBot.Model.Telegram
open ReminderBot.Logic

[<ApiController>]
[<Route("/api/v1/telegram/update/")>]
type Update() =
    inherit ControllerBase()
    
    [<HttpPost>]
    member this.Post([<FromServices>] cfg: IConfiguration, [<FromBody>] notification: Notification.This) =
        let connector, message =
            Notification.ExtractConnector notification,
            Notification.ExtractText notification
        
        (connector, message)
        ||> Resolver.Resolve cfg
        |> Executor.UnwrapResult
        |> Executor.SendAsync cfg connector.Id
        |> Async.Start
