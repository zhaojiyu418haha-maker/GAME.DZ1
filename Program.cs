using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Lesson_1
{
    class PlayerProfile
    {
        public string PlayerName { get; }          // 只读，不能改名
        public int MaxLevel { get; set; }          // 可修改
        public int Score { get; set; }              // 可修改

        public PlayerProfile(string playerName, int maxLevel, int score)
        {
            PlayerName = playerName;
            MaxLevel = maxLevel;
            Score = score;
        }
    }

    class Playering : PlayerProfile
    {
        public Playering(string playerName, int maxLevel, int score)
        : base(playerName, maxLevel, score)  // 调用基类构造函数
        {
            // 可以添加额外的初始化代码
        }
    }

    class Program
    {
        static List<PlayerProfile> players = new List<PlayerProfile>();
        static string filePath = "players.txt";
        static Random random = new Random();

        static void Main()
        {
            Console.Write("Сбросить все данные игроков? (y/n): ");
            string reset = Console.ReadLine();
            if (reset.ToLower() == "y")
            {
                if (File.Exists("players.txt"))
                {
                    File.Delete("players.txt");
                    Console.WriteLine("Все данные игроков сброшены.");
                }
            }

            // 1. 加载所有玩家（在最开始）
            LoadPlayers();

            // 2. 提示输入玩家姓名
            Console.Write("Введите имя игрока: ");
            string name = Console.ReadLine();

            // 3. 查找或创建玩家
            PlayerProfile currentPlayer = FindOrCreatePlayer(name);

            // 4. 开始游戏主循环
            bool continuePlaying = true;
            while (continuePlaying)
            {
                // 显示可用等级
                Console.WriteLine($"\nДоступные уровни: 1..{currentPlayer.MaxLevel}");

                // 选择等级
                int level = SelectLevel(currentPlayer.MaxLevel);

                // 获取该等级的数值范围
                int maxRange = GetRangeByLevel(level);
                Console.WriteLine($"\nВыбран уровень {level}. Загадано число от 1 до {maxRange}.");

                // 进行猜数字游戏
                PlayGuessingGame(level, maxRange, currentPlayer);

                // 通关后更新并保存玩家数据
                SavePlayers();

                // 通关后询问继续还是退出
                continuePlaying = AskContinue();
            }

            Console.WriteLine("\nИгра окончена. Нажмите любую клавишу для выхода...");
            Console.ReadKey();
        }

        // 根据等级获取数字范围
        static int GetRangeByLevel(int level)
        {
            return level switch
            {
                1 => 10,
                2 => 50,
                3 => 100,
                4 => 250,
                5 => 1000,
                _ => 100  // 默认
            };
        }

        // 让玩家选择等级
        static int SelectLevel(int maxLevel)
        {
            int level = 0;
            bool validInput = false;

            while (!validInput)
            {
                Console.Write($"Введите уровень (1..{maxLevel}): ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out level) && level >= 1 && level <= maxLevel)
                {
                    validInput = true;
                }
                else
                {
                    Console.WriteLine($"Ошибка! Введите число от 1 до {maxLevel}.");
                }
            }

            return level;
        }

        // 猜数字游戏核心（二分法原理）
        static void PlayGuessingGame(int level, int maxRange, PlayerProfile player)
        {
            int targetNumber = random.Next(1, maxRange + 1);
            int userGuess = 0;
            int attempts = 0;

            Console.WriteLine("Игра началась! Вводите числа (программа будет подсказывать 'Больше.' или 'Меньше.')");

            while (userGuess != targetNumber)
            {
                Console.Write("Ваше число: ");
                string input = Console.ReadLine();

                if (!int.TryParse(input, out userGuess))
                {
                    Console.WriteLine("Пожалуйста, введите корректное число.");
                    continue;
                }

                attempts++;

                if (userGuess < targetNumber)
                {
                    Console.WriteLine("Больше.");
                }
                else if (userGuess > targetNumber)
                {
                    Console.WriteLine("Меньше.");
                }
                else
                {
                    Console.WriteLine($"Поздравляю! Вы угадали число {targetNumber} за {attempts} попыток!");
                }
            }

            // 计算得分：等级² × 10
            int earnedScore = level * level * 10;
            player.Score += earnedScore;

            // 检查是否需要提升最大等级
            if (level == player.MaxLevel && player.MaxLevel < 5)
            {
                player.MaxLevel++;
            }

            // 显示玩家最新数据（符合作业要求的格式）
            Console.WriteLine($"\nТекущие данные: Игрок {player.PlayerName}, Макс. уровень {player.MaxLevel}, Счёт {player.Score}.");
        }

        // 询问是否继续
        static bool AskContinue()
        {
            int choice = -1;
            bool validInput = false;

            while (!validInput)
            {
                Console.WriteLine("\n1 - Выбрать уровень, 2 - Выйти");
                Console.Write("Ваш выбор: ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out choice) && (choice == 1 || choice == 2))
                {
                    validInput = true;
                }
                else
                {
                    Console.WriteLine("Ошибка! Введите 1 или 2.");
                }
            }

            return choice == 1;
        }

        // 加载文件
        static void LoadPlayers()
        {
            if (!File.Exists(filePath))
            {
                players = new List<PlayerProfile>();
                return;
            }

            string[] lines = File.ReadAllLines(filePath);
            players = new List<PlayerProfile>();

            for (int i = 0; i < lines.Length; i += 4)  // 每4行一个玩家（3行数据 + 1行 ---）
            {
                if (i + 2 >= lines.Length) break;  // 防止文件不完整

                string nameLine = lines[i];
                string levelLine = lines[i + 1];
                string scoreLine = lines[i + 2];
                // 第 i+3 行是 ---，可以忽略检查

                if (!nameLine.StartsWith("PlayerName: ")) continue;
                if (!levelLine.StartsWith("MaxLevel: ")) continue;
                if (!scoreLine.StartsWith("Score: ")) continue;

                string playerName = nameLine.Substring("PlayerName: ".Length);
                if (!int.TryParse(levelLine.Substring("MaxLevel: ".Length), out int level))
                    continue;
                if (!int.TryParse(scoreLine.Substring("Score: ".Length), out int score))
                    continue;

                // 确保等级在1~5之间
                level = Math.Clamp(level, 1, 5);

                players.Add(new PlayerProfile(playerName, level, score));
            }
        }

        // 保存文件
        static void SavePlayers()
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                foreach (var player in players)
                {
                    writer.WriteLine($"PlayerName: {player.PlayerName}");
                    writer.WriteLine($"MaxLevel: {player.MaxLevel}");
                    writer.WriteLine($"Score: {player.Score}");
                    writer.WriteLine("---");
                }
            }
        }

        // 查找或创建玩家
        static PlayerProfile FindOrCreatePlayer(string name)
        {
            // 忽略大小写查找
            PlayerProfile existing = players.FirstOrDefault(p => p.PlayerName.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                Console.WriteLine($"Найден существующий игрок: {existing.PlayerName}");
                return existing;
            }

            // 新建玩家
            Console.WriteLine($"Игрок не найден, создаем нового: {name}");
            PlayerProfile newPlayer = new PlayerProfile(name, 1, 0);
            players.Add(newPlayer);
            SavePlayers();  // 立即保存
            return newPlayer;
        }
    }
}