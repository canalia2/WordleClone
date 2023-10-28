using Microsoft.AspNetCore.Http;
using System.Diagnostics.Metrics;

namespace WordleClone.src
{
    public class SessionStorage : IStorage
    {
        private readonly ISession _session;

        public SessionStorage(IHttpContextAccessor httpContextAccessor)
        {
            _session = httpContextAccessor.HttpContext.Session;
        }
        public string[] GetColors(int row)
        {
            var colorCode = _session.GetString($"{row}c") ;
            return colorCode?.Split('-');
        }

        public string? GetWord(int row)
        {
            return _session.GetString(row.ToString());
        }

        public void SetWord(int row, string word)
        {
            _session.SetString(row.ToString(), word);
        }

        public void SetColors(int row, string[] colors)
        {
            _session.SetString($"{row}c", string.Join('-', colors));
            _session.SetString($"{row + 1}", string.Empty);
            _session.SetInt32($"Guess", row);
        }

        public int GetState(int row)
        {
            return _session.GetInt32($"{row}s") ?? -1;
        }

        public void SetState(int row, WordleState state)
        {
            _session.SetInt32($"{row}s", (int)state);
        }
        public int GetGuessRow()
        {
            return _session.GetInt32("Guess") ?? -1;
        }
        public int GetGuessResult(int row)
        {
            return _session.GetInt32($"{row}gr") ?? (int)WordleResult.None;
        }

        public void SetGuessResult(int row, WordleResult result)
        {
            _session.SetInt32($"{row}gr", (int)result);
        }
    }
}
