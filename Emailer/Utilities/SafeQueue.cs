using System.Collections;

namespace Emailer.Utilities
{
    public class SafeQueue : Queue
    {
        public override object Dequeue()
        {
            if(Count > 0)
            {
                lock(this)
                {
                    if (Count > 0)
                        return base.Dequeue();
                }
            }
            return null;
        }
    }
}
