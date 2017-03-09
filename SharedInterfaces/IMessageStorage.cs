using System;

namespace SharedInterfaces
{
    public interface IClosable
    {
        bool IsClosed { get; set; }
    }
}