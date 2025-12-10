using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZKS_Lab5.Models
{
    public record VliwConfig(
        int NumProcessors = 5,
        int NumBanks = 5,
        int TimeReadBank = 1,
        int TimeReadLeaf = 1,
        int TimeWrite = 1
    );
}
