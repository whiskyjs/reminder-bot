namespace ReminderBot.Helpers

open Microsoft.AspNetCore.Http.Extensions
open Microsoft.Extensions.Primitives

type Url(uri: string) =
    let builder: System.UriBuilder = System.UriBuilder(uri)

    static member DefaultUrl = ""

    static member Build(?uri: string) =
        match uri with
        | Some(str) -> Url(str)
        | None -> Url(Url.DefaultUrl)

    member this.Scheme scheme =
        builder.Scheme <- scheme
        this

    member this.Password password =
        builder.Password <- password
        this

    member this.UserName username =
        builder.UserName <- username
        this

    member this.Host host =
        builder.Host <- host
        this

    member this.Port port =
        builder.Port <- port
        this

    member this.Path path =
        builder.Path <- path
        this

    member this.Query list =
        let query = QueryBuilder()
        
        list
        |> Map.iter (fun key (value: string) -> query.Add(key, value |> StringValues))
        
        builder.Query <- query.ToQueryString().ToString()
        this

    member this.Fragment fragment =
        builder.Fragment <- fragment
        this

    member this.Uri() = builder.Uri

    override this.ToString() = builder.Uri.ToString()
