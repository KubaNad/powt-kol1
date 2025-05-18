namespace powt1.Models.DTOs;

public class StudentWithGroupsPostDTO
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public ICollection<int> Groups { get; set; }
}