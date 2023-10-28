namespace WordleClone.src
{
    public class Key
    {
        public string Value { get; private set; }
        public bool Active { get; private set; }
        public Key(string value, bool active) 
        {
            Value = value;
            Active = active;
        } 
    }
}
