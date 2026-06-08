using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIS.BLL.Services.DTOs.Transcript;
using SIS.Entities.Enums;

namespace SIS.API.Services
{
    public interface ITranscriptPdfService
    {
        /// <summary>
        /// Transcript PDF generasiya edir. lang: az (default) | en | ru.
        /// qrPng: QR doğrulama şəkli (PNG). Verilməsə QR bölməsi göstərilmir.
        /// </summary>
        byte[] GeneratePdf(TranscriptDto data, string? lang = "az", byte[]? qrPng = null);
    }

    /// <summary>
    /// Acadex 360 — professional, 3 dilli (AZ/EN/RU) akademik transcript PDF.
    /// Calculation logic-ə toxunmur — bütün dəyərlər TranscriptDto-dan (AcademicPerformanceService
    /// + TranscriptResultCalculator) gəlir. Yalnız layout və statik label-lər lokalizasiya olunur.
    /// QR doğrulama və Acadex 360 Digital Verification Seal əlavə olunub.
    /// </summary>
    public class TranscriptPdfService : ITranscriptPdfService
    {
        // Acadex theme rəngləri
        private const string Navy = "#0F172A";
        private const string Primary = "#4F46E5";
        private const string PrimaryLight = "#EEF2FF";
        private const string Accent = "#7C3AED";
        private const string Success = "#059669";
        private const string Warning = "#D97706";
        private const string Danger = "#DC2626";
        private const string Muted = "#64748B";
        private const string Border = "#E2E8F0";
        private const string SoftBg = "#F8FAFC";

