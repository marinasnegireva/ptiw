namespace Ptiw.Host.EventSystem
{
    public class Reaction
    {
        public Type? JobReactTo { get; set; }
        public bool NeedChangesToHappenToFire { get; set; }
        public Action Action { get; set; }
        public Task Task { get; set; }
    }
}