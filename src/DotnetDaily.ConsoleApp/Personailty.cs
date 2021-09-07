using System;
using System.Collections.Generic;

namespace DotnetDaily
{
    public class Personality
    {
        public IEnumerable<string> Greetings { get; set; } = Array.Empty<string>();
    
        public IEnumerable<string> Nicknames { get; set; } = Array.Empty<string>();
    }
}
