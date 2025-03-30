using UnityEngine;

namespace EchoRL
{
    public class NameComponent : Component
    {
        public string Name;
        public NameComponent(string name)
        {
            Name = name;
        }
    }
}
