using System.Collections.Generic;
using System.Linq;

namespace PZKS_Lab5.Models
{
    public class VliwTaskGraph
    {
        public VliwTask Root { get; }
        public List<VliwTask> AllTasks { get; }

        public List<VliwTask> Operations => AllTasks
            .Where(t => !t.IsLeaf)
            .OrderBy(t => t.Id)
            .ToList();

        public List<VliwTask> Leaves => AllTasks
            .Where(t => t.IsLeaf)
            .OrderBy(t => t.Name)
            .ToList();

        public VliwTaskGraph(VliwTask root, List<VliwTask> allTasks)
        {
            Root = root;
            AllTasks = allTasks;
        }
    }
}