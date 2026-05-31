using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SIS.BLL.Services.DTOs.Transcript;

namespace SIS.API.Services
{
    public interface ITranscriptPdfService
    {
        byte[] GeneratePdf(TranscriptDto data);
    }

    public class TranscriptPdfService : ITranscriptPdfService
    {
        public byte[] GeneratePdf(TranscriptDto data)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(35);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    // ── HEADER ──
                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("ACADEX").Bold().FontSize(22).FontColor(Colors.Blue.Darken2);
                                c.Item().Text("Student Information System").FontSize(8).FontColor(Colors.Grey.Medium);
                            });
                            row.ConstantItem(180).AlignRight().Column(c =>
                            {
                                c.Item().Text("AKADEMİK TRANSKRİPT").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                                c.Item().Text($"Tarix: {data.GeneratedAt:dd.MM.yyyy HH:mm}").FontSize(7).FontColor(Colors.Grey.Medium);
                            });
                        });
                        col.Item().PaddingVertical(6).LineHorizontal(2).LineColor(Colors.Blue.Darken2);
                    });

                    // ── CONTENT ──
                    page.Content().PaddingTop(8).Column(col =>
                    {
                        // ── Student + Course Info ──
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                            {
                                c.Item().Text("TƏLƏBƏ MƏLUMATLARI").Bold().FontSize(8).FontColor(Colors.Blue.Darken2);
                                c.Item().PaddingTop(4).Text($"Ad Soyad: {data.StudentName}").FontSize(9);
                                c.Item().Text($"Tələbə №: {data.StudentNumber}").FontSize(9);
                                if (!string.IsNullOrEmpty(data.Email))
                                    c.Item().Text($"E-poçt: {data.Email}").FontSize(8).FontColor(Colors.Grey.Darken1);
                                c.Item().Text($"Qeydiyyat ili: {data.EnrollmentYear}").FontSize(8);
                            });
                            row.ConstantItem(10);
                            row.RelativeItem().Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(10).Column(c =>
                            {
                                c.Item().Text("KURS MƏLUMATLARI").Bold().FontSize(8).FontColor(Colors.Blue.Darken2);
                                c.Item().PaddingTop(4).Text($"Kurs: {data.CourseName}").FontSize(9);
                                c.Item().Text($"Qrup: {data.GroupName}").FontSize(9);
                                c.Item().Text($"Müəllim: {data.TeacherName}").FontSize(9);
                                if (!string.IsNullOrEmpty(data.AcademicYear))
                                    c.Item().Text($"Akademik il: {data.AcademicYear}").FontSize(8);
                                c.Item().Text($"Tarix: {data.StartDate:dd.MM.yyyy} — {data.EndDate:dd.MM.yyyy}").FontSize(8).FontColor(Colors.Grey.Darken1);
                            });
                        });

                        col.Item().PaddingVertical(10);

                        // ── Attendance ──
                        col.Item().Text("DAVAMİYYƏT").Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
                        col.Item().PaddingTop(4).Row(row =>
                        {
                            void AttBox(IContainer c, string label, string value, string color)
                            {
                                c.Border(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(cc =>
                                {
                                    cc.Item().AlignCenter().Text(value).Bold().FontSize(14).FontColor(color);
                                    cc.Item().AlignCenter().Text(label).FontSize(7).FontColor(Colors.Grey.Medium);
                                });
                            }
                            AttBox(row.RelativeItem(), "Cəmi Dərs", data.TotalSessions.ToString(), Colors.Blue.Darken2);
                            AttBox(row.RelativeItem(), "İştirak", data.PresentCount.ToString(), Colors.Green.Darken1);
                            AttBox(row.RelativeItem(), "Gecikmə", data.LateCount.ToString(), Colors.Orange.Darken1);
                            AttBox(row.RelativeItem(), "Qayıb", data.AbsentCount.ToString(), Colors.Red.Darken1);
                            var attClr = data.AttendancePercentage >= 80 ? Colors.Green.Darken1 : data.AttendancePercentage >= 60 ? Colors.Orange.Darken1 : Colors.Red.Darken1;
                            AttBox(row.RelativeItem(), "Davamiyyət", $"%{data.AttendancePercentage:F0}", attClr);
                        });

                        col.Item().PaddingVertical(10);

                        // ── Official Assessments ──
                        col.Item().Text("RƏSMİ QİYMƏTLƏNDİRMƏ (Official Assessments)").Bold().FontSize(10).FontColor(Colors.Blue.Darken2);
                        col.Item().PaddingTop(4).Column(inner =>
                        {
                            // Header row
                            inner.Item().Background(Colors.Blue.Darken2).Padding(0).Row(hr =>
                            {
                                void H(IContainer c, string t, float w) => c.Width(w).Padding(5).Text(t).FontSize(7).FontColor(Colors.White).Bold();
                                H(hr.ConstantItem(140), "Başlıq", 140);
                                H(hr.ConstantItem(70), "Növ", 70);
                                H(hr.ConstantItem(70), "Tarix", 70);
                                H(hr.ConstantItem(50), "Max Bal", 50);
                                H(hr.ConstantItem(50), "Bal", 50);
                                H(hr.ConstantItem(50), "Çəki %", 50);
                                H(hr.RelativeItem(), "Çəkili Bal", 80);
                            });

                            foreach (var a in data.OfficialAssessments)
                            {
                                var scoreClr = !a.IsGraded ? Colors.Grey.Medium : a.Percentage >= 70 ? Colors.Green.Darken1 : a.Percentage >= 50 ? Colors.Orange.Darken1 : Colors.Red.Darken1;
                                inner.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Row(dr =>
                                {
                                    dr.ConstantItem(140).Padding(4).Text(a.Title).FontSize(8);
                                    dr.ConstantItem(70).Padding(4).Text(a.Type).FontSize(8);
                                    dr.ConstantItem(70).Padding(4).Text(a.Date.ToString("dd.MM.yy")).FontSize(8);
                                    dr.ConstantItem(50).Padding(4).Text(a.MaxScore.ToString("F0")).FontSize(8);
                                    dr.ConstantItem(50).Padding(4).Text(a.IsGraded ? a.Score?.ToString("F1") ?? "—" : "—").FontSize(8).FontColor(scoreClr);
                                    dr.ConstantItem(50).Padding(4).Text($"%{a.WeightPercent:F0}").FontSize(8);
                                    dr.RelativeItem().Padding(4).Text(a.IsGraded ? a.WeightedScore?.ToString("F2") ?? "—" : "—").FontSize(8);
                                });
                            }
                        });

                        // Official Grade box
                        var gradeClr = data.OfficialGrade >= 70 ? Colors.Green.Darken1 : data.OfficialGrade >= 50 ? Colors.Orange.Darken1 : Colors.Red.Darken1;
                        col.Item().PaddingTop(6).AlignRight().Width(300).Border(1).BorderColor(Colors.Blue.Darken2).Background(Colors.Blue.Lighten5).Padding(8).Row(r =>
                        {
                            r.RelativeItem().Column(c =>
                            {
                                c.Item().Text("RƏSMİ QİYMƏT").Bold().FontSize(9);
                                c.Item().Text("Official Course Grade").FontSize(7).FontColor(Colors.Grey.Medium);
                            });
                            r.ConstantItem(60).AlignRight().AlignMiddle().Text($"%{data.OfficialGrade:F1}").Bold().FontSize(16).FontColor(gradeClr);
                            r.ConstantItem(45).AlignRight().AlignMiddle().Column(c =>
                            {
                                c.Item().AlignRight().Text(data.LetterGrade).Bold().FontSize(14).FontColor(Colors.Blue.Darken2);
                                c.Item().AlignRight().Text($"GPA: {data.GPA:F2}").FontSize(7);
                            });
                        });

                        col.Item().PaddingVertical(10);

                        // ── Homework Assignments ──
                        if (data.HomeworkAssignments.Any())
                        {
                            col.Item().Text("EV TAPŞIRIQLARI (Homework Performance)").Bold().FontSize(10).FontColor(Colors.Purple.Darken1);
                            col.Item().Text("Bu bölmə GPA hesablamasına daxil deyil").FontSize(7).Italic().FontColor(Colors.Grey.Medium);
                            col.Item().PaddingTop(4).Column(inner =>
                            {
                                inner.Item().Background(Colors.Purple.Darken1).Padding(0).Row(hr =>
                                {
                                    void H2(IContainer c, string t) => c.Padding(5).Text(t).FontSize(7).FontColor(Colors.White).Bold();
                                    H2(hr.ConstantItem(140), "Başlıq");
                                    H2(hr.ConstantItem(70), "Son Tarix");
                                    H2(hr.ConstantItem(50), "Max Bal");
                                    H2(hr.ConstantItem(50), "Bal");
                                    H2(hr.ConstantItem(50), "Faiz");
                                    H2(hr.RelativeItem(), "Bağlantı");
                                });

                                foreach (var a in data.HomeworkAssignments)
                                {
                                    var sClr = !a.IsGraded ? Colors.Grey.Medium : a.Percentage >= 70 ? Colors.Green.Darken1 : Colors.Orange.Darken1;
                                    inner.Item().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Row(dr =>
                                    {
                                        dr.ConstantItem(140).Padding(4).Text(a.Title).FontSize(8);
                                        dr.ConstantItem(70).Padding(4).Text(a.DueDate.ToString("dd.MM.yy")).FontSize(8);
                                        dr.ConstantItem(50).Padding(4).Text(a.MaxScore.ToString("F0")).FontSize(8);
                                        dr.ConstantItem(50).Padding(4).Text(a.IsGraded ? a.Score?.ToString("F1") ?? "—" : "—").FontSize(8).FontColor(sClr);
                                        dr.ConstantItem(50).Padding(4).Text(a.Percentage.HasValue ? $"%{a.Percentage:F0}" : "—").FontSize(8);
                                        dr.RelativeItem().Padding(4).Text(a.IsLinkedToAssessment ? "Rəsmi" : "Ev tapş.").FontSize(8)
                                            .FontColor(a.IsLinkedToAssessment ? Colors.Green.Darken1 : Colors.Grey.Medium);
                                    });
                                }
                            });

                            if (data.HomeworkAverage.HasValue)
                                col.Item().PaddingTop(4).Text($"Ev Tapşırığı Ortalaması: %{data.HomeworkAverage:F1}").FontSize(9).FontColor(Colors.Purple.Darken1);

                            col.Item().PaddingVertical(8);
                        }

                        // ── Performance & Certificate ──
                        col.Item().Border(1).BorderColor(Colors.Grey.Lighten1).Padding(12).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("PERFORMANS STATUSU").Bold().FontSize(9).FontColor(Colors.Blue.Darken2);
                                c.Item().PaddingTop(4).Text(data.PerformanceStatus).Bold().FontSize(11).FontColor(gradeClr);
                            });
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("SERTİFİKAT").Bold().FontSize(9).FontColor(Colors.Blue.Darken2);
                                c.Item().PaddingTop(4).Text(data.CertificateEligible
                                        ? "Sertifikat almağa hüquqludur"
                                        : "Sertifikat almaq üçün şərtlər ödənmir")
                                    .Bold().FontSize(9)
                                    .FontColor(data.CertificateEligible ? Colors.Green.Darken1 : Colors.Red.Darken1);
                                c.Item().Text("Şərt: Qiymət >= 60% və Davamiyyət >= 70%").FontSize(7).FontColor(Colors.Grey.Medium);
                            });
                        });
                    });

                    // ── FOOTER ──
                    page.Footer().Column(col =>
                    {
                        col.Item().LineHorizontal(0.5f).LineColor(Colors.Grey.Lighten2);
                        col.Item().PaddingTop(4).Row(row =>
                        {
                            row.RelativeItem().Text($"Acadex SIS · {data.CourseName} · {data.GroupName}").FontSize(7).FontColor(Colors.Grey.Medium);
                            row.RelativeItem().AlignRight().Text(text =>
                            {
                                text.Span("Səhifə ").FontSize(7).FontColor(Colors.Grey.Medium);
                                text.CurrentPageNumber().FontSize(7).FontColor(Colors.Grey.Medium);
                                text.Span(" / ").FontSize(7).FontColor(Colors.Grey.Medium);
                                text.TotalPages().FontSize(7).FontColor(Colors.Grey.Medium);
                            });
                        });
                    });
                });
            });

            return document.GeneratePdf();
        }
    }
}
