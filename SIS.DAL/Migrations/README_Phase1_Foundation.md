# Phase 1 — Foundation Migration

## Nə əlavə olundu

1. **`SIS.Common/Time/IClock.cs`** + **`SystemClock.cs`** — mərkəzi zaman servisi
2. **`SIS.Entities/Enums/TermPhase.cs`** — semestr fazası enum-u
3. **`SIS.Entities/Models/AcademicYear.cs`** — dərs sezonu entity-si
4. **`SIS.Entities/Models/AcademicTerm.cs`** — genişləndirildi:
   - Yeni nullable tarix sahələri: `RegistrationStart`, `RegistrationEnd`, `DropDeadline`, `ExamPeriodStart`, `ExamPeriodEnd`, `GradeSubmissionDeadline`
   - Yeni nullable FK: `AcademicYearId`
   - Yeni metod: `GetCurrentPhase(DateTime now)`
5. **EF Configuration**: `AcademicYearConfiguration` (unique Name, query filter)
6. **Repository**: `IAcademicYearRepository` + `AcademicYearRepository`, UnitOfWork-ə qoşuldu
7. **DI**: `IClock` → `SystemClock` API-də qeyd olundu

## Migration yaratmaq (Package Manager Console-da)

```powershell
# SIS.API layihəsini Default Project seç, sonra:
Add-Migration AddAcademicYearAndTermPhases -Project SIS.DAL -StartupProject SIS.API
```

Və ya CLI ilə:

```bash
dotnet ef migrations add AddAcademicYearAndTermPhases --project SIS.DAL --startup-project SIS.API
```

## Migration-ın yaratdığı dəyişikliklər (gözləntilər)

- `AcademicYears` cədvəli yaradılır (Id, Name, StartDate, EndDate, IsActive, CreatedAt, UpdatedAt, IsDeleted)
- `AcademicTerms` cədvəlinə yeni nullable sütunlar əlavə olunur:
  - `RegistrationStart`, `RegistrationEnd`, `DropDeadline`,
  - `ExamPeriodStart`, `ExamPeriodEnd`, `GradeSubmissionDeadline`,
  - `AcademicYearId` (FK → AcademicYears.Id, ON DELETE RESTRICT)
- Unique index: `AcademicYears.Name`

**Köhnə məlumat qorunur:** Yeni sütunlar hamısı `nullable` olduğuna görə mövcud Term sətirləri toxunmaz.

## Migration-ı tətbiq et

```powershell
Update-Database -Project SIS.DAL -StartupProject SIS.API
```

Və ya:

```bash
dotnet ef database update --project SIS.DAL --startup-project SIS.API
```

## Köhnə Term-ləri yeni AcademicYear-a bağlamaq (opsional, data migration)

Migration tamamlandıqdan sonra DB-də bu SQL-i icra et:

```sql
-- 1. Mövcud Term-lərin tarix aralığından bir "Default" sezon yarat
DECLARE @MinDate datetime = (SELECT MIN(StartDate) FROM AcademicTerms);
DECLARE @MaxDate datetime = (SELECT MAX(EndDate) FROM AcademicTerms);

IF NOT EXISTS (SELECT 1 FROM AcademicYears WHERE Name = 'Köhnə semestrlər')
BEGIN
    INSERT INTO AcademicYears (Name, StartDate, EndDate, IsActive, CreatedAt, IsDeleted)
    VALUES ('Köhnə semestrlər', @MinDate, @MaxDate, 0, GETUTCDATE(), 0);
END

-- 2. Bütün AcademicYearId = NULL olan Term-ləri bu sezona bağla
DECLARE @DefaultYearId int = (SELECT Id FROM AcademicYears WHERE Name = 'Köhnə semestrlər');

UPDATE AcademicTerms
SET AcademicYearId = @DefaultYearId
WHERE AcademicYearId IS NULL;

-- 3. (Opsional) Köhnə Term-lərin yeni tarix sahələrini StartDate/EndDate-dən hesabla
UPDATE AcademicTerms
SET
    RegistrationStart = DATEADD(DAY, -14, StartDate),
    RegistrationEnd = DATEADD(DAY, -1, StartDate),
    DropDeadline = DATEADD(DAY, 30, StartDate),
    ExamPeriodStart = DATEADD(DAY, -14, EndDate),
    ExamPeriodEnd = EndDate,
    GradeSubmissionDeadline = DATEADD(DAY, 7, EndDate)
WHERE RegistrationStart IS NULL;
```

## Yoxlama

```sql
SELECT t.Id, t.Name, t.StartDate, t.EndDate, t.AcademicYearId, y.Name AS YearName
FROM AcademicTerms t
LEFT JOIN AcademicYears y ON y.Id = t.AcademicYearId;
```

## Növbəti addım (Phase 1B)

1. `AcademicYear` DTO-ları (List, Detail, Create, Update)
2. AutoMapper profillərinə əlavə
3. FluentValidation validator-ları
4. `IAcademicYearService` + `AcademicYearService`
5. `AcademicYearController` (API)
6. Refit `IAcademicYearApi` (UI)
7. Admin paneldə `Areas/Admin/Controllers/AcademicYearController.cs` + view-lər (Index/Create/Edit)
8. `AcademicTerm` create/edit formalarına `AcademicYearId` sahəsi əlavə

Phase 1B kifayət qədər mexaniki işdir — yalnız nümunələri kopyala-yapışdır + adları dəyiş.

## Növbəti addım (Phase 1C)

`IClock`-u BLL servislərində istifadə et və icazə qaydalarını `TermPhase` ilə qur:
- `EnrollmentService.EnrollStudentAsync` — yalnız `TermPhase.Registration`-da icazə
- `EnrollmentService.DropAsync` — yalnız `Today <= DropDeadline`
- `AttendanceService.RecordAsync` — yalnız `TermPhase.Active`-də
- `GradeService.AddGradeAsync` — `TermPhase.Active` və ya `ExamPeriod` və ya `GradeReview`-də
