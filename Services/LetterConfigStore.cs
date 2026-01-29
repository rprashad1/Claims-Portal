using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.Extensions.Hosting;

namespace ClaimsPortal.Services
{
    public class LetterRule
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Coverage { get; set; } = string.Empty;
        public string Claimant { get; set; } = string.Empty;
        public bool HasAttorney { get; set; }
        public string DocumentName { get; set; } = string.Empty;
        public string TemplateFile { get; set; } = string.Empty;
        public string MailTo { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }

    public class LetterConfigStore
    {
        private readonly string _filePath;
        private readonly object _sync = new();

        public LetterConfigStore(IHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "letterConfig.json");
            if (!File.Exists(_filePath))
            {
                File.WriteAllText(_filePath, "[]");
            }
        }

        public IReadOnlyList<LetterRule> GetAll()
        {
            lock (_sync)
            {
                var txt = File.ReadAllText(_filePath);
                return JsonSerializer.Deserialize<List<LetterRule>>(txt) ?? new List<LetterRule>();
            }
        }

        public void SaveAll(IEnumerable<LetterRule> rules)
        {
            lock (_sync)
            {
                var txt = JsonSerializer.Serialize(rules, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, txt);
            }
        }

        public void AddOrUpdate(LetterRule rule)
        {
            var list = GetAll().ToList();
            var existing = list.FirstOrDefault(r => r.Id == rule.Id);
            if (existing == null)
            {
                list.Add(rule);
            }
            else
            {
                list.Remove(existing);
                list.Add(rule);
            }
            SaveAll(list);
        }

        public void Delete(string id)
        {
            var list = GetAll().ToList();
            var existing = list.FirstOrDefault(r => r.Id == id);
            if (existing != null)
            {
                list.Remove(existing);
                SaveAll(list);
            }
        }
    }
}
