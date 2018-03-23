/// <summary>
/// Contains the abstract class for a Constant Entry. 
/// Children: Real,Integer
/// </summary>

namespace Compiler
{
    public class ConstantEntry : TableEntry
    {
        public int offset;

        public override void printEntry()
        {
            throw new System.NotImplementedException();
        }
    }
}
