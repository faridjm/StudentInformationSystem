# Implementation Plan

## Overview

The previous implementation (entities, EF configurations, repository, UnitOfWork, DTOs, Group delivery mode form, Refit client, Admin/Teacher/Student UI scaffolding, the badge helper, localization) is in place. The migration `AddGroupDeliveryModeAndOnlineLessons` is already applied.

This plan covers the **delta** to switch from the meeting-URL flow to the in-system live classroom flow described in design.md: a new SignalR hub, a new `OnlineLessonChatMessage` entity, classroom Razor pages with WebRTC + SignalR JavaScript, service/controller adjustments to drop required URL validation and return classroom URLs, and a follow-up migration `AddOnlineClassroom`.

## Tasks

- [x] 1. Create the OnlineLesson and OnlineLessonParticipant entities
- [x] 2. Add EF configurations and DbContext registration
- [x] 2.1 Create OnlineLesson and OnlineLessonParticipant EF configurations
- [x] 2.2 Register DbSets in AppDbContext
- [x] 3. Add the OnlineLesson repository and wire UnitOfWork
- [x] 3.1 Create the repository interface and implementation
- [x] 3.2 Add OnlineLessons to UnitOfWork
- [x] 4. Add NotificationType values and OnlineLesson DTOs
- [x] 5. Extend the Group DTOs and GroupService for delivery mode

- [ ] 6. Reshape the OnlineLesson data model and DTOs for the in-system classroom
- [x] 6.1 Relax OnlineLesson fields and add the chat entity
  - In `SIS.Entities/Models/OnlineLesson.cs` make `MeetingUrl` and `Platform` nullable; add `ICollection<OnlineLessonChatMessage> ChatMessages`
  - Create `SIS.Entities/Models/OnlineLessonChatMessage.cs` (BaseEntity: OnlineLessonId, SenderUserId, MessageText, SentAt, navs)
  - Update `OnlineLessonConfiguration` to mark `MeetingUrl` and `Platform` not required
  - Create `SIS.DAL/Configurations/OnlineLessonChatMessageConfiguration.cs` (key, MessageText required max 2000, FK to OnlineLesson cascade and to AppUser restrict, index on `(OnlineLessonId, SentAt)`, soft-delete query filter)
  - Add `DbSet<OnlineLessonChatMessage> OnlineLessonChatMessages` to `AppDbContext`
  - _Requirements: 2.1, 2.2, 16.1, 16.6_

- [x] 6.2 Extend the repository for chat
  - Add to `IOnlineLessonRepository` and `OnlineLessonRepository`: `AddChatMessageAsync`, `GetChatMessagesAsync(int lessonId, int take = 200)` (ordered by SentAt asc, includes Sender)
  - Build `SIS.DAL` and confirm no `error CS`
  - _Requirements: 16.1_

- [ ] 7. Adjust the OnlineLesson DTOs and service for the classroom flow
- [x] 7.1 Update DTOs
  - In `SIS.BLL/Services/DTOs/OnlineLesson/OnlineLessonDto.cs` make `MeetingUrl` and `Platform` nullable on `OnlineLessonDto`/`CreateOnlineLessonDto`/`UpdateOnlineLessonDto`
  - Add `StartOnlineLessonResultDto { LessonId, ClassroomUrl }`
  - Extend `JoinOnlineLessonResultDto` with `LessonId` and `ClassroomUrl`
  - Add `OnlineLessonChatMessageDto { Id, OnlineLessonId, SenderUserId, SenderName, MessageText, SentAt }`
  - _Requirements: 4.1, 8.1, 17.1_

