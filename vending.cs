using System;
using System.Collections.Generic;

namespace vending
{
    class Program
    {
        static void Main()
        {
            bool IsOpen = true;
            
            // Товары в автомате
            Prodookt[] prodookts = {
                new Prodookt("snikers", 70, 1, 30), 
                new Prodookt("baoonti", 80, 2, 50), 
                new Prodookt("Voda", 50, 3, 20)
            };
            
            // Денежный баланс автомата (купюры и монеты)
            AddMoney[] balans = {
                new AddMoney(1, 23), 
                new AddMoney(2, 12), 
                new AddMoney(5, 30), 
                new AddMoney(10, 20), 
                new AddMoney(50, 53), 
                new AddMoney(100, 37)
            };
            
            // Подсчет общего баланса автомата
            int AllBalans = CalculateTotalBalance(balans);
            System.Console.WriteLine($"Общий баланс автомата: {AllBalans} руб.\n");

            while(IsOpen)
            {
                System.Console.WriteLine("=== ТОВАРЫ ===\n");
                for(int i = 0; i < prodookts.Length; i++)
                {
                    prodookts[i].Info();
                }

                System.Console.WriteLine("\nВыберите продукт, введя его код (или 0 для выхода):");
                int IdClient = Convert.ToInt32(Console.ReadLine());
                
                if (IdClient == 0)
                {
                    IsOpen = false;
                    continue;
                }

                // Поиск товара по ID
                Prodookt selectedProduct = FindProductById(prodookts, IdClient);
                if (selectedProduct == null)
                {
                    System.Console.WriteLine("Товар с таким кодом не найден!");
                    Console.ReadKey();
                    Console.Clear();
                    continue;
                }

                System.Console.WriteLine("Введите количество товара, которое вы хотите приобрести:");
                int CountTovar = Convert.ToInt32(Console.ReadLine());

                // Проверка доступности товара
                if (!selectedProduct.CheckAvailability(CountTovar))
                {
                    System.Console.WriteLine("У нас нет столько товара!");
                    Console.ReadKey();
                    Console.Clear();
                    continue;
                }

                int totalCost = selectedProduct.PaySum(CountTovar);
                System.Console.WriteLine($"\nК оплате: {totalCost} руб.");

                // Прием денег от пользователя
                System.Console.WriteLine("\n=== ПРИЕМ ДЕНЕГ ===");
                System.Console.WriteLine("Вносите деньги по купюрам/монетам.");
                int acceptedMoney = AcceptMoneyFromUser(balans, totalCost);
                
                if (acceptedMoney < totalCost)
                {
                    System.Console.WriteLine($"Недостаточно средств! Внесено: {acceptedMoney}, требуется: {totalCost}");
                    System.Console.WriteLine("Деньги возвращены.");
                    Console.ReadKey();
                    Console.Clear();
                    continue;
                }

                // Вычисление сдачи
                int change = acceptedMoney - totalCost;
                
                if (change > 0)
                {
                    System.Console.WriteLine($"\nВаша сдача: {change} руб.");
                    GiveChange(balans, change);
                }

                // Добавление принятых денег в автомат
                AddMoneyToMachine(balans, acceptedMoney);

                // Покупка совершена - товар списывается
                selectedProduct.Purchase(CountTovar);
                
                System.Console.WriteLine($"\nПокупка совершена! Заберите ваш товар: {selectedProduct.GetName()} x{CountTovar}");
                
                // Обновление общего баланса
                AllBalans = CalculateTotalBalance(balans);
                System.Console.WriteLine($"Общий баланс автомата: {AllBalans} руб.\n");

                Console.WriteLine("\nНажмите любую клавишу для продолжения...");
                Console.ReadKey();
                Console.Clear();
            }
        }

        // Подсчет общего баланса автомата
        static int CalculateTotalBalance(AddMoney[] balans)
        {
            int total = 0;
            for(int i = 0; i < balans.Length; i++)
            {
                total += balans[i].BalansSum();
            }
            return total;
        }

        // Поиск товара по ID
        static Prodookt FindProductById(Prodookt[] prodookts, int id)
        {
            for(int i = 0; i < prodookts.Length; i++)
            {
                if (prodookts[i].GetId() == id)
                {
                    return prodookts[i];
                }
            }
            return null;
        }

        // Метод приема денег от пользователя
        // Возвращает общую сумму внесенных денег
        static int AcceptMoneyFromUser(AddMoney[] balans, int requiredAmount)
        {
            int totalInserted = 0;
            
            System.Console.WriteLine($"Необходимо внести: {requiredAmount} руб.");
            System.Console.WriteLine("Доступные номиналы для ввода:");
            
            for(int i = 0; i < balans.Length; i++)
            {
                balans[i].BalansInfo();
            }
            
            System.Console.WriteLine("\nВводите деньги по одному номиналу.");
            System.Console.WriteLine("Когда сумма будет достаточной, введите 0 для завершения.");
            
            while (totalInserted < requiredAmount)
            {
                System.Console.WriteLine($"\nТекущая внесенная сумма: {totalInserted} руб.");
                System.Console.WriteLine("Введите номинал купюры/монеты (или 0 для завершения):");
                
                if (!int.TryParse(Console.ReadLine(), out int nominal) || nominal < 0)
                {
                    System.Console.WriteLine("Некорректный ввод!");
                    continue;
                }
                
                if (nominal == 0)
                {
                    break;
                }
                
                // Проверяем, есть ли такой номинал в системе
                AddMoney moneyType = FindMoneyByNominal(balans, nominal);
                if (moneyType == null)
                {
                    System.Console.WriteLine("Такого номинала не существует!");
                    continue;
                }
                
                System.Console.WriteLine($"Введите количество купюр/монет номиналом {nominal}:");
                
                if (!int.TryParse(Console.ReadLine(), out int quantity) || quantity < 0)
                {
                    System.Console.WriteLine("Некорректный ввод количества!");
                    continue;
                }
                
                int insertedSum = nominal * quantity;
                totalInserted += insertedSum;
                
                System.Console.WriteLine($"Внесено: {insertedSum} руб. (номинал {nominal} x {quantity})");
            }
            
            return totalInserted;
        }

