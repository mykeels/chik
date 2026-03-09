using Bogus;
using Newtonsoft.Json;

namespace Chik.Exams.Data;

public static class Seeder
{
    private static readonly Faker Faker = new("en");

    public static async Task Seed(IServiceProvider services)
    {
        var userService = services.GetRequiredService<IUserService>();
        var quizService = services.GetRequiredService<IQuizService>();
        var quizQuestionService = services.GetRequiredService<IQuizQuestionService>();
        var quizQuestionTypeRepository = services.GetRequiredService<IQuizQuestionTypeRepository>();
        var examService = services.GetRequiredService<IExamService>();
        var examAnswerService = services.GetRequiredService<IExamAnswerService>();

        // Seed question types first (required for quiz questions)
        await SeedQuestionTypes(quizQuestionTypeRepository);

        var adminAuth = User.Admin;

        // seed one admin user
        var admin = await userService.Create(adminAuth, new User.Create(
            "admin",
            "admin123",
            [UserRole.Admin]
        ));

        // seed 3 teachers
        var teachers = new List<User>();
        for (int i = 1; i <= 3; i++)
        {
            var teacher = await userService.Create(adminAuth, new User.Create(
                $"teacher{i}",
                "teacher123",
                [UserRole.Teacher]
            ));
            teachers.Add(teacher);
        }

        // seed 10 students
        var students = new List<User>();
        for (int i = 1; i <= 10; i++)
        {
            var student = await userService.Create(adminAuth, new User.Create(
                $"student{i}",
                "student123",
                [UserRole.Student]
            ));
            students.Add(student);
        }

        var teacher1Auth = new Auth(
            Id: teachers[0].Id,
            Username: teachers[0].Username,
            Roles: teachers[0].Roles,
            CreatedAt: teachers[0].CreatedAt,
            UpdatedAt: teachers[0].UpdatedAt
        );

        // seed 3 quizzes: math, science, english
        var quizData = new[]
        {
            ("Mathematics", "Test your math skills with arithmetic, algebra, and geometry problems."),
            ("Science", "Explore physics, chemistry, and biology concepts."),
            ("English", "Assess reading comprehension, grammar, and vocabulary.")
        };

        var quizzes = new List<Quiz>();
        for (int i = 0; i < quizData.Length; i++)
        {
            var examiner = teachers[i % teachers.Count];
            var quiz = await quizService.Create(adminAuth, new Quiz.Create(
                quizData[i].Item1,
                quizData[i].Item2,
                adminAuth.Id,
                examiner.Id,
                TimeSpan.FromMinutes(60)
            ));
            quizzes.Add(quiz);
        }

        // seed 10 quiz questions for each quiz
        var allQuestions = new Dictionary<long, List<QuizQuestion>>();
        foreach (var quiz in quizzes)
        {
            var questions = new List<QuizQuestion>();
            for (int order = 0; order < 10; order++)
            {
                var (typeId, properties) = GetQuestionTypeAndProperties(order);
                var question = await quizQuestionService.Create(adminAuth, new QuizQuestion.Create(
                    quiz.Id,
                    GeneratePrompt(quiz.Title, order),
                    typeId,
                    properties,
                    Score: 10,
                    Order: order
                ));
                questions.Add(question);
            }
            allQuestions[quiz.Id] = questions;
        }

        // seed 1 exam for each student, per quiz
        var allExams = new List<(Exam Exam, List<QuizQuestion> Questions)>();
        foreach (var student in students)
        {
            var studentAuth = new Auth(
                Id: student.Id,
                Username: student.Username,
                Roles: student.Roles,
                CreatedAt: student.CreatedAt,
                UpdatedAt: student.UpdatedAt
            );

            foreach (var quiz in quizzes)
            {
                var exam = await examService.Create(teacher1Auth, new Exam.Create(
                    student.Id,
                    quiz.Id,
                    teacher1Auth.Id
                ));

                // start the exam
                exam = await examService.Start(studentAuth, exam.Id);

                allExams.Add((exam, allQuestions[quiz.Id]));

                // seed exam answers for each exam
                foreach (var question in allQuestions[quiz.Id])
                {
                    var answer = GenerateAnswer(question);
                    await examAnswerService.SubmitAnswer(studentAuth, exam.Id, question.Id, answer);
                }

                // submit the exam
                await examService.End(studentAuth, exam.Id);
            }
        }

        // auto-score exam answers
        foreach (var (exam, _) in allExams)
        {
            await examService.AutoScore(teacher1Auth, exam.Id);
        }

        // examiner score essay answers (those that can't be auto-scored)
        foreach (var (exam, questions) in allExams)
        {
            var examinerAuth = teacher1Auth;
            var answers = await examAnswerService.GetByExamId(examinerAuth, exam.Id);

            foreach (var answer in answers)
            {
                var question = questions.FirstOrDefault(q => q.Id == answer.QuestionId);
                if (question is not null && question.TypeId == QuizQuestionType.Types.Essay)
                {
                    await examAnswerService.ExaminerScore(
                        examinerAuth,
                        answer.Id,
                        Faker.Random.Int(5, 10),
                        Faker.Lorem.Sentence()
                    );
                }
            }
        }
    }

