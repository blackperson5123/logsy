using System;

namespace TaskManager
{
    public enum Priority
    {
        Low,
        Medium,
        High
    }

    public class TaskItem
    {
        private static int _nextId = 1;

        public int Id { get; }
        public string Title { get; }
        public string Description { get; }
        public Priority Priority { get; }

        public TaskItem(string title, string description, Priority priority)
        {
            Id = _nextId++;
            Title = title;
            Description = description;
            Priority = priority;
        }

        public override string ToString()
        {
            return $"{Id,4} | {Title,-20} | {Priority,-6} | {Description}";
        }
    }
}