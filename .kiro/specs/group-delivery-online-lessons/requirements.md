# Requirements Document

## Introduction

This feature adds a **Group Delivery Mode** capability and a **meeting-link based Online Lesson (Live Class)** system to the Acadex SIS platform. Administrators can choose how a group is delivered (in person, online, or hybrid) when creating a group. Teachers can create, start, end, edit, and cancel online lessons that are attached to scheduled class sessions, and students can join those lessons through an external meeting link (Google Meet, Zoom, Microsoft Teams, or a custom link).

No proprietary video streaming engine is built. Each online lesson is represented by a meeting URL that opens in a new browser tab. Joining an online lesson is tracked through participant records that the teacher can later use to mark attendance manually. The feature extends the existing Admin, Teacher, and Student panels without altering or breaking existing functionality.

The feature touches the layered architecture (SIS.Entities, SIS.DAL, SIS.BLL, SIS.API, SIS.UI), exposes new API endpoints consumed by the UI through Refit clients, and adds one Entity Framework migration that is applied manually from Visual Studio.

## Glossary

- **Online_Lesson_System**: The collection of backend services, API endpoints, and UI components introduced by this feature that manage delivery mode, online lessons, and participants.
- **Group_Service**: The existing group management service component, extended in this feature to handle delivery mode fields.
- **Online_Lesson_Service**: The new service component (`IOnlineLessonService`) responsible for the online lesson lifecycle and participant tracking.
- **Group**: An existing entity representing a cohort of students taught a course by a teacher.
- **Class_Session**: An existing entity (`ClassSession`) representing a single scheduled meeting of a group.
- **Online_Lesson**: A new entity representing a meeting-link based live class attached to a class session.
- **Online_Lesson_Participant**: A new entity recording that a student joined a specific online lesson.
- **Delivery_Mode**: An enum on the group describing its teaching format: InPerson, Online, or Hybrid.
- **Online_Platform**: An enum identifying the meeting platform: GoogleMeet, Zoom, MicrosoftTeams, or Custom.
- **Online_Lesson_Status**: An enum describing an online lesson's lifecycle state: Scheduled, Live, Ended, or Cancelled.
- **Meeting_Url**: The external link a participant opens to join an online lesson.
- **Enrolled_Student**: A student with an active enrollment record in the group that owns the online lesson.
- **Owning_Teacher**: The teacher assigned to the group that owns the online lesson.
- **Admin_Panel**: The existing administrator MVC area in SIS.UI.
- **Teacher_Panel**: The existing teacher MVC area in SIS.UI.
- **Student_Panel**: The existing student MVC area in SIS.UI.
- **Audit_Log_Service**: The existing component that writes `AuditLog` records for significant operations.
- **Notification_Service**: The existing component that creates `Notification` records for users.

## Requirements

### Requirement 1: Group Delivery Mode Configuration

**User Story:** As an administrator, I want to choose a teaching format when creating or editing a group, so that the group's sessions default to the correct delivery method.

#### Acceptance Criteria

1. THE Group_Service SHALL persist for every Group a Delivery_Mode value that is exactly one of InPerson, Online, or Hybrid.
2. WHERE the Delivery_Mode field is omitted or null when a Group is created, THE Group_Service SHALL set the Group's Delivery_Mode to InPerson.
3. WHERE the Delivery_Mode of a Group is Online or Hybrid, THE Group_Service SHALL persist the supplied DefaultOnlinePlatform value (one of GoogleMeet, Zoom, MicrosoftTeams, or Custom) and the supplied DefaultMeetingUrl, where the DefaultMeetingUrl is a valid URL not exceeding 2048 characters.
4. WHERE the Delivery_Mode of a Group is InPerson, THE Group_Service SHALL allow the DefaultOnlinePlatform and DefaultMeetingUrl to remain unset.
5. WHEN an administrator opens the Group create form, THE Admin_Panel SHALL display a "Tədris formatı" (Delivery Mode) selection field offering exactly the options InPerson, Online, and Hybrid, with InPerson selected by default.
6. WHILE the selected Delivery_Mode in the Group create form is Online or Hybrid, THE Admin_Panel SHALL display the Online_Platform selection field and the DefaultMeetingUrl input field.
7. WHILE the selected Delivery_Mode in the Group create form is InPerson, THE Admin_Panel SHALL hide the Online_Platform selection field and the DefaultMeetingUrl input field.
8. WHEN an administrator edits an existing Group, THE Admin_Panel SHALL display the current Delivery_Mode, DefaultOnlinePlatform, and DefaultMeetingUrl values for modification.

### Requirement 2: Online Lesson Data Model

