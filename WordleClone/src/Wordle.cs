using NetSpell.SpellChecker;
using NetSpell.SpellChecker.Dictionary;
using Newtonsoft.Json;
using System;
using System.Globalization;

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
        public int GuessSubmitted()
        {
            var row = GetCurrentRow();
            return Words[row].State == WordleState.Guessing ? row : -1;
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

            if (result == WordleResult.IncorrectWord || result == WordleResult.Correct)
                Words[row].SetColors(_word);

            return result;
        }

        public async Task<WordleResult> EvaluateGuess(int row)
        {
            var word = Words[row].GetWord();
            Words[row].State = WordleState.Guessing;

            for (int i = word.Length - 1; i >= 0; i--)
            {
                if (word[i] == ' ')
                {
                    return WordleResult.IncorrectLength;
                }
                else if (word[i] != _word[i])
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

        public async Task<bool> AdvanceRow()
        {
            var row = GetCurrentRow();
            var advanced = false;    

            if (Words[row].State == WordleState.Guessing)
            {
                if (await GetGuessResult() == WordleResult.Correct)
                {
                    Words[row].State = WordleState.Solved;
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
            var isWord = true;

            HttpResponseMessage response = await client.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
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
                if (Words[row].State == WordleState.Active || Words[row].State == WordleState.Guessing)
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
        IncorrectWord
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
        public WordleState State { get;set; }

        public Word(WordleState state, string letters, string[] colors) 
        {
            State = state;
            InitWord(letters, colors);
        }

        public bool AddLetter(string letter) 
        {
            int index;
            bool success = false;

            if ((index = _letters.Select(x => x.Letter).ToList().IndexOf(string.Empty)) != -1)
            {
                _letters[index].Letter = letter;
                success = true;
            }

            return success;
        }

        public bool DeleteLetter()
        {
            bool success = false;

            for (int i = _letters.Length - 1; i >= 0; i--)
            {
                if (_letters[i].Letter != string.Empty)
                {
                    _letters[i].Letter = string.Empty;
                    success = true;
                    break;
                }
            }

            return success;
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
