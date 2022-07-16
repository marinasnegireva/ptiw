namespace Ptiw.Host.EventSystem
{
    public class JobCompletionData
    {
        public Type Job { get; set; }
        public bool ChangesWereMade { get; set; } = false;
    }
}