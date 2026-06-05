# Design Document

## Overview

This feature adds a **Group Delivery Mode** capability and a **system-internal Live Classroom** to Acadex SIS. The classroom is built on **WebRTC peer-to-peer media** (mesh topology) with **SignalR signaling** — no external Zoom/Meet/Teams integration. Teachers start a live class for a `ClassSession`; students join from their panel into a Razor classroom page that runs `getUserMedia` + `RTCPeerConnection` in the browser, exchanging offers/answers/ICE candidates through an authorized SignalR hub. The classroom also hosts an in-class live chat and a participant list.

This is intentionally a final-project-grade demo. Mesh WebRTC scales to ~1 teacher + a small number of students. A code comment in the hub notes that production-grade scale needs an SFU; the SFU is **not** implemented.

The feature touches every layer of the existing architecture (`SIS.Entities` → `SIS.DAL` → `SIS.BLL` → `SIS.API` → `SIS.UI` via Refit) and adds one EF Core migration applied manually in Visual Studio.

The design reuses existing conventions:
- `BaseEntity` (Id, CreatedAt, UpdatedAt, IsDeleted) for entities.
- Repository + lazy `??=` UnitOfWork pattern.
- `IEntityTypeConfiguration<T>` discovered by `ApplyConfigurationsFromAssembly`.
- Thin API controllers delegating to BLL services, consumed by Refit clients.
- `IAuditLogService.LogAsync` and `INotificationService.SendToManyAsync` for side effects.
- `AppException` types (`NotFoundException`, `BadRequestException`, `UnauthorizedException`).
- SignalR hubs follow the existing `ChatHub`/`NotificationHub` pattern (`[Authorize]`, mapped under `/hubs/...` in `SIS.API/Program.cs`, AddSignalR already registered).

### Current scaffolding state

- Enums exist: `DeliveryMode` (InPerson/Online/Hybrid), `OnlinePlatform` (kept for future external links but not used by the live classroom flow), `OnlineLessonStatus` (Scheduled/Live/Ended/Cancelled).
- `Group` already has `DeliveryMode`, `DefaultOnlinePlatform`, `DefaultMeetingUrl`, and `ICollection<OnlineLesson>`.
- `OnlineLesson` and `OnlineLessonParticipant` entity classes exist, but the design **drops `MeetingUrl`/`Platform` from the primary flow** — `MeetingUrl` becomes optional and unused by the live classroom; the classroom is internal.
- `NotificationType` already has `OnlineLessonStarted` and `OnlineLessonCancelled`.
- A previous migration `AddGroupDeliveryModeAndOnlineLessons` already exists. This design adds **one further migration** for `OnlineLessonChatMessage` and the schema adjustments below.

## Requirements Traceability

| Requirement | Design element |
|---|---|
| R1 Group delivery config | `Group` fields, `GroupConfiguration` columns, `GroupService` mapping, Admin Create/Edit forms + JS toggle |
| R2 Data model | `OnlineLesson`, `OnlineLessonParticipant`, `OnlineLessonChatMessage` entities + configs |
| R3 Create lesson | `OnlineLessonService.CreateOnlineLessonAsync` (no MeetingUrl required) |
| R4 Start lesson | `StartOnlineLessonAsync` + notification fan-out + redirect to classroom |
| R5 End lesson | `EndOnlineLessonAsync` + hub `LessonEnded` broadcast |
| R6 Cancel lesson | `CancelOnlineLessonAsync` + notification |
| R7 Edit title/description | `UpdateOnlineLessonAsync` (link/platform fields removed from required input) |
| R8 Student join + participant | `JoinOnlineLessonAsync` returns the in-system classroom URL; participant upsert |
| R9 Teacher panel actions | Teacher Group Detail schedule tab — "Start Live Class" / "Open Classroom" / "End Lesson" |
| R10 Student visibility | Student schedule + dashboard — Online badge + "Join Live Class" |
| R11 Online default behavior | Start auto-creates a lesson with no external link |
| R12 Attendance integration | "Mark joined as Present" reads `OnlineLessonParticipant` |
| R13 Admin visibility | Group list/detail + Online Lessons history |
| R14 Validation/authorization | Service guards + hub `JoinLesson` access check |
| R15 Audit logging | `LogAsync` calls in each service operation |
| R16 Persistence/migration | DbSets, configs, `AddOnlineClassroom` migration (chat + adjustments) |
| R17 Service operations | `IOnlineLessonService` 10 methods + `IGroupService` extension |
| R18 Badges | Shared badge helper across Admin/Teacher/Student |

