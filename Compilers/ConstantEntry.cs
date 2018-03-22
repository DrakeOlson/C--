/// <summary>
/// Contains the abstract class for a Constant Entry. 
/// Children: Real,Integer
/// </summary>

namespace Compiler
{
    public abstract class ConstantEntry : TableEntry
    {
        public int offset;
    }
}
