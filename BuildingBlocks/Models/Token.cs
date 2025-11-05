using BuildingBlocks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Models
{
    public class Token
    {
        public string Value { get; set; }
        public TokenType TokenType { get; set; }
    }
}
