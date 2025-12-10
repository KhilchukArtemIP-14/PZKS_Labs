using System;
using System.Collections.Generic;
using System.Linq;
using BuildingBlocks.ExpressionTree.Nodes;
using PZKS_Lab5.Models;

namespace PZKS_Lab5.Services
{
    public class VliwScheduler
    {
        private readonly VliwConfig _config;

        private VliwTaskGraph _graph;
        private VliwProcessor[] _processors;
        private VliwMemoryBank[] _banks;
        private int _systemBarrier = 0;

        public VliwScheduler(VliwConfig config)
        {
            _config = config;
            _processors = Enumerable.Range(0, _config.NumProcessors).Select(i => new VliwProcessor(i)).ToArray();
            _banks = Enumerable.Range(0, _config.NumBanks).Select(i => new VliwMemoryBank(i)).ToArray();
        }

        public void Process(ExpressionNode root)
        {
            if (root == null) return;

            Console.WriteLine("=== VLIW Scheduler ===");

            _graph = VliwGraphBuilder.Build(root);

            InitializeMemory();
            RunScheduling();

            VliwVisualizer.PrintAll(_graph, _config, _banks);
        }

        private void InitializeMemory()
        {
            var leaves = _graph.Leaves;
            for (int i = 0; i < leaves.Count; i++)
            {
                var task = leaves[i];
                var bankIndex = i % _config.NumBanks;

                task.ResultBankId = bankIndex;
                task.EndTime = 0;
                task.Id = -1 * (i + 1); 

                _banks[bankIndex].ContentCount++;
            }
        }

        private void RunScheduling()
        {
            var tasksToSchedule = _graph.Operations;
            _systemBarrier = 0;

            while (tasksToSchedule.Count > 0)
            {
                var readyTasks = tasksToSchedule
                    .Where(t => t.IsReady)
                    .OrderBy(t => t.Id)
                    .ToList();

                if (readyTasks.Count == 0) throw new Exception("Oops, something went wrong...");

                var instructionPacket = readyTasks.Take(_config.NumProcessors).ToList();

                int packetStartTime = Math.Max(_systemBarrier, GetPacketReadyTime(instructionPacket));
                int maxPacketEndTime = packetStartTime;

                var availableProcs = _processors.ToList();

                var sortedPacket = instructionPacket
                    .OrderByDescending(t => t.Parents.Count(p => !p.IsLeaf))
                    .ToList();

                foreach (var task in sortedPacket)
                {
                    var bestProc = FindBestProcessor(task, availableProcs);
                    availableProcs.Remove(bestProc);

                    ScheduleTask(task, bestProc, packetStartTime);

                    if (task.EndTime > maxPacketEndTime) maxPacketEndTime = task.EndTime;
                    tasksToSchedule.Remove(task);
                }

                _systemBarrier = maxPacketEndTime;
            }
        }

        private int GetPacketReadyTime(List<VliwTask> packet)
        {
            int maxTime = 0;
            foreach (var task in packet)
            {
                int taskReady = task.Parents.Where(p => !p.IsLeaf).Select(p => p.EndTime).DefaultIfEmpty(0).Max();
                if (taskReady > maxTime) maxTime = taskReady;
            }
            return maxTime;
        }

        private VliwProcessor FindBestProcessor(VliwTask task, List<VliwProcessor> available)
        {
            return available
                .OrderByDescending(p => task.Parents.Count(parent => p.LocalResults.Contains(parent.Id)))
                .ThenBy(p => p.Id)
                .First();
        }

