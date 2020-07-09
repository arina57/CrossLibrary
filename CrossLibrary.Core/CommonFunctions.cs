using CrossLibrary.Dependency;
using CrossLibrary.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrossLibrary {
    /// <summary>
    /// Static functions that are crossplatform
    /// </summary>
    public static class CommonFunctions {

        /// <summary>
        /// This is just here so there doesn't need to be a bunch of ugly dependency injection calls through out the code
        /// </summary>
        public static ICrossFunctions CrossFunctions = CrossViewDependencyService.Get<ICrossFunctions>(CrossViewDependencyService.DependencyFetchTarget.GlobalInstance);


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




        public static Random StaticRandom { get; } = new Random();

        /// <summary>
        /// Shuffles an array using a <see cref="Random"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        public static void Shuffle<T>(this IList<T> list) {
            int n = list.Count;
            while (n > 1) {
                n--;
                int k = StaticRandom.Next(n + 1);
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
                var newColor = Color.FromArgb(startColor.A + a * i, startColor.R + r * i, startColor.G + g * i, startColor.B + b * i);
                gradient.Add(newColor);
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

            int numSentences = StaticRandom.Next(maxSentences - minSentences)
                + minSentences;
            int numWords = StaticRandom.Next(maxWords - minWords) + minWords;

            var sb = new StringBuilder();
            for (int p = 0; p < numLines; p++) {
                for (int s = 0; s < numSentences; s++) {
                    for (int w = 0; w < numWords; w++) {
                        if (w > 0) { sb.Append(" "); }
                        string word = words[StaticRandom.Next(words.Length)];
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






        /// <summary>
        /// Updates the text in intervals.
        /// If less than 20ms have passed since last update, then it won't update, to avoid excessive ui updates
        /// </summary>
        /// <param name="label"></param>
        /// <param name="durationMilis"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="formattedString"></param>
        /// <returns></returns>
        public static async Task AnimateTextNumberAsync(Action<string> action, int durationMilis, int from, int to, string formattedString = "{0}", CancellationToken cancellationToken = default) {
            if (from != to) {
                int lenght = Math.Abs(from - to);
                var delayPerLoop = durationMilis / lenght;
                var delaySinceLastDisplay = 0;
                const int displayMinDelay = 20;
                action.Invoke(string.Format(formattedString, from));
                for (int i = 0; i <= lenght; i++) {
                    if (cancellationToken.IsCancellationRequested) {
                        return;
                    }

                    if (delaySinceLastDisplay >= displayMinDelay) {
                        int step = from < to ? from + i : to + lenght - i;
                        action.Invoke(string.Format(formattedString, step));
                    }


                    try {
                        await Task.Delay(delayPerLoop, cancellationToken);
                    } catch (TaskCanceledException) {
                        return;
                    }
                    delaySinceLastDisplay += delayPerLoop;

                }
            }
            action.Invoke(string.Format(formattedString, to));
        }


        /// <summary>
        /// Updates the text in intervals.
        /// If less than 20ms have passed since last update, then it won't update, to avoid excessive ui updates
        /// </summary>
        /// <param name="label"></param>
        /// <param name="durationMilis"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="formattedString"></param>
        /// <returns></returns>
        public static async Task AnimateTextNumberSpeedAsync(Action<string> action, int from, int to, string formattedString = "{0}", CancellationToken cancellationToken = default, int frequency = 5) {
            if (from != to) {
                int lenght = Math.Abs(from - to);
                var delaySinceLastDisplay = 0;
                const int displayMinDelay = 20;
                action.Invoke(string.Format(formattedString, from));
                var delay = 1000 / frequency;
                for (int i = 0; i <= lenght; i++) {

                    if (cancellationToken.IsCancellationRequested) {
                        return;
                    }

                    if (delaySinceLastDisplay >= displayMinDelay) {
                        int step = from < to ? from + i : to + lenght - i;
                        action.Invoke(string.Format(formattedString, step));
                    }
                    try {
                        await Task.Delay(delay, cancellationToken);
                    } catch (TaskCanceledException) {
                        return;
                    }
                    
                    delaySinceLastDisplay += delay;

                }
            }
            action.Invoke(string.Format(formattedString, to));
        }

        /// <summary>
        /// Updates the text in intervals.
        /// If less than 20ms have passed since last update, then it won't update, to avoid excessive ui updates
        /// </summary>
        /// <param name="label"></param>
        /// <param name="durationMilis"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="formattedString"></param>
        /// <returns></returns>
        public static async Task AnimateTextNumberAsync(Action<string> action, int from, int to, string formattedString = "{0}", CancellationToken cancellationToken = default, int durationMs = 1000) {
            if (from != to) {
                int lenght = Math.Abs(from - to);
                var delaySinceLastDisplay = 0;
                const int displayMinDelay = 20;
                action.Invoke(string.Format(formattedString, from));
                var delay = durationMs / lenght;
                for (int i = 0; i <= lenght; i++) {

                    if (cancellationToken.IsCancellationRequested) {
                        return;
                    }

                    if (delaySinceLastDisplay >= displayMinDelay) {
                        int step = from < to ? from + i : to + lenght - i;
                        action.Invoke(string.Format(formattedString, step));
                    }
                    try {
                        await Task.Delay(delay, cancellationToken);
                    } catch (TaskCanceledException) {
                        return;
                    }

                    delaySinceLastDisplay += delay;

                }
            }
            action.Invoke(string.Format(formattedString, to));
        }


        /// <summary>
        /// Updates the text in intervals.
        /// If less than 20ms have passed since last update, then it won't update, to avoid excessive ui updates
        /// </summary>
        /// <param name="label"></param>
        /// <param name="durationMilis"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static async Task AnimateTextValuesAsync(Action<string> action, int durationMilis, CancellationToken cancellationToken = default, params string[] values) {
            if (values.Length > 0) {
                var delayPerLoop = durationMilis / values.Length;
                var delaySinceLastDisplay = 0;
                const int displayMinDelay = 20;
                action.Invoke(values[0]);
                foreach (var value in values) {
                    if (cancellationToken.IsCancellationRequested) {
                        return;
                    }

                    if (delaySinceLastDisplay >= displayMinDelay) {
                        action.Invoke(value);
                    }
                    
                    await Task.Delay(delayPerLoop, cancellationToken);
                    delaySinceLastDisplay += delayPerLoop;
                }
                if (!cancellationToken.IsCancellationRequested) {
                    action.Invoke(values.Last());
                }
            }

        }

        /// <summary>
        /// Updates the text in intervals.
        /// If less than 20ms have passed since last update, then it won't update, to avoid excessive ui updates
        /// </summary>
        /// <param name="label"></param>
        /// <param name="durationMilis"></param>
        /// <param name="values"></param>
        /// <returns></returns>
        public static async Task AnimateTextValuesAsync(Action<string> action, CancellationToken cancellationToken = default, int frequency = 5, params string[] values) {
            if (values.Length > 0) {
                var delayPerLoop = 1000 / frequency;
                var delaySinceLastDisplay = 0;
                const int displayMinDelay = 20;
                action.Invoke(values[0]);
                foreach (var value in values) {
                    if (cancellationToken.IsCancellationRequested) {
                        return;
                    }

                    if (delaySinceLastDisplay >= displayMinDelay) {
                        action.Invoke(value);
                    }

                    try {
                        await Task.Delay(delayPerLoop, cancellationToken);
                    } catch (TaskCanceledException) {
                        return;
                    }
                    delaySinceLastDisplay += delayPerLoop;
                }
                
                action.Invoke(values.Last());
                
            }

        }


        /// <summary>
        /// Updates float in 20ms intervals
        /// </summary>
        /// <param name="label"></param>
        /// <param name="durationMilis"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="formattedString"></param>
        /// <returns></returns>
        public static async Task AnimateFloatValueAsync(Action<float> action, int durationMilis, float from, float to, int frequency = 50, CancellationToken cancellationToken = default) {
            if (from != to) {
                var displayDelay = 1000 / frequency;
                var stepCount = durationMilis / (float)displayDelay;
                var stepAmount =  (to - from) / stepCount;
                var currentValue = from;
                action.Invoke(currentValue);
                for (int i = 0; i <= stepCount; i++) {
                    if (cancellationToken.IsCancellationRequested) {
                        return;
                    }

                    currentValue += stepAmount;
                    action.Invoke(currentValue);
                    try {
                        await Task.Delay(displayDelay, cancellationToken);
                    } catch (TaskCanceledException) {
                        return;
                    }
                    
                }
            }
            action.Invoke(to);
        }
    }
}
