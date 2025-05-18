
using powt1.Models.DTOs;

namespace powt1.Services;

public interface IDbService
{
    public Task<IEnumerable<StudentGetDTO>> GetStudentsDetailsAsync();
    public Task<StudentGetDTO> CreateStudentWithGroups(StudentWithGroupsPostDTO studentWithGroupsPost);
}