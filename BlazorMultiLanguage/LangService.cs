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
        public static string CurrentLanguage { get; private set; }
        public static string[] Languages { get; private set; }

        // to simplify reflexion
        static readonly Type _textResourcesType = typeof(TextResource);

        // language data
        static ImmutableDictionary<string, string> _textResources;

        // event for update each component when language changed
        public event Action OnChange;
        void NotifyStateChanged() => OnChange?.Invoke();

        // preserve user language
        const string LSKEY = "AppLanguage";

        // to manage localStorage
        readonly IJSRuntime _jsRuntime;

        public LangService(IJSRuntime jsRuntime) {
            _jsRuntime = jsRuntime;
        }

        public async Task LoadLanguageAsync(string lang = null) {
            if (lang == null) {
                // get storage user language
                var l = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", LSKEY);
                lang = l ?? "EN"; // set default
            }
            if (CurrentLanguage == lang) {
                return;
            }
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
                // let statics
                _textResources = ls.ToImmutableDictionary();
                CurrentLanguage = lang;
                GetLanguagesList(items.First());

                // save local storage
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", LSKEY, lang);

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
            if (Languages is not null) {
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
            Languages = ls.ToArray();
            
            // temporary by example
            Console.WriteLine("Currrent Language   : {0}", CurrentLanguage);
            Console.WriteLine("Available Languages : {0}", string.Join(" ", Languages));
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

    class TextResource
    {
        public string Id { get; set; }
        public string EN { get; set; }
        public string ES { get; set; }
        public string PT { get; set; }
        public string RU { get; set; }
        public string NO { get; set; }
        public string IT { get; set; }
        // ...and so forth
    }
}