- [x] 7.2 Adjust IOnlineLessonService and remove URL validation
  - Change `StartOnlineLessonAsync` return type to `Task<StartOnlineLessonResultDto>`
  - Add to the interface: `Task<bool> CanAccessClassroomAsync(int lessonId, string userId)`, `Task<IEnumerable<OnlineLessonChatMessageDto>> GetClassroomChatHistoryAsync(int lessonId, string requestingUserId)`, `Task<OnlineLessonChatMessageDto> SaveClassroomChatMessageAsync(int lessonId, string senderUserId, string text)`
  - In `OnlineLessonService` remove `ValidateMeetingUrl` calls from create/start/update; allow null `MeetingUrl`/`Platform`
  - Implement `CanAccessClassroomAsync`: load lesson + group; resolve `IsTeacherOwner` via `Teacher.AppUserId == userId`; resolve `IsEnrolledStudent` by checking enrollment of `Student.AppUserId == userId` in lesson's group
  - Implement `GetClassroomChatHistoryAsync` (calls `CanAccessClassroomAsync` then maps repo result)
  - Implement `SaveClassroomChatMessageAsync` (calls `CanAccessClassroomAsync`, persists, returns DTO with `SenderName` resolved from `AppUser.FullName`)
  - Update `StartOnlineLessonAsync` to return `StartOnlineLessonResultDto { LessonId, ClassroomUrl = $"/Teacher/OnlineLessons/Classroom/{lesson.Id}" }`
  - Update `JoinOnlineLessonAsync` to set `ClassroomUrl = $"/Student/OnlineLessons/Classroom/{lesson.Id}"` and `LessonId` on the success result
  - Update notification `RelatedUrl` in start to point at `/Student/OnlineLessons/Classroom/{lesson.Id}`
  - Build `SIS.BLL` and confirm no `error CS`
  - _Requirements: 3.1, 4.1, 4.3, 8.1, 8.4, 14.5, 14.6, 17.1, 17.2_

- [x] 8. Add classroom REST endpoints and update the Refit client
  - In `OnlineLessonController`: change `Start` action to return `StartOnlineLessonResultDto`; add `[HttpGet("{id}/chat")]` (returns chat history for authorized callers) and `[HttpPost("{id}/chat")]` (fallback save) — both call the service which performs the access check
  - In `IOnlineLessonApi` (Refit): change `StartAsync` return type to `StartOnlineLessonResultDto`; ensure `JoinAsync` returns the updated `JoinOnlineLessonResultDto`; add `GetChatAsync(int id)` and `SendChatAsync(int id, [Body] SendChatDto)` (a small `SendChatDto { string Text }` DTO)
  - Build `SIS.API` and `SIS.UI` and confirm no `error CS`
  - _Requirements: 4.1, 8.1, 17.1, 17.2_

- [x] 9. Create the SignalR OnlineClassHub
  - Create `SIS.API/Hubs/OnlineClassHub.cs` with `[Authorize]` and methods `JoinLesson`, `LeaveLesson`, `SendOffer`, `SendAnswer`, `SendIceCandidate`, `SendClassMessage` exactly as specified in design.md
  - Each method that touches the lesson calls `IOnlineLessonService.CanAccessClassroomAsync(lessonId, UserId)` and throws `HubException("Forbidden")` on denial
  - `JoinLesson` adds the connection to group `online-lesson-{lessonId}` and broadcasts `UserJoined` to others
  - `SendOffer`/`SendAnswer`/`SendIceCandidate` use `Clients.Client(targetConnectionId).SendAsync(...)` for direct peer routing
  - `SendClassMessage` saves via `SaveClassroomChatMessageAsync` and broadcasts `ReceiveClassMessage` to the group
  - Map the hub in `SIS.API/Program.cs`: `app.MapHub<OnlineClassHub>("/hubs/online-class");`
  - Add an `IOnlineClassNotifier` (SIS.BLL interface) + `OnlineClassNotifier` (SIS.API implementation using `IHubContext<OnlineClassHub>`) so `EndOnlineLessonAsync` can broadcast `LessonEnded` to the lesson group; register it in `SIS.API/Extensions/ServiceExtensions.cs`
  - Inject `IOnlineClassNotifier` into `OnlineLessonService` and call it from `EndOnlineLessonAsync` and `CancelOnlineLessonAsync`
  - Build the API and confirm no `error CS`
  - _Requirements: 4.3, 5.3, 6.2, 8.1, 14.5, 14.6_

