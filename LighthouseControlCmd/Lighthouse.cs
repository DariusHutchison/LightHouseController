using System;
using System.Configuration;

namespace LighthouseControlCmd
{
    internal class Lighthouse
    {
        public string Name { get; private set; }
        public ulong Address { get; private set; }

        public bool PoweredOn { get; set; }

        public Lighthouse(string name, ulong address)
        {
            Name = name;
            Address = address;
        }

        public override bool Equals(object obj)
        {
            return obj is Lighthouse lighthouse &&
                   Name == lighthouse.Name &&
                   Address == lighthouse.Address;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Address);
        }
    }
}
