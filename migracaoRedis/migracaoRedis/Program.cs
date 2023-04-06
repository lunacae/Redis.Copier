using StackExchange.Redis;

public static class RedisCopier
{
    public static void GetData(IDatabase sourceDb)
    {
        // Obtém todas as chaves do Redis de origem
        var keys = sourceDb.Execute("KEYS", "*");
        Dictionary<string, string> redisKeyValues = new();

        // Copia cada chave e valor do Redis de origem para o Redis de destino
        foreach(var key in ((RedisValue[]?)keys))
        {
            var value = sourceDb.StringGet(key.ToString());
            redisKeyValues.Add(key.ToString(), value);
        }
        var json = System.Text.Json.JsonSerializer.Serialize(redisKeyValues);
        File.WriteAllText(Directory.GetCurrentDirectory() + "redisKeyValues-old.json", json);
    }

    public static void SaveData(IDatabase destinyDb)
    {
        var json = File.ReadAllText(Directory.GetCurrentDirectory() + "redisKeyValues-old.json");
        var redisKeyValues = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);

        foreach(var keyValue in redisKeyValues)
        {
            destinyDb.StringSet(keyValue.Key, keyValue.Value);
        }
    }
}

class Program
{
    static void Main(string[] args)
    {
        // Conecta nos Redis de origem e destino
        var sourceConnection = ConnectionMultiplexer.Connect("localhost:6379");
        var destConnection = ConnectionMultiplexer.Connect("localhost:6380");

        // Seleciona o banco de dados a ser usado em cada conexão
        var redisDatabase = sourceConnection.GetDatabase();
        var destDb = destConnection.GetDatabase();

        // Salva as chaves e valores do Redis de origem
        RedisCopier.GetData(redisDatabase);
        
        //Método que salva os dados no novo redis de destino
        RedisCopier.SaveData(redisDatabase);
    }
}