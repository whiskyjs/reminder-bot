namespace ReminderBot.Helpers

open Microsoft.Extensions.Configuration

module Config =
    let GetRedmineUrl (cfg: IConfiguration) = cfg.GetValue("App:Service:Redmine:Url")
    
    let GetRedmineToken (cfg: IConfiguration) = cfg.GetValue("App:Service:Redmine:Token")
    
    let GetRedmineAuth (cfg: IConfiguration) =
        (GetRedmineUrl cfg, GetRedmineToken cfg)