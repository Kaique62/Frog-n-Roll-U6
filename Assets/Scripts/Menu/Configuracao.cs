using SQLite;

[Table("Configuracao")]
public class Configuracao
{
    [PrimaryKey]
    public string Chave { get; set; }
    public string Valor { get; set; }
}