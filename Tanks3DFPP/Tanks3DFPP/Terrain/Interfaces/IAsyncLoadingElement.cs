using System;
using Tanks3DFPP.Utilities;

namespace Tanks3DFPP.Terrain.Interfaces
{
    public interface IAsyncLoadingElement
    {
        event EventHandler<ProgressEventArgs> Progressing;

        event EventHandler Finished;

        int TotalProgress { get; }
    }
}
