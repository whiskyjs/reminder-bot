namespace ReminderBot.Model

open System.Text.Json.Serialization

module Redmine =
    type Link =
        { id: int }

    type NamedLink =
        { id: int
          name: string }

    type CustomField =
        { id: int
          name: string
          value: string }

    type User =
        { id: int
          login: string
          firstname: string
          lastname: string
          mail: string
          created_on: string
          last_login_on: string }

    type UserList =
        { users: User array
          total_count: int
          offset: int
          limit: int }

    type Comment =
        { id: int
          user: NamedLink
          notes: string
          created_on: string
          private_notes: bool }

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
          due_date: string option
          done_ratio: int
          is_private: bool
          estimated_hours: float option
          custom_fields: CustomField array option
          created_on: string
          updated_on: string
          closed_on: string option
          journals: Comment array option }

    type IssueList =
        { issues: Issue array
          total_count: int
          offset: int
          limit: int }
        
    type IssueDetail =
        { issue: Issue }

    type TimeEntry =
        { id: int
          project: NamedLink
          issue: Link option
          user: NamedLink
          activity: NamedLink
          hours: float
          comments: string
          spent_on: string
          created_on: string
          updated_on: string }

    type TimeEntryList =
        { time_entries: TimeEntry array
          total_count: int
          offset: int
          limit: int }