## Architecture

```
Teacher / Student MVC Views
   (Razor classroom page + WebRTC JS)
        │
        │  WebSocket (SignalR signaling only)
        ▼
SIS.API SignalR Hubs (/hubs/online-class, /hubs/chat, /hubs/notification)
        │  authorize + access check
        ▼
SIS.BLL Services (OnlineLessonService, AttendanceService, NotificationService, AuditLogService)
        │
        ▼
SIS.DAL UnitOfWork + Repositories
        │
        ▼
AppDbContext (EF Core, SQL Server)


Teacher Browser ──[SDP offer/answer + ICE]── SignalR ──[same]── Student Browser
        │                                                              │
        └──────── direct WebRTC media (audio/video) ──────────────────┘
```

The hub is **only** for signaling and chat fan-out. Audio/video flows peer-to-peer through `RTCPeerConnection`. REST endpoints (Refit) handle lifecycle (start/end/cancel/update/join → URL); the join endpoint returns the in-system classroom path, not an external URL.

## Data Models

All entities derive from `BaseEntity`.

### OnlineLesson (existing — adjusted)

```csharp
public class OnlineLesson : BaseEntity
{
    public int GroupId { get; set; }
    public int ClassSessionId { get; set; }
    public int TeacherId { get; set; }

    public string Title { get; set; } = null!;       // <= 200, auto-built if blank
    public string? Description { get; set; }          // <= 1000, optional

    // External meeting fields are deprecated in this flow.
    // Kept nullable for backward compatibility with the previous migration.
    public string? MeetingUrl { get; set; }           // nullable, unused by live classroom
    public OnlinePlatform? Platform { get; set; }     // nullable, unused

    public OnlineLessonStatus Status { get; set; } = OnlineLessonStatus.Scheduled;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public bool IsActive { get; set; } = true;

    public Group Group { get; set; } = null!;
    public ClassSession ClassSession { get; set; } = null!;
    public Teacher Teacher { get; set; } = null!;
    public ICollection<OnlineLessonParticipant> Participants { get; set; } = new List<OnlineLessonParticipant>();
    public ICollection<OnlineLessonChatMessage> ChatMessages { get; set; } = new List<OnlineLessonChatMessage>();
}
```

The migration relaxes `MeetingUrl` and `Platform` to nullable. The validation rule that previously rejected a missing meeting URL is removed.

### OnlineLessonParticipant (existing — unchanged)

```csharp
public class OnlineLessonParticipant : BaseEntity
{
    public int OnlineLessonId { get; set; }
    public int StudentId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime? LeftAt { get; set; }
    public int? DurationMinutes { get; set; }
    public bool IsAttendanceMarked { get; set; }

    public OnlineLesson OnlineLesson { get; set; } = null!;
    public Student Student { get; set; } = null!;
}
```

`(OnlineLessonId, StudentId)` unique index is already in place.

### OnlineLessonChatMessage (new)

A dedicated entity, kept separate from the existing `ChatMessage` to avoid coupling the user-to-user chat with classroom chat.

```csharp
public class OnlineLessonChatMessage : BaseEntity
{
    public int OnlineLessonId { get; set; }
    public string SenderUserId { get; set; } = null!;   // AppUser.Id
    public string MessageText { get; set; } = null!;    // <= 2000
    public DateTime SentAt { get; set; }

    public OnlineLesson OnlineLesson { get; set; } = null!;
    public AppUser Sender { get; set; } = null!;
}
```

### EF Configurations

**`OnlineLessonConfiguration`** — adjusts the existing config:
- `Title` max 200, required.
- `Description` max 1000.
- `MeetingUrl` max 2048, **nullable** (was required).
- `Platform` **nullable** (was required).
- Indexes on `GroupId`, `ClassSessionId`, `TeacherId`, `Status` (already present).
- `HasQueryFilter(o => !o.IsDeleted)`.

**`OnlineLessonChatMessageConfiguration`** (new):
- Key `Id`.
- `MessageText` required, max 2000.
- `HasOne(OnlineLesson).WithMany(l => l.ChatMessages).HasForeignKey(OnlineLessonId)` cascade.
- `HasOne(Sender).WithMany().HasForeignKey(SenderUserId)` restrict.
- Index on `(OnlineLessonId, SentAt)`.
- `HasQueryFilter(m => !m.IsDeleted)`.

### DbContext

Add `DbSet<OnlineLessonChatMessage> OnlineLessonChatMessages`. Existing `OnlineLessons` and `OnlineLessonParticipants` DbSets stay.

