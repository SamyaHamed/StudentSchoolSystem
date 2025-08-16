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
 
public class SchoolController:ControllerBase
{
          private readonly AppDbContext _context;

           public SchoolController(AppDbContext context) 
           {
             _context = context;
           }
            
           [Authorize(Roles ="Admin")]
           [HttpPost] //create new school 
           public async Task<ActionResult<CreateSchoolDto>> CreateSchool(CreateSchoolDto createDto)
           {
               var exists = await _context.Schools.AnyAsync(s=> s.SecritCode == createDto.SecritCode);
               if (exists)
               {
                   return BadRequest($"The Secrit Code {createDto.SecritCode} already in use");
               }
               var school = new School
               {
                 Name=createDto.Name?? string.Empty,
                 City = createDto.City?? string.Empty,
                 SecritCode =createDto.SecritCode?? string.Empty,
                 IsVerified = createDto.IsVerified?? false,
               };
               
               _context.Schools.Add(school);
               await _context.SaveChangesAsync();
               return CreatedAtAction(nameof(CreateSchool), new { id = school.Id }, school);
           }

           
           [Authorize(Roles= "Admin, User")]
           [HttpGet("{id}")] // return school by id 
           public async Task<ActionResult<ReturnSchoolDto>> GetSchool(int id)
           {
               var school = await _context.Schools.FindAsync(id);
               if (school == null)
               {
                   return NotFound("School not found");
               }

               var schoolDto = new CreateSchoolDto
               {
                  Name = school.Name,
                  City = school.City,
                  IsVerified = school.IsVerified,
                  SecritCode = school.SecritCode
               };
               
               return Ok(schoolDto);
           }

           [Authorize(Roles= "Admin, User")]
           [HttpGet] // return all schools
           public async Task<ActionResult<List<ReturnSchoolDto>>> GetAll( 
               [FromQuery] int pageNumber =1,
               [FromQuery] int pageSize=10,
               [FromQuery] string orderBy= "Name"
               )
           { 
                IQueryable <School> schoolsQuery =  _context.Schools;

               schoolsQuery = orderBy?.ToLower() switch
                   {
                       SchoolsConst.Id => schoolsQuery.OrderBy(s=>s.Id),
                       SchoolsConst.Name => schoolsQuery.OrderBy(s=>s.Name),
                       SchoolsConst.City => schoolsQuery.OrderBy(s=>s.City),
                       _ => schoolsQuery.OrderBy(s=>s.Name)
                   };
                   
                 
                  var schools = await schoolsQuery
                   .Skip((pageNumber-1)*pageSize)
                   .Take(pageSize)
                   .Select(s=>new ReturnSchoolDto
                   {
                       Id = s.Id,
                       Name = s.Name,
                       City= s.City,
                       IsVerified = s.IsVerified
                       
                   })
                   .ToListAsync();
               if (schools.Count ==0 )
               {
                   return NotFound("No schools found");
               }
               return Ok(schools);


           }
           
           [Authorize(Roles="Admin")]
           [HttpDelete("{id}")] // delete school by using id 
           public async Task<ActionResult> DeleteSchool(int id)
           {
               var school = await _context.Schools.FindAsync(id);
               if (school == null)
               {
                   return NotFound("School not found");
               }
               _context.Schools.Remove(school);
               await _context.SaveChangesAsync();
               return NoContent();
               
           }
           
           [Authorize (Roles="Admin")]
           [HttpPut("{id}")] // update for school 
           public async Task<IActionResult> UpdateSchool(int id, CreateSchoolDto updateDto)
           {
               var school = await _context.Schools.FindAsync(id);
               if (school == null)
               {
                   return NotFound("School not found");
               }
               school.Name = updateDto.Name?? school.Name;
               school.IsVerified = updateDto.IsVerified?? school.IsVerified;
               school.SecritCode = updateDto.SecritCode?? school.SecritCode;

               await _context.SaveChangesAsync();
               return NoContent();


           }

          
           
           
}