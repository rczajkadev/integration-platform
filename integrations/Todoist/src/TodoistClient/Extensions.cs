namespace Integrations.Todoist.TodoistClient;

internal static class Extensions
{
    extension(ITodoistApi api)
    {
        public async Task<IEnumerable<TodoistTask>> GetAllTasksByFilterAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            TodoistResponse? response = null;
            List<TodoistTask> tasks = [];

            do
            {
                response = await api.GetTasksByFilterAsync(query, response?.NextCursor, cancellationToken);
                tasks.AddRange(response.Results);
            } while (!string.IsNullOrWhiteSpace(response.NextCursor));

            return tasks;
        }

        public async Task<int> UpdateTasksAsync(
            IEnumerable<TodoistTask> tasks,
            Func<TodoistTask, TodoistUpdateTaskRequest> createRequestBody,
            int concurrentRequests = 5,
            CancellationToken cancellationToken = default)
        {
            using var semaphoreSlim = new SemaphoreSlim(concurrentRequests);
            var updateCounter = 0;

            IEnumerable<Task> apiCallTasks = tasks.Select(async task =>
            {
                await semaphoreSlim.WaitAsync(cancellationToken);

                try
                {
                    TodoistUpdateTaskRequest updateRequest = createRequestBody(task);
                    await api.UpdateTaskAsync(task.Id, updateRequest, cancellationToken);
                    Interlocked.Increment(ref updateCounter);
                }
                finally
                {
                    semaphoreSlim.Release();
                }
            });

            await Task.WhenAll(apiCallTasks);
            return updateCounter;
        }
    }
}
