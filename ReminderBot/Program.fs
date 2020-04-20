namespace ReminderBot

open System

open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting

open Serilog
open Serilog.Events

module Program =
    [<Literal>]
    let SuccessCode = 0
    
    [<Literal>]
    let ErrorCode = -1
    
    [<Literal>]
    let LogPath = "Logs/ReminderBot.log"

    let CreateHostBuilder args =
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(fun webBuilder ->
                webBuilder.UseStartup<Startup>() |> ignore
            )
            .UseSerilog()
        
    [<EntryPoint>]
    let main args =
        Log.Logger <-
            LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(LogPath,
                              rollingInterval = RollingInterval.Day,
                              fileSizeLimitBytes = Nullable(1024L * 1024L),
                              rollOnFileSizeLimit = true
                              )
                .WriteTo.Console()
                .CreateLogger()
                
        try
            try
                Log.Information("Начало работы.")
                CreateHostBuilder(args).Build().Run()
                Log.Information("Завершение работы.")
                SuccessCode
            with
                error ->
                    Log.Fatal(error, "Неожиданное завершение работы.")
                    ErrorCode
        finally
            Log.CloseAndFlush();                
