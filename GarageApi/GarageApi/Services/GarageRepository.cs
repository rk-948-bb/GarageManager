using GarageApi.Models;
using MongoDB.Driver;

namespace GarageApi.Services
{
    public class GarageRepository
    {
        private readonly IMongoCollection<Garage> _collection;
        private readonly HttpClient _http;

        public GarageRepository(IMongoCollection<Garage> collection, HttpClient http)
        {
            _collection = collection;
            _http = http;
        }

        public async Task<List<Garage>> GetAllAsync(CancellationToken ct = default)
        {
            var res = await _collection.Find(_ => true).ToListAsync(ct);
            return res;
        }

        public async Task<List<Garage>> FetchFromGovApiAsync(int limit = 5, CancellationToken ct = default)
        {
            var url = $"https://data.gov.il/api/3/action/datastore_search?resource_id=bb68386a-a331-4bbc-b668-bba2766d517d&limit={limit}";
            using var resp = await _http.GetAsync(url, ct);
            resp.EnsureSuccessStatusCode();
            using var stream = await resp.Content.ReadAsStreamAsync(ct);
            using var doc = await System.Text.Json.JsonDocument.ParseAsync(stream, cancellationToken: ct);

            if (!doc.RootElement.TryGetProperty("result", out var result) || !result.TryGetProperty("records", out var records))
                return new List<Garage>();

            var list = new List<Garage>();
            foreach (var rec in records.EnumerateArray())
            {
                // Map using exact field names from gov API
                string? externalId = TryGetString(rec, "mispar_mosah", "_id", "id");
                string? name = TryGetString(rec, "shem_mosah", "שם", "name");
                string? addr = TryGetString(rec, "ktovet", "כתובת", "address");
                string? telephone = TryGetString(rec, "telephone", "טלפון");
                string? mikud = TryGetString(rec, "mikud");

                list.Add(new Garage
                {
                    ExternalId = externalId,
                    Name = name,
                    Address = addr,
                    Telephone = telephone,
                    Mikud = mikud
                });
            }

            return list;
        }

        private static string? TryGetString(System.Text.Json.JsonElement el, params string[] names)
        {
            foreach (var n in names)
            {
                if (el.TryGetProperty(n, out var v) && v.ValueKind != System.Text.Json.JsonValueKind.Null)
                {
                    // Handle different JSON token types safely
                    switch (v.ValueKind)
                    {
                        case System.Text.Json.JsonValueKind.String:
                            return v.GetString();
                        case System.Text.Json.JsonValueKind.Number:
                            // Return the numeric value as string
                            return v.GetRawText();
                        case System.Text.Json.JsonValueKind.True:
                        case System.Text.Json.JsonValueKind.False:
                            return v.GetBoolean().ToString();
                        default:
                            // Fallback to raw text for objects/arrays or other kinds
                            return v.GetRawText();
                    }
                }
            }
            return null;
        }

        public async Task<List<Garage>> AddBulkIfNotExistAsync(IEnumerable<Garage> garages, CancellationToken ct = default)
        {
            // Normalize input (trim) to avoid mismatches
            var normalized = garages.Select(g => {
                g.ExternalId = g.ExternalId?.Trim();
                g.Name = g.Name?.Trim();
                return g;
            }).ToList();

            var incoming = normalized.Where(g => !string.IsNullOrEmpty(g.ExternalId)).ToList();
            var incomingIds = incoming.Select(g => g.ExternalId!).Distinct().ToList();

            var filter = Builders<Garage>.Filter.In(g => g.ExternalId, incomingIds);
            var existing = incomingIds.Count > 0 ? await _collection.Find(filter).Project(g => g.ExternalId).ToListAsync(ct) : new List<string>();
            var existingSet = new HashSet<string>(existing.Where(x => x != null)!);

            // Keep only items that are not in DB yet
            var toInsert = normalized.Where(g => string.IsNullOrEmpty(g.ExternalId) || !existingSet.Contains(g.ExternalId!)).ToList();

            if (toInsert.Count == 0) return new List<Garage>();

            // Deduplicate toInsert by ExternalId (if present) or by Name for items without ExternalId
            var deduped = toInsert
                .GroupBy(g => string.IsNullOrEmpty(g.ExternalId) ? $"__name__:{g.Name ?? string.Empty}" : $"__id__:{g.ExternalId}")
                .Select(grp => grp.First())
                .ToList();

            if (deduped.Count == 0) return new List<Garage>();

            var options = new InsertManyOptions { IsOrdered = false };
            try
            {
                await _collection.InsertManyAsync(deduped, options, ct);
            }
            catch (MongoBulkWriteException<Garage> ex)
            {
                // Ignore duplicate-key write errors, rethrow others
                if (ex.WriteErrors != null && ex.WriteErrors.Any(e => e.Category != ServerErrorCategory.DuplicateKey))
                    throw;
            }

            // Build filter to fetch the saved documents (by ExternalId and/or Name)
            var externalIdsToQuery = deduped.Where(d => !string.IsNullOrEmpty(d.ExternalId)).Select(d => d.ExternalId!).Distinct().ToList();
            var namesToQuery = deduped.Where(d => string.IsNullOrEmpty(d.ExternalId) && !string.IsNullOrEmpty(d.Name)).Select(d => d.Name!).Distinct().ToList();

            var filters = new List<FilterDefinition<Garage>>();
            if (externalIdsToQuery.Count > 0)
                filters.Add(Builders<Garage>.Filter.In(g => g.ExternalId, externalIdsToQuery));
            if (namesToQuery.Count > 0)
                filters.Add(Builders<Garage>.Filter.In(g => g.Name, namesToQuery));

            List<Garage> insertedDocs;
            if (filters.Count == 0)
            {
                // fallback: return what we attempted to insert (may not have Ids)
                insertedDocs = deduped;
            }
            else
            {
                var combinedFilter = filters.Count == 1 ? filters[0] : Builders<Garage>.Filter.Or(filters);
                insertedDocs = await _collection.Find(combinedFilter).ToListAsync(ct);
            }

            return insertedDocs;
        }

        public async Task<Garage?> AddAsync(Garage g, CancellationToken ct = default)
        {
            // check by ExternalId or Name
            if (!string.IsNullOrEmpty(g.ExternalId))
            {
                var exists = await _collection.Find(x => x.ExternalId == g.ExternalId).AnyAsync(ct);
                if (exists) return null;
            }

            if (!string.IsNullOrEmpty(g.Name))
            {
                var existsByName = await _collection.Find(x => x.Name == g.Name).AnyAsync(ct);
                if (existsByName) return null;
            }

            await _collection.InsertOneAsync(g, cancellationToken: ct);
            return g;
        }
    }
}