- [ ] 10. Replace the meeting-URL UI with the in-system classroom UI
- [x] 10.1 Drop the URL form fields and add an Edit Title modal
  - In Teacher `Areas/Teacher/Views/Courses/Detail.cshtml` schedule tab: remove the meeting URL/platform inputs from the Create modal (keep Title and Description); rename the Edit Link modal to **Edit Title**
  - In Admin `Group/Create.cshtml` and `Group/Edit.cshtml`: keep the delivery-mode select but mark the platform/URL fields as optional with a note "Daxili classroom üçün lazım deyil; xarici link saxlamaq istəyirsənsə doldur"
  - In Admin `Group/Index.cshtml` and `Group/Detail.cshtml`: hide the masked meeting-URL preview when empty (already conditional)
  - _Requirements: 1.5, 1.6, 1.8, 7.1_

- [x] 10.2 Teacher schedule actions point to the classroom
  - When Start succeeds, redirect to the returned `ClassroomUrl` (server returns it via `StartOnlineLessonResultDto`)
  - When status is `Live`, replace the "Join" link with **Open Classroom** (anchor to `/Teacher/OnlineLessons/Classroom/{id}`)
  - Keep **End Lesson** (calls existing endpoint)
  - _Requirements: 4.1, 5.1, 9.1, 9.2_

- [x] 10.3 Student schedule and dashboard route to the classroom
  - Update the student `joinOnlineLesson` JS so it navigates to `result.classroomUrl` (same window) instead of `window.open(result.meetingUrl, '_blank')`
  - Update the dashboard "next class" join button the same way
  - _Requirements: 8.4, 10.1, 10.3_

- [ ] 11. Build the live classroom Razor pages and JavaScript
- [x] 11.1 Create the Teacher and Student classroom controllers
  - Create `SIS.UI/Areas/Teacher/Controllers/OnlineLessonsController.cs` with `Classroom(int id)` action: load the lesson via `IOnlineLessonApi`, verify access (the API itself enforces this; UI also gates on response shape), pass `IsTeacher = true` to the view
  - Create `SIS.UI/Areas/Student/Controllers/OnlineLessonsController.cs` with `Classroom(int id)` action: same pattern, `IsTeacher = false`
  - Both redirect with a toast if status is `Ended`/`Cancelled`
  - _Requirements: 4.1, 8.1, 14.5, 14.6_

- [x] 11.2 Create the shared classroom Razor view
  - Create `SIS.UI/Areas/Teacher/Views/OnlineLessons/Classroom.cshtml` and `SIS.UI/Areas/Student/Views/OnlineLessons/Classroom.cshtml` — both render through a shared partial `Views/Shared/_OnlineClassroom.cshtml` that takes a small VM (LessonId, Title, GroupName, SessionTopic, IsTeacher, HubUrl, JwtToken)
  - Layout: header with Title + LIVE badge + Group/Topic; left column video grid (local + remotes); bottom toolbar (mic toggle, cam toggle, leave, end-class only when IsTeacher); right column participants list and live chat
  - Wire `@@section Scripts` to load the SignalR client (CDN already used by other hubs in the project) and the classroom script
  - _Requirements: 4.1, 18.1_

- [x] 11.3 Implement classroom.js (WebRTC + SignalR client)
  - Create `SIS.UI/wwwroot/js/online-classroom.js`
  - Mesh WebRTC: maintain `peers[connectionId] -> RTCPeerConnection`; on `UserJoined` create offer to the new peer; on `ReceiveOffer` set remote, create answer; ICE forwarded via `SendIceCandidate`
  - Add local stream tracks before negotiation; render local preview tile; render remote tiles when `ontrack` fires
  - Toolbar: mic/cam toggle (`getTracks().forEach(t => t.enabled = !t.enabled)`), leave (calls `LeaveLesson`, closes peers, navigates back)
  - Teacher-only end-class button hits the REST endpoint `/api/onlinelesson/{id}/end`
  - Chat: `SendClassMessage` invoke; on `ReceiveClassMessage` append a `<div>` using `textContent` (no `innerHTML`)
  - On hub start, request chat history through Refit (or REST) once and append messages
  - On `LessonEnded` event: stop tracks, close peers, show "Lesson ended" overlay, redirect after 3s
  - Error handling: friendly messages for permission denied and unsupported browser
  - _Requirements: 4.1, 5.3, 8.1, 11.1, 14.5, 18.3_