**User Story:** As a developer, I want a well-defined Online Lesson data model, so that online lessons can be stored, queried, and related to groups, sessions, and teachers.

#### Acceptance Criteria

1. THE Online_Lesson_System SHALL store each Online_Lesson with the fields Id, GroupId, ClassSessionId, TeacherId, Title, Description, MeetingUrl, Platform, Status, StartedAt, EndedAt, CreatedAt, UpdatedAt, and IsActive.
2. THE Online_Lesson_System SHALL allow the Description, StartedAt, EndedAt, and UpdatedAt fields of an Online_Lesson to be unset.
3. THE Online_Lesson_System SHALL store each Online_Lesson_Participant with the fields Id, OnlineLessonId, StudentId, JoinedAt, LeftAt, DurationMinutes, IsAttendanceMarked, and CreatedAt.
4. THE Online_Lesson_System SHALL allow the LeftAt and DurationMinutes fields of an Online_Lesson_Participant to be unset.
5. THE Online_Lesson_System SHALL set the Status of a newly created Online_Lesson to Scheduled.
6. THE Online_Lesson_System SHALL associate each Online_Lesson with exactly one Group, one Class_Session, and one Owning_Teacher.

### Requirement 3: Create Online Lesson

**User Story:** As a teacher, I want to create an online lesson for a class session, so that students have a meeting link to join the live class.

#### Acceptance Criteria

1. WHEN an Owning_Teacher submits a create online lesson request with a valid Meeting_Url (a syntactically valid absolute URL not exceeding 2048 characters) and an Online_Platform for a Class_Session, THE Online_Lesson_Service SHALL create an Online_Lesson with Status set to Scheduled and return its identifier.
2. WHEN an Owning_Teacher creates an Online_Lesson without supplying a Title, THE Online_Lesson_Service SHALL set a non-empty Title incorporating the Group name and the Class_Session, not exceeding 200 characters.
3. WHERE the owning Group's Delivery_Mode is Online and the Owning_Teacher does not supply a Meeting_Url, THE Online_Lesson_Service SHALL populate the Online_Lesson Meeting_Url from the Group DefaultMeetingUrl.
4. WHERE the owning Group's Delivery_Mode is Online and the Owning_Teacher does not supply an Online_Platform, THE Online_Lesson_Service SHALL populate the Online_Lesson Platform from the Group DefaultOnlinePlatform.
5. IF a create online lesson request targets a Class_Session that already has an Online_Lesson with Status Scheduled or Live, THEN THE Online_Lesson_Service SHALL reject the request, create no new Online_Lesson, and return an error indication identifying the conflicting lesson.
6. WHEN an Online_Lesson is created, THE Audit_Log_Service SHALL record a CreateOnlineLesson entry identifying the acting Owning_Teacher and the Online_Lesson.

### Requirement 4: Start Online Lesson

**User Story:** As a teacher, I want to start an online lesson, so that students are notified and can join the live class.

#### Acceptance Criteria

1. WHEN an Owning_Teacher starts an Online_Lesson, THE Online_Lesson_Service SHALL set the Status to Live and set StartedAt to the current time.
2. WHERE an Owning_Teacher starts an online lesson for a Class_Session that has no existing Online_Lesson, THE Online_Lesson_Service SHALL create an Online_Lesson using the owning Group DefaultMeetingUrl and DefaultOnlinePlatform and then set the Status to Live.
3. WHEN an Online_Lesson transitions to Live, THE Notification_Service SHALL create a notification for every Enrolled_Student of the owning Group with a title of "Online lesson started", a message of "{GroupName} üçün online dərs başladı", and a RelatedUrl pointing to the student schedule join location.
4. IF a start request targets a Class_Session that already has another Online_Lesson with Status Live, THEN THE Online_Lesson_Service SHALL reject the request and return a descriptive error.
5. WHEN an Online_Lesson is started, THE Audit_Log_Service SHALL record a StartOnlineLesson entry identifying the actor and the Online_Lesson.

### Requirement 5: End Online Lesson

**User Story:** As a teacher, I want to end an online lesson, so that the session is closed and students no longer see it as live.

#### Acceptance Criteria

1. WHEN an Owning_Teacher ends an Online_Lesson whose Status is Live, THE Online_Lesson_Service SHALL set the Status to Ended and set EndedAt to the current time.
2. IF an end request targets an Online_Lesson whose Status is not Live, THEN THE Online_Lesson_Service SHALL reject the request and return a descriptive error.
3. WHEN an Online_Lesson is ended, THE Audit_Log_Service SHALL record an EndOnlineLesson entry identifying the actor and the Online_Lesson.

### Requirement 6: Cancel Online Lesson

