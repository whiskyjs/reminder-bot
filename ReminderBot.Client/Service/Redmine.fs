namespace ReminderBot.Client.Service

open FSharp.Data

open System.Text.Json

open ReminderBot.Helpers
open ReminderBot.Model.Redmine

module Redmine =
    let Query: Map<string, string> =
        Map.empty
    
    let private call<'T> url token path query =
        let serviceUrl =
            Url(url)
                .Path(path)
                .Query(query)

        async {
            let! response =
                Http.AsyncRequestString(serviceUrl.ToString(), [], [("X-Redmine-API-Key", token)])
                
            return response |> JsonSerializer.Deserialize<'T>
        }

    module Users =
        let rec private _getList query url token acc =
            async {
                let! userList =
                    call<UserList> url token "/users.json" query
                    
                let acc = Array.append acc (userList.users)
                        
                return!
                    match acc |> Array.length >= userList.total_count with
                    | false ->
                        let nextQuery =
                            let nextOffset = query.Item("offset") |> int |> (+) 100 |> string
                            query.Add("offset", nextOffset)
                        
                        _getList nextQuery url token acc
                    | _ ->
                        async { return acc }
            }

        let getList query url token =
            let acc = [||]
            
            let query =
                query
                |> Map.add "limit" "100"
                |> Map.add "offset" (acc |> Array.length |> string)
            
            _getList query url token acc

    module TimeEntries =
        let rec private _getList query url token acc =
            async {
                let! timeEntryList =
                    call<TimeEntryList> url token "/time_entries.json" query
                   
                let acc = Array.append acc (timeEntryList.time_entries)

                return!
                    match acc |> Array.length >= timeEntryList.total_count with
                    | false ->
                        let nextQuery =
                            let nextOffset = query.Item("offset") |> int |> (+) 100 |> string
                            query.Add("offset", nextOffset)
                        
                        _getList nextQuery url token acc
                    | _ ->
                        async { return acc }
            }

        let getList query url token =
            let acc = [||]
            
            let query =
                query
                |> Map.add "limit" "100"
                |> Map.add "offset" (acc |> Array.length |> string)
            
            _getList query url token acc

    module Issues =
        let rec private _getList query url token acc =
            async {
                let! issueList =
                    call<IssueList> url token "/issues.json" query
                    
                let acc = Array.append acc (issueList.issues)

                return!
                    match acc |> Array.length >= issueList.total_count with
                    | false ->
                        let nextQuery =
                            let nextOffset = query.Item("offset") |> int |> (+) 100 |> string
                            query.Add("offset", nextOffset)
                            
                        _getList nextQuery url token acc
                    | _ ->
                        async { return acc }
            }

        let getList query url token =
            let acc = [||]
            
            let query =
                query
                |> Map.add "limit" "100"
                |> Map.add "offset" (acc |> Array.length |> string)
            
            _getList query url token acc
        
        let getDetail query issueId url token =
            async {
                let! {issue = issue} = call<IssueDetail> url token (sprintf "/issues/%d.json" issueId) query
                
                return issue
            }