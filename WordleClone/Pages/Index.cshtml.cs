using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics.Metrics;
using System.Text;
using WordleClone.src;

namespace WordleClone.Pages
{
    public class IndexModel : PageModel
    {
        public List<char> Keyboard { get; } = "QWERTYUIOPASDFGHJKLZXCVBNM".ToList();
        public int GuessSubmitted { get; private set; } = -1;
        public int CurrentRow { get; private set; }
        public Word[] Words { get; private set; }

        private readonly ISession _session;

        private string[] _words;
        private string[][] _colors;

        private string Word = "APRIL";
        private Wordle _wordle;
        private IStorage _storage;
        public IndexModel(IStorage storage, WordleFactory factory)
        {
            _storage = storage;
            _wordle = factory.Create();
            //SetWordleData();
        }

        public async Task OnGet()
        {
            await SetWordleData();
        }

        //public async Task OnPageHandlerSelectedAsync(PageHandlerExecutingContext context)
        //{
        //    base.OnPageHandlerExecuting(context);

        //    await SetWordleData();
        //}


        public IActionResult OnPostAddLetter(string letter)
        {
            if (_wordle.Finished())
                return RedirectToPage();

            if (_wordle.AddLetter(letter))
            {
                var word = _wordle.GetCurrentWord();
                _storage.SetWord(_wordle.GetCurrentRow(), word);
                return RedirectToPage();
            }

            return Page();
        }

        public IActionResult OnPostDeleteLastLetter()
        {
            if (_wordle.Finished())
                return RedirectToPage();

            if (_wordle.DeleteLetter())
            {
                var word = _wordle.GetCurrentWord();
                _storage.SetWord(_wordle.GetCurrentRow(), word);
                return RedirectToPage();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostSubmitGuess()
        {
            if (_wordle.Finished())
                return RedirectToPage();

            var result = await _wordle.GetGuessResult();

            if (result == WordleResult.Correct || result == WordleResult.IncorrectWord)
            {
                var colors = _wordle.GetCurrentColors();
                _storage.SetState(_wordle.GetCurrentRow(), _wordle.GetCurrentState());
                _storage.SetColors(_wordle.GetCurrentRow(), colors);
            }

            return RedirectToPage();
        }

        public string[] GetRowColors(int row)
        {
            var colorCode = _session.GetString($"{row}c") ?? string.Empty;

            return colorCode.Split('-');
        }

        private async Task SetWordleData()
        {
            GuessSubmitted = _wordle.GuessSubmitted();
            CurrentRow = _wordle.GetCurrentRow();
            Words = _wordle.Words;

            if (_wordle.GetCurrentState() == WordleState.Guessing)
            {
                if (await _wordle.AdvanceRow())
                {
                    var row = _wordle.GetCurrentRow();
                    _storage.SetWord(row, _wordle.GetCurrentWord());
                    _storage.SetState(row, _wordle.GetCurrentState());
                    _storage.SetState(row - 1, _wordle.Words[row - 1].State);
                }
            }
        }
    }
}