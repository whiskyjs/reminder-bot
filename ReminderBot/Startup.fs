namespace ReminderBot

open System.Text.Json.Serialization
open System.Threading

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

open ReminderBot.Logic

type Startup private () =
    let notifierInterval = 1000 * 60 * 2
    
    new(configuration: IConfiguration) as this =
        Startup()
        then
            this.Configuration <- configuration
            this.StartNotifier configuration |> ignore

    member this.ConfigureServices(services: IServiceCollection) =
        services
            .AddControllers()
            .AddJsonOptions(fun options -> options.JsonSerializerOptions.Converters.Add(JsonFSharpConverter()))
        |> ignore

    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        if (env.IsDevelopment()) then app.UseDeveloperExceptionPage() |> ignore

        // app.UseHttpsRedirection() |> ignore
        app.UseRouting() |> ignore

        app.UseAuthorization() |> ignore

        app.UseEndpoints(fun endpoints -> endpoints.MapControllers() |> ignore) |> ignore

    member val Configuration: IConfiguration = null with get, set

    member this.StartNotifier cfg =
        let token = CancellationToken()
        
        Timer.DoPeriodicWork (Notifier.Notify cfg) notifierInterval token
        |> Async.Start
        
        token