        public byte[] GeneratePdf(TranscriptDto data, string? lang = "az", byte[]? qrPng = null)
        {
            var L = PdfLocalization.Normalize(lang);
            string T(string key) => PdfLocalization.T(key, L);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(32);
                    // Lato (QuestPDF default) həm Azərbaycan latın, həm Kiril simvollarını dəstəkləyir.
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Lato").FontColor(Navy));

                    ComposeHeader(page, data, T, L);
                    ComposeContent(page, data, T, L, qrPng);
                    ComposeFooter(page, data, T);
                });
            });

            return document.GeneratePdf();
        }

        private bool IsFinal(TranscriptDto data) => data.TranscriptType == TranscriptType.Final;

        // ── HEADER ──
        private void ComposeHeader(PageDescriptor page, TranscriptDto data, Func<string, string> T, string lang)
        {
            page.Header().Column(col =>
            {
                col.Item().Background(Navy).Padding(16).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        c.Item().Row(r =>
                        {
                            r.AutoItem().Text("Acadex").Bold().FontSize(22).FontColor("#FFFFFF");
                            r.AutoItem().PaddingLeft(6).AlignBottom().Text("360").Bold().FontSize(13).FontColor("#34D399");
                        });
                        c.Item().Text(T("Platform")).FontSize(8).FontColor("#A5B4FC");
                    });
                    row.ConstantItem(220).AlignRight().Column(c =>
                    {
                        c.Item().AlignRight().Text(T("Transcript").ToUpperInvariant()).Bold().FontSize(13).FontColor("#FFFFFF");
                        // Transcript type rozeti
                        var typeText = IsFinal(data) ? T("FinalTranscript") : T("ProgressTranscript");
                        var typeBg = IsFinal(data) ? "#34D399" : "#FBBF24";
                        c.Item().PaddingTop(3).AlignRight().Background(typeBg).PaddingHorizontal(8).PaddingVertical(2)
                            .Text(typeText.ToUpperInvariant()).Bold().FontSize(7.5f).FontColor(Navy);
                        c.Item().PaddingTop(3).AlignRight().Text($"{T("GeneratedAt")}: {data.GeneratedAt:dd.MM.yyyy HH:mm}").FontSize(7.5f).FontColor("#CBD5E1");
                        if (!string.IsNullOrEmpty(data.VerificationCode))
                            c.Item().AlignRight().Text($"{T("VerificationCode")}: {data.VerificationCode}").FontSize(7).FontColor("#94A3B8");
                        c.Item().AlignRight().Text($"LANG: {lang.ToUpperInvariant()}").FontSize(7).FontColor("#64748B");
                    });
                });
                col.Item().Height(3).Background(Primary);
            });
        }

        // ── CONTENT ──
        private void ComposeContent(PageDescriptor page, TranscriptDto data, Func<string, string> T, string lang, byte[]? qrPng)
        {
            page.Content().PaddingTop(12).Column(col =>
            {
                col.Spacing(12);

                // ── Transcript type note (Progress / Final) ──
                col.Item().Element(c => TranscriptTypeNote(c, data, T));

                // ── Student + Course info (2 column) ──
                col.Item().Row(row =>
                {
                    row.RelativeItem().Element(c => InfoCard(c, T("StudentInformation"), inner =>
                    {
                        InfoLine(inner, T("FullName"), data.StudentName);
                        InfoLine(inner, T("StudentNumber"), data.StudentNumber);
                        if (!string.IsNullOrEmpty(data.Email)) InfoLine(inner, T("Email"), data.Email!);
                        InfoLine(inner, T("EnrollmentYear"), data.EnrollmentYear.ToString());
                    }));
                    row.ConstantItem(12);
                    row.RelativeItem().Element(c => InfoCard(c, T("CourseInformation"), inner =>
                    {
                        InfoLine(inner, T("Course"), data.CourseName);
                        InfoLine(inner, T("Group"), data.GroupName);
                        InfoLine(inner, T("Teacher"), data.TeacherName);
                        if (!string.IsNullOrEmpty(data.AcademicYear)) InfoLine(inner, T("AcademicYear"), data.AcademicYear!);
                        InfoLine(inner, T("Period"), $"{data.StartDate:dd.MM.yyyy} — {data.EndDate:dd.MM.yyyy}");
                    }));
                });

                // ── Academic summary cards (5 box) ──
                col.Item().Row(row =>
                {
                    row.Spacing(8);
                    var attClr = data.AttendancePercentage >= 80 ? Success : data.AttendancePercentage >= 60 ? Warning : Danger;
                    var gradeClr = data.OfficialGrade >= 70 ? Success : data.OfficialGrade >= 50 ? Warning : Danger;

                    SummaryCard(row.RelativeItem(), T("Attendance"), $"{data.AttendancePercentage:F1}%", attClr);
                    SummaryCard(row.RelativeItem(), T("OfficialGrade"), $"{data.OfficialGrade:F1}%", gradeClr);
                    SummaryCard(row.RelativeItem(), T("GPA"), data.GPA.ToString("F2"), Primary);
                    SummaryCard(row.RelativeItem(), T("CertificateStatus"), T(data.CertificateStatus), CertColor(data.CertificateStatus));
                    SummaryCard(row.RelativeItem(), T("CompletionLevel"), T(data.CompletionLevel), LevelColor(data.CompletionLevel));
                });

                // ── Attendance breakdown ──
                col.Item().Element(c => SectionTitle(c, T("AttendanceSummary")));
                col.Item().Row(row =>
                {
                    row.Spacing(8);
                    StatBox(row.RelativeItem(), T("Present"), data.PresentCount.ToString(), Success);
                    StatBox(row.RelativeItem(), T("Late"), data.LateCount.ToString(), Warning);
                    StatBox(row.RelativeItem(), T("Absent"), data.AbsentCount.ToString(), Danger);
                    StatBox(row.RelativeItem(), T("Excused"), data.ExcusedCount.ToString(), Muted);
                    StatBox(row.RelativeItem(), T("TakenSessions"), data.AttendanceTakenSessions.ToString(), Muted);
                    var attClr = data.AttendancePercentage >= 80 ? Success : data.AttendancePercentage >= 60 ? Warning : Danger;
                    StatBox(row.RelativeItem(), T("AttendancePercentage"), $"{data.AttendancePercentage:F1}%", attClr);
                });
                col.Item().Text(T("AttendanceFormula")).FontSize(7).Italic().FontColor(Muted);

                // ── Assessment breakdown ──
                col.Item().Element(c => SectionTitle(c, T("AssessmentSummary")));
                col.Item().Element(c => AssessmentTable(c, data, T));

                // Official grade highlighted box
                col.Item().Element(c => OfficialGradeBox(c, data, T));

                // ── Homework (optional, GPA-ya təsir etmir) ──
                if (data.HomeworkAssignments.Any())
                {
                    col.Item().Element(c => SectionTitle(c, T("HomeworkPerformance")));
                    col.Item().Text(T("HomeworkNote")).FontSize(7).Italic().FontColor(Muted);
                    col.Item().Element(c => HomeworkTable(c, data, T));
                    if (data.HomeworkAverage.HasValue)
                        col.Item().Text($"{T("HomeworkAverage")}: {data.HomeworkAverage:F1}%").FontSize(9).FontColor(Accent).Bold();
                }

                // ── Final result ──
                col.Item().Element(c => FinalResultCard(c, data, T));

                // ── QR verification + Digital seal ──
                col.Item().Element(c => VerificationAndSeal(c, data, T, qrPng));
            });
        }

        // ── FOOTER ──
        private void ComposeFooter(PageDescriptor page, TranscriptDto data, Func<string, string> T)
        {
            page.Footer().Column(col =>
            {
                col.Item().PaddingBottom(4).LineHorizontal(0.5f).LineColor(Border);
                col.Item().Text(T("FooterNote")).FontSize(6.5f).FontColor(Muted);
                col.Item().PaddingTop(3).Row(row =>
                {
                    row.RelativeItem().Text($"Acadex 360 · {data.CourseName} · {data.GroupName}").FontSize(7).FontColor(Muted);
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span($"{T("Page")} ").FontSize(7).FontColor(Muted);
                        text.CurrentPageNumber().FontSize(7).FontColor(Muted);
                        text.Span(" / ").FontSize(7).FontColor(Muted);
                        text.TotalPages().FontSize(7).FontColor(Muted);
                    });
                });
            });
        }

        // ── Transcript type note banner ──
        private void TranscriptTypeNote(IContainer c, TranscriptDto data, Func<string, string> T)
        {
            var final = IsFinal(data);
            var accent = final ? Primary : Warning;
            var bg = final ? PrimaryLight : "#FEF3C7";
            var noteKey = final ? "FinalNote" : "ProgressNote";
            c.Border(0.75f).BorderColor(accent).Background(bg).Padding(8).Row(row =>
            {
                row.ConstantItem(4).Background(accent);
                row.RelativeItem().PaddingLeft(8).Column(cc =>
                {
                    cc.Item().Text($"{T("TranscriptType")}: {(final ? T("FinalTranscript") : T("ProgressTranscript"))}")
                        .Bold().FontSize(9).FontColor(Navy);
                    cc.Item().PaddingTop(1).Text(T(noteKey)).FontSize(7.5f).Italic().FontColor(Muted);
                });
            });
        }

        // ── Reusable building blocks ──

        private void InfoCard(IContainer c, string title, Action<ColumnDescriptor> body)
        {
            c.Border(0.75f).BorderColor(Border).Background("#FFFFFF").Padding(12).Column(col =>
            {
                col.Item().Text(title.ToUpperInvariant()).Bold().FontSize(8).FontColor(Primary);
                col.Item().PaddingTop(6).Column(body);
            });
        }

        private void InfoLine(ColumnDescriptor col, string label, string value)
        {
            col.Item().PaddingBottom(3).Row(row =>
            {
                row.ConstantItem(95).Text(label).FontSize(8).FontColor(Muted);
                row.RelativeItem().Text(value).FontSize(8.5f).Bold().FontColor(Navy);
            });
        }

        private void SummaryCard(IContainer c, string label, string value, string color)
        {
            c.Border(0.75f).BorderColor(Border).Background(SoftBg).Column(col =>
            {
                col.Item().Height(3).Background(color);
                col.Item().Padding(8).Column(cc =>
                {
                    cc.Item().AlignCenter().Text(value).Bold().FontSize(12).FontColor(color);
                    cc.Item().AlignCenter().PaddingTop(2).Text(label).FontSize(6.5f).FontColor(Muted);
                });
            });
        }

        private void StatBox(IContainer c, string label, string value, string color)
        {
            c.Border(0.5f).BorderColor(Border).Padding(8).Column(cc =>
            {
                cc.Item().AlignCenter().Text(value).Bold().FontSize(13).FontColor(color);
                cc.Item().AlignCenter().Text(label).FontSize(7).FontColor(Muted);
            });
        }

        private void SectionTitle(IContainer c, string title)
        {
            c.PaddingTop(4).Row(row =>
            {
                row.ConstantItem(4).Background(Primary);
                row.AutoItem().PaddingLeft(8).AlignMiddle().Text(title.ToUpperInvariant()).Bold().FontSize(10).FontColor(Navy);
            });
        }

        private void AssessmentTable(IContainer c, TranscriptDto data, Func<string, string> T)
        {
            c.Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(2.6f);  // Assessment
                    cols.RelativeColumn(1.5f);  // Type
                    cols.RelativeColumn(1.1f);  // Date
                    cols.RelativeColumn(0.9f);  // Max
                    cols.RelativeColumn(0.9f);  // Score
                    cols.RelativeColumn(0.9f);  // Weight
                    cols.RelativeColumn(1.2f);  // Contribution
                    cols.RelativeColumn(1.2f);  // Status
                });

                // Header
                table.Cell().Background(Primary).PaddingVertical(5).PaddingHorizontal(5).Text(T("Assessment")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Primary).PaddingVertical(5).PaddingHorizontal(5).Text(T("Type")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Primary).PaddingVertical(5).PaddingHorizontal(5).Text(T("Date")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Primary).PaddingVertical(5).PaddingHorizontal(5).AlignRight().Text(T("MaxScore")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Primary).PaddingVertical(5).PaddingHorizontal(5).AlignRight().Text(T("Score")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Primary).PaddingVertical(5).PaddingHorizontal(5).AlignRight().Text(T("Weight")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Primary).PaddingVertical(5).PaddingHorizontal(5).AlignRight().Text(T("Contribution")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Primary).PaddingVertical(5).PaddingHorizontal(5).Text(T("Status")).FontSize(7.5f).Bold().FontColor("#FFFFFF");

                bool alt = false;
                foreach (var a in data.OfficialAssessments)
                {
                    var bg = alt ? SoftBg : "#FFFFFF";
                    alt = !alt;
                    var scoreClr = !a.IsGraded ? Muted : a.Percentage >= 70 ? Success : a.Percentage >= 50 ? Warning : Danger;
                    var statusText = a.IsGraded ? T("Graded") : T("NotGraded");
                    var statusClr = a.IsGraded ? Success : Muted;

                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).Text(a.Title).FontSize(8);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).Text(a.Type).FontSize(8);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).Text(a.Date.ToString("dd.MM.yy")).FontSize(8);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).AlignRight().Text(a.MaxScore.ToString("F0")).FontSize(8);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).AlignRight().Text(a.IsGraded ? a.Score?.ToString("F1") ?? "—" : "—").FontSize(8).FontColor(scoreClr);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).AlignRight().Text($"{a.WeightPercent:F0}%").FontSize(8);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).AlignRight().Text(a.IsGraded ? a.WeightedScore?.ToString("F2") ?? "—" : "—").FontSize(8);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).Text(statusText).FontSize(7.5f).FontColor(statusClr);
                }
            });
        }

        private void OfficialGradeBox(IContainer c, TranscriptDto data, Func<string, string> T)
        {
            var gradeClr = data.OfficialGrade >= 70 ? Success : data.OfficialGrade >= 50 ? Warning : Danger;
            c.PaddingTop(4).AlignRight().Width(320).Border(1).BorderColor(Primary).Background(PrimaryLight).Padding(10).Row(r =>
            {
                r.RelativeItem().AlignMiddle().Column(col =>
                {
                    col.Item().Text(T("OfficialGrade").ToUpperInvariant()).Bold().FontSize(9).FontColor(Navy);
                });
                r.ConstantItem(70).AlignRight().AlignMiddle().Text($"{data.OfficialGrade:F1}%").Bold().FontSize(16).FontColor(gradeClr);
                r.ConstantItem(55).AlignRight().AlignMiddle().Column(col =>
                {
                    col.Item().AlignRight().Text(data.LetterGrade).Bold().FontSize(14).FontColor(Primary);
                    col.Item().AlignRight().Text($"{T("GPA")}: {data.GPA:F2}").FontSize(7);
                });
            });
        }

        private void HomeworkTable(IContainer c, TranscriptDto data, Func<string, string> T)
        {
            c.Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(3);
                    cols.RelativeColumn(1.3f);
                    cols.RelativeColumn(1);
                    cols.RelativeColumn(1);
                    cols.RelativeColumn(1);
                    cols.RelativeColumn(1.3f);
                });

                table.Cell().Background(Accent).PaddingVertical(5).PaddingHorizontal(5).Text(T("Assessment")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Accent).PaddingVertical(5).PaddingHorizontal(5).Text(T("DueDate")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Accent).PaddingVertical(5).PaddingHorizontal(5).AlignRight().Text(T("MaxScore")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Accent).PaddingVertical(5).PaddingHorizontal(5).AlignRight().Text(T("Score")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Accent).PaddingVertical(5).PaddingHorizontal(5).AlignRight().Text("%").FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Accent).PaddingVertical(5).PaddingHorizontal(5).Text(T("Link")).FontSize(7.5f).Bold().FontColor("#FFFFFF");

                bool alt = false;
                foreach (var a in data.HomeworkAssignments)
                {
                    var bg = alt ? SoftBg : "#FFFFFF";
                    alt = !alt;
                    var sClr = !a.IsGraded ? Muted : a.Percentage >= 70 ? Success : Warning;

                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).Text(a.Title).FontSize(8);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).Text(a.DueDate.ToString("dd.MM.yy")).FontSize(8);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).AlignRight().Text(a.MaxScore.ToString("F0")).FontSize(8);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).AlignRight().Text(a.IsGraded ? a.Score?.ToString("F1") ?? "—" : "—").FontSize(8).FontColor(sClr);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).AlignRight().Text(a.Percentage.HasValue ? $"{a.Percentage:F0}%" : "—").FontSize(8);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).Text(a.IsLinkedToAssessment ? T("Official") : T("Homework")).FontSize(8)
                        .FontColor(a.IsLinkedToAssessment ? Success : Muted);
                }
            });
        }

        private void FinalResultCard(IContainer c, TranscriptDto data, Func<string, string> T)
        {
            var gradeClr = data.OfficialGrade >= 70 ? Success : data.OfficialGrade >= 50 ? Warning : Danger;

            c.PaddingTop(6).Border(1).BorderColor(Border).Background(SoftBg).Padding(14).Column(col =>
            {
                col.Item().Text(T("FinalResult").ToUpperInvariant()).Bold().FontSize(10).FontColor(Navy);

                // Sətir 1: Transcript type · Official grade · GPA · Attendance
                col.Item().PaddingTop(8).Row(row =>
                {
                    ResultField(row.RelativeItem(), T("TranscriptType"),
                        IsFinal(data) ? T("FinalTranscript") : T("ProgressTranscript"),
                        IsFinal(data) ? Primary : Warning, 11);
                    ResultField(row.RelativeItem(), T("OfficialGrade"), $"{data.OfficialGrade:F1}%", gradeClr, 13);
                    ResultField(row.RelativeItem(), T("GPA"), data.GPA.ToString("F2"), Primary, 13);
                    ResultField(row.RelativeItem(), T("Attendance"), $"{data.AttendancePercentage:F1}%",
                        data.AttendancePercentage >= 70 ? Success : Danger, 13);
                });

                // Sətir 2: Completion status · Academic distinction · Certificate eligibility
                col.Item().PaddingTop(10).Row(row =>
                {
                    ResultField(row.RelativeItem(), T("CompletionStatus"), T(data.CompletionStatus),
                        data.CompletionStatus == "Completed" ? Success : Warning, 11);
                    ResultField(row.RelativeItem(), T("AcademicDistinction"), T(data.CompletionLevel),
                        LevelColor(data.CompletionLevel), 11);
                    ResultField(row.RelativeItem(), T("CertificateEligibility"), T(data.CertificateStatus),
                        CertColor(data.CertificateStatus), 11);
                });

                col.Item().PaddingTop(8).Text(IsFinal(data) ? T("FinalNote") : T("ProgressNote"))
                    .FontSize(7).Italic().FontColor(Muted);
            });
        }

        private void ResultField(IContainer c, string label, string value, string color, float size)
        {
            c.Column(cc =>
            {
                cc.Item().Text(label).FontSize(7.5f).FontColor(Muted);
                cc.Item().Text(value).Bold().FontSize(size).FontColor(color);
            });
        }

        // ── QR verification + Acadex 360 Digital Verification Seal ──
        private void VerificationAndSeal(IContainer c, TranscriptDto data, Func<string, string> T, byte[]? qrPng)
        {
            c.PaddingTop(8).Border(0.75f).BorderColor(Border).Background("#FFFFFF").Padding(12).Row(row =>
            {
                row.Spacing(14);

                // Sol: QR doğrulama
                row.RelativeItem().Row(qr =>
                {
                    qr.Spacing(10);
                    if (qrPng != null)
                        qr.ConstantItem(92).Height(92).Image(qrPng).FitArea();
                    qr.RelativeItem().AlignMiddle().Column(cc =>
                    {
                        cc.Item().Text(T("VerifyDocument").ToUpperInvariant()).Bold().FontSize(9).FontColor(Primary);
                        cc.Item().PaddingTop(2).Text(T("ScanToVerify")).FontSize(7.5f).FontColor(Navy);
                        if (!string.IsNullOrEmpty(data.VerificationCode))
                            cc.Item().PaddingTop(4).Text($"{T("VerificationCode")}: ").FontSize(7.5f).FontColor(Muted);
                        if (!string.IsNullOrEmpty(data.VerificationCode))
                            cc.Item().Text(data.VerificationCode!).Bold().FontSize(11).FontColor(Navy);
                        if (!string.IsNullOrEmpty(data.VerificationUrl))
                            cc.Item().PaddingTop(2).Text(data.VerificationUrl!).FontSize(6.5f).FontColor(Primary);
                    });
                });

                // Sağ: rəqəmsal möhür + qeyd
                row.ConstantItem(190).Row(s =>
                {
                    s.ConstantItem(96).Height(96).Element(seal => DigitalSeal(seal, data, T));
                    s.RelativeItem().PaddingLeft(8).AlignMiddle().Column(cc =>
                    {
                        cc.Item().Text(IsFinal(data) ? T("SealVerified") : T("SealGenerated")).Bold().FontSize(8).FontColor(Navy);
                        cc.Item().Text(IsFinal(data) ? T("SealFinalRecord") : T("SealProgressRecord")).FontSize(7).FontColor(Muted);
                        cc.Item().PaddingTop(4).Text(T("SealNote")).FontSize(6.5f).Italic().FontColor(Muted);
                        cc.Item().PaddingTop(2).Text(T("NotDiplomaNote")).FontSize(6.5f).Italic().FontColor(Muted);
                    });
                });
            });
        }

        // Rəqəmsal möhür — yalnız QuestPDF primitivləri (ikiqat çərçivəli emblem).
        // Mətn Lato fontu ilə yazılır (AZ/EN/RU + Kiril dəstəkli); xarici asılılıq yoxdur.
        private void DigitalSeal(IContainer c, TranscriptDto data, Func<string, string> T)
        {
            var final = IsFinal(data);
            var ring = final ? Primary : Warning;
            var innerC = final ? Accent : "#B45309";
            var bgTint = final ? PrimaryLight : "#FEF3C7";
            var shortWord = (final ? T("SealVerifiedShort") : T("SealGeneratedShort")).ToUpperInvariant();

            c.Border(2).BorderColor(ring).Background(bgTint).Padding(2)
             .Border(0.8f).BorderColor(innerC).Padding(5).AlignMiddle().Column(col =>
             {
                 col.Item().PaddingBottom(3).AlignCenter().Width(22).Height(3).Background(ring);
                 col.Item().AlignCenter().Text("ACADEX 360").Bold().FontSize(9).FontColor(ring);
                 col.Item().AlignCenter().Text(shortWord).Bold().FontSize(7).FontColor(innerC);
                 col.Item().PaddingVertical(3).LineHorizontal(0.6f).LineColor(ring);
                 col.Item().AlignCenter().Text(T("SealDigital")).FontSize(5).FontColor(Muted);
             });
        }

        // ── Rəng köməkçiləri ──
        private static string CertColor(string cert) => cert switch
        {
            "Eligible" => Success,
            "NotEligible" => Danger,
            _ => Warning // Pending
        };

        private static string LevelColor(string level) => level switch
        {
            "HighHonorCompletion" => Accent,
            "HonorCompletion" => Primary,
            "NormalCompletion" => Success,
            "NotCompleted" => Danger,
            "NotEligible" => Danger,
            _ => Muted // Pending
        };
    }
}
