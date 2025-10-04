using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace testű
{
    class Versenyző
    {
        public string Vezetéknév { get; set; }
        public string Keresztnév { get; set; }
        public DateTime SzületésiIdő { get; set; }
        public bool Felnőtt { get; set; }
        public string Azonosító { get; set; }
        public string Email { get; set; }
    }

    internal class Program
    {
        static Random rnd = new Random();
        static List<(string Azonosító, DateTime Nap, int Kezdet, int Vég)> foglalások = new List<(string, DateTime, int, int)>();

        static DateTime randomDatum()
        {
            int év = rnd.Next(1960, DateTime.Now.Year - 14); // 14-től 65-ig
            int hónap = rnd.Next(1, 13);
            int nap = rnd.Next(1, DateTime.DaysInMonth(év, hónap) + 1);
            return new DateTime(év, hónap, nap);
        }

        static void Main(string[] args)
        {
            #region fejlec
            Console.WriteLine("Ceiling Gokart Center. (Kecskemét, 6000 Korház utca. 17)");
            Console.WriteLine("elérhetőségek:  +06-70-670-6713   www.Korház-GoKart.hu");
            Console.WriteLine();
            string uzenet = "Gokart időpont foglaló";
            int a = uzenet.Length;
            Console.WriteLine(uzenet);
            for (int i = 0; i < a; i++) { Console.Write("-"); }
            Console.WriteLine("\n");
            #endregion

            #region beolvasás
            List<string> vezeteknevek = new List<string>();
            using (StreamReader be = new StreamReader("vezeteknevek.txt"))
            {
                while (!be.EndOfStream)
                {
                    string sor = be.ReadLine();
                    vezeteknevek.AddRange(sor.Split(',').Select(x => x.Trim('\'', ' ')));
                }
            }
            List<string> keresztnevek = new List<string>();
            using (StreamReader be = new StreamReader("keresztnevek.txt"))
            {
                while (!be.EndOfStream)
                {
                    string sor = be.ReadLine();
                    keresztnevek.AddRange(sor.Split(',').Select(x => x.Trim('\'', ' ')));
                }
            }
            #endregion

            #region versenyzők
            int versenyzokSzama = rnd.Next(1, 151);
            List<Versenyző> versenyzők = new List<Versenyző>();
            for (int i = 0; i < versenyzokSzama; i++)
            {
                string vezeteknev = vezeteknevek[rnd.Next(vezeteknevek.Count)].Trim();
                string keresztnev = keresztnevek[rnd.Next(keresztnevek.Count)].Trim();
                DateTime szuletes = randomDatum();
                string azonosito = $"GO-{RemoveDiacritics(vezeteknev)}{RemoveDiacritics(keresztnev)}-{szuletes:yyyyMMdd}";
                string email = $"{vezeteknev.ToLower()}.{keresztnev.ToLower()}@gmail.com";
                bool felnott = (DateTime.Now.Year - szuletes.Year) > 18 ||
                               (DateTime.Now.Year - szuletes.Year == 18 && DateTime.Now.Month > szuletes.Month);
                versenyzők.Add(new Versenyző
                {
                    Vezetéknév = vezeteknev,
                    Keresztnév = keresztnev,
                    SzületésiIdő = szuletes,
                    Felnőtt = felnott,
                    Azonosító = azonosito,
                    Email = email
                });
            }
            #endregion

            elosztas(versenyzők);

            bool fut = true;
            while (fut)
            {
                Console.WriteLine("\n |1. a versenyzők információért | 2. Időpont foglalásért | 3. Táblázatért | 4. Új versenyző hozzáadása | 5. Kilépés");
                string input = Console.ReadLine();
                int val;
                if (!int.TryParse(input, out val))
                {
                    Console.WriteLine("Érvénytelen választás!");
                    continue;
                }

                switch (val)
                {
                    case 1:
                        Console.WriteLine("Információ");
                        Console.WriteLine($"\n versenyzők száma: {versenyzők.Count}\n");
                        foreach (var v in versenyzők)
                        {
                            Console.WriteLine($"{v.Vezetéknév} {v.Keresztnév}, {v.SzületésiIdő:yyyy.MM.dd}, {v.Felnőtt}, {v.Azonosító}, {v.Email}");
                            Console.WriteLine("------------------------------------------------------------------------------------------------------|");
                        }
                        break;
                    case 2:
                        Console.WriteLine("Időpont foglalás");
                        valasztfoglal(versenyzők);
                        break;
                    case 3:
                        Console.WriteLine("Táblázat");
                        Console.WriteLine("\nHónap végéig fennmaradó napok időtáblázata:");
                        idoszalag();
                        break;
                    case 4:
                        Console.WriteLine("versenyző hozzáadása");
                        ÚjVersenyzőHozzáadása(versenyzők);
                        break;
                    case 5:
                        Console.WriteLine("Kilépés!");
                        fut = false;
                        break;
                    default:
                        Console.WriteLine("Érvénytelen választás!");
                        break;
                }
            }
        }

        static string RemoveDiacritics(string text)
        {
            var normalizedString = text.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();
            foreach (var c in normalizedString)
            {
                var unicodeCategory = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != System.Globalization.UnicodeCategory.NonSpacingMark)
                    stringBuilder.Append(c);
            }
            return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
        }

        static void idoszalag()
        {
            DateTime maiNap = DateTime.Now;
            DateTime hónapVége = new DateTime(maiNap.Year, maiNap.Month, DateTime.DaysInMonth(maiNap.Year, maiNap.Month));
            for (DateTime nap = maiNap; nap <= hónapVége; nap = nap.AddDays(1))
            {
                Console.WriteLine($"{nap:yyyy.MM.dd} - {nap.DayOfWeek}");
                for (int óra = 8; óra < 19; óra++)
                {
                    bool foglalt = false;
                    foreach (var foglalas in foglalások)
                    {
                        if (foglalas.Nap.Date == nap.Date && foglalas.Kezdet <= óra && foglalas.Vég > óra)
                        {
                            foglalt = true;
                            break;
                        }
                    }
                    if (foglalt)
                    {
                        Console.Write($"  {óra}:00 - {óra + 1}:00 ");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.Write("[Foglalt]");
                        Console.ResetColor();
                        Console.WriteLine();
                    }
                    else
                    {
                        Console.Write($"  {óra}:00 - {óra + 1}:00");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.Write(" [Szabad]");
                        Console.ResetColor();
                        Console.WriteLine();
                    }
                }
            }
        }

        static void valasztfoglal(List<Versenyző> versenyzők)
        {
            Console.WriteLine("\nVersenyzők listája:");
            for (int i = 0; i < versenyzők.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {versenyzők[i].Vezetéknév} {versenyzők[i].Keresztnév} ({versenyzők[i].Azonosító})");
            }
            Console.Write("\nVálasszon versenyzőt sorszám alapján: ");
            string input = Console.ReadLine();
            int sorszám;
            if (!int.TryParse(input, out sorszám))
            {
                Console.WriteLine("Érvénytelen választás!");
                return;
            }
            sorszám--;
            if (sorszám < 0 || sorszám >= versenyzők.Count)
            {
                Console.WriteLine("Érvénytelen választás!");
                return;
            }
            var versenyző = versenyzők[sorszám];
            Console.WriteLine($"\nKiválasztott versenyző: {versenyző.Vezetéknév} {versenyző.Keresztnév} ({versenyző.Azonosító})");
            Console.Write("\nAdja meg a foglalás dátumát (pl. 2025.10.04): ");
            input = Console.ReadLine();
            DateTime nap;
            if (!DateTime.TryParse(input, out nap))
            {
                Console.WriteLine("Érvénytelen dátum!");
                return;
            }
            Console.Write("Adja meg a foglalás kezdő óráját (8-18): ");
            input = Console.ReadLine();
            int kezdet;
            if (!int.TryParse(input, out kezdet))
            {
                Console.WriteLine("Érvénytelen óra!");
                return;
            }
            Console.Write("Adja meg a foglalás hosszat (1-2): ");
            input = Console.ReadLine();
            int hossz;
            if (!int.TryParse(input, out hossz))
            {
                Console.WriteLine("Érvénytelen hossz!");
                return;
            }
            if (hossz < 1 || hossz > 2 || kezdet < 8 || kezdet > 18 || kezdet + hossz > 19)
            {
                Console.WriteLine("Érvénytelen időpont vagy hossz!");
                return;
            }
            bool foglalt = false;
            foreach (var foglalas in foglalások)
            {
                if (foglalas.Nap.Date == nap.Date && foglalas.Kezdet < kezdet + hossz && foglalas.Vég > kezdet)
                {
                    foglalt = true;
                    break;
                }
            }
            if (foglalt)
            {
                Console.WriteLine("Az időpont már foglalt!");
                return;
            }
            foglalások.Add((versenyző.Azonosító, nap, kezdet, kezdet + hossz));
            Console.WriteLine("Foglalás sikeres!");
        }

        static void elosztas(List<Versenyző> versenyzők)
        {
            DateTime maiNap = DateTime.Now;
            DateTime hónapVége = new DateTime(maiNap.Year, maiNap.Month, DateTime.DaysInMonth(maiNap.Year, maiNap.Month));
            foreach (var versenyző in versenyzők)
            {
                bool sikeres = false;
                int próbálkozások = 0;
                const int maxPróbálkozások = 100000;
                while (!sikeres && próbálkozások < maxPróbálkozások)
                {
                    próbálkozások++;
                    DateTime nap = maiNap.AddDays(rnd.Next(0, (hónapVége - maiNap).Days + 1));
                    int kezdet = rnd.Next(8, 18);
                    int hossz = rnd.Next(1, 3);
                    int vég = kezdet + hossz;
                    if (vég > 19)
                        continue;
                    bool foglalt = foglalások.Any(f => f.Nap == nap && f.Kezdet < vég && f.Vég > kezdet);
                    if (!foglalt)
                    {
                        foglalások.Add((versenyző.Azonosító, nap, kezdet, vég));
                        sikeres = true;
                    }
                }
                if (!sikeres)
                {
                    Console.WriteLine($"Figyelmeztetés: {versenyző.Azonosító} versenyzőnek nem sikerült időpontot foglalni.");
                }
            }
        }

        static void ÚjVersenyzőHozzáadása(List<Versenyző> versenyzők)
        {
            Console.Write("Adja meg a versenyző vezetéknevét: ");
            string vezeteknev = Console.ReadLine().Trim();
            Console.Write("Adja meg a versenyző keresztnevét: ");
            string keresztnev = Console.ReadLine().Trim();
            Console.Write("Adja meg a versenyző születési időét (yyyy.MM.dd): ");
            string szuletesInput = Console.ReadLine();
            DateTime szuletes;
            while (!DateTime.TryParseExact(szuletesInput, "yyyy.MM.dd", null, System.Globalization.DateTimeStyles.None, out szuletes))
            {
                Console.Write("Hibás formátum! Adja meg újra (yyyy.MM.dd): ");
                szuletesInput = Console.ReadLine();
            }

            string azonosito = $"GO-{RemoveDiacritics(vezeteknev)}{RemoveDiacritics(keresztnev)}-{szuletes:yyyyMMdd}";
            string email = $"{vezeteknev.ToLower()}.{keresztnev.ToLower()}@gmail.com";
            bool felnott = (DateTime.Now.Year - szuletes.Year) > 18 ||
                           (DateTime.Now.Year - szuletes.Year == 18 && DateTime.Now.Month > szuletes.Month);

            versenyzők.Add(new Versenyző
            {
                Vezetéknév = vezeteknev,
                Keresztnév = keresztnev,
                SzületésiIdő = szuletes,
                Felnőtt = felnott,
                Azonosító = azonosito,
                Email = email
            });

            Console.WriteLine($"Új versenyző sikeresen hozzáadva: {vezeteknev} {keresztnev} ({azonosito})");
        }
    }
}