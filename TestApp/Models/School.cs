namespace TestApp.Models;

public class School
{
    public int Id { set; get; }
    public string Name { set; get; }
    public string City { set; get; }
    public bool IsVerified { set; get; }
    public string SecritCode { set; get; }

    public List<Student> Students {
        set;
        get;
    }
}