**User Story:** As a teacher, I want to cancel an online lesson, so that students know the live class will not take place.

#### Acceptance Criteria

1. WHEN an Owning_Teacher cancels an Online_Lesson whose Status is Scheduled or Live, THE Online_Lesson_Service SHALL set the Status to Cancelled.
2. WHEN an Online_Lesson is cancelled, THE Notification_Service SHALL create a notification for every Enrolled_Student of the owning Group indicating "Online lesson cancelled".
3. IF a cancel request targets an Online_Lesson whose Status is Ended or Cancelled, THEN THE Online_Lesson_Service SHALL reject the request and return a descriptive error.
4. WHEN an Online_Lesson is cancelled, THE Audit_Log_Service SHALL record a CancelOnlineLesson entry identifying the actor and the Online_Lesson.

### Requirement 7: Edit Online Lesson Link

**User Story:** As a teacher, I want to edit an online lesson's meeting link and details, so that I can correct or update the join information before the lesson ends.

#### Acceptance Criteria

1. WHEN an Owning_Teacher updates an Online_Lesson with a valid Meeting_Url and Online_Platform, THE Online_Lesson_Service SHALL persist the updated Title, Description, Meeting_Url, and Platform and set UpdatedAt to the current time.
2. IF an update request supplies a Meeting_Url that is not a valid URL, THEN THE Online_Lesson_Service SHALL reject the request and return a descriptive error.
3. IF an update request targets an Online_Lesson whose Status is Ended or Cancelled, THEN THE Online_Lesson_Service SHALL reject the request and return a descriptive error.
4. WHEN an Online_Lesson is updated, THE Audit_Log_Service SHALL record an UpdateOnlineLesson entry identifying the actor and the Online_Lesson.

### Requirement 8: Student Join and Participant Tracking

**User Story:** As an enrolled student, I want to join an online lesson, so that I can attend the live class through the meeting link.

#### Acceptance Criteria

1. WHEN an Enrolled_Student joins an Online_Lesson whose Status is Live and no Online_Lesson_Participant exists for that student and lesson, THE Online_Lesson_Service SHALL create an Online_Lesson_Participant with JoinedAt set to the current server time.
2. WHEN an Enrolled_Student joins an Online_Lesson whose Status is Live and an Online_Lesson_Participant already exists for that student and lesson, THE Online_Lesson_Service SHALL update the existing Online_Lesson_Participant JoinedAt to the current server time and SHALL NOT create a second participant.
3. THE Online_Lesson_System SHALL guarantee, including under concurrent join requests, that at most one Online_Lesson_Participant exists for a given combination of OnlineLessonId and StudentId.
4. WHEN a join request against a Live Online_Lesson succeeds, THE Student_Panel SHALL open the Online_Lesson Meeting_Url in a new browser tab.
5. IF a join request targets an Online_Lesson whose Status is Scheduled, THEN THE Student_Panel SHALL inform the student that the lesson has "Not started yet" and SHALL NOT open the Meeting_Url.
6. IF a join request targets an Online_Lesson whose Status is Ended, THEN THE Student_Panel SHALL inform the student that the "Lesson ended" and SHALL NOT open the Meeting_Url.
7. IF a join request targets an Online_Lesson whose Status is Cancelled, THEN THE Student_Panel SHALL inform the student that the lesson is cancelled and SHALL NOT open the Meeting_Url.

### Requirement 9: Teacher Panel Online Lesson Actions

**User Story:** As a teacher, I want online lesson actions on each session in my group's schedule, so that I can manage the live class directly from the schedule view.

#### Acceptance Criteria

1. WHEN an Owning_Teacher views the Schedule tab of a Group detail page, THE Teacher_Panel SHALL display, for each Class_Session, the available online lesson actions among Create, Start, Edit Link, Cancel, Join, and End according to the current Online_Lesson_Status.
2. WHERE the owning Group's Delivery_Mode is Online, THE Teacher_Panel SHALL present the Start action as the primary action and prefill the Meeting_Url, Online_Platform, and Title from the Group defaults.
3. WHILE an Online_Lesson Status is Live, THE Teacher_Panel SHALL display the count of students who have joined the Online_Lesson.
4. WHERE the owning Group's Delivery_Mode is InPerson, THE Teacher_Panel SHALL allow the Owning_Teacher to create and start an Online_Lesson for any Class_Session.

### Requirement 10: Student Panel Online Lesson Visibility

**User Story:** As a student, I want to see when my classes are online or live, so that I know how and when to join.

#### Acceptance Criteria

