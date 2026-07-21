using EmployeeManagement.Doamin.Entities;
using EmployeeManagement.Doamin.Repository;
using EmployeeManagement.Infrastructure.Data;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;

namespace EmployeeManagement.Infrastructure.Repository
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public EmployeeRepository(IDbConnectionFactory dbConnectionFactory)
        {
            _connectionFactory = dbConnectionFactory;
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            var sql = @"SELECT Id, Name, Email, Salary, HireDate, IsActive, CreateAt, UpdatedAt, DeletedAt, IsDeleted FROM Employee WHERE ID = @id";

            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return await Map(reader);
            }

            return null;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            const string sql = @"
                SELECT Id, Name, Email, Salary, HireDate, IsActive, CreateAt, UpdatedAt, DeletedAt, IsDeleted
                FROM Employee
                ORDER BY Id";

            var employees = new List<Employee>();

            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
                employees.Add(await Map(reader));

            return employees;
        }

        public async Task<int> AddAsync(Employee employee)
        {
            const string sql = @"
                INSERT INTO Employee (Name, Email, Salary, HireDate, IsActive, CreateAt, IsDeleted)
                OUTPUT INSERTED.Id
                VALUES (@Name, @Email, @Salary, @HireDate, @IsActive, GETUTCDATE(), 0)";

            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            AddParameters(command, employee);

            var newId = await command.ExecuteScalarAsync();
            return Convert.ToInt32(newId);
        }

        public async Task<bool> UpdateAsync(Employee employee)
        {
            const string sql = @"
                UPDATE Employee
                SET Salary = @Salary,
                    IsActive = @IsActive,
                    UpdatedAt = GETUTCDATE()
                WHERE Id = @Id";

            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = employee.Id });
            command.Parameters.Add(new SqlParameter("@Salary", SqlDbType.Decimal) { Value = employee.Salary });
            command.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = employee.IsActive });

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            const string sql = "UPDATE Employee SET IsDeleted = 1, DeletedAt = GETUTCDATE() WHERE Id = @Id";

            using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.Add(new SqlParameter("@Id", SqlDbType.Int) { Value = id });

            var rowsAffected = await command.ExecuteNonQueryAsync();
            return rowsAffected > 0;
        }

        private static void AddParameters(SqlCommand command, Employee employee)
        {
            command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = employee.Name });
            command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 200) { Value = employee.Email });
            command.Parameters.Add(new SqlParameter("@Salary", SqlDbType.Decimal) { Value = employee.Salary });
            command.Parameters.Add(new SqlParameter("@HireDate", SqlDbType.DateTime2) { Value = employee.HireDate });
            command.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = employee.IsActive });
        }

        private static async Task<Employee> Map(SqlDataReader reader)
        {
            if (reader == null || reader.IsClosed)
                throw new ArgumentException("Invalid SqlDataReader provided.");
            if(!reader.HasRows)
                throw new InvalidOperationException("No data available in the SqlDataReader.");

            Employee entity = new()
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Email = reader.GetString(reader.GetOrdinal("Email")),
                Salary = reader.GetDecimal(reader.GetOrdinal("Salary")),
                HireDate = reader.GetDateTime(reader.GetOrdinal("HireDate")),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                CreateAt = reader.GetDateTime(reader.GetOrdinal("CreateAt")),
                UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                DeletedAt = reader.IsDBNull(reader.GetOrdinal("DeletedAt")) ? null : reader.GetDateTime(reader.GetOrdinal("DeletedAt")),
                IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
            };

            return entity;
        }
    }
}
