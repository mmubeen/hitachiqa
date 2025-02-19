using HitachiQA.Backend.API.Models;

namespace HitachiQA.Backend.API.Interfaces
{
    public interface ITestRunner
    {
        Task<TestRun> RunAsync(TestRun run);
    }
}