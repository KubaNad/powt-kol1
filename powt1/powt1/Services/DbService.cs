using Microsoft.Data.SqlClient;
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
        throw new NotImplementedException();
    }
}