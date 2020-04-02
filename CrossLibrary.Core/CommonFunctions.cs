﻿using CrossLibrary.Interfaces;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace CrossLibrary {
    /// <summary>
    /// Static functions that are crossplatform
    /// </summary>
    public static class CommonFunctions {

        /// <summary>
        /// This is just here so there doesn't need to be a bunch of ugly dependency injection calls through out the code
        /// </summary>
        public static ICrossFunctions CrossFunctions = DependencyService.Get<ICrossFunctions>(DependencyFetchTarget.GlobalInstance);


        /// <summary>
        /// Used to attach a view model to a view that was created from platform code.
        /// Should be called from Views constructor.
        /// Throws an exception if view already has a view model.
        /// View must use the same type of ViewModel
        /// </summary>
        /// <typeparam name="TViewModel1"></typeparam>
        /// <typeparam name="TViewModel2"></typeparam>
        /// <param name="crossView"></param>
        /// <param name="crossViewModel"></param>
        public static void AttachViewModel<TViewModel>(this ICrossView<TViewModel> crossView,
                                                                     TViewModel crossViewModel)
                                                                       where TViewModel : CrossViewModel {
            if (crossViewModel.HasCrossView) {
                throw new Exception("View model already has viewmodel attached");
            }
            crossViewModel.crossView = crossView;
            crossView.Prepare(crossViewModel);
            
        }



        public static IEnumerable<TSource> RecursiveSelect<TSource>(
                this IEnumerable<TSource> source,
                Func<TSource, IEnumerable<TSource>> childSelector) {

            var stack = new Stack<IEnumerator<TSource>>();
            var enumerator = source.GetEnumerator();

            try {
                while (true) {
                    if (enumerator.MoveNext()) {
                        TSource element = enumerator.Current;
                        yield return element;

                        stack.Push(enumerator);
                        enumerator = childSelector(element).GetEnumerator();
                    } else if (stack.Count > 0) {
                        enumerator.Dispose();
                        enumerator = stack.Pop();
                    } else {
                        yield break;
                    }
                }
            } finally {
                enumerator.Dispose();

                while (stack.Count > 0) // Clean up in case of an exception.
                {
                    enumerator = stack.Pop();
                    enumerator.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets the 
        /// </summary>
        /// <returns></returns>
        public static CultureInfo GetDefaultCulture() {
            return CrossFunctions.GetDefaultCulture();
        }


        /// <summary>
        /// Get's a path in the apps private folder
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetLocalDatabaseFilePath(string path) {
            return CrossFunctions.GetLocalDatabaseFilePath(path);
        }

        /// <summary>
        /// Gets the fully qualified name for the app bundle
        /// </summary>
        /// <returns></returns>
        public static string GetBundleName() {
            return CrossFunctions.GetBundleName();
        }

        /// <summary>
        /// Goes to the top page - top activity or viewcontroller
        /// </summary>
        public static void GoToTopPage() {
            CrossFunctions.GoToTopPage();
        }




        private static readonly Random staticRandom = new Random();
        /// <summary>
        /// Shuffles an array using a <see cref="Random"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = staticRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }


        /// <summary>
        /// Fills an array with a single value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="value"></param>
        public static void Populate<T>(this T[] array, T value) {
            for (int i = 0; i < array.Length; i++) {
                array[i] = value;
            }
        }

        /// <summary>
        /// Fills an list with a single value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="count"></param>
        /// <param name="value"></param>
        public static List<T> PopulateList<T>(int count, T value) {
            var list = new List<T>();
            for (int i = 0; i < count; i++) {
                list.Add(value);
            }
            return list;
        }

        /// <summary>
        /// Replaces all occurrences of a value in an array with another value
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="originalValue"></param>
        /// <param name="newValue"></param>
        public static void Replace<T>(this T[] array, T originalValue, T newValue) where T : IEquatable<T> {
            for (int i = 0; i < array.Length; i++) {
                if (array[i].Equals(originalValue)) {
                    array[i] = newValue;
                }
            }
        }

        /// <summary>
        /// Swaps items in a list at index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="firstPosition"></param>
        /// <param name="secondPositition"></param>
        public static void Swap<T>(this IList<T> list, int firstPosition, int secondPositition) {
            var value = list[firstPosition];
            list[firstPosition] = list[secondPositition];
            list[secondPositition] = value;
        }


        /// <summary>
        /// <see langword="true"/> if any character falls in the range of hiragana, katakana or kanji.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static bool ContainsJapaneseChars(this string text) {
            return text.ToCharArray().Any(c => c.IsJapanese());
        }

        /// <summary>
        /// <see langword="true"/> if the character falls in the range of hiragana, katakana or kanji.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsJapanese(this char c) {
            return c.IsHiragana() || c.IsKatakana() || c.IsKanji();
        }

        /// <summary>
        /// <see langword="true"/> if the character falls in the range of hiragana.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsHiragana(this char c) {
            return c >= 0x3040 && c <= 0x309F;
        }


        /// <summary>
        /// <see langword="true"/> if the character falls in the range of katakana.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsKatakana(this char c) {
            return c >= 0x30A0 && c <= 0x30FF;
        }

        /// <summary>
        ///  <see langword="true"/> if the character falls in the range of kanji.
        /// Not sure how accurate this is.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool IsKanji(this char c) {
            return c >= 0x4E00 && c <= 0x9FBF;
        }


        /// <summary>
        /// Finds the longest word in a string
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string GetLongestWord(this string text) {
            return text.Split().Aggregate(string.Empty, (max, current) => max.Length > current.Length ? max : current);
        }



        /// <summary>
        /// Checks if a table exists in a SQLite database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connection"></param>
        /// <returns></returns>
        public static bool TableExists<T>(this SQLiteConnection connection) {
            const string cmdText = "SELECT name FROM sqlite_master WHERE type='table' AND name=?";
            var cmd = connection.CreateCommand(cmdText, typeof(T).Name);
            return cmd.ExecuteScalar<string>() != null;
        }

        /// <summary>
        /// Checks if a table exists in a SQLite database
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="asyncConnection"></param>
        /// <returns></returns>
        public static async Task<bool> TableExistsAsync<T>(this SQLiteAsyncConnection asyncConnection) {
            try {
                var tableInfo = await asyncConnection.GetTableInfoAsync(typeof(T).Name);
                if (tableInfo.Count > 0) {
                    return true;
                } else {
                    return false;
                }
            } catch {
                return false;
            }
        }

        public static bool TryGetTable<T>(this SQLiteConnection connection, out TableQuery<T> returnTable) where T : new() {
            if (connection.TableExists<T>()) {
                returnTable = connection.Table<T>();
                return true;
            } else {
                returnTable = null;
                return false;
            }
        }

        /// <summary>
        /// Makes a letter from a number.
        /// Wraps around if over max.
        /// 0 = A,
        /// 25 = Z,
        /// 26 = A,
        /// </summary>
        /// <param name="index"></param>
        /// <param name="isCaps"></param>
        /// <returns></returns>
        public static string IndexToLetter(int index, bool isCaps = true) {
            int charNumber = isCaps ? 65 : 97;
            char c = (char)(charNumber + index % 25);
            return c.ToString();
        }

        /// <summary>
        /// Genearates a gradient between two colors in the number of steps specified
        /// </summary>
        /// <param name="startColor"></param>
        /// <param name="endColor"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        public static List<Color> GetGradient(Color startColor, Color endColor, int steps) {
            var gradient = new List<Color>();
            var r = (endColor.R - startColor.R) / (steps);
            var b = (endColor.B - startColor.B) / (steps);
            var g = (endColor.G - startColor.G) / (steps);
            var a = (endColor.A - startColor.A) / (steps);
            for (var i = 0; i <= steps; i++) {
                gradient.Add(new Color(startColor.R + r * i, startColor.G + g * i, startColor.B + b * i, startColor.A + a * i));
            }

            return gradient;
        }


 

        public static (int outerIndex, int innerIndex) GetIndexOfPosition<T>(this T[][] jaggered, int position) {
            var innerIndex = position;
            for (int outerIndex = 0; outerIndex < jaggered.Length; outerIndex++) {
                if (innerIndex >= jaggered[outerIndex].Length) {
                    innerIndex -= jaggered[outerIndex].Length;
                } else {
                    return (outerIndex, innerIndex);
                }
            }
            throw new IndexOutOfRangeException();
        }


        /// <summary>
        /// Generates Lorem Ipsum
        /// </summary>
        /// <param name="minWords"></param>
        /// <param name="maxWords"></param>
        /// <param name="minSentences"></param>
        /// <param name="maxSentences"></param>
        /// <param name="numLines"></param>
        /// <returns></returns>
        public static string LoremIpsum(int minWords, int maxWords, int minSentences, int maxSentences, int numLines) {
            var loremIpsum = "sed ut perspiciatis unde omnis iste natus error sit voluptatem accusantium doloremque laudantium totam rem aperiam eaque ipsa quae ab illo inventore veritatis et quasi architecto beatae vitae dicta sunt explicabo nemo enim ipsam voluptatem quia voluptas sit aspernatur aut odit aut fugit sed quia consequuntur magni dolores eos qui ratione voluptatem sequi nesciunt neque porro quisquam est qui dolorem ipsum quia dolor sit amet consectetur adipisci velit sed quia non numquam eius modi tempora incidunt ut labore et dolore magnam aliquam quaerat voluptatem ut enim ad minima veniam quis nostrum exercitationem ullam corporis suscipit laboriosam nisi ut aliquid ex ea commodi consequatur quis autem vel eum iure reprehenderit qui in ea voluptate velit esse quam nihil molestiae consequatur vel illum qui dolorem eum fugiat quo voluptas nulla pariatur";

            var words = loremIpsum.Split(' ');

            int numSentences = staticRandom.Next(maxSentences - minSentences)
                + minSentences;
            int numWords = staticRandom.Next(maxWords - minWords) + minWords;

            var sb = new StringBuilder();
            for (int p = 0; p < numLines; p++) {
                for (int s = 0; s < numSentences; s++) {
                    for (int w = 0; w < numWords; w++) {
                        if (w > 0) { sb.Append(" "); }
                        string word = words[staticRandom.Next(words.Length)];
                        if (w == 0) { word = word.Substring(0, 1).Trim().ToUpper() + word.Substring(1); }
                        sb.Append(word);
                    }
                    sb.Append(". ");
                }
                if (p < numLines - 1) {
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        public static void ShowMessageShort(string message) {
            CrossFunctions.ShowMessageShort(message);
        }

        public static void ShowMessageLong(string message) {
            CrossFunctions.ShowMessageLong(message);
        }



        public static void RemoveEvents<T>(this T target, string eventName) where T : class {
            if (ReferenceEquals(target, null)) {
                throw new NullReferenceException("Argument \"target\" may not be null.");
            }
            FieldInfo fieldInfo = typeof(T).GetField(eventName, BindingFlags.Static | BindingFlags.NonPublic);
            if (ReferenceEquals(fieldInfo, null)) {
                throw new ArgumentException($"{typeof(T).Name} does not have a property with the name \"{eventName}\"");
            }
            object eventInstance = fieldInfo.GetValue(target);
            PropertyInfo propInfo = typeof(T).GetProperty("Events", BindingFlags.NonPublic | BindingFlags.Instance);
            EventHandlerList list = (EventHandlerList)propInfo.GetValue(target, null);
            list.RemoveHandler(eventInstance, list[eventInstance]);
        }

        public static void SerializeJsonIntoStream(object value, Stream stream) {
            using (var streamWriter = new StreamWriter(stream, new UTF8Encoding(false), 1024, true))
            using (var jsonTextWriter = new JsonTextWriter(streamWriter) { Formatting = Formatting.None }) {
                var jsonSerializer = new JsonSerializer();
                jsonSerializer.Serialize(jsonTextWriter, value);
                jsonTextWriter.Flush();
            }
        }

        private static T DeserializeJsonFromStream<T>(Stream stream) {
            if (stream == null || stream.CanRead == false) {
                return default(T);
            }
            using (var streamReader = new StreamReader(stream)) {
                using (var jsonTestReader = new JsonTextReader(streamReader)) {
                    var jsonSerializer = new JsonSerializer();
                    var searchResult = jsonSerializer.Deserialize<T>(jsonTestReader);
                    return searchResult;
                }
            }
        }

        public static HttpContent CreateHttpContent(object content) {
            HttpContent httpContent = null;

            if (content != null) {

                var memoryStream = new MemoryStream();
                SerializeJsonIntoStream(content, memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                httpContent = new StreamContent(memoryStream);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            }

            return httpContent;
        }
    }
}
