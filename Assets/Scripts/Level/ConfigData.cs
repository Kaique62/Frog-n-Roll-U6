using System.Collections.Generic; // Required for List<T>

// Este arquivo contém apenas as classes para estruturar os dados do JSON.
// Ele não é um componente e não deve ser anexado a nenhum GameObject.

[System.Serializable]
public class ConfigEntry
{
    // Os nomes dos campos devem corresponder exatamente às chaves do JSON ("key" e "value").
    public string key;
    public string value;
}

[System.Serializable]
public class ConfigData
{
    // O nome do campo deve ser "Values" para corresponder à lista no JSON.
    public List<ConfigEntry> Values;
}