    private static (long TypeId, string Properties) GetQuestionTypeAndProperties(int index)
    {
        return (index % 6) switch
        {
            0 => (QuizQuestionType.Types.MultipleChoice, JsonConvert.SerializeObject(new
            {
                type = "multiple-choice",
                options = new[]
                {
                    new { text = "Option A", isCorrect = true },
                    new { text = "Option B", isCorrect = false },
                    new { text = "Option C", isCorrect = true },
                    new { text = "Option D", isCorrect = false }
                }
            })),
            1 => (QuizQuestionType.Types.SingleChoice, JsonConvert.SerializeObject(new
            {
                type = "single-choice",
                options = new[]
                {
                    new { text = "Option A", isCorrect = true },
                    new { text = "Option B", isCorrect = false },
                    new { text = "Option C", isCorrect = false },
                    new { text = "Option D", isCorrect = false }
                }
            })),
            2 => (QuizQuestionType.Types.TrueOrFalse, JsonConvert.SerializeObject(new
            {
                type = "true-or-false",
                correctAnswer = true
            })),
            3 => (QuizQuestionType.Types.FillInTheBlank, JsonConvert.SerializeObject(new
            {
                type = "fill-in-the-blank",
                acceptedAnswers = new[] { Faker.Lorem.Word(), Faker.Lorem.Word() }
            })),
            4 => (QuizQuestionType.Types.ShortAnswer, JsonConvert.SerializeObject(new
            {
                type = "short-answer",
                acceptedAnswers = new[] { Faker.Lorem.Sentence(3) },
                maxLength = 200
            })),
            _ => (QuizQuestionType.Types.Essay, JsonConvert.SerializeObject(new
            {
                type = "essay",
                minWords = 50,
                maxWords = 300
            }))
        };
    }

    private static string GeneratePrompt(string subject, int index)
    {
        return (subject, index % 6) switch
        {
            ("Mathematics", 0) => "Which of the following are prime numbers?",
            ("Mathematics", 1) => "What is the result of 12 × 12?",
            ("Mathematics", 2) => "Is the square root of 144 equal to 12?",
            ("Mathematics", 3) => "The formula for the area of a circle is π___ ².",
            ("Mathematics", 4) => "Briefly explain what a prime number is.",
            ("Mathematics", _) => "Describe the relationship between circumference and diameter.",
            ("Science", 0) => "Which of the following are noble gases?",
            ("Science", 1) => "What is the chemical symbol for water?",
            ("Science", 2) => "Does light travel faster than sound?",
            ("Science", 3) => "Photosynthesis converts sunlight into ___.",
            ("Science", 4) => "What is Newton's second law of motion?",
            ("Science", _) => "Explain the process of cellular respiration.",
            ("English", 0) => "Which of the following are examples of metaphors?",
            ("English", 1) => "What is the past tense of 'run'?",
            ("English", 2) => "Is 'quickly' an adverb?",
            ("English", 3) => "A word that names a person, place, or thing is called a ___.",
            ("English", 4) => "What is the difference between a simile and a metaphor?",
            ("English", _) => "Write a short essay on the importance of reading.",
            _ => $"Question {index + 1} for {subject}."
        };
    }

    private static string GenerateAnswer(QuizQuestion question)
    {
        return question.TypeId switch
        {
            QuizQuestionType.Types.MultipleChoice => JsonConvert.SerializeObject(new[] { "Option A", "Option C" }),
            QuizQuestionType.Types.SingleChoice => JsonConvert.SerializeObject("Option A"),
            QuizQuestionType.Types.TrueOrFalse => JsonConvert.SerializeObject(true),
            QuizQuestionType.Types.FillInTheBlank => JsonConvert.SerializeObject(Faker.Lorem.Word()),
            QuizQuestionType.Types.ShortAnswer => JsonConvert.SerializeObject(Faker.Lorem.Sentence(5)),
            QuizQuestionType.Types.Essay => JsonConvert.SerializeObject(Faker.Lorem.Paragraphs(2)),
            _ => JsonConvert.SerializeObject(Faker.Lorem.Sentence())
        };
    }

    private static async Task SeedQuestionTypes(IQuizQuestionTypeRepository repository)
    {
        var existingTypes = await repository.GetAll();
        if (existingTypes.Count > 0)
        {
            return;
        }

        var questionTypes = new[]
        {
            new QuizQuestionType.Create("Multiple Choice", "Select multiple correct answers from a list of options"),
            new QuizQuestionType.Create("Single Choice", "Select one correct answer from a list of options"),
            new QuizQuestionType.Create("Fill in the Blank", "Fill in the missing word or phrase"),
            new QuizQuestionType.Create("Essay", "Write a detailed response to the question"),
            new QuizQuestionType.Create("Short Answer", "Provide a brief text response"),
            new QuizQuestionType.Create("True or False", "Determine if the statement is true or false"),
        };

        foreach (var type in questionTypes)
        {
            await repository.Create(type);
        }
    }
}
