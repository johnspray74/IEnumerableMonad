namespace ProgrammingParadigms
{
    // The motivation interface needs a little explaining.
    // It is used when you have a dataflow programming paradigm that is uses pull.
    // For such as aprogramming paradigm, imagine we have two objects A, and B, which need to be wired together.
    // A is the data source and B is the data destination, so the direction of dataflow is A to B.
    // But when A and B are implemented, B will have a field of the interface and A will implement the interface.
    // So the WireTo or WireIn methods would need to be used backwards, against the direction of the flow of the data.
    // While this would work, it would be terribly confusing. 
    // And in the case of WireIn which can be chained, you would start at the destination end and chain your domain abstractions back to the source, which just seems unnatural.

    // So what we do to solve this is put in another set of ports in the opposite direction, which WireTo and WireIn can use in the same direction as the dataflow.
    // These ports use this interface.
    // if you call the port field in A wireForward, then you also add a private method called wireForwardInitialize.
    // After the wireForward port is wired, the WireTo method looks for a mthod with the same name as the port, but with an "Initialize" suffix.
    // If it finds such a method, it calls it. 
    // So what you do in this methid is:
    // wireForward(this);
    // That sends the A object over to the B object.
    // The B object implements WireForward like this
    // WireForward.Push(object o) { dataflowPort = o; }

    public interface IWireForward
    {
        void Push(object o);
    }
}
