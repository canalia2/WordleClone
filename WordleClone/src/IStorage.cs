namespace WordleClone.src
{
    public interface IStorage
    {
        string? GetWord(int row);
        void SetWord(int row, string word);
        string[] GetColors(int row);
        void SetColors(int row, string[] colors);
        int GetGuessRow();
        int GetState(int row);
        void SetState(int row, WordleState state);
        int GetGuessResult(int row);
        void SetGuessResult(int row, WordleResult result);
        
    }
}