## Components and Interfaces

### DAL Layer

`IOnlineLessonRepository` adds:
```csharp
Task AddChatMessageAsync(OnlineLessonChatMessage msg);
Task<IEnumerable<OnlineLessonChatMessage>> GetChatMessagesAsync(int onlineLessonId, int take = 200);
```
Existing repository methods stay.

### BLL Layer

#### DTOs (extended)

```csharp
public class StartOnlineLessonResultDto
{
    public int LessonId { get; set; }
    public string ClassroomUrl { get; set; } = "";   // /Teacher/OnlineLessons/Classroom/{id}
}

public class JoinOnlineLessonResultDto
{
    public bool CanJoin { get; set; }
    public int? LessonId { get; set; }
    public string? ClassroomUrl { get; set; }        // /Student/OnlineLessons/Classroom/{id}
    public OnlineLessonStatus Status { get; set; }
    public string Message { get; set; } = "";        // not started / ended / cancelled / ok
}

public class OnlineLessonChatMessageDto
{
    public int Id { get; set; }
    public int OnlineLessonId { get; set; }
    public string SenderUserId { get; set; } = "";
    public string SenderName { get; set; } = "";
    public string MessageText { get; set; } = "";
    public DateTime SentAt { get; set; }
}
```

`OnlineLessonDto`, `CreateOnlineLessonDto`, `UpdateOnlineLessonDto`, `OnlineLessonParticipantDto`, `MarkPresentResultDto` from the previous design stay; their `MeetingUrl`/`Platform` fields become optional.

#### `IOnlineLessonService`

```csharp
Task<int> CreateOnlineLessonAsync(CreateOnlineLessonDto dto, int teacherId);
Task UpdateOnlineLessonAsync(UpdateOnlineLessonDto dto, int teacherId);
Task<StartOnlineLessonResultDto> StartOnlineLessonAsync(int classSessionId, int teacherId);
Task EndOnlineLessonAsync(int onlineLessonId, int teacherId);
Task CancelOnlineLessonAsync(int onlineLessonId, int teacherId);
Task<JoinOnlineLessonResultDto> JoinOnlineLessonAsync(int onlineLessonId, int studentId);

Task<IEnumerable<OnlineLessonDto>> GetTeacherGroupOnlineLessonsAsync(int groupId, int teacherId);
Task<IEnumerable<OnlineLessonDto>> GetStudentOnlineLessonsAsync(int studentId);
Task<IEnumerable<OnlineLessonParticipantDto>> GetOnlineLessonParticipantsAsync(int onlineLessonId);
Task<MarkPresentResultDto> MarkJoinedStudentsAsPresentAsync(int onlineLessonId, int teacherId);
Task<IEnumerable<OnlineLessonDto>> GetAllOnlineLessonsAsync();           // admin

// Classroom helpers
Task<IEnumerable<OnlineLessonChatMessageDto>> GetClassroomChatHistoryAsync(int onlineLessonId, string requestingUserId);
Task<bool> CanAccessClassroomAsync(int onlineLessonId, string userId);   // teacher-of-group OR enrolled student
Task<OnlineLessonChatMessageDto> SaveClassroomChatMessageAsync(int onlineLessonId, string senderUserId, string text);
```

#### Service behavior changes (vs the previous design)

- **No URL validation.** `ValidateMeetingUrl` is removed. `Create` and `Update` no longer require a meeting URL.
- **Start** sets the lesson `Live`, returns the **classroom URL** computed for the caller's role (teacher → `/Teacher/OnlineLessons/Classroom/{id}`).
- **Join** does the enrollment check, upserts the participant, and returns the **student classroom URL** (`/Student/OnlineLessons/Classroom/{id}`).
- **CanAccessClassroomAsync** centralizes the rule used by both the MVC route and the SignalR hub: the user is either the owning teacher (group.TeacherId == teacher.Id resolved from userId) **or** a student with an active enrollment in the lesson's group. Anyone else is denied.
- **GetClassroomChatHistoryAsync** returns chat after the same access check.
- **SaveClassroomChatMessageAsync** persists a message; HTML-encoding for XSS is applied at render time in the Razor view (not at storage), keeping the raw text intact for editing.

### API Layer

#### REST controller `OnlineLessonController`

