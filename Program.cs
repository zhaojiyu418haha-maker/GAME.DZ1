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

        static void Main()
        {
            Console.Write("是否重置所有玩家数据？(y/n): ");
            string reset = Console.ReadLine();  
            if (reset.ToLower() == "y")
            {
                if (File.Exists("players.txt"))
                {
                    File.Delete("players.txt");
                    Console.WriteLine("所有玩家数据已重置。");
                }
            }

            // 1. 加载所有玩家（在最开始）
            LoadPlayers();

            // 2. 提示输入玩家姓名
            Console.Write("请输入玩家姓名：");
            string name = Console.ReadLine();

            // 3. 查找或创建玩家
            PlayerProfile currentPlayer = FindOrCreatePlayer(name);

            // 4. 输出当前玩家信息（仅用于验证）
            Console.WriteLine($"当前玩家：{currentPlayer.PlayerName}，最高关卡：{currentPlayer.MaxLevel}，分数：{currentPlayer.Score}");

            // 5. 示例：让玩家通关一次（模拟）
            Console.WriteLine("\n模拟通关一次...");
            currentPlayer.MaxLevel = Math.Min(currentPlayer.MaxLevel + 1, 5);
            currentPlayer.Score += 100;
            Console.WriteLine($"新关卡：{currentPlayer.MaxLevel}，新分数：{currentPlayer.Score}");

            // 6. 保存所有玩家（覆盖原文件）
            SavePlayers();


            Console.WriteLine("\n游戏结束，按任意键退出...");
            Console.ReadKey();
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
                Console.WriteLine($"找到已有玩家：{existing.PlayerName}");
                return existing;
            }

            // 新建玩家
            Console.WriteLine($"未找到玩家，创建新玩家：{name}");
            PlayerProfile newPlayer = new PlayerProfile(name, 1, 0);
            players.Add(newPlayer);
            SavePlayers();  // 立即保存
            return newPlayer;
        }
    }
}
