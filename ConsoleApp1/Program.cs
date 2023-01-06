using System.Security.Cryptography;
using ConsoleApp1;
using System.Text;
using System.Xml.Linq;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            // Crie uma lista de itens vazia e uma lista de histórico vazia
            List<Item> items = new List<Item>();
            List<HistoryRecord> history = new List<HistoryRecord>();
            //Testar(items, history);

            while (true)
            {
                Console.WriteLine("Digite uma opção:");
                Console.WriteLine("I - Inserir um novo item");
                Console.WriteLine("A - Atualizar um item existente");
                Console.WriteLine("E - Excluir um item existente");
                Console.WriteLine("U - Utlima versão");
                Console.WriteLine("L - Listar todos os itens");
                Console.WriteLine("Q - Sair");

                string input = Console.ReadLine();
                switch (input.ToUpper())
                {
                    case "I":
                        // Insira um novo item
                        Console.WriteLine("Digite o nome do item:");
                        string name = Console.ReadLine();
                        Console.WriteLine("Digite o email do item:");
                        string email = Console.ReadLine();

                        items.Add(new Item { Id = GetNextId(items), Nome = name, Email = email });
                        if(UpdateHistory(items, history))
                            Console.WriteLine("Item inserido com sucesso!");
                        break;
                    case "A":
                        // Atualize um item existente
                        Console.WriteLine("Digite o ID do item a ser atualizado:");
                        int id = int.Parse(Console.ReadLine());
                        Item item = items.FirstOrDefault(i => i.Id == id);
                        if (item == null)
                        {
                            Console.WriteLine("Item não encontrado!");
                            break;
                        }
                        Console.WriteLine("Digite o novo nome do item:");
                        item.Nome = Console.ReadLine();
                        Console.WriteLine("Digite o novo email do item:");
                        item.Email = Console.ReadLine();
                        if(UpdateHistory(items, history))
                            Console.WriteLine("Item atualizado com sucesso!");
                        break;
                    case "E":
                        // Exclua um item existente
                        Console.WriteLine("Digite o ID do item a ser excluído:");
                        id = int.Parse(Console.ReadLine());
                        item = items.FirstOrDefault(i => i.Id == id);
                        if (item == null)
                        {
                            Console.WriteLine("Item não encontrado!");
                            break;
                        }
                        items.Remove(item);
                        if(UpdateHistory(items, history))
                            Console.WriteLine("Item excluído com sucesso!");
                        break;
                    case "U":
                        // Liste ultima versão
                        foreach (Item i in GetLatestVersions(history))
                        {
                            Console.WriteLine($"{i.Id}: {i.Nome} ({i.Email})");
                        }
                        break;
                    case "L":
                        // Liste todos os itens
                        foreach (HistoryRecord h in history)
                        {
                            Console.WriteLine($"{Enum.GetName(h.Type.GetType(), h.Type)} ({h.Timestamp:dd/MM/yyyy HH:mm:ss})-> {h.ItemId}: {h.Name} ({h.Email})");
                        }
                        break;
                    case "Q":
                        // Saia do loop
                        return;
                    default:
                        Console.WriteLine("Opção inválida!");
                        break;
                }
            }
        }

        private static string HashEncode(string input)
        {
            byte[] inputBytes = Encoding.UTF8.GetBytes(input);

            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hashBytes = sha1.ComputeHash(inputBytes);
                string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                return hash;
            }
        }

        public static void Testar(List<Item> items, List<HistoryRecord> history)
        {
            // Insira um novo item na lista
            items.Add(new Item { Id = 1, Nome = "Item 1", Email = "item1@example.com" });
            if(UpdateHistory(items, history));
            Console.WriteLine("Inserido item 1");
            Listar(items, history);


            // Atualize o item na lista
            items[0].Nome = "Item 1 atualizado";
            items[0].Email = "item1-updated@example.com";
            if(UpdateHistory(items, history));
            Console.WriteLine("Atualizado item 1");
            Listar(items, history);


            // Exclua o item da lista
            items.RemoveAt(0);
            if(UpdateHistory(items, history));
            Console.WriteLine("Excluído item 1");

            // Obtenha a última versão de cada item na lista
            Listar(items, history);
        }

        static int GetNextId(List<Item> items)
        {
            // Se a lista estiver vazia, retorne 1
            if (items.Count == 0)
            {
                return 1;
            }
            // Senão, retorne o ID mais alto atual + 1
            return items.Max(i => i.Id) + 1;
        }

        public static void Listar(List<Item> items, List<HistoryRecord> history)
        {
            if(items?.Any() ==false)
                Console.WriteLine("Não há nenhum item.");

            List<Item> latestVersions = GetLatestVersions(history);
            foreach (Item item in latestVersions)
            {
                Console.WriteLine($"{item.Id}: {item.Nome} ({item.Email})");
            }
        }

        public static bool UpdateHistory(List<Item> items, List<HistoryRecord> history)
        {
            var tempHash = HashEncode(string.Join(';', items.Select(x => x.Nome + "-" + x.Email)));
            Console.WriteLine($"Hash input: {tempHash}");

            var actualHash = HashEncode(string.Join(';', GetLatestVersions(history).Select(x => x.Nome + "-" + x.Email)));
            Console.WriteLine($"Hash atual: {actualHash}");

            if (history.Any() && tempHash == actualHash)
            {
                Console.WriteLine("Não há diferenças.");
                return false;
            }

            // Adicione um registro de histórico para cada item na lista
            foreach (Item item in items)
            {
                HistoryRecord? lastRecord = history.LastOrDefault(r => r.ItemId == item.Id);

                // Se o item foi modificado, adicione um novo registro de histórico
                if (lastRecord == null || lastRecord.Name != item.Nome || lastRecord.Email != item.Email)
                {
                    history.Add(new HistoryRecord
                    {
                        ItemId = item.Id,
                        Name = item.Nome,
                        Email = item.Email,
                        Timestamp = DateTime.Now,
                        Type = lastRecord == null ? HistoryType.Novo : HistoryType.Modificado
                    });
                }
            }

            // Adicione um registro de histórico para cada item excluído
            foreach (HistoryRecord record in history.ToList())
            {
                if (record.Type != HistoryType.Excluido && !items.Any(i => i.Id == record.ItemId))
                {
                    history.Add(new HistoryRecord
                    {
                        ItemId = record.ItemId,
                        Name = record.Name,
                        Email = record.Email,
                        Timestamp = DateTime.Now,
                        Type = HistoryType.Excluido
                    });
                }
            }

            return true;
        }

        static List<Item> GetLatestVersions(List<HistoryRecord> history)
        {
            return history
                    .OrderByDescending(r => r.Timestamp)
                    .DistinctBy(x => x.ItemId).Select(x => new Item()
                    {
                        Email = x.Email,
                        Id = x.ItemId,
                        Nome = x.Name
                    }).ToList();
        }
    }

    class Item
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Email { get; set; }
    }

    class HistoryRecord
    {
        public int ItemId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime Timestamp { get; set; }
        public HistoryType Type { get; set; }
    }

    enum HistoryType
    {
        Novo,
        Modificado,
        Excluido
    }

    

}
