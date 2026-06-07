using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIS.BLL.Services.DTOs.Transcript;

namespace SIS.API.Services
{
    public interface ITranscriptPdfService
    {
        /// <summary>Transcript PDF generasiya edir. lang: az (default) | en | ru.</summary>
        byte[] GeneratePdf(TranscriptDto data, string? lang = "az");
    }

    /// <summary>
    /// Acadex 360 — professional, 3 dilli (AZ/EN/RU) akademik transcript PDF.
    /// Calculation logic-ə toxunmur — bütün dəyərlər TranscriptDto-dan (AcademicPerformanceService)
    /// gəlir. Yalnız layout və statik label-lər lokalizasiya olunur.
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

        public byte[] GeneratePdf(TranscriptDto data, string? lang = "az")
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
                    ComposeContent(page, data, T, L);
                    ComposeFooter(page, data, T);
                });
            });

            return document.GeneratePdf();
        }

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
                    row.ConstantItem(210).AlignRight().Column(c =>
                    {
                        c.Item().AlignRight().Text(T("Transcript").ToUpperInvariant()).Bold().FontSize(13).FontColor("#FFFFFF");
                        c.Item().AlignRight().Text($"{T("GeneratedAt")}: {data.GeneratedAt:dd.MM.yyyy HH:mm}").FontSize(7.5f).FontColor("#CBD5E1");
                        c.Item().AlignRight().Text($"LANG: {lang.ToUpperInvariant()}").FontSize(7).FontColor("#64748B");
                    });
                });
                col.Item().Height(3).Background(Primary);
            });
        }

        // ── CONTENT ──
        private void ComposeContent(PageDescriptor page, TranscriptDto data, Func<string, string> T, string lang)
        {
            page.Content().PaddingTop(12).Column(col =>
            {
                col.Spacing(12);

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

                // ── Academic summary cards (4 box) ──
                col.Item().Row(row =>
                {
                    row.Spacing(10);
                    var attClr = data.AttendancePercentage >= 80 ? Success : data.AttendancePercentage >= 60 ? Warning : Danger;
                    var gradeClr = data.OfficialGrade >= 70 ? Success : data.OfficialGrade >= 50 ? Warning : Danger;

                    SummaryCard(row.RelativeItem(), T("Attendance"), $"{data.AttendancePercentage:F1}%", attClr);
                    SummaryCard(row.RelativeItem(), T("OfficialGrade"), $"{data.OfficialGrade:F1}%", gradeClr);
                    SummaryCard(row.RelativeItem(), T("GPA"), data.GPA.ToString("F2"), Primary);

                    var certText = data.CertificateEligible ? T("Eligible") : T("NotEligible");
                    var certClr = data.CertificateEligible ? Success : Danger;
                    SummaryCard(row.RelativeItem(), T("CertificateStatus"), certText, certClr);
                });

                // ── Attendance breakdown ──
                col.Item().Element(c => SectionTitle(c, T("AttendanceSummary")));
                col.Item().Row(row =>
                {
                    row.Spacing(8);
                    StatBox(row.RelativeItem(), T("Present"), data.PresentCount.ToString(), Success);
                    StatBox(row.RelativeItem(), T("Late"), data.LateCount.ToString(), Warning);
                    StatBox(row.RelativeItem(), T("Absent"), data.AbsentCount.ToString(), Danger);
                    StatBox(row.RelativeItem(), T("TotalSessions"), data.TotalSessions.ToString(), Muted);
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
                col.Item().Padding(10).Column(cc =>
                {
                    cc.Item().AlignCenter().Text(value).Bold().FontSize(15).FontColor(color);
                    cc.Item().AlignCenter().PaddingTop(2).Text(label).FontSize(7).FontColor(Muted);
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
                    cols.RelativeColumn(3);   // Assessment
                    cols.RelativeColumn(1.6f); // Type
                    cols.RelativeColumn(1.2f); // Date
                    cols.RelativeColumn(1);   // Max
                    cols.RelativeColumn(1);   // Score
                    cols.RelativeColumn(1);   // Weight
                    cols.RelativeColumn(1.3f); // Contribution
                });

                // Header
                table.Cell().Background(Primary).PaddingVertical(5).PaddingHorizontal(5).Text(T("Assessment")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Primary).PaddingVertical(5).PaddingHorizontal(5).Text(T("Type")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Primary).PaddingVertical(5).PaddingHorizontal(5).Text(T("Date")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Primary).PaddingVertical(5).PaddingHorizontal(5).AlignRight().Text(T("MaxScore")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Primary).PaddingVertical(5).PaddingHorizontal(5).AlignRight().Text(T("Score")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Primary).PaddingVertical(5).PaddingHorizontal(5).AlignRight().Text(T("Weight")).FontSize(7.5f).Bold().FontColor("#FFFFFF");
                table.Cell().Background(Primary).PaddingVertical(5).PaddingHorizontal(5).AlignRight().Text(T("Contribution")).FontSize(7.5f).Bold().FontColor("#FFFFFF");

                bool alt = false;
                foreach (var a in data.OfficialAssessments)
                {
                    var bg = alt ? SoftBg : "#FFFFFF";
                    alt = !alt;
                    var scoreClr = !a.IsGraded ? Muted : a.Percentage >= 70 ? Success : a.Percentage >= 50 ? Warning : Danger;

                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).Text(a.Title).FontSize(8);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).Text(a.Type).FontSize(8);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).Text(a.Date.ToString("dd.MM.yy")).FontSize(8);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).AlignRight().Text(a.MaxScore.ToString("F0")).FontSize(8);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).AlignRight().Text(a.IsGraded ? a.Score?.ToString("F1") ?? "—" : "—").FontSize(8).FontColor(scoreClr);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).AlignRight().Text($"{a.WeightPercent:F0}%").FontSize(8);
                    table.Cell().Background(bg).BorderBottom(0.5f).BorderColor(Border).Padding(5).AlignRight().Text(a.IsGraded ? a.WeightedScore?.ToString("F2") ?? "—" : "—").FontSize(8);
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
            // Nəticə: keçdi/keçmədi — sertifikat eligibility ilə uyğun (calculation toxunulmur)
            var resultText = data.CertificateEligible ? T("Passed") : (data.OfficialGrade >= 50 ? T("Pending") : T("Failed"));
            var resultClr = data.CertificateEligible ? Success : (data.OfficialGrade >= 50 ? Warning : Danger);

            c.PaddingTop(6).Border(1).BorderColor(Border).Background(SoftBg).Padding(14).Column(col =>
            {
                col.Item().Text(T("FinalResult").ToUpperInvariant()).Bold().FontSize(10).FontColor(Navy);
                col.Item().PaddingTop(8).Row(row =>
                {
                    row.RelativeItem().Column(cc =>
                    {
                        cc.Item().Text(T("OfficialGrade")).FontSize(8).FontColor(Muted);
                        cc.Item().Text($"{data.OfficialGrade:F1}%").Bold().FontSize(13).FontColor(gradeClr);
                    });
                    row.RelativeItem().Column(cc =>
                    {
                        cc.Item().Text(T("GPA")).FontSize(8).FontColor(Muted);
                        cc.Item().Text(data.GPA.ToString("F2")).Bold().FontSize(13).FontColor(Primary);
                    });
                    row.RelativeItem().Column(cc =>
                    {
                        cc.Item().Text(T("Result")).FontSize(8).FontColor(Muted);
                        cc.Item().Text(resultText).Bold().FontSize(13).FontColor(resultClr);
                    });
                    row.RelativeItem().Column(cc =>
                    {
                        cc.Item().Text(T("CertificateStatus")).FontSize(8).FontColor(Muted);
                        cc.Item().Text(data.CertificateEligible ? T("Eligible") : T("NotEligible"))
                            .Bold().FontSize(12).FontColor(data.CertificateEligible ? Success : Danger);
                    });
                });
                col.Item().PaddingTop(8).Text(T("CertificateCondition")).FontSize(7).Italic().FontColor(Muted);
            });
        }
    }
}
