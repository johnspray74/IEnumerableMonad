using ProgrammingParadigms;

namespace DomainAbstractions
{
    public class StartEvent
    {
        private IEvent output;

        public void Run()
        {
            output.Execute();
        }


    }
}
