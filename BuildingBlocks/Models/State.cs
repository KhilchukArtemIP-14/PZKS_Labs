using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BuildingBlocks.Models
{
    public enum State
    {
        START,
        END,
        OPERATION,
        VARIABLE_OR_FUNCTION,
        FUNCTION_OPENING_BRACKET,
        OPENING_BRACKET,
        CLOSING_BRACKET,
        CONSTANT,
        CONSTANT_DOT,
        CONSTANT_DOT_CONSTANT,
        ERR
    }
}
