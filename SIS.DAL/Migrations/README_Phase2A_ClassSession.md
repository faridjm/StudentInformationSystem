# Phase 2A — ClassSession + WeeklySchedulePattern

## Nə əlavə olundu

1. **`ClassSessionStatus` enum** — Scheduled, Completed, Cancelled, Makeup
2. **`WeeklySchedulePattern`** entity — müəllimin həftəlik şablonu (DayOfWeek + saat)
3. **`ClassSession`** entity — hər konkret dərs günü (Date, StartTime, EndTime, Status, Topic, Notes, OriginalSessionId)
4. **`CourseOffering`** entity-də iki yeni collection (`WeeklySchedules`, `ClassSessions`)
5. EF Configuration faylları (unique index-lər, FK delete davranışları)
6. Repository və UnitOfWork qoşulması

## Tipləri

- `DateOnly` — yalnız gün (saat olmadan)
- `TimeOnly` — yalnız saat (gün olmadan)
- `DayOfWeek` — System.DayOfWeek (Sunday=0..Saturday=6)

EF Core 10 bu tipləri SQL Server-də `date`, `time`, `int` kimi xəritələyir.

## Migration

```powershell
Add-Migration AddClassSessionAndWeeklySchedule -Project SIS.DAL -StartupProject SIS.API
```

Migration faylına bax — iki yeni cədvəl yaradılmalıdır:

### `WeeklySchedulePatterns`
| Sütun | Tip | Notes |
|---|---|---|
| Id | int | PK |
| CourseOfferingId | int | FK → CourseOfferings, Cascade delete |
| DayOfWeek | int | 0..6 |
| StartTime | time(7) | |
| EndTime | time(7) | |
| Room | nvarchar(50) | nullable |
| CreatedAt/UpdatedAt/IsDeleted | — | BaseEntity |

**Unique index:** `(CourseOfferingId, DayOfWeek, StartTime)` filtered by `IsDeleted = 0`

### `ClassSessions`
| Sütun | Tip | Notes |
|---|---|---|
| Id | int | PK |
| CourseOfferingId | int | FK → CourseOfferings, Cascade delete |
| Date | date | |
| StartTime | time(7) | |
| EndTime | time(7) | |
| Room | nvarchar(50) | nullable |
| Status | int | 0=Scheduled, 1=Completed, 2=Cancelled, 3=Makeup |
| Topic | nvarchar(200) | nullable |
| Notes | nvarchar(2000) | nullable |
| CancellationReason | nvarchar(500) | nullable |
| OriginalSessionId | int | nullable FK → ClassSessions, Restrict delete |
| CreatedAt/UpdatedAt/IsDeleted | — | BaseEntity |

**Unique index:** `(CourseOfferingId, Date, StartTime)` filtered by `IsDeleted = 0`
**Index:** `Date` (sürətli axtarış)

## Tətbiq et

```powershell
Update-Database -Project SIS.DAL -StartupProject SIS.API
```

## Yoxlama SQL-ləri

```sql
-- Cədvəllər yarandı?
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_NAME IN ('WeeklySchedulePatterns', 'ClassSessions');

-- WeeklySchedulePatterns sütunları
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'WeeklySchedulePatterns'
ORDER BY ORDINAL_POSITION;

-- ClassSessions sütunları
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'ClassSessions'
ORDER BY ORDINAL_POSITION;

-- FK-lar
SELECT
    fk.name AS FK_Name,
    OBJECT_NAME(fk.parent_object_id) AS [Table],
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS [Column],
    OBJECT_NAME(fk.referenced_object_id) AS [RefTable]
FROM sys.foreign_keys fk
JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
WHERE OBJECT_NAME(fk.parent_object_id) IN ('WeeklySchedulePatterns', 'ClassSessions');
```

## Növbəti addım (Phase 2B)

Service qatı:
- `IWeeklySchedulePatternService` + implementation
- `IClassSessionService` + implementation (generator metodu daxil)
- DTO-lar (List, Detail, Create, Update)
- AutoMapper profili
- FluentValidation
- API Controller-lər
- DI qeydiyyatı