        private void ScheduleTask(VliwTask task, VliwProcessor processor, int fixedStartTime)
        {
            task.ProcessorId = processor.Id;
            task.StartTime = fixedStartTime;

            var leavesToRead = task.Parents.Where(p => p.IsLeaf).ToList();
            var resultsToRead = task.Parents.Where(p => !p.IsLeaf && !processor.LocalResults.Contains(p.Id)).ToList();

            bool isMixedMode = task.Parents.Any(p => !p.IsLeaf && processor.LocalResults.Contains(p.Id));
            int durationForLeaves = isMixedMode ? 0 : _config.TimeReadLeaf;

            int actualReadStart = FindReadWindow(fixedStartTime, leavesToRead, resultsToRead, durationForLeaves);

            ReserveReadResources(actualReadStart, leavesToRead, resultsToRead, durationForLeaves);

            task.ReadStartTime = actualReadStart;
            task.CalcStartTime = CalculateCalcStart(actualReadStart, leavesToRead, resultsToRead, durationForLeaves);

            int endCalcTime = task.CalcStartTime + task.OperationDuration;

            task.WriteStartTime = endCalcTime;

            var targetBank = SelectBestBankForWrite(task.WriteStartTime, _config.TimeWrite);
            int writeActualStart = targetBank.FindFreeWindowForWrite(task.WriteStartTime, _config.TimeWrite);

            for (int t = 0; t < _config.TimeWrite; t++)
                targetBank.ReserveWrite(writeActualStart + t, "W");

            targetBank.ContentCount++;
            task.ResultBankId = targetBank.Id;
            task.EndTime = writeActualStart + _config.TimeWrite;

            processor.FreeTime = task.EndTime;
            processor.LocalResults.Add(task.Id);
        }

        private int FindReadWindow(int startTime, List<VliwTask> leaves, List<VliwTask> results, int leafDuration)
        {
            if (leaves.Count == 0 && results.Count == 0) return startTime;

            int t = startTime;
            while (true)
            {
                bool collision = false;
                int tempTime = t;

                if (leaves.Count > 0)
                {
                    foreach (var leaf in leaves)
                        if (!_banks[leaf.ResultBankId].CanRead(tempTime)) { collision = true; break; }
                    if (!collision) tempTime += leafDuration;
                }

                if (!collision && results.Count > 0)
                {
                    foreach (var res in results)
                    {
                        if (!_banks[res.ResultBankId].CanRead(tempTime)) { collision = true; break; }
                        tempTime += _config.TimeReadBank;
                    }
                }

                if (!collision) return t;
                t++;
            }
        }

        private void ReserveReadResources(int startTime, List<VliwTask> leaves, List<VliwTask> results, int leafDuration)
        {
            int reserveTime = startTime;

            if (leaves.Count > 0 && leafDuration > 0)
            {
                foreach (var leaf in leaves)
                    _banks[leaf.ResultBankId].ReserveRead(reserveTime, "Rl");
                reserveTime += leafDuration;
            }

            foreach (var res in results)
            {
                _banks[res.ResultBankId].ReserveRead(reserveTime, "Rr");
                reserveTime += _config.TimeReadBank;
            }
        }

        private int CalculateCalcStart(int readStartTime, List<VliwTask> leaves, List<VliwTask> results, int leafDuration)
        {
            int time = readStartTime;
            if (leaves.Count > 0) time += leafDuration;
            if (results.Count > 0) time += results.Count * _config.TimeReadBank;
            return time;
        }

        private VliwMemoryBank SelectBestBankForWrite(int readyTime, int duration)
        {
            VliwMemoryBank bestBank = _banks[0];
            int bestFreeTime = bestBank.FindFreeWindowForWrite(readyTime, duration);
            int minContent = bestBank.ContentCount;

            for (int i = 1; i < _config.NumBanks; i++)
            {
                var current = _banks[i];
                int currentFreeTime = current.FindFreeWindowForWrite(readyTime, duration);

                if (currentFreeTime < bestFreeTime)
                {
                    bestBank = current;
                    bestFreeTime = currentFreeTime;
                    minContent = current.ContentCount;
                }
                else if (currentFreeTime == bestFreeTime)
                {
                    if (current.ContentCount < minContent)
                    {
                        bestBank = current;
                        minContent = current.ContentCount;
                    }
                }
            }
            return bestBank;
        }
    }
}