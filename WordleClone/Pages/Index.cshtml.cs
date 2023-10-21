using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics.Metrics;
using System.Text;

namespace WordleClone.Pages
{
    public class IndexModel : PageModel
    { 
        public string[] Words { get { return _words; } }
        public string[][] Colors { get { return _colors; } }
        public List<char> Keyboard { get; } = "QWERTYUIOPASDFGHJKLZXCVBNM".ToList();
        public int GuessSubmitted { get; private set; } = -1;
        public int CurrentRow { get; private set; }

        private readonly ISession _session;

        private string[] _words;
        private string[][] _colors;

        private string Word = "APRIL";
        public IndexModel(IHttpContextAccessor httpContextAccessor)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _words = Enumerable.Repeat(string.Empty, 5).ToArray();
            _colors = new string[5][];

            for (int i = 0; i < 5; i++)
            {
                _colors[i] = Enumerable.Repeat(string.Empty, 5).ToArray();
            }
        }

        public void OnGet()
        {
            SetWordleData();
        }

        public IActionResult OnPostAddLetter(char letter)
        {
            if (Solved())
                return RedirectToPage();

            var row = GetCurrentRow();
            var word = GetWord(row);
           
            if (word.Length < 5)
            {
                word += letter;
            }

            _session.SetString(row.ToString(), word);

            return RedirectToPage();
        }

        private int GetCurrentRow()
        {
            int i = 0;
            string? word;

            while ((word = _session.GetString(i.ToString())) is not null)
            {
                _words[i] = word;
                i++;
            }

            return Math.Max(0, i - 1);
        }

        public IActionResult OnPostDeleteLastLetter()
        {
            if (Solved())
                return RedirectToPage();

            var row = GetCurrentRow();
            var word = GetWord(row);

            if (word.Length > 0)
            {
                word = word.Substring(0, word.Length - 1);
            }

            _session.SetString(row.ToString(), word);

            return RedirectToPage();
        }

        public IActionResult OnPostSubmitGuess()
        {
            if (Solved())
                return RedirectToPage();

            var row = GetCurrentRow();
            var word = GetWord(row);            

            if (word.Length < Word.Length)
            {
                // handle submitting when invalid
            }

            var colors = GetColors(word);
            _session.SetString($"{row}c", colors.ToString());
            _session.SetString($"{row + 1}", string.Empty);
            _session.SetString($"{nameof(GuessSubmitted)}", row.ToString());

            return RedirectToPage();
        }

        private string GetColors(string word)
        {
            var colors = new StringBuilder();

            for (int i = 0; i < word.Length; i++)
            {
                var c = word[i];

                // all the letters for the word being guessed are distinct.
                // don't need to make additional check to make sure i'm doing yellow letters correctly.
                if (c == Word[i])
                {
                    colors.Append("green");
                }
                else if (c != Word[i] && Word.Contains(c))
                {
                    colors.Append("yellow");
                }
                else
                {
                    colors.Append("grey");
                }

                if (i <= Word.Length - 1)
                    colors.Append('-');
            }

            return colors.ToString();
        }

        public string[] GetRowColors(int row)
        {
            var colorCode = _session.GetString($"{row}c") ?? string.Empty;

            return colorCode.Split('-');
        }

        private string GetWord(int row)
        {
            return _session.GetString(row.ToString()) ?? string.Empty;
        }

        private void SetWordleData()
        {
            int i = 0;
            string? word;

            while ((word = _session.GetString(i.ToString())) is not null)
            {
                _words[i] = word;
                _colors[i] = GetRowColors(i);
                i++;
            }

            GuessSubmitted = int.TryParse(_session.GetString($"{nameof(GuessSubmitted)}"), out int value) ? value : -1;
            CurrentRow = i - 1;
            _session.Remove($"{nameof(GuessSubmitted)}");
        }

        private bool Solved()
        {
            var row = GetCurrentRow();
            return GetRowColors(row).All(color => color == "green");
        }
    }

}