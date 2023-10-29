using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using WordleClone.src;

namespace WordleClone.Pages
{
    public class IndexModel : PageModel
    {
        public List<Key> Keyboard { get; private set; }
        public bool GuessSubmitted { get; private set; }
        public int CurrentRow { get; private set; }
        public Word[] Words { get; private set; }
        public WordleResult Result { get; private set; }
        public bool Lost { get; private set; }

        private Wordle _wordle;
        private IStorage _storage;
        public IndexModel(IStorage storage, WordleFactory factory)
        {
            _storage = storage;
            _wordle = factory.Create();
            SetWordleData();
        }

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
            _storage.SetGuessResult(_wordle.GetCurrentRow(), result);

            if (result == WordleResult.Correct || result == WordleResult.IncorrectWord)
            {
                var colors = _wordle.GetCurrentColors();
                _storage.SetColors(_wordle.GetCurrentRow(), colors);
            }

            _storage.SetState(_wordle.GetCurrentRow(), _wordle.GetCurrentState());

            return RedirectToPage();
        }

        private void SetWordleData()
        {
            GuessSubmitted = _wordle.GuessSubmitted();
            CurrentRow = _wordle.GetCurrentRow();
            Words = _wordle.Words;
            Result = _wordle.GetCurrentResult();
            Lost = _wordle.Lost;
            var usedLetters = _wordle.GetWrongLetters();
            Keyboard = new List<Key>();
            "QWERTYUIOPASDFGHJKLZXCVBNM".ToList().ForEach(key => Keyboard.Add(new Key(key.ToString(),!usedLetters.Contains(key))));
            Keyboard.Insert(Keyboard.FindIndex(key => key.Value == "Z"), new Key("ENTER", true));
            Keyboard.Add(new Key("DELETE", true));

            if (_wordle.GetCurrentState() == WordleState.Guessing)
            {
                if (_wordle.AdvanceRow())
                {
                    var row = _wordle.GetCurrentRow();
                    _storage.SetWord(row, _wordle.GetCurrentWord());
                    _storage.SetState(row, _wordle.GetCurrentState());
                    _storage.SetState(row - 1, _wordle.Words[row - 1].State);
                }
                else
                {
                    var row = _wordle.GetCurrentRow();
                    _storage.SetState(row, _wordle.GetCurrentState());
                }
            }
        }
    }
}