Same endpoints as before, with the **`MeetingUrl`/`Platform` fields removed from the required body**. New responses:
- `POST /api/onlinelesson/session/{sessionId}/start` → `StartOnlineLessonResultDto` (caller is the teacher; result carries the teacher classroom URL).
- `POST /api/onlinelesson/{id}/join` → `JoinOnlineLessonResultDto` with `ClassroomUrl`.
- `GET /api/onlinelesson/{id}/chat` → chat history for authorized callers.
- `POST /api/onlinelesson/{id}/chat` → save a message (mostly used as fallback; the hub is the primary path).

#### SignalR hub `OnlineClassHub` (new)

```csharp
[Authorize]
public class OnlineClassHub : Hub
{
    private readonly IOnlineLessonService _service;
    public OnlineClassHub(IOnlineLessonService service) { _service = service; }

    private string UserId => Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "";
    private static string GroupName(int lessonId) => $"online-lesson-{lessonId}";

    public async Task JoinLesson(int lessonId)
    {
        if (!await _service.CanAccessClassroomAsync(lessonId, UserId))
            throw new HubException("Forbidden");

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(lessonId));

        // Tell existing peers about the new participant; tell newcomer about existing peers.
        await Clients.OthersInGroup(GroupName(lessonId))
            .SendAsync("UserJoined", new { connectionId = Context.ConnectionId, userId = UserId });
    }

    public Task LeaveLesson(int lessonId) =>
        Clients.OthersInGroup(GroupName(lessonId))
               .SendAsync("UserLeft", new { connectionId = Context.ConnectionId, userId = UserId });

    // WebRTC signaling — all targeted at a specific peer connectionId.
    public Task SendOffer(int lessonId, string targetConnectionId, object offer) =>
        Clients.Client(targetConnectionId)
               .SendAsync("ReceiveOffer", new { fromConnectionId = Context.ConnectionId, fromUserId = UserId, offer });

    public Task SendAnswer(int lessonId, string targetConnectionId, object answer) =>
        Clients.Client(targetConnectionId)
               .SendAsync("ReceiveAnswer", new { fromConnectionId = Context.ConnectionId, answer });

    public Task SendIceCandidate(int lessonId, string targetConnectionId, object candidate) =>
        Clients.Client(targetConnectionId)
               .SendAsync("ReceiveIceCandidate", new { fromConnectionId = Context.ConnectionId, candidate });

    public async Task SendClassMessage(int lessonId, string message)
    {
        if (!await _service.CanAccessClassroomAsync(lessonId, UserId))
            throw new HubException("Forbidden");
        var dto = await _service.SaveClassroomChatMessageAsync(lessonId, UserId, message ?? "");
        await Clients.Group(GroupName(lessonId)).SendAsync("ReceiveClassMessage", dto);
    }

    public override Task OnDisconnectedAsync(Exception? ex) =>
        // Group cleanup is automatic; peers will time out the RTCPeerConnection.
        base.OnDisconnectedAsync(ex);
}

// NOTE: This is a mesh-WebRTC demo. Production-grade scale would require an SFU
// (e.g., LiveKit, mediasoup). Not implemented in this project.
```

Hub is mapped in `Program.cs`:
```csharp
app.MapHub<OnlineClassHub>("/hubs/online-class");
```

The existing `app.UseAuthentication()` / `app.UseAuthorization()` pipeline plus the JWT-via-query-string accessor used by the existing hubs covers token authentication for SignalR.

### UI Layer

#### Refit client `IOnlineLessonApi`

Updated return types: `StartAsync` returns `StartOnlineLessonResultDto`, `JoinAsync` returns `JoinOnlineLessonResultDto` (with `ClassroomUrl`). `Create`/`Update` payloads keep `Title`/`Description` and drop the meeting URL requirement on the server side; the form fields can stay but become optional.

#### MVC routes — Classroom page

Two thin Razor views that share one `_ClassroomLayout.cshtml` partial for video grid + chat:

- `GET /Teacher/OnlineLessons/Classroom/{id}` — Teacher view: end-class button visible.
- `GET /Student/OnlineLessons/Classroom/{id}` — Student view: leave-only.

Both controllers:
1. Call `IOnlineLessonApi.GetByIdAsync` (added) or reuse existing fetch to load the lesson.
2. Verify access via `CanAccessClassroomAsync` (server-side guard).
3. If the lesson is `Ended` or `Cancelled`, redirect back with a notice.
4. Pass `OnlineLessonId`, `LessonTitle`, `GroupName`, `SessionTopic`, `IsTeacher`, `HubUrl="/hubs/online-class"`, and the JWT (already used by other AJAX calls) to the view.

#### Classroom view layout

