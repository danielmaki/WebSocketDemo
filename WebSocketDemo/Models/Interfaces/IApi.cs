using System;

namespace WebSocketDemo.Models
{
    public interface IApi
    {
        public string Name { get; }

        public Uri Endpoint { get; }
    }
}