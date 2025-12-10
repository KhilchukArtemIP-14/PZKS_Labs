using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZKS_Lab5.Models
{
    public class VliwProcessor
    {
        public int Id { get; private set; }
        public int FreeTime { get; set; } = 0;
        public HashSet<int> LocalResults { get; private set; } = new HashSet<int>();
        public VliwProcessor(int id) { Id = id; }
    }
}
