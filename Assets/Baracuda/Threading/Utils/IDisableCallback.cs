using System;

namespace Baracuda.Threading.Internal
{
    public interface IDisableCallback
    {
        event Action onDisable;
    }
}