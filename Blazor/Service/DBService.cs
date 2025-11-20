using Blazor.Models;
using Npgsql;
using System.Text.Json;

namespace Blazor.Service
{
    public class DBService
    {
        private readonly string _connectionString;

        public DBService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
            
            // Debug: Tjek om connection string blev læst korrekt
            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new InvalidOperationException("Connection string is empty or null.");
            }
        }

        /// <summary>
        /// Opretter en ny Plate i databasen
        /// </summary>
        public async Task<bool> CreatePlateAsync(Plate plate)
        {
            const string sql = @"
                INSERT INTO plates (id, row1, row2, row3)
                VALUES (@id, @row1, @row2, @row3)
                ON CONFLICT (id) DO NOTHING;";

            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("id", plate.Id);
                
                // Brug JSONB type eksplicit for PostgreSQL JSONB kolonner
                var row1Param = new NpgsqlParameter("row1", NpgsqlTypes.NpgsqlDbType.Jsonb)
                {
                    Value = JsonSerializer.Serialize(plate.Row1)
                };
                var row2Param = new NpgsqlParameter("row2", NpgsqlTypes.NpgsqlDbType.Jsonb)
                {
                    Value = JsonSerializer.Serialize(plate.Row2)
                };
                var row3Param = new NpgsqlParameter("row3", NpgsqlTypes.NpgsqlDbType.Jsonb)
                {
                    Value = JsonSerializer.Serialize(plate.Row3)
                };
                
                command.Parameters.Add(row1Param);
                command.Parameters.Add(row2Param);
                command.Parameters.Add(row3Param);

                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                // Log fejl her hvis nødvendigt
                throw new Exception($"Fejl ved oprettelse af plate: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Henter en Plate fra databasen baseret på ID
        /// </summary>
        public async Task<Plate?> ReadPlateAsync(string id)
        {
            const string sql = @"
                SELECT id, row1, row2, row3
                FROM plates
                WHERE id = @id;";

            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("id", id);

                await using var reader = await command.ExecuteReaderAsync();

                if (await reader.ReadAsync())
                {
                    var row1Json = reader.GetString(1);
                    var row2Json = reader.GetString(2);
                    var row3Json = reader.GetString(3);

                    var row1 = JsonSerializer.Deserialize<List<int>>(row1Json) ?? new List<int>();
                    var row2 = JsonSerializer.Deserialize<List<int>>(row2Json) ?? new List<int>();
                    var row3 = JsonSerializer.Deserialize<List<int>>(row3Json) ?? new List<int>();

                    return new Plate(
                        reader.GetString(0),
                        row1,
                        row2,
                        row3
                    );
                }

                return null;
            }
            catch (Exception ex)
            {
                // Log fejl her hvis nødvendigt
                throw new Exception($"Fejl ved læsning af plate: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Henter alle Plates fra databasen
        /// </summary>
        public async Task<List<Plate>> ReadAllPlatesAsync()
        {
            const string sql = @"
                SELECT id, row1, row2, row3
                FROM plates
                ORDER BY id;";

            var plates = new List<Plate>();

            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(sql, connection);
                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var row1Json = reader.GetString(1);
                    var row2Json = reader.GetString(2);
                    var row3Json = reader.GetString(3);

                    var row1 = JsonSerializer.Deserialize<List<int>>(row1Json) ?? new List<int>();
                    var row2 = JsonSerializer.Deserialize<List<int>>(row2Json) ?? new List<int>();
                    var row3 = JsonSerializer.Deserialize<List<int>>(row3Json) ?? new List<int>();

                    plates.Add(new Plate(
                        reader.GetString(0),
                        row1,
                        row2,
                        row3
                    ));
                }

                return plates;
            }
            catch (Exception ex)
            {
                // Log fejl her hvis nødvendigt
                throw new Exception($"Fejl ved læsning af alle plates: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Opretter database tabellen hvis den ikke eksisterer
        /// </summary>
        public async Task EnsureTableExistsAsync()
        {
            const string sql = @"
                CREATE TABLE IF NOT EXISTS plates (
                    id VARCHAR(255) PRIMARY KEY,
                    row1 JSONB NOT NULL,
                    row2 JSONB NOT NULL,
                    row3 JSONB NOT NULL
                );";

            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(sql, connection);
                await command.ExecuteNonQueryAsync();
            }
            catch (NpgsqlException npgsqlEx)
            {
                throw new Exception($"PostgreSQL fejl ved oprettelse af tabel: {npgsqlEx.Message}. Connection string host: {GetConnectionStringHost()}", npgsqlEx);
            }
            catch (Exception ex)
            {
                throw new Exception($"Fejl ved oprettelse af tabel: {ex.Message}. Connection string host: {GetConnectionStringHost()}", ex);
            }
        }

        private string GetConnectionStringHost()
        {
            try
            {
                var builder = new NpgsqlConnectionStringBuilder(_connectionString);
                return builder.Host ?? "ikke sat";
            }
            catch
            {
                return "kunne ikke parse connection string";
            }
        }

    }
}