1. WHILE the next class on the Student_Panel dashboard is online or has a Live Online_Lesson, THE Student_Panel SHALL display an Online badge and a Join button for that class.
2. WHEN an Enrolled_Student views the Schedule of a Course detail page, THE Student_Panel SHALL display, for each Class_Session, a delivery badge, an online lesson status badge, and a Join button.
3. WHILE an Online_Lesson Status is Live, THE Student_Panel SHALL present the Join button for that lesson as active.
4. WHILE an Online_Lesson Status is Scheduled or Ended or Cancelled, THE Student_Panel SHALL present the Join button in a disabled or informational state corresponding to that status.

### Requirement 11: Online Group Default Behavior

**User Story:** As a teacher of an online group, I want online lessons to work with minimal setup, so that I can start a live class quickly using the group defaults.

#### Acceptance Criteria

1. WHERE a Group Delivery_Mode is Online, THE Teacher_Panel and Student_Panel SHALL display an Online badge for every Class_Session of the Group.
2. WHERE a Group Delivery_Mode is Online and a Class_Session has no Online_Lesson, WHEN the Owning_Teacher starts the online lesson, THE Online_Lesson_Service SHALL create an Online_Lesson for that Class_Session using the Group DefaultMeetingUrl and DefaultOnlinePlatform and set the Status to Live.
3. WHERE a Group Delivery_Mode is InPerson, THE Online_Lesson_System SHALL display sessions in the standard in-person presentation while still allowing online lessons to be created.

### Requirement 12: Attendance Integration

**User Story:** As a teacher, I want to mark students who joined the online lesson as present, so that I can record attendance based on online participation without automatic attendance.

#### Acceptance Criteria

1. WHEN an Owning_Teacher views the Attendance tab for a Class_Session that has an Online_Lesson, THE Teacher_Panel SHALL display a "Joined online lesson" badge for each student who has an Online_Lesson_Participant record for that lesson and SHALL NOT display the badge for students without such a record.
2. WHEN an Owning_Teacher views the Attendance tab for a Class_Session that has an Online_Lesson, THE Teacher_Panel SHALL display a "Mark joined students as Present" action.
3. WHEN an Owning_Teacher invokes the "Mark joined students as Present" action for an Online_Lesson that has at least one Online_Lesson_Participant, THE Online_Lesson_Service SHALL set the attendance status to Present for every Enrolled_Student who has an Online_Lesson_Participant record for that lesson and SHALL leave students without such a record unchanged.
4. WHEN the "Mark joined students as Present" action completes, THE Teacher_Panel SHALL report the number of students set to Present.
5. IF an Owning_Teacher invokes the "Mark joined students as Present" action for an Online_Lesson that has no Online_Lesson_Participant, THEN THE Online_Lesson_Service SHALL make no attendance changes and return an error indication.
6. WHEN an Owning_Teacher manually changes an attendance status after marking joined students as Present, THE Online_Lesson_System SHALL persist the manually chosen status and retain it unless the mark-present action is invoked again.
7. THE Online_Lesson_System SHALL NOT create attendance records automatically when a student joins an Online_Lesson.

### Requirement 13: Admin Panel Online Lesson Visibility

**User Story:** As an administrator, I want to see delivery and online lesson information, so that I can monitor how groups deliver their classes.

#### Acceptance Criteria

1. WHEN an administrator views the Group list or Group detail page, THE Admin_Panel SHALL display the Delivery_Mode, DefaultOnlinePlatform, and a masked preview of the DefaultMeetingUrl for each Group.
2. WHERE the Online Lessons overview is enabled under Academic Operations, THE Admin_Panel SHALL display each Online_Lesson with its Group, Course, Teacher, Class_Session, Status, StartedAt, EndedAt, and participant count.

### Requirement 14: Validation and Authorization Rules

**User Story:** As a system owner, I want online lesson operations to be validated and authorized, so that data stays consistent and users only act within their permissions.

#### Acceptance Criteria

