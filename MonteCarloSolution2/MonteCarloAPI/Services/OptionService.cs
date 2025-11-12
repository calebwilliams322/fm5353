using System.Collections.Concurrent;
using MonteCarloAPI.Models;

namespace MonteCarloAPI.Services
{
    public class OptionService
    {
        private readonly ConcurrentDictionary<int, OptionConfigDTO> _options = new();
        private int _nextId = 1;

        public Task<OptionConfigDTO> AddOptionAsync(OptionConfigDTO option)
        {
            option.Id = _nextId++;
            option.CreatedAt = DateTime.UtcNow;
            _options[option.Id] = option;
            return Task.FromResult(option);
        }

        public Task<OptionConfigDTO?> GetOptionByIdAsync(int id)
        {
            _options.TryGetValue(id, out var option);
            return Task.FromResult(option);
        }

        public Task<List<OptionConfigDTO>> GetAllOptionsAsync()
        {
            var list = _options.Values.OrderBy(o => o.Id).ToList();
            return Task.FromResult(list);
        }

        public Task<OptionConfigDTO?> UpdateOptionAsync(int id, OptionConfigDTO updated)
        {
            if (!_options.TryGetValue(id, out var existing))
                return Task.FromResult<OptionConfigDTO?>(null);

            // Update the option parameters
            existing.OptionParameters = updated.OptionParameters;
            existing.CreatedAt = DateTime.UtcNow; // Update timestamp

            _options[id] = existing;
            return Task.FromResult<OptionConfigDTO?>(existing);
        }

        public Task<bool> DeleteOptionAsync(int id)
        {
            var removed = _options.TryRemove(id, out _);
            return Task.FromResult(removed);
        }
    }
}