```
┌────────────────────────────────────────────────────────────┐
│ {LessonTitle}      [LIVE badge]      {GroupName}           │
│ Topic: {SessionTopic}                                      │
├──────────────────────────────────────┬─────────────────────┤
│ Video grid                           │ Participants        │
│ ┌──────┐ ┌──────┐ ┌──────┐           │ • Teacher Name      │
│ │local │ │peer1 │ │peer2 │ ...       │ • Student Name      │
│ └──────┘ └──────┘ └──────┘           │ ...                 │
│                                      ├─────────────────────┤
│  [🎤 mic] [📷 cam] [🚪 leave]         │ Class chat          │
│  [⏹ End class — teacher only]        │ ...messages...      │
│                                      │ [input] [send]      │
└──────────────────────────────────────┴─────────────────────┘
```

Errors:
- `Camera or microphone permission denied.` — when `getUserMedia` rejects.
- `Your browser does not support WebRTC.` — when `RTCPeerConnection` is missing.

#### Classroom JavaScript

A single `classroom.js` (or inline `<script>` block in the view) drives the page:

```js
// 1. SignalR connection
const conn = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl, { accessTokenFactory: () => jwtToken })
    .withAutomaticReconnect()
    .build();

// 2. WebRTC state
const peers = {};                 // connectionId -> RTCPeerConnection
let localStream = null;
const iceConfig = { iceServers: [{ urls: "stun:stun.l.google.com:19302" }] };

// 3. Wire up before connecting
conn.on("UserJoined", ({ connectionId }) => createOfferTo(connectionId));
conn.on("UserLeft", ({ connectionId }) => removePeer(connectionId));
conn.on("ReceiveOffer", handleIncomingOffer);
conn.on("ReceiveAnswer", handleIncomingAnswer);
conn.on("ReceiveIceCandidate", handleIncomingIce);
conn.on("ReceiveClassMessage", appendChatMessage);
conn.on("LessonEnded", () => showEndedState());

// 4. Start
await navigator.mediaDevices.getUserMedia({ audio: true, video: true });
await conn.start();
await conn.invoke("JoinLesson", lessonId);
```

`createOfferTo` / `handleIncomingOffer` follow the standard mesh pattern: the user already in the room creates the offer when a new user joins; both sides add `localStream` tracks before negotiation; ICE candidates are forwarded through `SendIceCandidate`. Chat input calls `conn.invoke("SendClassMessage", lessonId, text)`. `End Lesson` (teacher) hits the REST endpoint, which broadcasts `LessonEnded` to the hub group via `IHubContext<OnlineClassHub>` injected into `OnlineLessonService.EndOnlineLessonAsync`.

#### Teacher Group Detail — schedule tab

