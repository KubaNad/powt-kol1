using Microsoft.Data.SqlClient;
using powt1.Exeptions;
using powt1.Models;
using powt1.Models.DTOs;

namespace powt1.Services;

public class DbService(IConfiguration config) : IDbService
{
    private readonly string? _connectionString = config.GetConnectionString("Default");
    public async Task<IEnumerable<StudentGetDTO>> GetStudentsDetailsAsync()
    {
        var result = new List<StudentGetDTO>();
        
        await using var connection = new SqlConnection(_connectionString);
        const string sql1 = "SELECT Id, FirstName, LastName, Age FROM Student";
        await using var command1 = new SqlCommand(sql1, connection);
        await connection.OpenAsync();
        await using var reader = await command1.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new StudentGetDTO
            {
                Id = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Age = reader.GetInt32(3),
                Groups = new List<Group>()
            });
        }
        await reader.DisposeAsync();
        foreach (var studentGetDTO in result)
        {
            int id = studentGetDTO.Id;
            const string sql2 = "SELECT G.Id, Name FROM Student " +
                                "LEFT JOIN GroupAssignment GA on Student.Id = GA.Student_Id " +
                                "LEFT JOIN [Group] G on Group_Id = G.Id " +
                                "Where Student_Id = @id";
            await using var command2 = new SqlCommand(sql2, connection);
            command2.Parameters.AddWithValue("@id", id);
            await using var reader2 = await command2.ExecuteReaderAsync();
            while (await reader2.ReadAsync())
            {
                studentGetDTO.Groups.Add(new Group
                {
                    Id = reader2.GetInt32(0),
                    Name = reader2.GetString(1),
                });
            }
        }

        return result;
    }

    public async Task<StudentGetDTO> CreateStudentWithGroups(StudentWithGroupsPostDTO studentWithGroupsPost)
    {
        
        await using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        if (studentWithGroupsPost.Groups is not null && studentWithGroupsPost.Groups.Count != 0)
        {
            foreach (var group in studentWithGroupsPost.Groups)
            {
                const string sql1 = "SELECT 1 FROM [Group] Where Id = @id";
                await using var command1 = new SqlCommand(sql1, connection);
                command1.Parameters.AddWithValue("@id", group);
                await using var reader = await command1.ExecuteReaderAsync();
                if (!await reader.ReadAsync())
                {
                    throw new NotFoundException($"Group with id {group} does not exist");
                }
            }
        }
        
        
        await using var transaction = await connection.BeginTransactionAsync();

        try
        {
            const string insertStudentSql =
                "INSERT INTO Student (FirstName, LastName, Age) VALUES (@FirstName, @LastName, @Age);" +
                "SELECT CAST(SCOPE_IDENTITY() as int)";
            await using var command2 = new SqlCommand(insertStudentSql, connection, (SqlTransaction)transaction);
            command2.Parameters.AddWithValue("@FirstName", studentWithGroupsPost.FirstName);
            command2.Parameters.AddWithValue("@LastName", studentWithGroupsPost.LastName);
            command2.Parameters.AddWithValue("@Age", studentWithGroupsPost.Age);
            
            var  newStudentId = Convert.ToInt32(await command2.ExecuteScalarAsync());

            if (studentWithGroupsPost.Groups is not null && studentWithGroupsPost.Groups.Count != 0)
            {
                foreach (var group in studentWithGroupsPost.Groups)
                {
                    const string insertGroupSql =
                        "INSERT INTO GroupAssignment (Student_Id, Group_Id) VALUES (@Student_Id, @Group_Id)";
                    await using var command3 = new SqlCommand(insertGroupSql, connection, (SqlTransaction)transaction);
                    command3.Parameters.AddWithValue("@Student_Id", newStudentId);
                    command3.Parameters.AddWithValue("@Group_Id", group);
                    await command3.ExecuteNonQueryAsync();
                }
            }

            await transaction.CommitAsync();
            
            
            const string sql9 = "SELECT Id, FirstName, LastName, Age FROM Student WHERE Id = @Id";
            await using var command9 = new SqlCommand(sql9, connection);
            command9.Parameters.AddWithValue("@Id", newStudentId);
            await using var reader = await command9.ExecuteReaderAsync();

            await reader.ReadAsync();
            
             var result = new StudentGetDTO
            {
                Id = reader.GetInt32(0),
                FirstName = reader.GetString(1),
                LastName = reader.GetString(2),
                Age = reader.GetInt32(3),
                Groups = new List<Group>()
            };
            
            await reader.DisposeAsync();
            
            int id = result.Id;
            const string sql8 = "SELECT G.Id, Name FROM Student " +
                                "LEFT JOIN GroupAssignment GA on Student.Id = GA.Student_Id " +
                                "LEFT JOIN [Group] G on Group_Id = G.Id " +
                                "Where Student_Id = @id";
            await using var command8 = new SqlCommand(sql8, connection);
            command8.Parameters.AddWithValue("@id", id);
            await using var reader2 = await command8.ExecuteReaderAsync();
            while (await reader2.ReadAsync())
            {
                result.Groups.Add(new Group
                {
                    Id = reader2.GetInt32(0),
                    Name = reader2.GetString(1),
                });
            }

            return result;


        }
        catch (Exception e)
        {
            await transaction.RollbackAsync();
            throw;
        }
        
    }
}