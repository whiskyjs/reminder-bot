namespace ReminderBot.Helpers

open System

module Array =
    let rec private _chunk<'T> size (acc: 'T[][]) (array: 'T[]) =
        match array with
        | [||] -> acc
        | _ -> 
            let count = Math.Min(Array.length(array), size)

            ([| Array.take count array |] |> Array.append acc, Array.skip count array)
            ||> _chunk size 
    
    let chunk size array =
        if size <= 0 then failwithf "Размер чанка должен быть больше 0, передано: '%A'" size 
        
        _chunk size [||] array