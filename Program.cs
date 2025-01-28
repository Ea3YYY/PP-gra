using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DungeonGame
{
    public class Rand
    {
        private static Random rng = new Random();
        public int Run(int min, int max) => rng.Next(min, max + 1);
    }

    public class Hero
    {
        public string Name;
        private int Strength;
        private int Dexterity;
        private int Intelligence;
        public double HP;
        public double MP;
        public int RoomCount;

        public int GetDexterity() { return Dexterity; }

        public Hero(string name, string heroClass, int roomCount = 0)
        {
            Name = name;
            RoomCount = roomCount;
            switch (heroClass)
            {
                case "warrior": Init(15, 10, 5); break;
                case "assassin": Init(5, 15, 10); break;
                case "sorcerer": Init(5, 5, 20); break;
                default: Init(); break;
            }
        }

        private void Init(int strength = 10, int dexterity = 10, int intelligence = 10)
        {
            Strength = strength;
            Dexterity = dexterity;
            Intelligence = intelligence;
            HP = 50 + strength;
            MP = 10 + (3 * intelligence);
        }

        public void DisplayStats()
        {
            Console.WriteLine($"\n🔹 Twoje statystyki: HP: {HP}, MP: {MP}, Siła: {Strength}, Zręczność: {Dexterity}, Inteligencja: {Intelligence}, Pokój: {RoomCount}\n");
        }

        public void Attack(Enemy enemy)
        {
            Rand rand = new Rand();
            double damage = Strength * rand.Run(5, 10) / 10.0;

            if (rand.Run(0, 100) > enemy.Dexterity)
            {
                Console.WriteLine("Trafiłeś przeciwnika!");
                enemy.HP -= damage;
            }
            else Console.WriteLine("Przeciwnik uniknął ataku!");
        }

        public void LevelUp()
        {
            Console.WriteLine("Wybierz statystykę do ulepszenia: 1: Siła, 2: Zręczność, 3: Inteligencja");
            int choice;
            while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 3)
            {
                Console.WriteLine("Niepoprawny wybór, spróbuj ponownie.");
            }

            switch (choice)
            {
                case 1: Strength += 5; HP += 5; break;
                case 2: Dexterity += 5; break;
                case 3: Intelligence += 5; MP += (3 * Intelligence); break;
            }

            Console.WriteLine("Twoje statystyki zostały ulepszone!");
            DisplayStats();
        }

        public void CastSpell(Enemy enemy)
        {
            if (MP >= 10)
            {
                MP -= 10;
                double damage = Intelligence * 2;
                Console.WriteLine($"Użyłeś zaklęcia i zadałeś {damage} obrażeń!");
                enemy.HP -= damage;
            }
            else Console.WriteLine("Nie masz wystarczająco many!");
        }
    }

    public class Enemy
    {
        public string Name;
        public int Strength;
        public int Dexterity;
        public double HP;

        public Enemy(string name, int strength, int dexterity, double hp)
        {
            Name = name;
            Strength = strength;
            Dexterity = dexterity;
            HP = hp;
        }

        public void Attack(Hero hero)
        {
            Rand rand = new Rand();
            double damage = Strength * rand.Run(5, 10) / 10.0;

            if (rand.Run(0, 100) > hero.GetDexterity())
            {
                Console.WriteLine($"{Name} trafił cię!");
                hero.HP -= damage;
            }
            else Console.WriteLine("Uniknąłeś ataku!");
        }
    }

    class Program
    {
      static void Main(string[] args)
{
    Hero hero = null;

    if (File.Exists("savegame.json"))
    {
        Console.WriteLine("Znaleziono zapisany stan gry. Czy chcesz go wczytać? (tak/nie)");
        string loadDecision = Console.ReadLine().ToLower();

        if (loadDecision == "tak")
        {
            hero = LoadGame();
            Console.WriteLine("Gra została wczytana.");
        }
        else
        {
            Console.WriteLine("Rozpoczynasz nową grę.");
        }
    }

    if (hero == null)
    {
        Console.WriteLine("Podaj nick swojej postaci:");
        string playerName = Console.ReadLine();

        Console.WriteLine("Wybierz klasę postaci: warrior, assassin, sorcerer");
        string heroClass = Console.ReadLine().ToLower();

        hero = new Hero(playerName, heroClass);
    }

    if (hero.HP <= 0)
    {
        Console.WriteLine("Twoja postać jest martwa! Musisz rozpocząć nową grę.");
        return;
    }

    Rand rand = new Rand();
    List<Enemy> enemies = new List<Enemy>
    {
        new Enemy("Goblin", 10, 5, 30),
        new Enemy("Ork", 12, 7, 40),
        new Enemy("Wilkołak", 15, 10, 50)
    };

    while (hero.HP > 0)
    {
        hero.RoomCount++;

        if (hero.RoomCount == 10)
        {
            Console.WriteLine("Dotarłeś do głównego bossa!");
            Enemy finalBoss = new Enemy("Demoniczny Lord", 25, 15, 100);
            Fight(hero, finalBoss);
            if (hero.HP <= 0) return;
            Console.WriteLine("Pokonałeś głównego bossa! Gratulacje!");
            return;
        }

        Console.WriteLine($"Pokój {hero.RoomCount}. Wybierz jeden z trzech pokoi: (1, 2, 3)");
        int choice;
        while (!int.TryParse(Console.ReadLine(), out choice) || choice < 1 || choice > 3)
        {
            Console.WriteLine("Niepoprawny wybór, spróbuj ponownie.");
        }

        int roomType = rand.Run(0, 2);
        if (roomType == 0)
        {
            Enemy enemy = enemies[rand.Run(0, enemies.Count - 1)];

            if (enemy.HP <= 0)
            {
                Console.WriteLine($"Spotkałeś {enemy.Name}, ale wygląda na martwego... Idziesz dalej.");
            }
            else
            {
                Fight(hero, enemy);
                if (hero.HP <= 0) return;
            }
        }
        else if (roomType == 1)
        {
            Console.WriteLine("Znalazłeś skarb! Twoje statystyki wzrastają.");
            hero.LevelUp();
        }
        else
        {
            Console.WriteLine("Wpadłeś w pułapkę! Tracisz 10 HP.");
            hero.HP -= 10;
            hero.DisplayStats();
        }

        SaveGame(hero);
    }

    Console.WriteLine("Koniec gry.");
}

        static void Fight(Hero hero, Enemy enemy)
        {
            Console.WriteLine($"Spotkałeś {enemy.Name}! (HP: {enemy.HP})");

            while (enemy.HP > 0 && hero.HP > 0)
            {
                Console.WriteLine("Wybierz akcję: 1: Atak, 2: Zaklęcie, 3: Ulepsz statystyki");
                int action;
                while (!int.TryParse(Console.ReadLine(), out action) || action < 1 || action > 3)
                {
                    Console.WriteLine("Niepoprawny wybór, spróbuj ponownie.");
                }

                switch (action)
                {
                    case 1: hero.Attack(enemy); break;
                    case 2: hero.CastSpell(enemy); break;
                    case 3: hero.LevelUp(); break;
                }

                if (enemy.HP > 0)
                {
                    enemy.Attack(hero);
                    Console.WriteLine($"{enemy.Name} ma {enemy.HP} HP.");
                    hero.DisplayStats();
                }
            }

            if (hero.HP <= 0)
            {
                Console.WriteLine($"Zostałeś pokonany przez {enemy.Name}!");
                return;
            }

            Console.WriteLine($"Pokonałeś {enemy.Name}!");
        }

        static void SaveGame(Hero hero)
        {
            string filePath = "savegame.json";
            string json = JsonConvert.SerializeObject(hero, Formatting.Indented);
            File.WriteAllText(filePath, json);
            Console.WriteLine("Gra została zapisana.");
        }

        static Hero LoadGame()
        {
            string filePath = "savegame.json";
            if (File.Exists(filePath))
            {
                string json = File.ReadAllText(filePath);
                Hero hero = JsonConvert.DeserializeObject<Hero>(json);
                Console.WriteLine("Gra została wczytana.");
                hero.DisplayStats();
                return hero;
            }
            return null;
        }
    }
}
