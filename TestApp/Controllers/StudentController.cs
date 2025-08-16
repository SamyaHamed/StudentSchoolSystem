using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestApp.Constants;
using TestApp.Data;
using TestApp.Dto;
using TestApp.Models;

namespace TestApp.Controllers;

   [ApiController]
   [Route("api/schools")]
   public class StudentController : ControllerBase
   {
       private readonly AppDbContext _context;

       public StudentController(AppDbContext context)
       {
           _context = context;
       }
       
       

       // POST api/students
       [Authorize(Roles = "Admin")]
       [HttpPost("{schoolId}/students")]
       public async Task<ActionResult<CreateStudentDto>> CreateStudent( [FromRoute] int schoolId,CreateStudentDto createDto)
       {
           // Check if the SchoolId exists
           var exists = await _context.Students.AnyAsync(s => s.NationalId == createDto.NationalId);
           if (exists)
           {
               return BadRequest($"This {createDto.NationalId} already exists");
           }
           
           var school = await _context.Schools.FindAsync(schoolId);
           if (school == null)
           {
               return BadRequest("Invalid School ID");
           }

           var student = new Student
           {
               FirstName = createDto.FirstName?? string.Empty,
               LastName = createDto.LastName?? string.Empty,
               Age = createDto.Age?? StudentsConst.AgeEmpty,
               NationalId = createDto.NationalId?? string.Empty,
               SchoolId = schoolId 
           };

           _context.Students.Add(student);
           await _context.SaveChangesAsync();

           var studentDto = new CreateStudentDto{
               FirstName = student.FirstName,
               LastName = student.LastName,
               Age = student.Age, 
               NationalId = student.NationalId
           };

           return CreatedAtAction(nameof(GetById), new { schoolId = student.SchoolId,studentId = student.Id }, studentDto);
       }

   

       // GET api/students/{id}
       [Authorize(Roles = "Admin, User")]
       [HttpGet("{schoolId}/students/{studentId}")]
       public async Task<ActionResult<CreateStudentDto>> GetById( [FromRoute] int studentId, [FromRoute] int schoolId  )
       {
           var student = await _context.Students.Where(s => s.Id == studentId && s.SchoolId == schoolId)
               .Select(s => new CreateStudentDto
               {
                   FirstName = s.FirstName,
                   LastName = s.LastName,
                   Age =  s.Age,
                   NationalId = s.NationalId,
               }).FirstOrDefaultAsync();
           
           if (student == null)
               return NotFound();

           return Ok(student);
       }
       
       [Authorize(Roles = "Admin, User")]
       [HttpGet("{schoolId}/students")]
       public async Task<ActionResult<List<ReturnStudentDto>>> GetAll( [FromRoute] int schoolId,
           [FromQuery] int pageNumber=1,
           [FromQuery] int pageSize=5,
           [FromQuery]  string orderBy= "FirstName")
       {
           var studentsQuery =  _context.Students.Where(s=> s.SchoolId==schoolId);
               
           studentsQuery = orderBy?.ToLower() switch
               {
                   StudentsConst.FirstName => studentsQuery.OrderBy(s=> s.FirstName),
                   StudentsConst.LastName=> studentsQuery.OrderBy(s =>s.LastName),
                   StudentsConst.Id=> studentsQuery.OrderBy(s=>s.Id),
                   StudentsConst.Age=>studentsQuery.OrderBy(s=>s.Age),
                     _=> studentsQuery.OrderBy(s=>s.FirstName)
                   
               };
               
               var students = await studentsQuery
               .Skip((pageNumber-1)*pageSize)
               .Take(pageSize)
               .Select(s => new ReturnStudentDto
               {
                   FirstName = s.FirstName,
                   LastName = s.LastName,
                   Age = s.Age,
                   Id =s.Id
               })
               .ToListAsync();

           return Ok(students);
       }

       
       [Authorize(Roles = "Admin")]
       [HttpDelete("{schoolId}/students/{studentId}")]
       public async Task<ActionResult> Delete([FromRoute]int schoolId, [FromRoute]int studentId)
       {
           var student = await _context.Students.FirstOrDefaultAsync(s=>s.SchoolId==schoolId && s.Id ==studentId);
           if (student == null)
               return NotFound();
           _context.Students.Remove(student);
           await _context.SaveChangesAsync();
           return NoContent();
           
       }

       [Authorize(Roles = "Admin")]
       [HttpPut("{schoolId}/students/{studentId}")] //Http for update .
       public async Task<IActionResult> UpdateStudent([FromRoute] int schoolId,[FromRoute] int studentId, CreateStudentDto updateDto)
       {
           var student = await _context.Students.FirstOrDefaultAsync(s=>s.SchoolId == schoolId && s.Id ==studentId);
           if (student == null)
           {
               return NotFound();
           }
           student.FirstName = updateDto.FirstName?? student.FirstName;
           student.LastName = updateDto.LastName?? student.LastName;
           student.Age = updateDto.Age?? student.Age;
           student.NationalId = updateDto.NationalId?? student.NationalId;
           
           await _context.SaveChangesAsync();
           return NoContent();
       }

       [Authorize(Roles = "Admin, User")]
       [HttpGet("{schoolId}/students/search")] 
       public async Task<ActionResult<List<ReturnStudentDto>>> SearchStudents([FromRoute] int schoolId,
           [FromQuery] string name)
       {
           if (string.IsNullOrWhiteSpace(name))
           {
               return BadRequest("Name parameter is required.");
           }

           var students = await _context.Students.Where(s => s.SchoolId == schoolId && 
              ((s.FirstName+" "+s.LastName).ToLower().Contains(name.ToLower())))
                  .Select(s=> new ReturnStudentDto
                  {
                      FirstName = s.FirstName,
                      LastName = s.LastName,
                      Age = s.Age,
                      Id =s.Id
                  }).ToListAsync();
           
           return Ok(students);
       }
       

      
       


   }
