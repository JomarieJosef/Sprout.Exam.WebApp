using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Sprout.Exam.Business.DataTransferObjects;
using Sprout.Exam.Common.Enums;
using Sprout.Exam.DataAccess;
using Sprout.Exam.Common;


namespace Sprout.Exam.WebApp.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]

    public class EmployeesController : ControllerBase
    {

        private readonly SproutExamDbContext _context;
        public EmployeesController(SproutExamDbContext context)
        {
            _context = context;

        }

        /// <summary>
        /// Refactor this method to go through proper layers and fetch from the DB.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var result = await Task.FromResult(_context.Employees.Where(x => x.IsDeleted == false).ToList());
            List<EmployeeDto> employeeDtos = new List<EmployeeDto>();

            foreach (var emp in result)
            {
                EmployeeDto employeeDto = new EmployeeDto();
                employeeDto.Id = emp.Id;
                employeeDto.FullName = emp.FullName;
                employeeDto.Birthdate = emp.Birthdate.ToString("yyyy-MM-dd");
                employeeDto.Tin = emp.Tin;
                employeeDto.TypeId = emp.EmployeeTypeId;

                employeeDtos.Add(employeeDto);
            }
            return Ok(employeeDtos);
        }

        /// <summary>
        /// Refactor this method to go through proper layers and fetch from the DB.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await Task.FromResult(_context.Employees.FirstOrDefault(m => m.Id == id));
            EmployeeDto employeeDto = new EmployeeDto();
            employeeDto.Id = result.Id;
            employeeDto.FullName = result.FullName;
            employeeDto.Birthdate = result.Birthdate.ToString("yyyy-MM-dd");
            employeeDto.Tin = result.Tin;
            employeeDto.TypeId = result.EmployeeTypeId;

            return Ok(employeeDto);
        }

        /// <summary>
        /// Refactor this method to go through proper layers and update changes to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(EditEmployeeDto input)
        {
            var item = await Task.FromResult(_context.Employees.FirstOrDefault(m => m.Id == input.Id));
            if (item == null) return NotFound();
            item.FullName = input.FullName;
            item.Tin = input.Tin;
            item.Birthdate = input.Birthdate;
            item.EmployeeTypeId = input.TypeId;

            _context.SaveChanges();
            return Ok(item);
        }

        /// <summary>
        /// Refactor this method to go through proper layers and insert employees to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> Post(CreateEmployeeDto input)
        {
            Employee emp = new Employee();
            emp.FullName = input.FullName;
            emp.Birthdate = input.Birthdate;
            emp.Tin = input.Tin;
            emp.EmployeeTypeId = input.TypeId;
            //emp.IsDeleted == false;

            _context.Employees.Add(emp);
            _context.SaveChanges();

            var id = await Task.FromResult(_context.Employees.Max(m => m.Id) + 1);

            return Created($"/api/employees/{id}", id);
        }


        /// <summary>
        /// Refactor this method to go through proper layers and perform soft deletion of an employee to the DB.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await Task.FromResult(_context.Employees.FirstOrDefault(m => m.Id == id));
            if (result == null) return NotFound();

            result.IsDeleted = true;

            _context.SaveChanges();
            return Ok(id);
        }



        /// <summary>
        /// Refactor this method to go through proper layers and use Factory pattern
        /// </summary>
        /// <param name="id"></param>
        /// <param name="absentDays"></param>
        /// <param name="workedDays"></param>
        /// <returns></returns>
        [HttpPost("{id}/calculate")]
        public async Task<IActionResult> Calculate(Calculatedto cal)
        {
            var result = await Task.FromResult(_context.Employees.FirstOrDefault(m => m.Id == cal.Id));

            if (result == null) return NotFound();
            EmployeeFactory empFactory = new EmployeeFactory();
            IEmployee emp = empFactory.GetEmployeeType(result.EmployeeTypeId);

            var type = (EmployeeType)result.EmployeeTypeId;
            decimal salary = 0;

            if (type == EmployeeType.Regular)
            {
                salary = emp.CalculatePay(cal.absentDays);
            }
            else
            {
                salary = emp.CalculatePay(cal.workedDays);
            }

            return Ok(salary);
        }

    }
}
