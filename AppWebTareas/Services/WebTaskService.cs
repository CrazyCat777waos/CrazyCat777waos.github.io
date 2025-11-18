using System.Text.Json;
using Microsoft.JSInterop;
using AppWebTareas.Models;

namespace AppWebTareas.Services
{
    public class WebTaskService
    {
        private readonly IJSRuntime _jsRuntime;
        private List<TaskItem> _tasks = new();
        private readonly string _storageKey = "tareasapp_tasks";

        public WebTaskService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
            _ = LoadFromStorageAsync();
        }

        private async Task LoadFromStorageAsync()
        {
            try
            {
                var tasksJson = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", _storageKey);
                if (!string.IsNullOrEmpty(tasksJson))
                {
                    _tasks = JsonSerializer.Deserialize<List<TaskItem>>(tasksJson) ?? new List<TaskItem>();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tasks: {ex.Message}");
            }
        }

        private async Task SaveToStorageAsync()
        {
            try
            {
                var tasksJson = JsonSerializer.Serialize(_tasks);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", _storageKey, tasksJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving tasks: {ex.Message}");
            }
        }

        public async Task<List<TaskItem>> GetAllTasksAsync()
        {
            await LoadFromStorageAsync();
            return _tasks.OrderByDescending(t => t.CreatedAt).ToList();
        }

        public async Task<List<TaskItem>> GetPendingTasksAsync()
        {
            await LoadFromStorageAsync();
            return _tasks.Where(t => !t.IsCompleted)
                        .OrderBy(t => t.DueDate)
                        .ToList();
        }

        public async Task<TaskItem?> GetTaskAsync(int id)
        {
            await LoadFromStorageAsync();
            return _tasks.FirstOrDefault(t => t.Id == id);
        }

        public async Task AddTaskAsync(TaskItem task)
        {
            await LoadFromStorageAsync();
            task.Id = _tasks.Count > 0 ? _tasks.Max(t => t.Id) + 1 : 1;
            task.CreatedAt = DateTime.Now;
            task.UpdatedAt = DateTime.Now;
            _tasks.Add(task);
            await SaveToStorageAsync();
        }

        public async Task UpdateTaskAsync(TaskItem task)
        {
            await LoadFromStorageAsync();
            var existingTask = _tasks.FirstOrDefault(t => t.Id == task.Id);
            if (existingTask != null)
            {
                existingTask.Title = task.Title;
                existingTask.Description = task.Description;
                existingTask.DueDate = task.DueDate;
                existingTask.IsCompleted = task.IsCompleted;
                existingTask.ReminderTime = task.ReminderTime;
                existingTask.UpdatedAt = DateTime.Now;
                await SaveToStorageAsync();
            }
        }

        public async Task DeleteTaskAsync(int id)
        {
            await LoadFromStorageAsync();
            var task = _tasks.FirstOrDefault(t => t.Id == id);
            if (task != null)
            {
                _tasks.Remove(task);
                await SaveToStorageAsync();
            }
        }

        public async Task<List<TaskItem>> GetTasksDueSoonAsync()
        {
            await LoadFromStorageAsync();
            var soon = DateTime.Now.AddDays(1);
            return _tasks.Where(t => !t.IsCompleted && t.DueDate <= soon)
                        .OrderBy(t => t.DueDate)
                        .ToList();
        }
    }
}