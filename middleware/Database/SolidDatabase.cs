
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
//-------------------------------------------- application
    public Application? GetApplication(string appName)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT id, resource_name, resource_desc, created_at
            FROM applications
            WHERE resource_name = @AppName
            """;

        command.Parameters.AddWithValue("@AppName", appName);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return null;

        return new Application
        {
            Id = reader.GetInt32(0),
            ResourceName = reader.GetString(1),
            ResourceDesc = reader.GetString(2),
            DtCreation = reader.GetString(3)
        };
    }

    public int InsertApplication(string resourceName, string resourceDesc)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO applications (
                resource_name,
                resource_desc,
                created_at
            )
            VALUES (
                @ResourceName,
                @ResourceDesc,
                @CreatedAt
            );

            SELECT last_insert_rowid();
            """;

        command.Parameters.AddWithValue("@ResourceName", resourceName);
        command.Parameters.AddWithValue("@ResourceDesc", resourceDesc);
        command.Parameters.AddWithValue(
            "@CreatedAt",
            DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss'Z'") //formato ISO 8601
        );

        return Convert.ToInt32(command.ExecuteScalar());
    }


    public List<Channel> GetChannels(string appName)
    {
        var result = new List<Channel>();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                c.id,
                c.application_id,
                c.resource_name,
                c.resource_desc,
                c.created_at
            FROM channels c
            JOIN applications a ON c.application_id = a.id
            WHERE a.resource_name = @AppName
            """;

        command.Parameters.AddWithValue("@AppName", appName);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var channel = new Channel
            {
                Id = reader.GetInt32(0),
                ApplicationId = reader.GetInt32(1),
                ResourceName = reader.GetString(2),
                ResourceDesc = reader.GetString(3),
                DtCreation = reader.GetString(4)
            };

            result.Add(channel);
        }

        return result;
    }


    public Channel? GetChannel(string appName, string channelName)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                c.id,
                c.application_id,
                c.resource_name,
                c.resource_desc,
                c.created_at
            FROM channels c
            JOIN applications a ON c.application_id = a.id
            WHERE a.resource_name = @AppName
            AND c.resource_name = @ChannelName
            """;

        command.Parameters.AddWithValue("@AppName", appName);
        command.Parameters.AddWithValue("@ChannelName", channelName);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return null;

        return new Channel
        {
            Id = reader.GetInt32(0),
            ApplicationId = reader.GetInt32(1),
            ResourceName = reader.GetString(2),
            ResourceDesc = reader.GetString(3),
            DtCreation = reader.GetString(4)
        };
    }

    public bool UpdateApplication(string appName,string resourceName,string resourceDesc){
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE applications
            SET resource_name = @ResourceName,
                resource_desc = @ResourceDesc
            WHERE resource_name = @AppName
            """;

        command.Parameters.AddWithValue("@AppName", appName);
        command.Parameters.AddWithValue("@ResourceName", resourceName);
        command.Parameters.AddWithValue("@ResourceDesc", resourceDesc);

        return command.ExecuteNonQuery() > 0;
    }

//------------------------------------channels

    public int? InsertChannel(string appName,string resourceName,string resourceDesc){
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO channels (
                application_id,
                resource_name,
                resource_desc,
                created_at
            )
            SELECT
                id,
                @ResourceName,
                @ResourceDesc,
                datetime('now')
            FROM applications
            WHERE resource_name = @AppName
            RETURNING id;
            """;

        command.Parameters.AddWithValue("@AppName", appName);
        command.Parameters.AddWithValue("@ResourceName", resourceName);
        command.Parameters.AddWithValue("@ResourceDesc", resourceDesc);

        var id = command.ExecuteScalar();

        return id == null ? null : Convert.ToInt32(id);
    }

   public bool UpdateChannel(string appName,string channelName,string resourceName,string resourceDesc){
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            UPDATE channels
            SET resource_name = @ResourceName,
                resource_desc = @ResourceDesc
            WHERE resource_name = @ChannelName
            AND application_id = (
                SELECT id
                FROM applications
                WHERE resource_name = @AppName
            )
            """;

        command.Parameters.AddWithValue("@AppName", appName);
        command.Parameters.AddWithValue("@ChannelName", channelName);
        command.Parameters.AddWithValue("@ResourceName", resourceName);
        command.Parameters.AddWithValue("@ResourceDesc", resourceDesc);

        return command.ExecuteNonQuery() > 0;
    }


    public bool DeleteApplication(string appName)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            DELETE FROM applications
            WHERE resource_name = @AppName
            """;

        command.Parameters.AddWithValue("@AppName", appName);

        return command.ExecuteNonQuery() > 0;
    }

    public bool DeleteChannel(string appName, string channelName)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            DELETE FROM channels
            WHERE resource_name = @ChannelName
            AND application_id = (
                SELECT id
                FROM applications
                WHERE resource_name = @AppName
            )
            """;

        command.Parameters.AddWithValue("@AppName", appName);
        command.Parameters.AddWithValue("@ChannelName", channelName);

        return command.ExecuteNonQuery() > 0;
    }



//---------------------------subscriptions

    public Subscription? GetSubscription(string appName,string channelName,string subName){
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT
                sb.id,
                sb.channel_id,
                sb.resource_name,
                sb.evt,
                sb.endpoint,
                sb.created_at,
                sb.expires_at
            FROM subscriptions sb
            JOIN channels ch
                ON sb.channel_id = ch.id
            JOIN applications ap
                ON ch.application_id = ap.id
            WHERE ap.resource_name = @AppName
            AND ch.resource_name = @ChannelName
            AND sb.resource_name = @SubName
            LIMIT 1;
            """;

        command.Parameters.AddWithValue("@AppName", appName);
        command.Parameters.AddWithValue("@ChannelName", channelName);
        command.Parameters.AddWithValue("@SubName", subName);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return null;

        return new Subscription
        {
            Id = reader.GetInt32(0),
            ChannelId = reader.GetInt32(1),
            ResourceName = reader.GetString(2),
            Evt = reader.GetInt32(3),
            Endpoint = reader.GetString(4),
            DtCreation = reader.GetString(5),
            DtExpiration = reader.IsDBNull(6)
                ? ""
                : reader.GetString(6)
        };
    }

    public bool DeleteSubscription(string appName,string channelName,string subName){
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            DELETE FROM subscriptions
            WHERE resource_name = @SubName
            AND channel_id IN (
                SELECT ch.id
                FROM channels ch
                JOIN applications ap
                    ON ch.application_id = ap.id
                WHERE ap.resource_name = @AppName
                    AND ch.resource_name = @ChannelName
            );
            """;

        command.Parameters.AddWithValue("@AppName", appName);
        command.Parameters.AddWithValue("@ChannelName", channelName);
        command.Parameters.AddWithValue("@SubName", subName);

        return command.ExecuteNonQuery() > 0;
    }

//---------------------------datarecords

    public DataRecord? GetDataRecord(string appName,string channelName,string recordName){
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
                dr.expires_at
            FROM data_records dr
            JOIN channels ch
                ON dr.channel_id = ch.id
            JOIN applications ap
                ON ch.application_id = ap.id
            WHERE ap.resource_name = @AppName
            AND ch.resource_name = @ChannelName
            AND dr.resource_name = @RecordName
            LIMIT 1;
            """;

        command.Parameters.AddWithValue("@AppName", appName);
        command.Parameters.AddWithValue("@ChannelName", channelName);
        command.Parameters.AddWithValue("@RecordName", recordName);

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return null;

        return new DataRecord
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
    }

    public Subscription? InsertSubscription(
        string appName,
        string channelName,
        string resourceName,
        int evt,
        string endpoint,
        string? expiration)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO subscriptions (
                channel_id,
                resource_name,
                evt,
                endpoint,
                created_at,
                expires_at
            )
            SELECT
                ch.id,
                @ResourceName,
                @Evt,
                @Endpoint,
                @CreatedAt,
                @ExpiresAt
            FROM channels ch
            JOIN applications ap
                ON ch.application_id = ap.id
            WHERE ap.resource_name = @AppName
            AND ch.resource_name = @ChannelName
            RETURNING
                id,
                channel_id,
                resource_name,
                evt,
                endpoint,
                created_at,
                expires_at;
            """;

        command.Parameters.AddWithValue("@AppName", appName);
        command.Parameters.AddWithValue("@ChannelName", channelName);
        command.Parameters.AddWithValue("@ResourceName", resourceName);
        command.Parameters.AddWithValue("@Evt", evt);
        command.Parameters.AddWithValue("@Endpoint", endpoint);

        command.Parameters.AddWithValue(
            "@CreatedAt",
            DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss'Z'")
        );

        command.Parameters.AddWithValue(
            "@ExpiresAt",
            (object?)expiration ?? DBNull.Value
        );

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return null;

        return new Subscription
        {
            Id = reader.GetInt32(0),
            ChannelId = reader.GetInt32(1),
            ResourceName = reader.GetString(2),
            Evt = reader.GetInt32(3),
            Endpoint = reader.GetString(4),
            DtCreation = reader.GetString(5),
            DtExpiration = reader.IsDBNull(6)
                ? ""
                : reader.GetString(6)
        };
    }


    public DataRecord? InsertDataRecord(
        string appName,
        string channelName,
        string resourceName,
        string contentType,
        string content,
        string? expiration)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            INSERT INTO data_records (
                channel_id,
                resource_name,
                content_type,
                content,
                created_at,
                expires_at
            )
            SELECT
                ch.id,
                @ResourceName,
                @ContentType,
                @Content,
                @CreatedAt,
                @ExpiresAt
            FROM channels ch
            JOIN applications ap
                ON ch.application_id = ap.id
            WHERE ap.resource_name = @AppName
            AND ch.resource_name = @ChannelName

            RETURNING
                id,
                channel_id,
                resource_name,
                content_type,
                content,
                created_at,
                expires_at;
            """;

        command.Parameters.AddWithValue("@AppName", appName);
        command.Parameters.AddWithValue("@ChannelName", channelName);
        command.Parameters.AddWithValue("@ResourceName", resourceName);
        command.Parameters.AddWithValue("@ContentType", contentType);
        command.Parameters.AddWithValue("@Content", content);

        command.Parameters.AddWithValue(
            "@CreatedAt",
            DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss'Z'")
        );

        command.Parameters.AddWithValue(
            "@ExpiresAt",
            (object?)expiration ?? DBNull.Value
        );

        using var reader = command.ExecuteReader();

        if (!reader.Read())
            return null;

        return new DataRecord
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
    }

    public bool DeleteDataRecord(string appName,string channelName,string recordName)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            DELETE FROM data_records
            WHERE resource_name = @RecordName
            AND channel_id IN (
                SELECT ch.id
                FROM channels ch
                JOIN applications ap
                    ON ch.application_id = ap.id
                WHERE ap.resource_name = @AppName
                    AND ch.resource_name = @ChannelName
            );
            """;

        command.Parameters.AddWithValue("@AppName", appName);
        command.Parameters.AddWithValue("@ChannelName", channelName);
        command.Parameters.AddWithValue("@RecordName", recordName);

        return command.ExecuteNonQuery() > 0;
    }




    public List<string> DiscoverDataRecords(
        string appName,
        string channelName)
    {
        var paths = new List<string>();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT dr.resource_name
            FROM data_records dr
            JOIN channels ch ON dr.channel_id = ch.id
            JOIN applications ap ON ch.application_id = ap.id
            WHERE ap.resource_name = @AppName
            AND ch.resource_name = @ChannelName
            ORDER BY dr.resource_name;
            """;

        command.Parameters.AddWithValue("@AppName", appName);
        command.Parameters.AddWithValue("@ChannelName", channelName);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            string recordName = Uri.EscapeDataString(reader.GetString(0));

            paths.Add(
                $"/api/solid/{Uri.EscapeDataString(appName)}" +
                $"/{Uri.EscapeDataString(channelName)}/{recordName}"
            );
        }

        return paths;
    }


    public List<string> DiscoverSubscriptions(
        string appName,
        string channelName)
    {
        var paths = new List<string>();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT sb.resource_name
            FROM subscriptions sb
            JOIN channels ch ON sb.channel_id = ch.id
            JOIN applications ap ON ch.application_id = ap.id
            WHERE ap.resource_name = @AppName
            AND ch.resource_name = @ChannelName
            ORDER BY sb.resource_name;
            """;

        command.Parameters.AddWithValue("@AppName", appName);
        command.Parameters.AddWithValue("@ChannelName", channelName);

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            string subName = Uri.EscapeDataString(reader.GetString(0));

            paths.Add(
                $"/api/solid/{Uri.EscapeDataString(appName)}" +
                $"/{Uri.EscapeDataString(channelName)}/subs/{subName}"
            );
        }

        return paths;
    }
//-------------------------------------


    public List<string> DiscoverApplications()
    {
        var paths = new List<string>();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT resource_name
            FROM applications
            ORDER BY resource_name
            """;

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var name = Uri.EscapeDataString(reader.GetString(0));
            paths.Add($"/api/solid/{name}");
        }

        return paths;
    }

    public List<string> DiscoverChannels(string? appName = null)
    {
        var paths = new List<string>();

        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText = """
            SELECT a.resource_name, c.resource_name
            FROM channels c
            JOIN applications a ON c.application_id = a.id
            WHERE @AppName IS NULL OR a.resource_name = @AppName
            ORDER BY a.resource_name, c.resource_name
            """;

        command.Parameters.AddWithValue(
            "@AppName",
            (object?)appName ?? DBNull.Value
        );

        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            string application = Uri.EscapeDataString(reader.GetString(0));
            string channel = Uri.EscapeDataString(reader.GetString(1));

            paths.Add($"/api/solid/{application}/{channel}");
        }

        return paths;
    }


}


