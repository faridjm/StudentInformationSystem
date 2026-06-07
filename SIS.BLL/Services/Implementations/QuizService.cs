using SIS.BLL.Services.DTOs.Quiz;
using SIS.BLL.Services.Interfaces;
using SIS.DAL.UnitOfWork;
using SIS.Entities.Enums;
using SIS.Entities.Models;
using static SIS.BLL.Exceptions.AppException;

namespace SIS.BLL.Services.Implementations
{
    public class QuizService : IQuizService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStudentService _studentService;
        private readonly ICurrentUser _currentUser;
        private readonly IAuditLogService _audit;
        private readonly INotificationService _notifications;
        private readonly IEmailService _emailService;

        public QuizService(IUnitOfWork unitOfWork, IStudentService studentService, ICurrentUser currentUser, IAuditLogService audit, INotificationService notifications, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _studentService = studentService;
            _currentUser = currentUser;
            _audit = audit;
            _notifications = notifications;
            _emailService = emailService;
        }

        // ── Teacher: read ────────────────────────────────────────────
        public async Task<QuizDto?> GetByAssessmentItemAsync(int assessmentItemId)
        {
            var quiz = await _unitOfWork.Quizzes.GetByAssessmentItemAsync(assessmentItemId);
            if (quiz == null) return null;
            return await MapToDtoAsync(quiz.Id);
        }

        public async Task<QuizDto?> GetByIdAsync(int id) => await MapToDtoAsync(id);

