namespace WordleClone.src
{
    public class WordleFactory
    {
        private IStorage _storage;
        private IWebHostEnvironment _environment;
        const int _rows = 5;
        const int _columns = 5;
        public WordleFactory(IStorage storage, IWebHostEnvironment environment) 
        { 
            _storage = storage;
            _environment = environment;
        }

        public Wordle Create()
        {
            var words = new Word[_rows];

            for (int i = 0; i < _rows; i++)
            {
                string? word;
                string[] colors;
                WordleState state;
                WordleResult result;

                if ((word = _storage.GetWord(i)) is null)
                {
                    word = string.Empty;
                }

                colors = GetColors(i);
                state = GetState(i);
                result = (WordleResult)_storage.GetGuessResult(i);

                words[i] = new Word(state, word, colors, result);
            }

            return new Wordle(words, _environment);
        }

        private string[] GetColors(int row)
        {
            string[] colors;

            if ((colors = _storage.GetColors(row)) is null)
            {
                colors = new string[_columns];

                for (int i = 0; i < _columns; i++)
                {
                    colors[i] = "white";
                }
            }

            return colors;
        }

        private WordleState GetState(int row)
        {
            if (_storage.GetState(row) == -1)
            {
                return row == 0 ? WordleState.Active : WordleState.Inactive;
            }
            else
            {
                return (WordleState)_storage.GetState(row);
            }
        }
    }
}
