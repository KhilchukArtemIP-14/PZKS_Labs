using BuildingBlocks.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZKS_Lab5.Models
{
    public class VliwTask
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public TokenType Type { get; set; }
        public int OperationDuration { get; set; }

        public List<VliwTask> Parents { get; set; } = new List<VliwTask>();
        public List<VliwTask> Children { get; set; } = new List<VliwTask>();

        public int StartTime { get; set; } = -1;
        public int ReadStartTime { get; set; } = -1;
        public int CalcStartTime { get; set; } = -1;
        public int WriteStartTime { get; set; } = -1;
        public int EndTime { get; set; } = -1;

        public int ProcessorId { get; set; } = -1;
        public int ResultBankId { get; set; } = -1;

        public bool IsLeaf => Parents.Count == 0;
        public bool IsScheduled => EndTime != -1;
        public bool IsReady => Parents.All(p => p.IsLeaf || p.IsScheduled);

        public override string ToString() => IsLeaf ? Name : $"Op {Id} ({Name})";
    }
}