- [x] 12. Confirm notification, attendance, and badges still apply
  - Verify start fan-out uses the new `ClassroomUrl` for `RelatedUrl` (covered in task 7.2)
  - The "Mark joined as Present" tab already reads `OnlineLessonParticipant`; no change needed
  - Badge helper (`OnlineLessonBadges`) is already shared and used; no change needed
  - _Requirements: 4.3, 12.1, 12.2, 12.4, 18.1, 18.2, 18.3_

- [x] 13. Final build verification and migration handoff
  - Build `SIS.slnx` and confirm `0 Hata`; ignore MSB3021/MSB3027 (dev-server file locks)
  - Hand the user the exact commands for Visual Studio Package Manager Console (Default project: `SIS.DAL`, Startup: `SIS.API`):
    `Add-Migration AddOnlineClassroom` then `Update-Database`
  - Confirm the migration relaxes `OnlineLessons.MeetingUrl`/`Platform` to nullable and creates `OnlineLessonChatMessages`
  - _Requirements: 16.2, 16.5, 16.6_

## Task Dependency Graph

```json
{
  "waves": [
    { "wave": 1, "tasks": ["6.1"] },
    { "wave": 2, "tasks": ["6.2"] },
    { "wave": 3, "tasks": ["7.1"] },
    { "wave": 4, "tasks": ["7.2"] },
    { "wave": 5, "tasks": ["8"] },
    { "wave": 6, "tasks": ["9"] },
    { "wave": 7, "tasks": ["10.1", "10.2", "10.3", "11.1"] },
    { "wave": 8, "tasks": ["11.2"] },
    { "wave": 9, "tasks": ["11.3"] },
    { "wave": 10, "tasks": ["12"] },
    { "wave": 11, "tasks": ["13"] }
  ]
}
```

```
6.1 (entity + chat config + DbSet)
   │
   ▼
6.2 (repo chat methods)
   │
   ▼
7.1 (DTO updates) ─► 7.2 (service: drop URL validation, classroom URLs, access check, chat methods)
                                                        │
                                                        ▼
                                              8 (REST + Refit chat endpoints)
                                                        │
                                                        ▼
                                              9 (OnlineClassHub + IOnlineClassNotifier)
                                                        │
              ┌─────────────────────────────────────────┼─────────────────────────────────────────┐
              ▼                                         ▼                                         ▼
   10.1 (drop URL inputs)                   10.2 (teacher → classroom)               11.1 (classroom controllers)
              │                                         │                                         │
              └────────────────┬────────────────────────┘                                         │
                               ▼                                                                  ▼
                  10.3 (student → classroom)                                          11.2 (classroom Razor partial)
                                                                                                  │
                                                                                                  ▼
                                                                                  11.3 (classroom.js: WebRTC + SignalR)
                                                                                                  │
                                                                                                  ▼
                                                                                  12 (notification/attendance/badges)
                                                                                                  │
                                                                                                  ▼
                                                                                  13 (build + migration handoff)
```

## Notes

- **Build verification:** after each layer, run `dotnet build SIS.XXX\SIS.XXX.csproj` and scan only for `error CS`. MSB3021/MSB3027 are dev-server file locks, not code errors.
- **Migration:** never run `Add-Migration`/`Update-Database` automatically. Task 13 hands the exact commands to the user.
- **No external links:** `MeetingUrl`/`Platform` stay in the schema as nullable for backward compatibility but are not used by the classroom flow.
- **Mesh WebRTC only:** suitable for small classes. The `OnlineClassHub` carries a code comment noting that an SFU is required for production scale; not implemented here.
- **XSS:** classroom chat is stored verbatim and rendered via `textContent` (live append) and Razor HTML-encoding (history). No `innerHTML` for user-provided content.
- **JWT for SignalR:** existing hubs (`/hubs/chat`, `/hubs/notification`) already use the JWT-via-query pattern for WebSocket auth; the new hub piggybacks on the same authentication pipeline.
- **Code comments** in Azerbaijani per project convention.
