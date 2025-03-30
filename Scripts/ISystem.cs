using System.Collections.Generic;
using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// System interface
    /// </summary>
    public interface ISystem
    {
        void Initialize();
        void Process(List<Entity> entities);
    }
}
