namespace TestApp.Dto;

public class CreateSchoolDto
{
    public string? Name { get; set; }
    public string City { get; set; }
    public bool? IsVerified { get; set; }
    public string? SecritCode { set; get; }
    
}