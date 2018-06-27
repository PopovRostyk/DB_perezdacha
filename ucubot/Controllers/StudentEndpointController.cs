using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using ucubot.Model;
using Dapper;

namespace ucubot.Controllers
{
    [Route("api/[controller]")]
    public class StudentEndpointController : Controller
    {
        private readonly IConfiguration _configuration;

        public StudentEndpointController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<Student> ShowSignals()
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            using (var connection = new MySqlConnection(connectionString))
            {

                connection.Open();
                var res = connection.Query<Student>(
                        "select id as Id, first_name as FirstName, last_name as LastName, user_id as UserId from student")
                    .AsList();
                return res;

            }

          
        }

        [HttpGet("{id}")]
        public Student ShowStudent(long id)
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            using (var connection = new MySqlConnection(connectionString))
            {

                connection.Open();
                var res = connection.Query<Student>(
                    "select id as Id, first_name as FirstName, last_name as LastName, user_id as UserId from student where id=@Id",
                    new {Id = id}).ToList();
                connection.Close();

                if (res.Any())
                {
                    return res[0];
                }
                else
                {
                    return null;
                }
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent(Student student)
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            var connection = new MySqlConnection(connectionString);

            connection.Open();
            try
            {
                connection.Execute("INSERT INTO student (first_name, last_name, user_id) VALUES (@fn, @ln, @uid)",
                    new {fn = student.FirstName, ln = student.LastName, uid = student.UserId});
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                connection.Close();
                return StatusCode(409);
            }

            connection.Close();
            return Accepted();
        }

        [HttpPut]
        public async Task<IActionResult> UpdateStudent(Student student)
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            using (var connection = new MySqlConnection(connectionString))
            {

                connection.Open();
                try
                {
                    connection.Execute(
                        "update student SET first_name = @fn, last_name = @ln, user_id = @uid where id = @id;",
                        new {fn = student.FirstName, ln = student.LastName, uid = student.UserId, id = student.Id});
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            }

            return Accepted();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveStudent(long id)
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            var connection = new MySqlConnection(connectionString);

            connection.Open();

            try
            {
                connection.Execute("DELETE FROM student WHERE id = @id;", new {id = id});
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                connection.Close();
                return StatusCode(409);
            }

            connection.Close();
            return Accepted();
        }
    }
}