namespace SIS.Entities.Enums
{
    /// <summary>
    /// AcademicTerm-in cari fazası. Bütün icazə qaydaları bu enum üzərindən qurulur.
    /// </summary>
    public enum TermPhase
    {
        /// <summary>Qeydiyyat hələ açılmayıb. Admin hazırlıq edir.</summary>
        Planning = 0,

        /// <summary>Tələbə qeydiyyatı açıqdır.</summary>
        Registration = 1,

        /// <summary>Qeydiyyat bitib, amma dərslər hələ başlamayıb. Müəllim təqvim qurur.</summary>
        Preparation = 2,

        /// <summary>Dərslər gedir. Davamiyyət/qiymət qeyd olunur.</summary>
        Active = 3,

        /// <summary>İmtahan dövrü. Yeni davamiyyət yox, yalnız final qiymət.</summary>
        ExamPeriod = 4,

        /// <summary>Qiymət təsdiqi pəncərəsi. Sonra hər şey kilidlənir.</summary>
        GradeReview = 5,

        /// <summary>Semestr bağlanıb. Yalnız oxu icazəsi (transkript və s.).</summary>
        Closed = 6
    }
}
