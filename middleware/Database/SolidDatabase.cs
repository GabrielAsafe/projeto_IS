
using Microsoft.Data.Sqlite;


namespace middleware.Models;
public class SolidDatabase
{
    private readonly string connectionString;

    public SolidDatabase(IWebHostEnvironment env)
    {
        // 1. Localizar a base de dados
        //env saca o path relativo ao projeto
        string rootPath = Path.Combine(
            env.ContentRootPath, "Data", "database.db"
        );

        string middlewarePath = Path.GetFullPath(
            Path.Combine(
                env.ContentRootPath, "..", "Data", "database.db"
            )
        );

        //a db tem que estar em algum lado, e ele procura no dois
        string dbPath = File.Exists(rootPath)
            ? rootPath
            : middlewarePath;

        if (!File.Exists(dbPath))
            throw new FileNotFoundException(
                "SQLite database not found", dbPath
            );

        // 2. Configurar a connection string
        connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadWrite,
            ForeignKeys = true
        }.ToString();
    }

    public List<Dictionary<string, object?>> GetInterestedSubscriptions(int channelId,int eventType)
    {
        var subscriptions = new List<Dictionary<string, object?>>();

        // 3. Abrir uma ligação apenas para esta operação
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        // 4. Preparar a consulta
        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT *
            FROM subscriptions
            WHERE channel_id = @ChannelId
              AND evt = @EventType
              AND (
                expires_at IS NULL
                OR trim(expires_at) = ''
                OR datetime(expires_at) > datetime('now')
              );
            """;

        // 5. Adicionar os parâmetros
        command.Parameters.AddWithValue("@ChannelId", channelId);
        command.Parameters.AddWithValue("@EventType", eventType);

        // 6. Executar a consulta
        using var reader = command.ExecuteReader();

        // 7. Percorrer as subscriptions encontradas
        while (reader.Read())
        {
            var subscription = new Dictionary<string, object?>();

            for (int i = 0; i < reader.FieldCount; i++)
            {
                subscription[reader.GetName(i)] =
                    reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            subscriptions.Add(subscription);
        }

        // 8. Devolver as subscriptions encontradas
        return subscriptions;
    }

public List<(DataRecord Record, string Path)> GetExpiredDataRecords()
{
    var result = new List<(DataRecord Record, string Path)>();

    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    using var command = connection.CreateCommand();

    command.CommandText = """
        SELECT
            dr.id,
            dr.channel_id,
            dr.resource_name,
            dr.content_type,
            dr.content,
            dr.created_at,
            dr.expires_at,
            a.resource_name,
            c.resource_name
        FROM data_records dr
        JOIN channels c ON dr.channel_id = c.id
        JOIN applications a ON c.application_id = a.id
        WHERE dr.expires_at IS NOT NULL
          AND datetime(dr.expires_at) <= datetime('now');
        """;

    using var reader = command.ExecuteReader();

    while (reader.Read())
    {
        var record = new DataRecord
        {
            Id = reader.GetInt32(0),
            ChannelId = reader.GetInt32(1),
            ResourceName = reader.GetString(2),
            ContentType = reader.GetString(3),
            Content = reader.GetString(4),
            DtCreation = reader.GetString(5),
            DtExpiration = reader.IsDBNull(6)
                ? ""
                : reader.GetString(6)
        };

        string appName = Uri.EscapeDataString(reader.GetString(7));
        string channelName = Uri.EscapeDataString(reader.GetString(8));
        string recordName = Uri.EscapeDataString(record.ResourceName);

        string path =
            $"/api/solid/{appName}/{channelName}/{recordName}";

        result.Add((record, path));
    }

    return result;
}

public bool DeleteExpiredDataRecord(int id)
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    using var command = connection.CreateCommand();

    command.CommandText = """
        DELETE FROM data_records
        WHERE id = @Id
          AND expires_at IS NOT NULL
          AND datetime(expires_at) <= datetime('now');
        """;

    command.Parameters.AddWithValue("@Id", id);

    return command.ExecuteNonQuery() == 1;
}

public int DeleteExpiredSubscriptions()
{
    using var connection = new SqliteConnection(connectionString);
    connection.Open();

    using var command = connection.CreateCommand();

    command.CommandText = """
        DELETE FROM subscriptions
        WHERE expires_at IS NOT NULL
          AND datetime(expires_at) <= datetime('now');
        """;

    return command.ExecuteNonQuery();
}
}


