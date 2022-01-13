using System.Net.Http.Json;
using Shared.Models;
using Client.Static;

namespace Client.Services
{
    internal sealed class InMemoryDatabaseCache
    {
        private readonly HttpClient _httpclient;

        public InMemoryDatabaseCache(HttpClient httpClient)
        {
            _httpclient = httpClient;
        }

        private List<Category> _categories = null;
        internal List<Category> Categories 
        {
            get
            {
                return _categories;
            }

            set
            {
                _categories = value;
                NotifyCategoriesDataChanged();
            }
        }

        private bool _gettingCategoriesFromDatabaseAndCache = false;
        internal async Task GetCategoriesFromDatabaseAndCache()
        {
            //Only allow one Get request to run at a time
            if(_gettingCategoriesFromDatabaseAndCache == false)
            {
                _gettingCategoriesFromDatabaseAndCache = true;
                _categories = await _httpclient.GetFromJsonAsync<List<Category>>(ApiEndpoints.s_categories);
                _gettingCategoriesFromDatabaseAndCache = false;
            }
            
        }

        internal event Action OnCategoriesDataChanged;
        private void NotifyCategoriesDataChanged() => OnCategoriesDataChanged?.Invoke();
    }
}