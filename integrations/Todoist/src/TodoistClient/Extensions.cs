namespace Integrations.Todoist.TodoistClient;

internal static class Extensions
{
    extension(ITodoistApi api)
    {
        public async Task<IEnumerable<TodoistTask>> GetTasksAsync(
            IList<string> ids,
            CancellationToken cancellationToken = default)
        {
            TodoistResponse<TodoistTask>? response = null;
            List<TodoistTask> tasks = [];

            do
            {
                response = await api.GetTasksAsync(string.Join(",", ids), response?.NextCursor, cancellationToken);
                tasks.AddRange(response.Results);
            }
            while (!string.IsNullOrWhiteSpace(response.NextCursor));

            return tasks;
        }

        public async Task<IEnumerable<TodoistTask>> GetTasksAsync(
            CancellationToken cancellationToken = default)
        {
            TodoistResponse<TodoistTask>? response = null;
            List<TodoistTask> tasks = [];

            do
            {
                response = await api.GetTasksAsync(response?.NextCursor, cancellationToken);
                tasks.AddRange(response.Results);
            }
            while (!string.IsNullOrWhiteSpace(response.NextCursor));

            return tasks;
        }

        public async Task<IEnumerable<TodoistTask>> GetTasksByProjectAsync(
            string projectId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(projectId))
                return [];

            TodoistResponse<TodoistTask>? response = null;
            List<TodoistTask> tasks = [];

            do
            {
                response = await api.GetTasksByProjectAsync(projectId, response?.NextCursor, cancellationToken);
                tasks.AddRange(response.Results);
            }
            while (!string.IsNullOrWhiteSpace(response.NextCursor));

            return tasks;
        }

        public async Task<IEnumerable<TodoistTask>> GetTasksByFilterAsync(
            string query,
            CancellationToken cancellationToken = default)
        {
            TodoistResponse<TodoistTask>? response = null;
            List<TodoistTask> tasks = [];

            do
            {
                response = await api.GetTasksByFilterAsync(query, response?.NextCursor, cancellationToken);
                tasks.AddRange(response.Results);
            }
            while (!string.IsNullOrWhiteSpace(response.NextCursor));

            return tasks;
        }

        public async Task<int> UpdateTasksAsync(
            IEnumerable<TodoistTask> tasks,
            Func<TodoistTask, object> createRequestBody,
            int concurrentRequests = 5,
            CancellationToken cancellationToken = default)
        {
            using var semaphore = new SemaphoreSlim(concurrentRequests);
            var updateCounter = 0;

            var apiCallTasks = tasks.Select(async task =>
            {
                await semaphore.WaitAsync(cancellationToken);

                try
                {
                    var updateRequest = createRequestBody(task);
                    await api.UpdateTaskAsync(task.Id, updateRequest, cancellationToken);
                    Interlocked.Increment(ref updateCounter);
                }
                finally
                {
                    semaphore.Release();
                }
            });

            await Task.WhenAll(apiCallTasks);
            return updateCounter;
        }
    }

    extension(TodoistProject project)
    {
        public int CountTasks(IList<TodoistProject> allProjects, IList<TodoistTask> allTasks)
        {
            return project.GetSubprojects(allProjects)
                .Aggregate(
                    allTasks.Count(t => t.ProjectId == project.Id),
                    (counter, next) => counter + allTasks.Count(t => t.ProjectId == next.Id));
        }

        public IEnumerable<TodoistProject> GetSubprojects(IList<TodoistProject> allProjects)
        {
            var subprojects = allProjects.Where(p => p.ParentId == project.Id).ToList();

            if (subprojects.Count == 0) return subprojects;

            var subprojectsRecursive = subprojects.SelectMany(p => p.GetSubprojects(allProjects));
            return subprojects.Concat(subprojectsRecursive);
        }
    }
}
