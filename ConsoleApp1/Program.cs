using ConsoleApp1;

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
                        items.Add(new Item { Id = GetNextId(items), Name = name, Email = email });
                        UpdateHistory(items, history);
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
                        item.Name = Console.ReadLine();
                        Console.WriteLine("Digite o novo email do item:");
                        item.Email = Console.ReadLine();
                        UpdateHistory(items, history);
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
                        UpdateHistory(items, history);
                        Console.WriteLine("Item excluído com sucesso!");
                        break;
                    case "U":
                        // Liste ultima versão
                        foreach (Item i in items)
                        {
                            Console.WriteLine($"{i.Id}: {i.Name} ({i.Email})");
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

        public static void Testar(List<Item> items, List<HistoryRecord> history)
        {
            // Insira um novo item na lista
            items.Add(new Item { Id = 1, Name = "Item 1", Email = "item1@example.com" });
            UpdateHistory(items, history);
            Console.WriteLine("Inserido item 1");
            Listar(items, history);


            // Atualize o item na lista
            items[0].Name = "Item 1 atualizado";
            items[0].Email = "item1-updated@example.com";
            UpdateHistory(items, history);
            Console.WriteLine("Atualizado item 1");
            Listar(items, history);


            // Exclua o item da lista
            items.RemoveAt(0);
            UpdateHistory(items, history);
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

            List<Item> latestVersions = GetLatestVersions(items, history);
            foreach (Item item in latestVersions)
            {
                Console.WriteLine($"{item.Id}: {item.Name} ({item.Email})");
            }
        }

        public static void UpdateHistory(List<Item> items, List<HistoryRecord> history)
        {
            // Adicione um registro de histórico para cada item na lista
            foreach (Item item in items)
            {
                HistoryRecord? lastRecord = history.LastOrDefault(r => r.ItemId == item.Id);

                // Se o item foi modificado, adicione um novo registro de histórico
                if (lastRecord == null || lastRecord.Name != item.Name || lastRecord.Email != item.Email)
                {
                    history.Add(new HistoryRecord
                    {
                        ItemId = item.Id,
                        Name = item.Name,
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
        }

        static List<Item> GetLatestVersions(List<Item> items, List<HistoryRecord> history)
        {
            List<Item> latestVersions = new List<Item>();
            foreach (Item item in items)
            {
                // Encontre a última alteração ou inserção para este item
                HistoryRecord mostRecentChange = history
                    .Where(r => r.ItemId == item.Id)
                    .OrderByDescending(r => r.Timestamp)
                    .FirstOrDefault(r => r.Type == HistoryType.Novo || r.Type == HistoryType.Modificado);

                // Se não houver alterações ou inserções para este item, isso significa que a última versão é a atual
                if (mostRecentChange == null)
                {
                    latestVersions.Add(item);
                }
                // Se houver alterações ou inserções, adicione a última versão ao resultado
                else
                {
                    latestVersions.Add(new Item
                    {
                        Id = mostRecentChange.ItemId,
                        Name = mostRecentChange.Name,
                        Email = mostRecentChange.Email
                    });
                }
            }
            return latestVersions;
        }
    }

    class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
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