        public async Task<QuizDetailDto?> GetDetailAsync(int id)
        {
            var quiz = await _unitOfWork.Quizzes.GetWithQuestionsAsync(id);
            if (quiz == null) return null;
            return new QuizDetailDto
            {
                Id = quiz.Id,
                AssessmentItemId = quiz.AssessmentItemId,
                AssessmentItemTitle = quiz.AssessmentItem?.Title ?? "",
                GroupId = quiz.GroupId,
                Title = quiz.Title,
                Description = quiz.Description,
                IsActive = quiz.IsActive,
                DurationMinutes = quiz.DurationMinutes,
                Questions = quiz.Questions.OrderBy(q => q.Order).Select(q => new QuizQuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Type = q.Type,
                    Points = q.Points,
                    Order = q.Order,
                    Options = q.Options.OrderBy(o => o.Order).Select(o => new QuizOptionDto
                    {
                        Id = o.Id,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect,
                        Order = o.Order
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<IEnumerable<QuizDto>> GetByGroupAsync(int groupId)
        {
            var quizzes = await _unitOfWork.Quizzes.GetByGroupAsync(groupId);
            return quizzes.Select(MapToDto);
        }

        public async Task<IEnumerable<QuizDto>> GetActiveByGroupAsync(int groupId)
        {
            var quizzes = await _unitOfWork.Quizzes.GetByGroupAsync(groupId);
            return quizzes.Where(q => q.IsActive).Select(MapToDto);
        }

        // ── Teacher: create / update / manage ────────────────────────
        public async Task<QuizDto> CreateAsync(CreateQuizDto dto)
        {
            var item = await _unitOfWork.AssessmentItems.GetByIdAsync(dto.AssessmentItemId)
                ?? throw new NotFoundException("Qiymətləndirmə elementi tapılmadı");

            if (item.Type != AssessmentType.Quiz)
                throw new BadRequestException("Quiz yalnız Type=Quiz olan qiymətləndirmə elementinə bağlana bilər");

            var existing = await _unitOfWork.Quizzes.GetByAssessmentItemAsync(dto.AssessmentItemId);
            if (existing != null)
                throw new ConflictException("Bu qiymətləndirmə üçün artıq quiz mövcuddur");

            var now = DateTime.UtcNow;
            var quiz = new Quiz
            {
                AssessmentItemId = item.Id,
                GroupId = item.GroupId,
                Title = string.IsNullOrWhiteSpace(dto.Title) ? item.Title : dto.Title,
                Description = dto.Description,
                DurationMinutes = dto.DurationMinutes,
                IsActive = false,
                CreatedAt = now
            };

            int order = 0;
            foreach (var qd in dto.Questions)
            {
                var q = new QuizQuestion
                {
                    Text = qd.Text,
                    Type = qd.Type,
                    Points = qd.Points <= 0 ? 1 : qd.Points,
                    Order = order++,
                    CreatedAt = now
                };
                int oOrder = 0;
                foreach (var od in qd.Options)
                    q.Options.Add(new QuizOption { Text = od.Text, IsCorrect = od.IsCorrect, Order = oOrder++, CreatedAt = now });
                quiz.Questions.Add(q);
            }

            await _unitOfWork.Quizzes.AddAsync(quiz);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync("Create", "Quiz", quiz.Id.ToString(),
                $"Quiz yaradıldı: {quiz.Title} (AssessmentItem #{quiz.AssessmentItemId})");

            return (await MapToDtoAsync(quiz.Id))!;
        }

        public async Task UpdateAsync(UpdateQuizDto dto)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(dto.Id)
                ?? throw new NotFoundException("Quiz tapılmadı");

            quiz.Title = dto.Title;
            quiz.Description = dto.Description;
            quiz.DurationMinutes = dto.DurationMinutes;
            _unitOfWork.Quizzes.Update(quiz);
            await _unitOfWork.SaveAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdAsync(id)
                ?? throw new NotFoundException("Quiz tapılmadı");
            _unitOfWork.Quizzes.SoftDelete(quiz);
            await _unitOfWork.SaveAsync();
        }

        public async Task SetActiveAsync(int id, bool active)
        {
            var quiz = await _unitOfWork.Quizzes.GetWithQuestionsAsync(id)
                ?? throw new NotFoundException("Quiz tapılmadı");

            if (active && !quiz.Questions.Any())
                throw new BadRequestException("Sualı olmayan quiz aktivləşdirilə bilməz");

            quiz.IsActive = active;
            _unitOfWork.Quizzes.Update(quiz);
            await _unitOfWork.SaveAsync();

            await _audit.LogAsync(active ? "Activate" : "Deactivate", "Quiz", quiz.Id.ToString(),
                $"Quiz {(active ? "aktivləşdirildi" : "dayandırıldı")}: {quiz.Title}");

            if (active)
            {
                var enrolled = await _unitOfWork.Enrollments.GetByGroupAsync(quiz.GroupId);
                var enrolledList = enrolled.Where(e => !e.IsDeleted && e.Student != null && e.Student.AppUserId != null).ToList();
                var studentUserIds = enrolledList.Select(e => e.Student.AppUserId);
                await _notifications.SendToManyAsync(studentUserIds,
                    "Yeni quiz aktiv", $"\"{quiz.Title}\" quiz-i cavablandırmaq üçün açıqdır",
                    NotificationType.QuizActivated, "/Student/Quizzes");

                // Email — fail olsa main əməliyyatı pozma
                var group = await _unitOfWork.Groups.GetGroupWithDetailsAsync(quiz.GroupId);
                var courseName = group?.Course?.Name ?? "";
                var groupName = group?.Name ?? "";
                foreach (var e in enrolledList)
                {
                    var email = e.Student?.AppUser?.Email;
                    var name = e.Student?.AppUser?.FullName ?? "";
                    if (string.IsNullOrEmpty(email)) continue;
                    _ = _emailService.SendQuizActivatedAsync(email, name, quiz.Title, courseName, groupName, null);
                }
            }
        }

        public async Task<QuizQuestionDto> AddQuestionAsync(int quizId, CreateQuizQuestionDto dto)
        {
            var quiz = await _unitOfWork.Quizzes.GetWithQuestionsAsync(quizId)
                ?? throw new NotFoundException("Quiz tapılmadı");

            await EnsureNoSubmittedAttemptsAsync(quizId);

            if (dto.Options == null || dto.Options.Count < 2)
                throw new BadRequestException("Sualda ən azı 2 variant olmalıdır");
            if (!dto.Options.Any(o => o.IsCorrect))
                throw new BadRequestException("Ən azı bir düz variant qeyd edilməlidir");
            if (dto.Type != QuestionType.MultipleChoice && dto.Options.Count(o => o.IsCorrect) != 1)
                throw new BadRequestException("Bu sual tipi üçün yalnız bir düz variant ola bilər");

            var now = DateTime.UtcNow;
            int order = quiz.Questions.Any() ? quiz.Questions.Max(q => q.Order) + 1 : 0;
            var question = new QuizQuestion
            {
                QuizId = quizId,
                Text = dto.Text,
                Type = dto.Type,
                Points = dto.Points <= 0 ? 1 : dto.Points,
                Order = order,
                CreatedAt = now
            };
            int oOrder = 0;
            foreach (var od in dto.Options)
                question.Options.Add(new QuizOption { Text = od.Text, IsCorrect = od.IsCorrect, Order = oOrder++, CreatedAt = now });

            quiz.Questions.Add(question);
            await _unitOfWork.SaveAsync();

            return new QuizQuestionDto
            {
                Id = question.Id,
                Text = question.Text,
                Type = question.Type,
                Points = question.Points,
                Order = question.Order,
                Options = question.Options.Select(o => new QuizOptionDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    IsCorrect = o.IsCorrect,
                    Order = o.Order
                }).ToList()
            };
        }

        public async Task DeleteQuestionAsync(int quizId, int questionId)
        {
            var quiz = await _unitOfWork.Quizzes.GetWithQuestionsAsync(quizId)
                ?? throw new NotFoundException("Quiz tapılmadı");

            await EnsureNoSubmittedAttemptsAsync(quizId);

            var question = quiz.Questions.FirstOrDefault(q => q.Id == questionId)
                ?? throw new NotFoundException("Sual tapılmadı");

            question.IsDeleted = true;
            question.UpdatedAt = DateTime.UtcNow;
            foreach (var o in question.Options)
            {
                o.IsDeleted = true;
                o.UpdatedAt = DateTime.UtcNow;
            }
            await _unitOfWork.SaveAsync();
        }

        public async Task<IEnumerable<QuizResultDto>> GetResultsAsync(int quizId)
        {
            var attempts = await _unitOfWork.QuizAttempts.GetByQuizAsync(quizId);
            return attempts.Select(a => new QuizResultDto
            {
                AttemptId = a.Id,
                QuizId = a.QuizId,
                EnrollmentId = a.EnrollmentId,
                StudentName = a.Enrollment?.Student?.AppUser?.FullName ?? "",
                StudentNumber = a.Enrollment?.Student?.StudentNumber ?? "",
                Score = a.Score,
                IsSubmitted = a.IsSubmitted,
                SubmittedAt = a.SubmittedAt
            });
        }

        // ── Student: take ────────────────────────────────────────────
        public async Task<TakeQuizDto> StartAttemptAsync(int quizId, int enrollmentId)
        {
            var quiz = await _unitOfWork.Quizzes.GetWithQuestionsAsync(quizId)
                ?? throw new NotFoundException("Quiz tapılmadı");

            if (!quiz.IsActive)
                throw new BadRequestException("Quiz hazırda aktiv deyil");

            await ValidateStudentEnrollmentAsync(enrollmentId, quiz);

            var attempt = await _unitOfWork.QuizAttempts.GetByQuizAndEnrollmentAsync(quizId, enrollmentId);
            if (attempt != null && attempt.IsSubmitted)
                return BuildTakeDto(quiz, enrollmentId, attempt, alreadySubmitted: true);

            if (attempt == null)
            {
                attempt = new QuizAttempt
                {
                    QuizId = quizId,
                    EnrollmentId = enrollmentId,
                    StartedAt = DateTime.UtcNow,
                    IsSubmitted = false,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.QuizAttempts.AddAsync(attempt);
                await _unitOfWork.SaveAsync();
            }

            return BuildTakeDto(quiz, enrollmentId, attempt, alreadySubmitted: false);
        }

        public async Task<QuizResultDto> SubmitAttemptAsync(SubmitQuizDto dto)
        {
            var quiz = await _unitOfWork.Quizzes.GetWithQuestionsAsync(dto.QuizId)
                ?? throw new NotFoundException("Quiz tapılmadı");

            if (!quiz.IsActive)
                throw new BadRequestException("Quiz hazırda aktiv deyil");

            var enrollment = await ValidateStudentEnrollmentAsync(dto.EnrollmentId, quiz);

            var attempt = await _unitOfWork.QuizAttempts.GetByQuizAndEnrollmentAsync(dto.QuizId, dto.EnrollmentId);
            if (attempt != null && attempt.IsSubmitted)
                throw new ConflictException("Bu quiz-i artıq cavablandırmısınız");

            var now = DateTime.UtcNow;
            bool isNew = attempt == null;
            attempt ??= new QuizAttempt
            {
                QuizId = dto.QuizId,
                EnrollmentId = dto.EnrollmentId,
                StartedAt = now,
                CreatedAt = now
            };

            // ── Auto grading ──
            decimal totalPoints = quiz.Questions.Sum(q => q.Points);
            decimal earnedPoints = 0m;

            foreach (var q in quiz.Questions)
            {
                var submitted = dto.Answers.FirstOrDefault(a => a.QuizQuestionId == q.Id);
                var selected = submitted?.SelectedOptionIds?.Distinct().ToList() ?? new List<int>();
                var (isCorrect, points) = GradeQuestion(q, selected);
                earnedPoints += points;

                attempt.Answers.Add(new QuizAnswer
                {
                    QuizQuestionId = q.Id,
                    SelectedOptionIds = selected.Count > 0 ? string.Join(",", selected) : null,
                    IsCorrect = isCorrect,
                    PointsAwarded = points,
                    CreatedAt = now
                });
            }

            decimal percentage = totalPoints > 0 ? Math.Round(earnedPoints / totalPoints * 100m, 2) : 0m;
            attempt.Score = percentage;
            attempt.IsSubmitted = true;
            attempt.SubmittedAt = now;

            if (isNew)
                await _unitOfWork.QuizAttempts.AddAsync(attempt);
            await _unitOfWork.SaveAsync();

            // ── Nəticəni rəsmi AssessmentSubmission-a yaz (GPA/Transcript-ə düşür) ──
            decimal maxScore = quiz.AssessmentItem?.MaxScore ?? 0m;
            decimal officialScore = Math.Round(percentage / 100m * maxScore, 2);
            string feedback = $"Quiz nəticəsi: {percentage}% ({earnedPoints}/{totalPoints} bal)";

            var assessmentSub = await _unitOfWork.AssessmentSubmissions
                .GetByItemAndEnrollmentAsync(quiz.AssessmentItemId, dto.EnrollmentId);

            if (assessmentSub == null)
            {
                assessmentSub = new AssessmentSubmission
                {
                    AssessmentItemId = quiz.AssessmentItemId,
                    EnrollmentId = dto.EnrollmentId,
                    Score = officialScore,
                    Feedback = feedback,
                    IsGraded = true,
                    GradedAt = now,
                    CreatedAt = now
                };
                await _unitOfWork.AssessmentSubmissions.AddAsync(assessmentSub);
            }
            else
            {
                assessmentSub.Score = officialScore;
                assessmentSub.Feedback = feedback;
                assessmentSub.IsGraded = true;
                assessmentSub.GradedAt = now;
                _unitOfWork.AssessmentSubmissions.Update(assessmentSub);
            }
            await _unitOfWork.SaveAsync();

            // GPA-nı yenilə
            await _studentService.UpdateGPAAsync(enrollment.StudentId);

            await _audit.LogAsync("Submit", "Quiz", quiz.Id.ToString(),
                $"Quiz cavablandırıldı: {quiz.Title} — nəticə {percentage}% (Enrollment #{dto.EnrollmentId})");

            var group = await _unitOfWork.Groups.GetGroupWithDetailsAsync(quiz.GroupId);
            await _notifications.SendNotificationAsync(group?.Teacher?.AppUserId,
                "Quiz cavablandı", $"\"{quiz.Title}\" quiz-inə yeni cavab daxil oldu",
                NotificationType.QuizSubmitted, $"/Teacher/Quizzes/Results/{quiz.Id}");

            return new QuizResultDto
            {
                AttemptId = attempt.Id,
                QuizId = quiz.Id,
                EnrollmentId = dto.EnrollmentId,
                StudentName = enrollment.Student?.AppUser?.FullName ?? "",
                StudentNumber = enrollment.Student?.StudentNumber ?? "",
                Score = percentage,
                IsSubmitted = true,
                SubmittedAt = now
            };
        }

        public async Task<QuizResultDto?> GetMyResultAsync(int quizId, int enrollmentId)
        {
            var attempt = await _unitOfWork.QuizAttempts.GetByQuizAndEnrollmentAsync(quizId, enrollmentId);
            if (attempt == null) return null;
            return new QuizResultDto
            {
                AttemptId = attempt.Id,
                QuizId = attempt.QuizId,
                EnrollmentId = attempt.EnrollmentId,
                Score = attempt.Score,
                IsSubmitted = attempt.IsSubmitted,
                SubmittedAt = attempt.SubmittedAt
            };
        }

        // ── Detallı nəticə (sual-sual) ─────────────────────────────────
        public async Task<QuizAttemptDetailDto?> GetAttemptDetailAsync(int attemptId, string requestingUserId)
        {
            var attempt = await _unitOfWork.QuizAttempts.GetDetailAsync(attemptId);
            if (attempt == null || !attempt.IsSubmitted) return null;

            // Authorization: ya qrupun müəllimi, ya da cəhdin sahibi olan tələbə
            var quiz = attempt.Quiz;
            var group = await _unitOfWork.Groups.GetGroupWithDetailsAsync(quiz.GroupId);
            bool authorized = false;

            var teacher = await _unitOfWork.Teachers.GetTeacherByUserIdAsync(requestingUserId);
            if (teacher != null && group != null && group.TeacherId == teacher.Id)
                authorized = true;

            if (!authorized)
            {
                var student = await _unitOfWork.Students.GetStudentByUserIdAsync(requestingUserId);
                if (student != null && attempt.Enrollment?.StudentId == student.Id)
                    authorized = true;
            }

            if (!authorized)
                throw new UnauthorizedException("Bu nəticəyə baxmaq icazəniz yoxdur");

            var dto = new QuizAttemptDetailDto
            {
                AttemptId = attempt.Id,
                QuizId = attempt.QuizId,
                QuizTitle = quiz.Title,
                EnrollmentId = attempt.EnrollmentId,
                StudentName = attempt.Enrollment?.Student?.AppUser?.FullName ?? "",
                StudentNumber = attempt.Enrollment?.Student?.StudentNumber ?? "",
                Score = attempt.Score,
                SubmittedAt = attempt.SubmittedAt,
                TotalQuestions = quiz.Questions.Count,
                CorrectCount = attempt.Answers.Count(a => a.IsCorrect),
                WrongCount = attempt.Answers.Count(a => !a.IsCorrect)
            };

            // Sualları order üzrə map et — hər sualın option-ları + tələbənin seçimi
            foreach (var q in quiz.Questions.OrderBy(q => q.Order))
            {
                var ans = attempt.Answers.FirstOrDefault(a => a.QuizQuestionId == q.Id);
                var selectedIds = ParseSelectedOptionIds(ans?.SelectedOptionIds);
                var correctIds = q.Options.Where(o => o.IsCorrect).Select(o => o.Id).ToList();

                dto.Questions.Add(new QuizQuestionResultDto
                {
                    QuestionId = q.Id,
                    Order = q.Order,
                    Text = q.Text,
                    Points = q.Points,
                    PointsAwarded = ans?.PointsAwarded ?? 0,
                    IsCorrect = ans?.IsCorrect ?? false,
                    SelectedOptionIds = selectedIds,
                    CorrectOptionIds = correctIds,
                    Options = q.Options.OrderBy(o => o.Order).Select(o => new QuizOptionResultDto
                    {
                        Id = o.Id,
                        Text = o.Text,
                        IsCorrect = o.IsCorrect,
                        WasSelected = selectedIds.Contains(o.Id)
                    }).ToList()
                });
            }

            return dto;
        }

        private static List<int> ParseSelectedOptionIds(string? csv)
        {
            if (string.IsNullOrWhiteSpace(csv)) return new List<int>();
            return csv.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(s => int.TryParse(s.Trim(), out var id) ? id : 0)
                .Where(id => id > 0)
                .ToList();
        }

        // ── Helpers ──────────────────────────────────────────────────
        private static (bool isCorrect, decimal points) GradeQuestion(QuizQuestion q, List<int> selected)
        {
            var correct = q.Options.Where(o => o.IsCorrect).Select(o => o.Id).OrderBy(x => x).ToList();
            var sel = selected.OrderBy(x => x).ToList();

            bool ok;
            if (q.Type == QuestionType.MultipleChoice)
                ok = correct.Count > 0 && correct.SequenceEqual(sel);
            else // SingleChoice / TrueFalse
                ok = correct.Count == 1 && sel.Count == 1 && sel[0] == correct[0];

            return (ok, ok ? q.Points : 0m);
        }

        private async Task EnsureNoSubmittedAttemptsAsync(int quizId)
        {
            var hasAttempts = await _unitOfWork.QuizAttempts.AnyAsync(a => a.QuizId == quizId && a.IsSubmitted);
            if (hasAttempts)
                throw new BadRequestException("Təhvil verilmiş cəhdlər olduğu üçün suallar dəyişdirilə bilməz");
        }

        private async Task<Enrollment> ValidateStudentEnrollmentAsync(int enrollmentId, Quiz quiz)
        {
            var enrollment = await _unitOfWork.Enrollments.GetEnrollmentWithDetailsAsync(enrollmentId)
                ?? throw new NotFoundException("Qeydiyyat tapılmadı");

            if (enrollment.GroupId != quiz.GroupId)
                throw new BadRequestException("Bu quiz sizin qrupunuza aid deyil");

            if (_currentUser.IsStudent())
            {
                var me = _currentUser.UserId != null
                    ? await _studentService.GetStudentByUserIdAsync(_currentUser.UserId)
                    : null;
                if (me == null || enrollment.StudentId != me.Id)
                    throw new UnauthorizedException("Bu qeydiyyat sizə aid deyil");
            }

            return enrollment;
        }

        private static TakeQuizDto BuildTakeDto(Quiz quiz, int enrollmentId, QuizAttempt attempt, bool alreadySubmitted) => new()
        {
            QuizId = quiz.Id,
            AttemptId = attempt.Id,
            Title = quiz.Title,
            Description = quiz.Description,
            AssessmentItemTitle = quiz.AssessmentItem?.Title ?? "",
            EnrollmentId = enrollmentId,
            DurationMinutes = quiz.DurationMinutes,
            StartedAt = attempt.StartedAt,
            AlreadySubmitted = alreadySubmitted,
            MyScore = alreadySubmitted ? attempt.Score : null,
            Questions = alreadySubmitted
                ? new List<TakeQuestionDto>()
                : quiz.Questions.OrderBy(q => q.Order).Select(q => new TakeQuestionDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Type = q.Type,
                    Points = q.Points,
                    Order = q.Order,
                    Options = q.Options.OrderBy(o => o.Order).Select(o => new TakeOptionDto
                    {
                        Id = o.Id,
                        Text = o.Text,
                        Order = o.Order
                    }).ToList()
                }).ToList()
        };

        private async Task<QuizDto?> MapToDtoAsync(int quizId)
        {
            var quiz = await _unitOfWork.Quizzes.GetByIdWithCountsAsync(quizId);
            return quiz == null ? null : MapToDto(quiz);
        }

        private static QuizDto MapToDto(Quiz q) => new()
        {
            Id = q.Id,
            AssessmentItemId = q.AssessmentItemId,
            AssessmentItemTitle = q.AssessmentItem?.Title ?? "",
            GroupId = q.GroupId,
            Title = q.Title,
            Description = q.Description,
            IsActive = q.IsActive,
            DurationMinutes = q.DurationMinutes,
            MaxScore = q.AssessmentItem?.MaxScore ?? 0m,
            WeightPercent = q.AssessmentItem?.WeightPercent ?? 0m,
            QuestionCount = q.Questions?.Count ?? 0,
            AttemptCount = q.Attempts?.Count(a => a.IsSubmitted) ?? 0
        };
    }
}
