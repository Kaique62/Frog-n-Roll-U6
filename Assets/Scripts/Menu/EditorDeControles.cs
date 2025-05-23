using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class EditorDeControles : MonoBehaviour
{
    [Header("Referências dos botões para editar")]
    public RectTransform botao1;
    public RectTransform botao2;
    public RectTransform botao3;
    public RectTransform botao4;

    [Header("Joystick (fixo, não editável pelo usuário)")]
    public RectTransform joystick;

    [Header("Canvas com os botões (GameCt)")]
    public GameObject canvasControles;

    private Dictionary<string, RectTransform> botoes = new Dictionary<string, RectTransform>();
    private string caminhoArquivo;

    void Start()
    {
        caminhoArquivo = Path.Combine(Application.persistentDataPath, "config", "botoes_mobile.json");

        AdicionarBotao(botao1);
        AdicionarBotao(botao2);
        AdicionarBotao(botao3);
        AdicionarBotao(botao4);
        AdicionarBotao(joystick); // ← joystick entra no sistema, mas não será arrastável

        CarregarConfiguracoes();
    }

    void AdicionarBotao(RectTransform rt)
    {
        if (rt != null)
        {
            botoes[rt.name] = rt;

            // Só adiciona o componente de edição se não for o joystick
            if (rt.GetComponent<EditarBotao>() == null && rt != joystick)
                rt.gameObject.AddComponent<EditarBotao>();
        }
    }

    public void SalvarConfiguracoes()
    {
        List<DadosBotao> lista = new List<DadosBotao>();

        foreach (var item in botoes)
        {
            RectTransform rt = item.Value;
            lista.Add(new DadosBotao
            {
                nome = item.Key,
                posicao = new Vetor2(rt.anchoredPosition),
                tamanho = (rt == joystick) ? null : new Vetor2(rt.sizeDelta) // ← ignora tamanho do joystick
            });
        }

        string json = JsonUtility.ToJson(new ListaDeBotoes { botoes = lista }, true);

        Directory.CreateDirectory(Path.GetDirectoryName(caminhoArquivo));
        File.WriteAllText(caminhoArquivo, json);
        Debug.Log("Configurações salvas em: " + caminhoArquivo);
    }

    public void CarregarConfiguracoes()
    {
        if (!File.Exists(caminhoArquivo))
        {
            Debug.Log("Nenhum arquivo de configuração encontrado.");
            return;
        }

        string json = File.ReadAllText(caminhoArquivo);
        ListaDeBotoes dados = JsonUtility.FromJson<ListaDeBotoes>(json);

        foreach (DadosBotao dado in dados.botoes)
        {
            if (botoes.TryGetValue(dado.nome, out RectTransform rt))
            {
                rt.anchoredPosition = dado.posicao.ToVector2();
                if (dado.tamanho != null && rt != joystick)
                    rt.sizeDelta = dado.tamanho.ToVector2();
            }
        }

        Debug.Log("Configurações de botões carregadas.");
    }

    public void SalvarEFecharEditor()
    {
        SalvarConfiguracoes();

        if (canvasControles != null)
        {
            canvasControles.SetActive(false);
            Debug.Log("Editor de controles fechado.");
        }
        else
        {
            Debug.LogWarning("Canvas de controles não atribuído.");
        }
    }

    /// <summary>
    /// Inverte os valores de X e Y das posições dos botões salvos no JSON.
    /// </summary>
    public void InverterControles()
    {
        if (!File.Exists(caminhoArquivo))
        {
            Debug.LogWarning("Não há arquivo para inverter.");
            return;
        }

        string json = File.ReadAllText(caminhoArquivo);
        ListaDeBotoes dados = JsonUtility.FromJson<ListaDeBotoes>(json);

        foreach (DadosBotao dado in dados.botoes)
        {
            // Inverte apenas o eixo X
            dado.posicao.x = -dado.posicao.x;
        }

        string novoJson = JsonUtility.ToJson(dados, true);
        File.WriteAllText(caminhoArquivo, novoJson);
        Debug.Log("Eixo X dos controles invertido e salvo.");

        CarregarConfiguracoes(); // Aplica as mudanças visualmente
    }

}

[System.Serializable]
public class DadosBotao
{
    public string nome;
    public Vetor2 posicao;
    public Vetor2 tamanho;
}

[System.Serializable]
public class ListaDeBotoes
{
    public List<DadosBotao> botoes;
}

[System.Serializable]
public class Vetor2
{
    public float x;
    public float y;

    public Vetor2(Vector2 v)
    {
        x = v.x;
        y = v.y;
    }

    public Vector2 ToVector2() => new Vector2(x, y);
}
