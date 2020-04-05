namespace ReminderBot.Helpers

open System

module Date =
    let CurrentDateStr () = DateTime.Now.ToString("yyyy-MM-dd")
    
    let IsToday str =
        let now = DateTime.Now
        let date = str |> DateTime.Parse
        
        date.Year = now.Year && date.Month = now.Month && date.Day = now.Day
