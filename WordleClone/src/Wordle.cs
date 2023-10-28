
namespace WordleClone.src
{
    public class Wordle
    {
        public Word[] Words { get; private set; }
        string _word = "APRIL";
        IWebHostEnvironment _environment;

        public Wordle(Word[] words, IWebHostEnvironment environment) 
        {
            Words = words;
            _environment = environment;
        }

        public string GetCurrentWord()
        {
            var row = GetCurrentRow();
            return Words[row].GetWord();
        }

        public string[] GetCurrentColors()
        {
            var row = GetCurrentRow();
            return Words[row].GetColors();
        }
        public WordleState GetCurrentState()
        {
            var row = GetCurrentRow();
            return Words[row].State;
        }

        public WordleResult GetCurrentResult()
        {
            var row = GetCurrentRow();
            return Words[row].Result;
        }

        public string GetWrongLetters()
        {
            var row = GetCurrentRow();
            var used = new HashSet<string>();

            for (int i = 0; i <= row; i++)
            {
                var greyLetters = Words[i].GetLettersByColor(x => x == "grey");
                var goodLetters = Words[i].GetLettersByColor(x => x == "yellow" || x == "green");
                used.UnionWith(greyLetters.Select(x => x.Letter));

                foreach (var letter in  goodLetters)
                {
                    if (used.Contains(letter.Letter))
                    {
                        used.Remove(letter.Letter);
                    }
                }
            }

            return string.Join(string.Empty, used);
        }

        public bool GuessSubmitted()
        {
            var row = GetCurrentRow();
            return Words[row].State == WordleState.Guessing;
        }

        /// <summary>
        /// Game is finished if the current row has all green tiles or if the last row has no white tiles.
        /// meaning a guess has been submitted.
        /// </summary>
        /// <returns></returns>
        public bool Finished()
        {
            return Words.Any(word => word.State == WordleState.Solved) || Words.Last().State == WordleState.Complete;
        }

        public bool AddLetter(string letter)
        {
            var row = GetCurrentRow();
            return Words[row].AddLetter(letter);
        }

        public bool DeleteLetter()
        {
            var row = GetCurrentRow();
            return Words[row].DeleteLetter();
        }

        public async Task<WordleResult> GetGuessResult()
        {
            var row = GetCurrentRow();
            var result = await EvaluateGuess(row);
            Words[row].Result = result;

            if (result == WordleResult.IncorrectWord || result == WordleResult.Correct)
                Words[row].SetColors(_word);

            return result;
        }

        public async Task<WordleResult> EvaluateGuess(int row)
        {
            var word = Words[row].GetWord();
            Words[row].State = WordleState.Guessing;

            if (word.Length < 5)
                return WordleResult.IncorrectLength;

            for (int i = word.Length - 1; i >= 0; i--)
            {   
                if (word[i] != _word[i])
                {
                    bool IsWord = await IsWordAsync(word);

                    if (IsWord)
                    {
                        return WordleResult.IncorrectWord;
                    }
                    else
                    {
                        return WordleResult.NotAWord;
                    }
                }
            }

            return WordleResult.Correct;
        }

        public bool AdvanceRow()
        {
            var row = GetCurrentRow();
            var advanced = false;    

            if (Words[row].State == WordleState.Guessing)
            {
                if (Words[row].Result == WordleResult.Correct)
                {
                    Words[row].State = WordleState.Solved;
                }
                else if (Words[row].Result == WordleResult.NotAWord || Words[row].Result == WordleResult.IncorrectLength)
                {
                    Words[row].State = WordleState.Active;
                }
                else
                {
                    Words[row].State = WordleState.Complete;
                    if (++row < Words.Length)
                    {
                        Words[row].State = WordleState.Active;
                        advanced = true;
                    }
                }
            }
            return advanced;
        }

