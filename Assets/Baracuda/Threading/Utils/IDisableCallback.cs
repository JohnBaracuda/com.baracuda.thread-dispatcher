using System;

namespace Baracuda.Threading.Internal
{
    /// <summary>
    /// 
    /// </summary>
    /// <footer><a href="https://johnbaracuda.com/dispatcher.html#IDisableCallback">Documentation</a></footer>
    public interface IDisableCallback
    {
        event Action onDisable;
    }
}