using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PZKS_Lab5.Models
{
    public enum SlotState { Free, Read, Write }

    public class VliwMemoryBank
    {
        public int Id { get; private set; }
        public int ContentCount { get; set; } = 0;

        private Dictionary<int, SlotState> _slots = new Dictionary<int, SlotState>();
        private Dictionary<int, string> _events = new Dictionary<int, string>();

        public VliwMemoryBank(int id) { Id = id; }

        public bool CanRead(int time)
        {
            if (!_slots.ContainsKey(time)) return true;
            return _slots[time] == SlotState.Read;
        }

        public bool CanWrite(int time)
        {
            return !_slots.ContainsKey(time);
        }

        public void ReserveRead(int time, string dbg)
        {
            if (!CanRead(time)) throw new Exception($"Bank {Id} Read Collision at {time}");
            _slots[time] = SlotState.Read;
            if (!_events.ContainsKey(time)) _events[time] = dbg;
            else if (!_events[time].Contains(dbg)) _events[time] += "," + dbg;
        }

        public void ReserveWrite(int time, string dbg)
        {
            if (!CanWrite(time)) throw new Exception($"Bank {Id} Write Collision at {time}");
            _slots[time] = SlotState.Write;
            _events[time] = dbg;
        }

        public int FindFreeWindowForWrite(int earliestStart, int duration)
        {
            int t = earliestStart;
            while (true)
            {
                bool isFree = true;
                for (int i = 0; i < duration; i++)
                    if (!CanWrite(t + i)) { isFree = false; break; }
                if (isFree) return t;
                t++;
            }
        }

        public string GetEventString(int time)
        {
            if (!_slots.ContainsKey(time)) return ".  ";
            if (_slots[time] == SlotState.Write) return "W  ";
            return "R  ";
        }
    }
}
