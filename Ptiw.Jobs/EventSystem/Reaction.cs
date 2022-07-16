namespace Ptiw.Jobs.EventSystem
{
    public class Reaction
    {
        public string JobNameReactTo { get; set; }
        public bool NeedChangesToHappenToFire { get; set; }
        public Action Action { get; set; }
    }
}