For each session row, the actions become:
- No lesson → **Start Live Class** (one click, no URL form).
- `Scheduled` → **Start Live Class**, **Cancel**.
- `Live` → **Open Classroom** (the teacher's own classroom URL), **End Lesson**, plus the live joined count.
- `Ended`/`Cancelled` → status badge only.

The previous "Edit Link" UI is removed. A simple **Edit Title** modal stays for renaming.

#### Student Group/Course Detail — schedule + dashboard

- Online group → delivery badge per session.
- `Live` → **Join Live Class** button → calls join → navigates to the returned `ClassroomUrl`.
- `Scheduled` → "Not started yet" (disabled).
- `Ended` → "Lesson ended".
- `Cancelled` → "Cancelled".

#### Admin Group + History

Group list/detail keeps the delivery badge from the previous design (no meeting URL preview is shown — the masked-URL line is replaced with platform name only when present, otherwise hidden). The Online Lessons history page shows lesson title, group, course, teacher, session, status, started/ended timestamps, participant count, and duration. No video recording.

### Notification

Start fan-out unchanged: title `"Online class started"`, message `"{GroupName} üçün online dərs başladı"`, type `OnlineLessonStarted`. The `RelatedUrl` now points to **the student classroom URL** so clicking the notification opens the live class directly. Cancel notification stays the same.

## Correctness Properties

These invariants must hold regardless of input or timing.

### Property 1: Single active lesson per session
At most one `OnlineLesson` with `Status` in {Scheduled, Live} exists per `ClassSessionId`. Enforced by `GetActiveBySessionAsync` in create/start.

**Validates: Requirements 3.5, 4.4, 14.7**

### Property 2: Unique participant
At most one `OnlineLessonParticipant` exists per `(OnlineLessonId, StudentId)`. Enforced by the DB unique index plus check-then-upsert with duplicate-catch.

**Validates: Requirements 8.3**

### Property 3: Validated one-directional status transitions
Scheduled → Live (start), Live → Ended (end), Scheduled/Live → Cancelled (cancel). Operations against an illegal source status are rejected with no state change.

**Validates: Requirements 5.2, 6.3, 7.3**

### Property 4: Ownership
Only the owning teacher (`group.TeacherId == teacherId`) can create/start/edit/cancel/end a lesson. Any other teacher is rejected with no change.

**Validates: Requirements 14.5**

### Property 5: Classroom access
Only the owning teacher or an actively enrolled student can open the classroom page, send chat, or invoke any hub method. The hub's `JoinLesson` and the REST/MVC routes both call `CanAccessClassroomAsync`. URL manipulation by non-members is rejected.

**Validates: Requirements 14.5, 14.6**

### Property 6: No automatic attendance
Joining never writes an `AttendanceRecord`. Attendance is written only by `MarkJoinedStudentsAsPresentAsync` and only for students that have a participant record.

**Validates: Requirements 12.3, 12.7**

### Property 7: Chat XSS safety
Classroom chat messages are stored verbatim and rendered HTML-encoded; the live append path uses `textContent` (not `innerHTML`) to avoid script injection.

**Validates: Requirements 14.5, 15.4**

### Property 8: Additive-only schema
Existing functionality keeps working after the migration; new columns are nullable, new tables are independent, and previously-required `MeetingUrl` is relaxed to nullable.

**Validates: Requirements 16.6**

## Error Handling

- Validation and state errors throw `BadRequestException`; missing entities throw `NotFoundException`; ownership/enrollment violations throw `UnauthorizedException`. The API exception middleware maps these to 400/403/404; Refit surfaces them to the UI, where AJAX handlers show the message.
- The participant unique index is the last line of defense for Property 2; the service catches `DbUpdateException` from a duplicate and converts it into the "already joined → update" outcome.
- `OnlineClassHub` access denial throws `HubException("Forbidden")`; the client JS treats this as a hard fail and shows a banner.
- `getUserMedia` failures show a friendly message and disable mic/cam controls; the user can stay in the room as text-only.
- Audit logging failures never break the main operation.

## Testing Strategy

Manual end-to-end runs after applying the migration:

1. **Build check.** `dotnet build SIS.slnx` — only `error CS` lines matter; MSB3021/MSB3027 are dev-server file locks.
2. **Group setup.** Create an Online group; confirm the form no longer requires a meeting URL (it can be left blank).
3. **Teacher start.** Open the group's schedule tab → "Start Live Class" on a session → page redirects to `/Teacher/OnlineLessons/Classroom/{id}`. Camera/mic prompt appears. Status flips to Live; an enrolled student receives a notification linking to the classroom.
4. **Student join.** Student clicks "Join Live Class" (or the notification) → browser opens the classroom page → permission prompt → both videos appear in the grid; chat messages flow both ways.
5. **Multi-student.** Join a second student → a third tile appears; mesh negotiation completes via `UserJoined` + `SendOffer`.
6. **Toggle controls.** Mic and camera toggles affect outbound stream and the local tile placeholder.
7. **End lesson.** Teacher clicks "End Lesson" → status flips to Ended → students see the "Lesson ended" overlay and connections release.
8. **Authorization.** A teacher who does not own the group and a student who is not enrolled are blocked at both the REST endpoint and the hub `JoinLesson` call (HubException + REST 403).
9. **Attendance.** Teacher attendance tab shows "Joined" badges for students who connected; "Mark joined as Present" sets only those students to Present.
10. **Audit + history.** Audit log lists Create/Start/End/Cancel entries; Admin Online Lessons page lists the lesson with participant count and duration.

## Migration

A new EF Core migration **`AddOnlineClassroom`** that:
- Relaxes `OnlineLessons.MeetingUrl` to nullable (`AlterColumn`).
- Relaxes `OnlineLessons.Platform` to nullable (`AlterColumn`).
- Creates `OnlineLessonChatMessages` table with FK to `OnlineLessons` (cascade) and to `AspNetUsers` (restrict), index on `(OnlineLessonId, SentAt)`, soft-delete query filter columns.

User runs in Visual Studio Package Manager Console (Default project: `SIS.DAL`, Startup: `SIS.API`):
```
Add-Migration AddOnlineClassroom
Update-Database
```
Existing data and the previously applied `AddGroupDeliveryModeAndOnlineLessons` migration stay intact; the column relaxations are non-destructive.
