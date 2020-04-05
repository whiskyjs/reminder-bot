namespace ReminderBot.Model

open System.Text.Json.Serialization

module Redmine =
    [<JsonFSharpConverter>]
    type Link =
        { id: int }

    [<JsonFSharpConverter>]
    type NamedLink =
        { id: int
          name: string }

    [<JsonFSharpConverter>]
    type CustomField =
        { id: int
          name: string
          value: string }

    [<JsonFSharpConverter>]
    type User =
        { id: int
          login: string
          firstname: string
          lastname: string
          mail: string
          created_on: string
          last_login_on: string }

    [<JsonFSharpConverter>]
    type UserList =
        { users: User array
          total_count: int
          offset: int
          limit: int }

    [<JsonFSharpConverter>]
    type Comment =
        { id: int
          user: NamedLink
          notes: string
          created_on: string
          private_notes: bool }

    [<JsonFSharpConverter>]
    type Issue =
        { id: int
          project: NamedLink
          tracker: NamedLink
          status: NamedLink
          priority: NamedLink
          author: NamedLink
          subject: string
          description: string
          start_date: string
          due_date: string option Skippable
          done_ratio: int
          is_private: bool
          estimated_hours: float option Skippable
          custom_fields: CustomField array
          created_on: string
          updated_on: string
          closed_on: string option Skippable
          journals: Comment array }

    [<JsonFSharpConverter>]
    type IssueList =
        { issues: Issue array
          total_count: int
          offset: int
          limit: int }
        
    [<JsonFSharpConverter>]
    type IssueDetail =
        { issue: Issue }

    [<JsonFSharpConverter>]
    type TimeEntry =
        { id: int
          project: NamedLink
          issue: Link
          user: NamedLink
          activity: NamedLink
          hours: float
          comments: string
          spent_on: string
          created_on: string
          updated_on: string }

    [<JsonFSharpConverter>]
    type TimeEntryList =
        { time_entries: TimeEntry array
          total_count: int
          offset: int
          limit: int }
