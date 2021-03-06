namespace ReminderBot

module Timer =
    open System.Threading
    open System.Threading.Tasks

    let AwaitTaskVoid : (Task -> Async<unit>) = Async.AwaitIAsyncResult >> Async.Ignore

    let DoPeriodicWork (f: unit -> Async<unit>) (milliseconds: int) (token: CancellationToken) =
        async {
            while not token.IsCancellationRequested do
                do! f()
                do! Task.Delay(milliseconds, token) |> AwaitTaskVoid
        }