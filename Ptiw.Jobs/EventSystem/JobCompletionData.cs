namespace Ptiw.Jobs.EventSystem
{
    public class JobCompletionData
    {
        public string JobName { get; set; }
        public bool ChangesWereMade { get; set; } = false;
    }
}