        private async Task<bool> IsWordAsync(string word)
        {
            var client = new HttpClient();
            string? apiUrl = $"https://api.dictionaryapi.dev/api/v2/entries/en/{word}";
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
            var isWord = true;

            HttpResponseMessage response = await client.GetAsync(apiUrl);

            if (!response.IsSuccessStatusCode)
            {
                string body = await response.Content.ReadAsStringAsync();

                // If the response contains results, the word is valid.
                if (body.Contains("No Definitions Found"))
                    isWord = false;
            }

            return isWord;
        }

        public int GetCurrentRow()
        {
            int currentRow = -1; 

            for (int row = 0; row < Words.Length; row++)
            {
                if (Words[row].State == WordleState.Active || Words[row].State == WordleState.Guessing || Words[row].State == WordleState.Solved)
                {
                    currentRow = row;
                    break;
                }  
            }

            if (currentRow == -1)
                throw new Exception("No active state");

            return currentRow;
        }
    }

    public class Tile
    {
        public string? Letter { get; set; }
        public string? Color { get; set; }
    }

    public enum WordleResult
    {
        Correct,
        NotAWord,
        IncorrectLength,
        IncorrectWord,
        None
    }

    public enum WordleState
    {
        Active,
        Complete,
        Inactive,
        Guessing,
        Solved
    }

    public class Word
    {
        public Tile this[int index]
        {
            get { return _letters[index]; }
            set { _letters[index] = value; }
        }
        public int Length { get { return _letters.Length; } }

        private Tile[] _letters;
        public WordleState State { get; set; }
        public WordleResult Result { get; set; }
        public Word(WordleState state, string letters, string[] colors, WordleResult result)
        {
            State = state;
            Result = result;
            InitWord(letters, colors);
        }

        public bool AddLetter(string letter)
        {
            int index;
            bool added = false;

            if ((index = _letters.Select(x => x.Letter).ToList().IndexOf(string.Empty)) != -1)
            {
                _letters[index].Letter = letter;
                added = true;
            }

            return added;
        }

        public bool DeleteLetter()
        {
            bool deleted = false;

            for (int i = _letters.Length - 1; i >= 0; i--)
            {
                if (_letters[i].Letter != string.Empty)
                {
                    _letters[i].Letter = string.Empty;
                    deleted = true;
                    break;
                }
            }

            return deleted;
        }

        public void SetColors(string targetWord)
        {
            Dictionary<char, int> letterCounts = targetWord
            .GroupBy(c => c)
            .ToDictionary(grp => grp.Key, grp => grp.Count());
            var guessedWord = GetWord();
            var colors = new string[guessedWord.Length];

            for (int i = 0; i < guessedWord.Length; i++)
            {
                if (targetWord[i] == guessedWord[i])
                {
                    _letters[i].Color = "green";

                    if (--letterCounts[guessedWord[i]] == 0)
                        letterCounts.Remove(guessedWord[i]);

                }
                else if (letterCounts.ContainsKey(guessedWord[i]))
                {
                    _letters[i].Color = "yellow";

                    if (--letterCounts[guessedWord[i]] == 0)
                        letterCounts.Remove(guessedWord[i]);
                }
                else
                {
                    _letters[i].Color = "grey";
                }
            }
        }

        public string GetWord()
        {
            return string.Join(string.Empty, _letters.Select(x => x.Letter));
        }

        public string[] GetColors()
        {
            return _letters.Select(x => x.Color).ToArray();
        }

        public IEnumerable<Tile> GetLettersByColor(params Func<string, bool>[] predicates)
        {
            return _letters.Where(letter => predicates.Any(predicate => predicate(letter.Color ?? string.Empty))).ToList();
        }

        private void InitWord(string letters, string[] colors)
        {
            _letters = new Tile[5];

            for (int i = 0; i < _letters.Length; i++)
            {
                _letters[i] = new Tile() 
                { 
                    Letter = letters.Length > i ? letters[i].ToString() : string.Empty, 
                    Color = colors.Length > i ? colors[i] : "white"
                };
            }
        }
    }
}
