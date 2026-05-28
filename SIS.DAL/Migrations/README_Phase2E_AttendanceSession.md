# Phase 2E — Attendance-ı ClassSession-a bağlamaq

## Nə dəyişdi

1. **`AttendanceRecord`** entity-sinə nullable `ClassSessionId` FK əlavə olundu
2. **`ClassSession`**-da `AttendanceRecords` collection navigation əlavə olundu
3. **EF Configuration**: FK setup (Restrict delete, çünki sessiya silinmədən davamiyyət qalır)
4. **DTO**: `CreateAttendanceDto` və `AttendanceDto`-da `ClassSessionId` sahəsi
5. **Service**: `RecordBulkAttendanceAsync` indi:
   - `ClassSessionId` varsa, mövcudluğunu yoxlayır
   - Ləğv olunmuş sessiyaya icazə vermir
   - Sessiyanın `Date`-ini `SessionDate`-ə avtomatik təyin edir
   - Mövcud qeydi `ClassSessionId`-yə görə tapır (yoxsa SessionDate-ə görə — geriyə uyğunluq)
   - Yeni davamiyyət qeyd edildikdə **sessiyanın statusu `Scheduled` → `Completed`**-a keçir
6. **Teacher UI**: Davamiyyət səhifəsi yenidən qurulub — sessiya seçim əsaslı (sol sidebar)

## Migration

```powershell
Add-Migration AddClassSessionIdToAttendanceRecord -Project SIS.DAL -StartupProject SIS.API
```

Sonra:
```powershell
Update-Database -Project SIS.DAL -StartupProject SIS.API
```

## Migration faylının yaratdığı dəyişikliklər

- `AttendanceRecords` cədvəlinə:
  - `ClassSessionId int NULL` sütunu
  - FK constraint: `AttendanceRecords.ClassSessionId → ClassSessions.Id`, ON DELETE RESTRICT
  - Index: `AttendanceRecords.ClassSessionId`

**Köhnə qeydlər toxunulmur** — `ClassSessionId` null qalır, onlar köhnə qaydada işləyir.

## Yoxlama SQL

```sql
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'AttendanceRecords';
```

Yeni sütunu görməlisən: `ClassSessionId`, `int`, `YES`.

## Test ssenarisi

1. **Admin paneldə**:
   - Bir CourseOffering yarat (yeni semestrlə bağlı — Payız 2026 və s.)
   - Tələbə qeyd et (admin override işləyəcək)
   - "Dərs Cədvəli" → həftəlik şablon → "Sessiyaları Yarat"

2. **Müəllim panelinə keç** (admin paneli bağla, müəllim hesabı ilə daxil ol):
   - "Kurslarım" → kursa Davamiyyət düyməsi
   - Sol panelde sessiyalar görünür (tarix sırası ilə)
   - **Ən son keçmiş sessiya avtomatik seçilir** (yoxdursa, ən yaxın gələcəkdəki)
   - Hər tələbənin statusu (Present/Absent/Late) seç → "Davamiyyəti Saxla"
   - Səhifə yeniləndikdə sessiyanın yanında ✓ (Completed) görünür
   - Eyni sessiyaya yenidən baxsan, əvvəlki dəyərlər doldurulmuş gəlir

3. **DB-də yoxla**:
```sql
SELECT a.Id, a.EnrollmentId, a.SessionDate, a.Status, a.ClassSessionId
FROM AttendanceRecords a
WHERE a.ClassSessionId IS NOT NULL;
```
Yeni qeydlər `ClassSessionId` ilə görünməlidir.

## Növbəti addımlar

- **Phase 3** — Assignment (tapşırıqlar) entity + UI
- **Phase 4** — PDF Transcript
- Davamiyyət UI-da **kaskad seç** ("Hamısını iştirak etdi qeyd et") düyməsi əlavə oluna bilər
