namespace  TestApp.Models;

public class Student
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { set; get; }
    public int Age { get; set; }
    public string NationalId { get; set; }
    
    public int SchoolId { get; set; }
    
    public School school { get; set; }//capital letter
}