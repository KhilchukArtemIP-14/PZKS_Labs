using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Models
{
    public class ValidationResponse
    {
        public List<string> Errors { get; set; }
        public List<Token> Tokens { get; set; }
    }
}
