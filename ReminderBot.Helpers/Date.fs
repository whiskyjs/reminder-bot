namespace ReminderBot.Helpers

open System

module Date =
    let CurrentDateStr () =
        DateTime.Now.ToString("yyyy-MM-dd")
    
    let IsToday str =
        let now = CurrentDateStr () |> DateTime.Parse
        let date = str |> DateTime.Parse
        
        date.Year = now.Year && date.Month = now.Month && date.Day = now.Day

    let IsHoliday str =
        let date = 
            match box str with
            | :? string as str -> DateTime.Parse str 
            | :? DateTime as date -> date
            | rest -> failwithf "Формат даты не поддерживается: %A" rest
            
        date.DayOfWeek = DayOfWeek.Saturday || date.DayOfWeek = DayOfWeek.Sunday 
        
    let IsHolidayToday () =
        IsHoliday (CurrentDateStr () |> DateTime.Parse)