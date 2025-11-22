using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace bookService.ExternalProviders
{
    public class OpenLibraryProvider : IExternalBookProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<OpenLibraryProvider> _logger;
        private const string BaseUrl = "https://openlibrary.org";

        public string ProviderName => "OpenLibrary";

        public OpenLibraryProvider(HttpClient httpClient, ILogger<OpenLibraryProvider> logger)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(BaseUrl);
            _logger = logger;
        }

        public async Task<IEnumerable<ExternalBookSearchDto>> SearchBooksAsync(string query, int limit = 10)
        {
            try
            {
                // Aumenta o limite para filtrar depois
                var response = await _httpClient.GetAsync($"/search.json?q={Uri.EscapeDataString(query)}&limit={limit * 3}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var searchResult = JsonSerializer.Deserialize<OpenLibrarySearchResponse>(content);

                if (searchResult?.Docs == null || !searchResult.Docs.Any())
                    return Enumerable.Empty<ExternalBookSearchDto>();

                // Filtra e ordena resultados por qualidade
                var results = searchResult.Docs
                    .Where(doc => 
                        !string.IsNullOrWhiteSpace(doc.Title) && 
                        doc.AuthorName != null && 
                        doc.AuthorName.Any())
                    .Select(doc => new
                    {
                        Book = new ExternalBookSearchDto
                        {
                            ExternalId = doc.Key?.Replace("/works/", "").Replace("/books/", "") ?? "",
                            Title = doc.Title ?? "Unknown Title",
                            Author = string.Join(", ", doc.AuthorName ?? new List<string> { "Unknown Author" }),
                            CoverUrl = doc.CoverI.HasValue 
                                ? $"https://covers.openlibrary.org/b/id/{doc.CoverI}-L.jpg" 
                                : null,
                            FirstPublishYear = doc.FirstPublishYear,
                            Provider = ProviderName,
                            EditionCount = doc.EditionCount,
                            Languages = doc.Language
                        },
                        // Pontuação de qualidade
                        Quality = (doc.AuthorName?.Any() == true ? 10 : 0) +
                                 (doc.CoverI.HasValue ? 5 : 0) +
                                 (doc.FirstPublishYear.HasValue ? 3 : 0) +
                                 (doc.Isbn?.Any() == true ? 2 : 0)
                    })
                    .OrderByDescending(x => x.Quality)
                    .Take(limit)
                    .Select(x => x.Book);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching books in Open Library");
                return Enumerable.Empty<ExternalBookSearchDto>();
            }
        }

        public async Task<IEnumerable<ExternalBookSearchDto>> SearchBooksByTitleAsync(string title, int limit = 10)
        {
            try
            {
                // Busca específica por título retorna resultados melhores
                var response = await _httpClient.GetAsync($"/search.json?title={Uri.EscapeDataString(title)}&limit={limit * 2}");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var searchResult = JsonSerializer.Deserialize<OpenLibrarySearchResponse>(content);

                if (searchResult?.Docs == null || !searchResult.Docs.Any())
                    return Enumerable.Empty<ExternalBookSearchDto>();

                var results = searchResult.Docs
                    .Where(doc => 
                        !string.IsNullOrWhiteSpace(doc.Title) && 
                        doc.AuthorName != null && 
                        doc.AuthorName.Any())
                    .Select(doc => new
                    {
                        Book = new ExternalBookSearchDto
                        {
                            ExternalId = doc.Key?.Replace("/works/", "").Replace("/books/", "") ?? "",
                            Title = doc.Title ?? "Unknown Title",
                            Author = string.Join(", ", doc.AuthorName ?? new List<string> { "Unknown Author" }),
                            CoverUrl = doc.CoverI.HasValue 
                                ? $"https://covers.openlibrary.org/b/id/{doc.CoverI}-L.jpg" 
                                : null,
                            FirstPublishYear = doc.FirstPublishYear,
                            Provider = ProviderName,
                            EditionCount = doc.EditionCount,
                            Languages = doc.Language
                        },
                        Quality = (doc.AuthorName?.Any() == true ? 10 : 0) +
                                 (doc.CoverI.HasValue ? 5 : 0) +
                                 (doc.FirstPublishYear.HasValue ? 3 : 0) +
                                 (doc.Isbn?.Any() == true ? 2 : 0) +
                                 (doc.EditionCount.HasValue ? doc.EditionCount.Value : 0)
                    })
                    .OrderByDescending(x => x.Quality)
                    .Take(limit)
                    .Select(x => x.Book);

                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching books by title in Open Library");
                return Enumerable.Empty<ExternalBookSearchDto>();
            }
        }

        public async Task<ExternalBookDetailDto?> GetBookByIsbnAsync(string isbn)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/isbn/{isbn}.json");
                
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                var book = JsonSerializer.Deserialize<OpenLibraryBook>(content);

                if (book == null)
                    return null;

                string? workId = book.Works?.FirstOrDefault()?.Key?.Replace("/works/", "");
                ExternalBookDetailDto? workDetails = null;
                
                if (!string.IsNullOrEmpty(workId))
                {
                    workDetails = await GetBookByIdAsync(workId);
                }

                return new ExternalBookDetailDto
                {
                    ExternalId = workId ?? book.Key?.Replace("/books/", "") ?? "",
                    Title = book.Title ?? workDetails?.Title ?? "Unknown Title",
                    Author = book.Authors?.FirstOrDefault()?.Name ?? workDetails?.Author ?? "Unknown Author",
                    Description = workDetails?.Description ?? "No description available",
                    CoverUrl = book.Covers?.FirstOrDefault() != null
                        ? $"https://covers.openlibrary.org/b/id/{book.Covers.First()}-L.jpg"
                        : workDetails?.CoverUrl,
                    PublishYear = book.PublishDate ?? workDetails?.PublishYear,
                    PageCount = book.NumberOfPages ?? workDetails?.PageCount,
                    Provider = ProviderName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting book by ISBN from Open Library");
                return null;
            }
        }

        public async Task<ExternalBookDetailDto?> GetBookByIdAsync(string id)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/works/{id}.json");
                
                if (!response.IsSuccessStatusCode)
                    return null;

                var content = await response.Content.ReadAsStringAsync();
                var work = JsonSerializer.Deserialize<OpenLibraryWork>(content);

                if (work == null)
                    return null;

                // Get author details
                string? authorName = "Unknown Author";
                if (work.Authors?.Any() == true)
                {
                    var authorKey = work.Authors.First().Author?.Key;
                    if (!string.IsNullOrEmpty(authorKey))
                    {
                        var authorResponse = await _httpClient.GetAsync($"{authorKey}.json");
                        if (authorResponse.IsSuccessStatusCode)
                        {
                            var authorContent = await authorResponse.Content.ReadAsStringAsync();
                            var author = JsonSerializer.Deserialize<OpenLibraryAuthor>(authorContent);
                            authorName = author?.Name ?? authorName;
                        }
                    }
                }

                // Extract description
                string? description = null;
                if (work.Description.HasValue)
                {
                    var desc = work.Description.Value;
                    if (desc.ValueKind == JsonValueKind.String)
                    {
                        description = desc.GetString();
                    }
                    else if (desc.ValueKind == JsonValueKind.Object)
                    {
                        if (desc.TryGetProperty("value", out var valueElement))
                        {
                            description = valueElement.GetString();
                        }
                    }
                }

                return new ExternalBookDetailDto
                {
                    ExternalId = id,
                    Title = work.Title ?? "Unknown Title",
                    Author = authorName,
                    Description = description ?? "No description available",
                    CoverUrl = work.Covers?.FirstOrDefault() != null
                        ? $"https://covers.openlibrary.org/b/id/{work.Covers.First()}-L.jpg"
                        : null,
                    PublishYear = work.FirstPublishDate,
                    Provider = ProviderName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting book by ID from Open Library");
                return null;
            }
        }

        // Internal models for Open Library API responses
        private class OpenLibrarySearchResponse
        {
            [JsonPropertyName("docs")]
            public List<OpenLibrarySearchDoc>? Docs { get; set; }
        }

        private class OpenLibrarySearchDoc
        {
            [JsonPropertyName("key")]
            public string? Key { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("author_name")]
            public List<string>? AuthorName { get; set; }

            [JsonPropertyName("cover_i")]
            public int? CoverI { get; set; }

            [JsonPropertyName("first_publish_year")]
            public int? FirstPublishYear { get; set; }

            [JsonPropertyName("isbn")]
            public List<string>? Isbn { get; set; }

            [JsonPropertyName("edition_count")]
            public int? EditionCount { get; set; }

            [JsonPropertyName("language")]
            public List<string>? Language { get; set; }
        }

        private class OpenLibraryBook
        {
            [JsonPropertyName("key")]
            public string? Key { get; set; }

            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("authors")]
            public List<OpenLibraryAuthorRef>? Authors { get; set; }

            [JsonPropertyName("covers")]
            public List<int>? Covers { get; set; }

            [JsonPropertyName("publish_date")]
            public string? PublishDate { get; set; }

            [JsonPropertyName("number_of_pages")]
            public int? NumberOfPages { get; set; }

            [JsonPropertyName("works")]
            public List<OpenLibraryWorkRef>? Works { get; set; }
        }

        private class OpenLibraryWork
        {
            [JsonPropertyName("title")]
            public string? Title { get; set; }

            [JsonPropertyName("authors")]
            public List<OpenLibraryAuthorRef>? Authors { get; set; }

            [JsonPropertyName("description")]
            public JsonElement? Description { get; set; }

            [JsonPropertyName("covers")]
            public List<int>? Covers { get; set; }

            [JsonPropertyName("first_publish_date")]
            public string? FirstPublishDate { get; set; }
        }

        private class OpenLibraryAuthorRef
        {
            [JsonPropertyName("author")]
            public OpenLibraryKey? Author { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }

        private class OpenLibraryWorkRef
        {
            [JsonPropertyName("key")]
            public string? Key { get; set; }
        }

        private class OpenLibraryKey
        {
            [JsonPropertyName("key")]
            public string? Key { get; set; }
        }

        private class OpenLibraryAuthor
        {
            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }
    }
}