1. IF a Group create or edit request includes a Delivery_Mode that is not one of InPerson, Online, or Hybrid, THEN THE Group_Service SHALL reject the request, make no Group change, and return a validation error identifying the Delivery_Mode field.
2. IF a create or update online lesson request does not include a Meeting_Url, THEN THE Online_Lesson_Service SHALL reject the request, create or modify no Online_Lesson, and return a validation error identifying the Meeting_Url field.
3. IF a create or update online lesson request includes a Meeting_Url that is not a well-formed absolute URL not exceeding 2048 characters, THEN THE Online_Lesson_Service SHALL reject the request, create or modify no Online_Lesson, and return a validation error identifying the Meeting_Url field.
4. IF a create or update online lesson request does not include an Online_Platform that is one of GoogleMeet, Zoom, MicrosoftTeams, or Custom, THEN THE Online_Lesson_Service SHALL reject the request, create or modify no Online_Lesson, and return a validation error identifying the Online_Platform field.
5. IF a teacher who is not the Owning_Teacher attempts to create, start, edit, cancel, or end an Online_Lesson, THEN THE Online_Lesson_Service SHALL reject the request, make no change, and return an authorization error.
6. IF a student who is not an Enrolled_Student of the owning Group attempts to join an Online_Lesson, THEN THE Online_Lesson_Service SHALL reject the request, create no Online_Lesson_Participant, and return an authorization error.
7. IF a create or start online lesson request would result in more than one Online_Lesson with Status Scheduled or Live for the same Class_Session, THEN THE Online_Lesson_Service SHALL reject the request and create no additional Online_Lesson.

### Requirement 15: Audit Logging

**User Story:** As an administrator, I want significant online lesson and delivery operations to be audit logged, so that actions are traceable.

#### Acceptance Criteria

1. WHEN a Group is created with a Delivery_Mode, THE Audit_Log_Service SHALL record the chosen Delivery_Mode in the CreateGroup audit entry.
2. WHEN an Online_Lesson is created, updated, started, ended, or cancelled, THE Audit_Log_Service SHALL record a CreateOnlineLesson, UpdateOnlineLesson, StartOnlineLesson, EndOnlineLesson, or CancelOnlineLesson entry respectively.
3. WHERE student join audit logging is enabled, WHEN an Enrolled_Student joins an Online_Lesson, THE Audit_Log_Service SHALL record a StudentJoinOnlineLesson entry.
4. THE Audit_Log_Service SHALL exclude sensitive values from every online lesson audit entry.

### Requirement 16: Data Persistence and Migration

**User Story:** As a developer, I want the database schema extended through a single migration, so that the new entities and group fields are created consistently and applied manually in Visual Studio.

#### Acceptance Criteria

1. THE Online_Lesson_System SHALL provide DbSet collections named OnlineLessons and OnlineLessonParticipants.
2. THE Online_Lesson_System SHALL add the DeliveryMode, DefaultOnlinePlatform, and DefaultMeetingUrl columns to the Group table.
3. THE Online_Lesson_System SHALL define database indexes on the Online_Lesson GroupId, ClassSessionId, TeacherId, and Status columns.
4. THE Online_Lesson_System SHALL define a database index on the Online_Lesson_Participant OnlineLessonId and StudentId columns and a unique constraint on the combination of OnlineLessonId and StudentId.
5. THE Online_Lesson_System SHALL provide a single Entity Framework migration named AddGroupDeliveryModeAndOnlineLessons that creates the OnlineLesson and Online_Lesson_Participant tables and the new Group columns.
6. THE Online_Lesson_System SHALL preserve all existing Admin, Teacher, and Student panel functionality after the migration is applied.

### Requirement 17: Online Lesson Service Operations

**User Story:** As a developer, I want a dedicated online lesson service interface, so that the API and UI have a consistent set of operations for the online lesson lifecycle.

#### Acceptance Criteria

1. THE Online_Lesson_Service SHALL expose operations for CreateOnlineLessonAsync, UpdateOnlineLessonAsync, StartOnlineLessonAsync, EndOnlineLessonAsync, CancelOnlineLessonAsync, and JoinOnlineLessonAsync.
2. THE Online_Lesson_Service SHALL expose operations for GetTeacherGroupOnlineLessonsAsync, GetStudentOnlineLessonsAsync, and GetOnlineLessonParticipantsAsync.
3. WHERE the mark-present capability is enabled, THE Online_Lesson_Service SHALL expose a MarkJoinedStudentsAsPresentAsync operation.
4. THE Group_Service SHALL accept and persist the Delivery_Mode, DefaultOnlinePlatform, and DefaultMeetingUrl fields in its create and edit operations.

### Requirement 18: Delivery and Status Badge Presentation

**User Story:** As a user of the panels, I want consistent colored badges for delivery mode and lesson status, so that I can quickly recognize delivery format and lesson state.

#### Acceptance Criteria

1. WHEN the Teacher_Panel or Student_Panel renders a schedule row, THE Online_Lesson_System SHALL display a delivery badge and, where an Online_Lesson exists, an online lesson status badge.
2. THE Online_Lesson_System SHALL render the Online delivery badge in purple, the InPerson delivery badge in blue, and the Hybrid delivery badge in teal.
3. THE Online_Lesson_System SHALL render the Scheduled status badge in blue, the Live status badge in green, the Ended status badge in gray, and the Cancelled status badge in red.
