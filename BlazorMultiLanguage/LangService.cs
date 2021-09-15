// --------------------------------
// blazorspread.net
// --------------------------------
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BlazorMultiLanguage
{
    public class LangService {
        public static string CurrentCulture { get; private set; }
        public static string[] Cultures { get; private set; }

        // to simplify reflexion
        static readonly Type _textResourcesType = typeof(TextResource);

        // language data
        static ImmutableDictionary<string, string> _textResources;

        // event for update each component when language changed
        public event Action OnChange;
        void NotifyStateChanged() => OnChange?.Invoke();

        // preserve user language
        const string STORAGESKEY = "CurrentCulture";

        // to manage localStorage
        readonly IJSRuntime _jsRuntime;

        public LangService(IJSRuntime jsRuntime) {
            _jsRuntime = jsRuntime;
        }

        public async Task LoadLanguageAsync(string lang = null) {
            if (lang == null) {
                // get storage user language
                var l = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", STORAGESKEY);
                lang = l ?? "EN"; // set default
            }
            if (CurrentCulture == lang) {
                return;
            }
            CurrentCulture = lang;
            try {
                // load from embedded resource
                var js = ResourceReader.Read("Languages.json");
                var items = JsonSerializer.Deserialize<IEnumerable<TextResource>>(js);
                var ls = new Dictionary<string, string>();
                foreach (var i in items) {
                    if (string.IsNullOrEmpty(i.Id)) {
                        continue;
                    }
                    ls.Add(i.Id, GetText(i, lang));
                }
                // let data
                _textResources = ls.ToImmutableDictionary();
                
                // for SetLanguagage
                GetLanguagesList(items.First());
                
                // save local storage
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", STORAGESKEY, lang);

                // notify
                NotifyStateChanged();
            } catch (Exception e) {
                Console.WriteLine("Exception: " + e.Message);
            }
        }

        /// <summary>
        /// Get available languages by reflexion
        /// </summary>
        /// <param name="textResource">The first or any item</param>
        static void GetLanguagesList(TextResource textResource) {
            if (Cultures is not null) {
                return;
            }
            var ls = new List<string>();
            foreach (var p in _textResourcesType.GetProperties()) {
                if (p.Name != "Id") {
                    if (string.IsNullOrEmpty(GetText(textResource, p.Name))) {
                        continue;
                    }
                    ls.Add(p.Name);
                }
            }
            Cultures = ls.ToArray();
        }

        /// <summary>
        /// Initialize dictionary with current lang, using reflexion
        /// </summary>
        static string GetText(TextResource src, string lang) {
            return _textResourcesType.GetProperty(lang).GetValue(src, null)?.ToString();
        }

        /// <summary>
        /// Get text from key, without method name 
        /// </summary>
        public virtual string this[string key]
        {
            get {
                if (_textResources.ContainsKey(key)) {
                    return _textResources[key];
                }
                return $"[{key}]"; // untranslate text
            }
        }
    }

    record TextResource
    (
        string Id, // Key
        string EN, // English
        string ES, // Spanish
        string PT, // Portuguese
        string RU, // Russian 
        string NO, // Norwegian
        string IT  // Italian
                   // ...
    );
}
