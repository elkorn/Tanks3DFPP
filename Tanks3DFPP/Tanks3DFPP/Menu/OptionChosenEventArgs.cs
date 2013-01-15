using System;

namespace Tanks3DFPP.Menu
{
    internal class OptionChosenEventArgs: EventArgs
    {
        public OptionChosenEventArgs(int optionCode)
        {
            this.Code = optionCode;
        }

        public int Code { get; private set; }
    }
}
