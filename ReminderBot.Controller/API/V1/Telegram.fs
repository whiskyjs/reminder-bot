namespace ReminderBot.Controller.API.V1.Telegram

open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.Mvc

open ReminderBot.Model.Telegram
open ReminderBot.Logic

[<ApiController>]
[<Route("/api/v1/telegram/update/")>]
type Update(_logger: ILogger<Update>) =
    inherit ControllerBase()
    
    [<HttpGet>]
    member this.Get(): string = "¯\_(ツ)_/¯"
    
    [<HttpPost>]
    member this.Post([<FromServices>] cfg: IConfiguration, [<FromBody>] notification: Notification.This) =
        let connector, message =
            Notification.ExtractConnector notification,
            Notification.ExtractText notification
        
        (connector, message)
        ||> Resolver.Resolve cfg
        |> Executor.Execute
        |> Executor.Send cfg connector.Id
