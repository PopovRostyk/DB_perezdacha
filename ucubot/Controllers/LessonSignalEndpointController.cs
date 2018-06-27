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
    public class LessonSignalEndpointController : Controller
    {
        private readonly IConfiguration _configuration;

        public LessonSignalEndpointController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IEnumerable<LessonSignalDto> ShowSignals()
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var res = connection.Query<LessonSignalDto>(
                        "select lesson_signal.id as Id, lesson_signal.timestamp as TimeStamp, lesson_signal.signal_type as Type, student.user_id as UserId from lesson_signal join (student) on (lesson_signal.student_id = student.id);")
                    .AsList();

                return res;
            }
        }

        [HttpGet("{id}")]
        public LessonSignalDto ShowSignal(long id)
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            using (var connection = new MySqlConnection(connectionString))
            {

                connection.Open();
                var res = connection.Query<LessonSignalDto>(
                    "select lesson_signal.id as Id, lesson_signal.timestamp as TimeStamp, lesson_signal.signal_type as Type, student.user_id as UserId from lesson_signal join (student) on (lesson_signal.student_id = student.id) where lesson_signal.id=@id;",
                    new {id = id}).ToList();
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
        public async Task<IActionResult> CreateSignal(SlackMessage message)
        {
            var userId = message.user_id;
            var signalType = message.text.ConvertSlackMessageToSignalType();

            var connectionString = _configuration.GetConnectionString("BotDatabase");
            var connection = new MySqlConnection(connectionString);

            connection.Open();
            var stds = connection.Query<Student>(
                "select id as Id, first_name as FirstName, last_name as LastName, user_id as UserId from student where user_id=@uId",
                new {uId = userId}).AsList();

            if (!stds.Any())
            {
                connection.Close();
                return BadRequest();
            }

            connection.Execute("INSERT INTO lesson_signal (student_id, signal_type) VALUES (@std, @st)",
                new {std = stds[0].Id, st = signalType});

            connection.Close();
            return Accepted();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RemoveSignal(long id)
        {
            var connectionString = _configuration.GetConnectionString("BotDatabase");
            using (
                var connection = new MySqlConnection(connectionString))
            {

                connection.Open();
                try
                {
                    connection.Execute("DELETE FROM lesson_signal WHERE id = @id;", new {id = id});
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return BadRequest();
                }

            }

            return Accepted();
        }
    }
}