namespace Core.Entity;

public class Group
{
    public int Id { get; set; }

    public required string Name { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();

}
