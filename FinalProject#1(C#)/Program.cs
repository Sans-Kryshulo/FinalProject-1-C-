using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace QuizApp
{
    class Program
    {
        static string usersFilePath = "users.txt";
        static string quizzesFilePath = "quizzes.txt";
        static Dictionary<string, User> users = new Dictionary<string, User>();
        static Dictionary<string, List<Question>> quizzes = new Dictionary<string, List<Question>>();
        static List<QuizResult> quizResults = new List<QuizResult>();

        static void Main(string[] args)
        {
            LoadUsersFromFile();
            LoadQuizzesFromFile();

            Console.WriteLine("Welcome to the Quiz App!");

            while (true)
            {
                Console.WriteLine("1. Login\n2. Register\n3. Manage Quizzes\n4. Exit");
                Console.Write("Select an option: ");
                string option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        Login();
                        break;
                    case "2":
                        Register();
                        break;
                    case "3":
                        ManageQuizzes();
                        break;
                    case "4":
                        SaveUsersToFile();
                        SaveQuizzesToFile();
                        return;
                    default:
                        Console.WriteLine("Invalid option. Try again.");
                        break;
                }
            }
        }

        static void LoadUsersFromFile()
        {
            if (File.Exists(usersFilePath))
            {
                var lines = File.ReadAllLines(usersFilePath);
                foreach (var line in lines)
                {
                    var parts = line.Split(',');
                    if (parts.Length == 3 && DateTime.TryParse(parts[2], out DateTime dateOfBirth))
                    {
                        users[parts[0]] = new User(parts[0], parts[1], dateOfBirth);
                    }
                }
            }
        }

        static void SaveUsersToFile()
        {
            var lines = users.Select(u => $"{u.Value.Login},{u.Value.Password},{u.Value.DateOfBirth:yyyy-MM-dd}");
            File.WriteAllLines(usersFilePath, lines);
        }

        static void LoadQuizzesFromFile()
        {
            if (File.Exists(quizzesFilePath))
            {
                var lines = File.ReadAllLines(quizzesFilePath);
                string currentCategory = null;
                List<Question> questions = null;

                foreach (var line in lines)
                {
                    if (line.StartsWith("Category:"))
                    {
                        if (currentCategory != null && questions != null)
                        {
                            quizzes[currentCategory] = questions;
                        }
                        currentCategory = line.Substring(9).Trim();
                        questions = new List<Question>();
                    }
                    else if (questions != null)
                    {
                        var parts = line.Split('|');
                        if (parts.Length > 2)
                        {
                            string text = parts[0];
                            string[] options = parts[1].Split(';');
                            string[] correctAnswers = parts[2].Split(';');
                            questions.Add(new Question(text, options, correctAnswers));
                        }
                    }
                }

                if (currentCategory != null && questions != null)
                {
                    quizzes[currentCategory] = questions;
                }
            }
        }

        static void SaveQuizzesToFile()
        {
            var lines = new List<string>();
            foreach (var category in quizzes.Keys)
            {
                lines.Add($"Category:{category}");
                foreach (var question in quizzes[category])
                {
                    string options = string.Join(";", question.Options);
                    string correctAnswers = string.Join(";", question.CorrectAnswers);
                    lines.Add($"{question.Text}|{options}|{correctAnswers}");
                }
            }
            File.WriteAllLines(quizzesFilePath, lines);
        }

        static void Login()
        {
            Console.Write("Enter login: ");
            string login = Console.ReadLine();
            Console.Write("Enter password: ");
            string password = Console.ReadLine();

            if (users.ContainsKey(login) && users[login].Password == password)
            {
                Console.WriteLine("Login successful!");
                UserMenu(users[login]);
            }
            else
            {
                Console.WriteLine("Invalid login or password.");
            }
        }

        static void Register()
        {
            Console.Write("Enter login: ");
            string login = Console.ReadLine();

            if (users.ContainsKey(login))
            {
                Console.WriteLine("This login is already taken.");
                return;
            }

            Console.Write("Enter password: ");
            string password = Console.ReadLine();
            Console.Write("Enter your date of birth (yyyy-mm-dd): ");
            if (DateTime.TryParse(Console.ReadLine(), out DateTime dateOfBirth))
            {
                var user = new User(login, password, dateOfBirth);
                users[login] = user;
                Console.WriteLine("Registration successful! You can now log in.");
                SaveUsersToFile();
            }
            else
            {
                Console.WriteLine("Invalid date format.");
            }
        }

        static void ManageQuizzes()
        {
            while (true)
            {
                Console.WriteLine("1. Create a new quiz\n2. Edit an existing quiz\n3. Back");
                Console.Write("Select an option: ");
                string option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        CreateQuiz();
                        break;
                    case "2":
                        EditQuiz();
                        break;
                    case "3":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Try again.");
                        break;
                }
            }
        }

        static void CreateQuiz()
        {
            Console.Write("Enter quiz category: ");
            string category = Console.ReadLine();

            if (quizzes.ContainsKey(category))
            {
                Console.WriteLine("This category already exists.");
                return;
            }

            List<Question> questions = new List<Question>();

            for (int i = 0; i < 20; i++)
            {
                Console.WriteLine($"\nQuestion {i + 1}:");
                Console.Write("Enter question text: ");
                string text = Console.ReadLine();
                Console.Write("Enter options separated by semicolon (;): ");
                string[] options = Console.ReadLine().Split(';');
                Console.Write("Enter correct answers separated by semicolon (;): ");
                string[] correctAnswers = Console.ReadLine().Split(';');

                questions.Add(new Question(text, options, correctAnswers));
            }

            quizzes[category] = questions;
            Console.WriteLine("Quiz created successfully!");
            SaveQuizzesToFile();
        }

        static void EditQuiz()
        {
            Console.Write("Enter quiz category to edit: ");
            string editCategory = Console.ReadLine();

            if (!quizzes.ContainsKey(editCategory))
            {
                Console.WriteLine("This category does not exist.");
                return;
            }

            var questions = quizzes[editCategory];

            for (int i = 0; i < questions.Count; i++)
            {
                Console.WriteLine($"\nQuestion {i + 1}: {questions[i].Text}");
                Console.Write("Edit question text (leave blank to keep unchanged): ");
                string newText = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newText))
                {
                    questions[i].Text = newText;
                }

                Console.Write("Edit options separated by semicolon (leave blank to keep unchanged): ");
                string newOptions = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newOptions))
                {
                    questions[i].Options = newOptions.Split(';');
                }

                Console.Write("Edit correct answers separated by semicolon (leave blank to keep unchanged): ");
                string newCorrectAnswers = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(newCorrectAnswers))
                {
                    questions[i].CorrectAnswers = newCorrectAnswers.Split(';');
                }
            }

            Console.WriteLine("Quiz updated successfully!");
            SaveQuizzesToFile();
        }

        static void UserMenu(User user)
        {
            while (true)
            {
                Console.WriteLine("\n1. Start a new quiz\n2. View past results\n3. View Top-20 quiz results\n4. Change settings\n5. Logout");
                Console.Write("Select an option: ");
                string option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        StartQuiz(user);
                        break;
                    case "2":
                        ViewPastResults(user);
                        break;
                    case "3":
                        ViewTop20Results();
                        break;
                    case "4":
                        ChangeSettings(user);
                        break;
                    case "5":
                        return;
                    default:
                        Console.WriteLine("Invalid option. Try again.");
                        break;
                }
            }
        }

        static void StartQuiz(User user)
        {
            Console.WriteLine("Available categories:");
            foreach (var category in quizzes.Keys)
            {
                Console.WriteLine(category);
            }

            Console.Write("Enter category or type 'mixed' for a random quiz: ");
            string selectedCategory = Console.ReadLine();

            List<Question> questions;
            if (selectedCategory.ToLower() == "mixed")
            {
                questions = quizzes.Values.SelectMany(q => q).OrderBy(_ => Guid.NewGuid()).Take(20).ToList();
            }
            else if (quizzes.ContainsKey(selectedCategory))
            {
                questions = quizzes[selectedCategory];
            }
            else
            {
                Console.WriteLine("Invalid category.");
                return;
            }

            int correctAnswers = 0;

            foreach (var question in questions)
            {
                Console.WriteLine($"\n{question.Text}");
                for (int i = 0; i < question.Options.Length; i++)
                {
                    Console.WriteLine($"{i + 1}. {question.Options[i]}");
                }

                Console.Write("Enter your answers as numbers separated by semicolon (;): ");
                var userAnswers = Console.ReadLine()
                    .Split(';')
                    .Select(a => a.Trim())
                    .Where(a => int.TryParse(a, out _))
                    .Select(a => question.Options[int.Parse(a) - 1])
                    .ToArray();

                if (userAnswers.OrderBy(a => a).SequenceEqual(question.CorrectAnswers.OrderBy(a => a)))
                {
                    correctAnswers++;
                }
            }

            Console.WriteLine($"\nYou answered {correctAnswers} out of {questions.Count} correctly.");
            quizResults.Add(new QuizResult(user.Login, correctAnswers, DateTime.Now));
        }

        static void ViewPastResults(User user)
        {
            var userResults = quizResults.Where(q => q.UserLogin == user.Login).ToList();

            if (!userResults.Any())
            {
                Console.WriteLine("No quiz results found.");
                return;
            }

            Console.WriteLine("Your past results:");
            foreach (var result in userResults)
            {
                Console.WriteLine($"Date: {result.Date}, Score: {result.Score}");
            }
        }

        static void ViewTop20Results()
        {
            var topResults = quizResults.OrderByDescending(q => q.Score).Take(20).ToList();

            if (!topResults.Any())
            {
                Console.WriteLine("No results available.");
                return;
            }

            Console.WriteLine("Top-20 results:");
            foreach (var result in topResults)
            {
                Console.WriteLine($"User: {result.UserLogin}, Score: {result.Score}, Date: {result.Date}");
            }
        }

        static void ChangeSettings(User user)
        {
            Console.WriteLine("1. Change password\n2. Change date of birth");
            Console.Write("Select an option: ");
            string option = Console.ReadLine();

            switch (option)
            {
                case "1":
                    Console.Write("Enter new password: ");
                    user.Password = Console.ReadLine();
                    Console.WriteLine("Password updated successfully!");
                    SaveUsersToFile();
                    break;
                case "2":
                    Console.Write("Enter new date of birth (yyyy-mm-dd): ");
                    if (DateTime.TryParse(Console.ReadLine(), out DateTime newDob))
                    {
                        user.DateOfBirth = newDob;
                        Console.WriteLine("Date of birth updated successfully!");
                        SaveUsersToFile();
                    }
                    else
                    {
                        Console.WriteLine("Invalid date format.");
                    }
                    break;
                default:
                    Console.WriteLine("Invalid option.");
                    break;
            }
        }
    }

    class User
    {
        public string Login { get; }
        public string Password { get; set; }
        public DateTime DateOfBirth { get; set; }

        public User(string login, string password, DateTime dateOfBirth)
        {
            Login = login;
            Password = password;
            DateOfBirth = dateOfBirth;
        }
    }

    class Question
    {
        public string Text { get; set; }
        public string[] Options { get; set; }
        public string[] CorrectAnswers { get; set; }

        public Question(string text, string[] options, string[] correctAnswers)
        {
            Text = text;
            Options = options;
            CorrectAnswers = correctAnswers;
        }
    }

    class QuizResult
    {
        public string UserLogin { get; }
        public int Score { get; }
        public DateTime Date { get; }

        public QuizResult(string userLogin, int score, DateTime date)
        {
            UserLogin = userLogin;
            Score = score;
            Date = date;
        }
    }
}
