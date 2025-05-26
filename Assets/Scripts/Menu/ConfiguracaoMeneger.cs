using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class ConfiguracoesManager
{
    private static string configPath => Path.Combine(Application.persistentDataPath, "config.json");
    private static Dictionary<string, string> configuracoes = null;

    private static void CarregarConfiguracoes()
    {
        if (configuracoes != null) return;

        if (File.Exists(configPath))
        {
            string json = File.ReadAllText(configPath);
            Wrapper wrapper = JsonUtility.FromJson<Wrapper>(json);
            configuracoes = new Dictionary<string, string>();
            if (wrapper != null && wrapper.Valores != null)
            {
                foreach (var item in wrapper.Valores)
                {
                    configuracoes[item.chave] = item.valor;
                }
            }
        }
        else
        {
            configuracoes = new Dictionary<string, string>();
        }
    }

    public static string Ler(string chave)
    {
        CarregarConfiguracoes();
        return configuracoes.TryGetValue(chave, out var valor) ? valor : null;
    }

    public static void Salvar(string chave, string valor)
    {
        CarregarConfiguracoes();
        configuracoes[chave] = valor;
        SalvarEmArquivo();
    }

    private static void SalvarEmArquivo()
    {
        Wrapper wrapper = new Wrapper();
        wrapper.Valores = new List<ConfigItem>();
        foreach (var kvp in configuracoes)
        {
            wrapper.Valores.Add(new ConfigItem { chave = kvp.Key, valor = kvp.Value });
        }

        string json = JsonUtility.ToJson(wrapper, true);
        File.WriteAllText(configPath, json);
    }

    [System.Serializable]
    private class ConfigItem
    {
        public string chave;
        public string valor;
    }

    [System.Serializable]
    private class Wrapper
    {
        public List<ConfigItem> Valores = new List<ConfigItem>();
    }
}
