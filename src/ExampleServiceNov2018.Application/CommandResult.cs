namespace ExampleServiceNov2018.Application
{
    public abstract class CommandResult
    {
        public string AggregateId;
        
        public class Handled : CommandResult
        {
            public int Revision;            
        }
        
        //Rejected, //Failed
    }
}