using SQLite;

[Table("Configuration")]
public class Config
{
    [PrimaryKey]
    public string Chave { get; set; }
    public string Valor { get; set; }
}