        // Поиск типа денег по номиналу
        static AddMoney FindMoneyByNominal(AddMoney[] balans, int nominal)
        {
            for(int i = 0; i < balans.Length; i++)
            {
                if (balans[i].GetNominal() == nominal)
                {
                    return balans[i];
                }
            }
            return null;
        }

        // Добавление принятых денег в автомат
        // Распределяет внесенную сумму по номиналам
        static void AddMoneyToMachine(AddMoney[] balans, int amount)
        {
            System.Console.WriteLine("\n=== ЗАЧИСЛЕНИЕ ДЕНЕГ В АВТОМАТ ===");
            
            // Простой алгоритм: добавляем деньги начиная с крупных номиналов
            int remaining = amount;
            
            for(int i = balans.Length - 1; i >= 0 && remaining > 0; i--)
            {
                int nominal = balans[i].GetNominal();
                int countToAdd = remaining / nominal;
                
                if (countToAdd > 0)
                {
                    balans[i].AddQuantity(countToAdd);
                    remaining -= countToAdd * nominal;
                    System.Console.WriteLine($"Добавлено: {nominal} руб. x {countToAdd}");
                }
            }
            
            // Остаток (если есть) добавляем мелочью
            if (remaining > 0)
            {
                for(int i = 0; i < balans.Length && remaining > 0; i++)
                {
                    int nominal = balans[i].GetNominal();
                    if (remaining >= nominal)
                    {
                        int countToAdd = remaining / nominal;
                        balans[i].AddQuantity(countToAdd);
                        remaining -= countToAdd * nominal;
                        System.Console.WriteLine($"Добавлено: {nominal} руб. x {countToAdd}");
                    }
                }
            }
        }

        // Выдача сдачи пользователю
        static void GiveChange(AddMoney[] balans, int changeAmount)
        {
            System.Console.WriteLine("\n=== ВЫДАЧА СДАЧИ ===");
            
            int remaining = changeAmount;
            bool canGiveChange = true;
            
            // Выдаем сдачу от крупных номиналов к мелким
            for(int i = balans.Length - 1; i >= 0 && remaining > 0; i--)
            {
                int nominal = balans[i].GetNominal();
                int availableCount = balans[i].GetQuantity();
                int countNeeded = remaining / nominal;
                
                // Берем максимум из доступного или нужного
                int countToGive = Math.Min(countNeeded, availableCount);
                
                if (countToGive > 0)
                {
                    balans[i].RemoveQuantity(countToGive);
                    remaining -= countToGive * nominal;
                    
                    if (countToGive == 1)
                    {
                        System.Console.WriteLine($"Выдано: {nominal} руб. x {countToGive}");
                    }
                    else
                    {
                        System.Console.WriteLine($"Выдано: {nominal} руб. x {countToGive}");
                    }
                }
            }
            
            if (remaining > 0)
            {
                System.Console.WriteLine($"\nВНИМАНИЕ: Недостаточно мелких купюр для полной выдачи сдачи!");
                System.Console.WriteLine($"Невыданная сумма: {remaining} руб.");
                System.Console.WriteLine("Обратитесь к обслуживающему персоналу.");
            }
            else
            {
                System.Console.WriteLine($"\nСдача выдана полностью!");
            }
        }
    }

    // Класс товара
    class Prodookt
    {
        private string Name;
        private int Count;
        private int Id;
        private int Money;
        
        public Prodookt(string name, int count, int id, int money)
        {
            Name = name;
            Count = count;
            Id = id;
            Money = money;
        }
        
        public void Info()
        {
            System.Console.WriteLine($"Код: {Id} | {Name} | Остаток: {Count} шт. | Цена: {Money} руб./шт.");
        }

        // Проверка доступности товара
        public bool CheckAvailability(int countTovar)
        {
            return Count >= countTovar;
        }

        // Расчет стоимости покупки
        public int PaySum(int countTovar)
        {
            return Money * countTovar;
        }

        // Совершение покупки (списание товара)
        public void Purchase(int countTovar)
        {
            if (Count >= countTovar)
            {
                Count -= countTovar;
            }
        }

        // Геттеры
        public string GetName() => Name;
        public int GetCount() => Count;
        public int GetId() => Id;
        public int GetMoney() => Money;
    }

    // Класс для хранения номиналов и их количества в автомате
    class AddMoney
    {
        private int Nominal;
        private int Quantity;
        
        public AddMoney(int nominal, int quantity)
        {
            Nominal = nominal;
            Quantity = quantity;
        }
        
        public void BalansInfo()
        {
            System.Console.WriteLine($"{Nominal} руб. - доступно: {Quantity} шт.");
        }

        // Подсчет суммы для данного номинала
        public int BalansSum()
        {
            return Nominal * Quantity;
        }

        // Добавить количество
        public void AddQuantity(int count)
        {
            Quantity += count;
        }

        // Удалить количество (для выдачи сдачи)
        public void RemoveQuantity(int count)
        {
            if (Quantity >= count)
            {
                Quantity -= count;
            }
        }

        // Геттеры
        public int GetNominal() => Nominal;
        public int GetQuantity() => Quantity;
    }
}
