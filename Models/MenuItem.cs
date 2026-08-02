 namespace CampusEats.Api.Models;

public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }   // money = decimal, never double/float
    public string Category { get; set; } = "";
    public bool Available { get; set; } = true;
}
