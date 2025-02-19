using HitachiQA.Backend.API.Interfaces;
using HitachiQA.Backend.API.Models;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace HitachiQA.Backend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RunController : ControllerBase
    {
        private readonly ITestRunner _testRunner;

        public RunController(ITestRunner testRunner)
        {
            _testRunner = testRunner;
        }
        // GET: api/<RunController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return ["Not implemented", "We'll eventually return you status/logs here"];
        }

        // GET api/<RunController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "Not implemented, We'll eventually return a status/log here";
        }

        // POST api/<RunController>
        [HttpPost]
        public async Task<TestRun> Post([FromBody] TestRun run)
        {
            return await _testRunner.RunAsync(run);
        }
    }
}
